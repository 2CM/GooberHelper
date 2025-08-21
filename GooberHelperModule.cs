using System;
using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Monocle;
using Microsoft.Xna.Framework;
using System.Text.RegularExpressions;
using Celeste.Mod.GooberHelper.Entities;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Celeste.Mod.Helpers;
using Celeste.Mod.GooberHelper.Components;
using System.Collections.Generic;
using Celeste.Mod.GooberHelper.UI;
using static Celeste.Mod.GooberHelper.OptionsManager;
using FMOD.Studio;

namespace Celeste.Mod.GooberHelper {
    public class GooberHelperModule : EverestModule {
        public static GooberHelperModule Instance { get; private set; }


        public override Type SettingsType => typeof(GooberHelperModuleSettings);
        public static GooberHelperModuleSettings Settings => (GooberHelperModuleSettings) Instance._Settings;

        public override Type SessionType => typeof(GooberHelperModuleSession);
        public static GooberHelperModuleSession Session => (GooberHelperModuleSession) Instance._Session;

        private static ILHook playerUpdateHook;
        private static ILHook playerStarFlyCoroutineHook;
        private static ILHook playerDashCoroutineHook;
        private static ILHook playerPickupCoroutineHook;
        private static ILHook playerRedDashCoroutineHook;
        private static ILHook playerDashCoroutineHook2;
        private static ILHook silverBlockAwakeHook;
        private static ILHook platinumBlockAwakeHook;

        private static Color lastPlayerHairColor = Player.NormalHairColor;

        private static Regex refillRoutineRegex = new Regex("RefillRoutine");

        private static Effect playerMaskEffect = null;
        private static bool startedRendering = false;

        public bool UseAwesomeRetention {
            get {
                return GetOptionBool(Option.CustomSwimming) || GetOptionBool(Option.VerticalSpeedToHorizontalSpeedOnGroundJump);
            }
        }


        public GooberHelperModule() {
            Instance = this;
#if DEBUG
            // debug builds use verbose logging
            Logger.SetLogLevel(nameof(GooberHelperModule), LogLevel.Verbose);
#else
            // release builds use info logging to reduce spam in log files
            Logger.SetLogLevel(nameof(GooberHelperModule), LogLevel.Info);
#endif
        }

        public override void Load() {
            ModIntegration.FrostHelperAPI.Load();

            FluidSimulation.Load();
            //i really gotta refactor this man
            AbstractTrigger<GooberPhysicsOptions>.Load();
            AbstractTrigger<GooberVisualOptions>.Load();
            AbstractTrigger<GooberMiscellaneousOptions>.Load();
            AbstractTrigger<RefillFreezeLength>.Load();
            AbstractTrigger<RetentionFrames>.Load();

            GooberHelperOptions.Load();

            Everest.Events.Level.OnCreatePauseMenuButtons += createPauseMenuButton;

            playerUpdateHook = new ILHook(typeof(Player).GetMethod("orig_Update"), modifyPlayerUpdate);
            playerStarFlyCoroutineHook = new ILHook(typeof(Player).GetMethod("StarFlyCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), modifyPlayerStarFlyCoroutine);
            playerDashCoroutineHook = new ILHook(typeof(Player).GetMethod("DashCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), modifyPlayerDashCoroutine);
            playerPickupCoroutineHook = new ILHook(typeof(Player).GetMethod("PickupCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), modifyPlayerPickupCoroutine);
            playerRedDashCoroutineHook = new ILHook(typeof(Player).GetMethod("RedDashCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), modifyDashSpeedThing);
            playerDashCoroutineHook2 = new ILHook(typeof(Player).GetMethod("DashCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), modifyDashSpeedThing);
            
            if(Everest.Loader.DependencyLoaded(new EverestModuleMetadata() { Name = "CollabUtils2", Version = new Version(1, 10, 0) })) {
                //feel free to opp PLEASE i need to learn a better way of doing this
                Type type = Type.GetType("Celeste.Mod.CollabUtils2.Entities.SilverBlock, CollabUtils2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

                silverBlockAwakeHook = new ILHook(type.GetMethod("Awake"), makeGoldenBlocksOrSimilarEntitiesAlwaysLoad);
            }

            if(Everest.Loader.DependencyLoaded(new EverestModuleMetadata() { Name = "PlatinumStrawberry", Version = new Version(1, 0, 0) })) {
                Type type = Type.GetType("Celeste.Mod.PlatinumStrawberry.Entities.PlatinumBlock, PlatinumStrawberry, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

                platinumBlockAwakeHook = new ILHook(type.GetMethod("Awake"), makeGoldenBlocksOrSimilarEntitiesAlwaysLoad);
            }

            IL.Celeste.GoldenBlock.Awake += makeGoldenBlocksOrSimilarEntitiesAlwaysLoad;

            IL.Celeste.Player.OnCollideH += modifyPlayerOnCollideH;
            IL.Celeste.Player.OnCollideV += modifyPlayerOnCollideV;
            IL.Celeste.Player.StarFlyUpdate += modifyPlayerStarFlyUpdate;
            IL.Celeste.Player.DashUpdate += modifyPlayerDashUpdate;
            IL.Celeste.Player.NormalUpdate += modifyPlayerNormalUpdate;
            IL.Celeste.Player.RedDashUpdate += modifyPlayerRedDashUpdate;
            IL.Celeste.Player.HitSquashUpdate += modifyPlayerHitSquashUpdate;
            IL.Celeste.Player.DreamDashEnd += modifyPlayerDreamDashEnd;
            IL.Celeste.Player.DreamDashUpdate += modifyPlayerDreamDashUpdate;
            IL.Celeste.Player.LaunchUpdate += modifyPlayerLaunchUpdate;
            IL.Celeste.Player.WallJumpCheck += modifyPlayerWallJumpCheck;

            On.Celeste.Player.Update += modPlayerUpdate;
            On.Celeste.Player.Jump += modPlayerJump;
            On.Celeste.Player.Rebound += modPlayerRebound;
            On.Celeste.Player.ReflectBounce += modPlayerReflectBounce;
            On.Celeste.Player.PointBounce += modPlayerPointBounce;
            On.Celeste.Player.OnCollideH += modPlayerOnCollideH;
            On.Celeste.Player.OnCollideV += modPlayerOnCollideV;
            On.Celeste.Player.SuperWallJump += modPlayerSuperWallJump;
            On.Celeste.Player.DreamDashBegin += modPlayerDreamDashBegin;
            On.Celeste.Player.SideBounce += modPlayerSideBounce;
            On.Celeste.Player.SuperBounce += modPlayerSuperBounce;
            On.Celeste.Player.WallJump += modPlayerWallJump;
            On.Celeste.Player.ClimbJump += modPlayerClimbJump;
            On.Celeste.Player.StarFlyBegin += modPlayerStarFlyBegin;
            On.Celeste.Player.ExplodeLaunch_Vector2_bool_bool += modPlayerExplodeLaunch;
            On.Celeste.Player.FinalBossPushLaunch += modPlayerFinalBossPushLaunch;
            On.Celeste.Player.AttractBegin += modPlayerAttractBegin;
            On.Celeste.Player.SwimBegin += modPlayerSwimBegin;
            On.Celeste.Player.WallJumpCheck += modPlayerWallJumpCheck;
            On.Celeste.Player.DashBegin += modPlayerDashBegin;
            On.Celeste.Player.NormalEnd += modPlayerNormalEnd;
            On.Celeste.Player.SuperJump += modPlayerSuperJump;
            On.Celeste.Player.ctor += modPlayerCtor;
            On.Celeste.Player.BeforeUpTransition += modPlayerBeforeUpTransition;
            On.Celeste.Player.Boost += modPlayerBoost;
            On.Celeste.Player.RedBoost += modPlayerRedBoost;
            On.Celeste.Player.Render += modPlayerRender;
            On.Celeste.Player.SwimCheck += modPlayerSwimCheck;
            On.Celeste.Player.SwimJumpCheck += modPlayerSwimJumpCheck;
            On.Celeste.Player.UnderwaterMusicCheck += modPlayerUnderwaterMusicCheck;
            On.Celeste.Player.Pickup += modPlayerPickup;
            On.Celeste.Player.NormalBegin += modPlayerNormalBegin;

            On.Celeste.BounceBlock.WindUpPlayerCheck += modBounceBlockWindUpPlayerCheck;

            On.Celeste.CrystalStaticSpinner.OnPlayer += modCrystalStaticSpinnerOnPlayer;

            On.Celeste.Celeste.Freeze += modCelesteFreeze;

            On.Celeste.Level.LoadLevel += modLevelLevelLoad;
            On.Celeste.Level.Pause += modLevelPause;
            On.Celeste.Level.Update += modLevelUpdate;
            On.Monocle.Scene.BeforeUpdate += modSceneBeforeUpdate;

            On.Celeste.PlayerDeadBody.Render += modPlayerDeadBodyRender;

            On.Celeste.Holdable.Release += modHoldableRelease;

            On.Celeste.TheoCrystal.ctor_Vector2 += modTheoCrystalCtor;

            //code adapted from https://github.com/0x0ade/CelesteNet/blob/405a7e5e4d78727cd35ee679a730400b0a46667a/CelesteNet.Client/Components/CelesteNetMainComponent.cs#L71-L75 (thank you snip for posting this link 8 months ago)
            using (new DetourConfigContext(new DetourConfig(
                "GooberHelper",
                int.MinValue  // this simulates before: "*"
            )).Use()) {
                On.Celeste.Player.SwimUpdate += modPlayerSwimUpdate;
            }

            using (new DetourConfigContext(new DetourConfig(
                "GooberHelper",
                int.MaxValue
            )).Use()) {
                On.Celeste.PlayerHair.Render += modPlayerHairRender;
            }
        }

        public override void Unload() {
            FluidSimulation.Unload();
            AbstractTrigger<GooberPhysicsOptions>.Unload();
            AbstractTrigger<GooberVisualOptions>.Unload();
            AbstractTrigger<GooberMiscellaneousOptions>.Unload();
            AbstractTrigger<RefillFreezeLength>.Unload();
            AbstractTrigger<RetentionFrames>.Unload();

            GooberHelperOptions.Unload();

            Everest.Events.Level.OnCreatePauseMenuButtons -= createPauseMenuButton;

            playerUpdateHook.Dispose();
            playerStarFlyCoroutineHook.Dispose();
            playerDashCoroutineHook.Dispose();
            playerPickupCoroutineHook.Dispose();
            playerRedDashCoroutineHook.Dispose();
            playerDashCoroutineHook2.Dispose();

            IL.Celeste.Player.OnCollideH -= modifyPlayerOnCollideH;
            IL.Celeste.Player.OnCollideV -= modifyPlayerOnCollideV;
            IL.Celeste.Player.StarFlyUpdate -= modifyPlayerStarFlyUpdate;
            IL.Celeste.Player.DashUpdate -= modifyPlayerDashUpdate;
            IL.Celeste.Player.NormalUpdate -= modifyPlayerNormalUpdate;
            IL.Celeste.Player.RedDashUpdate -= modifyPlayerRedDashUpdate;
            IL.Celeste.Player.HitSquashUpdate -= modifyPlayerHitSquashUpdate;
            IL.Celeste.Player.DreamDashEnd -= modifyPlayerDreamDashEnd;
            IL.Celeste.Player.DreamDashUpdate -= modifyPlayerDreamDashUpdate;
            IL.Celeste.Player.LaunchUpdate -= modifyPlayerLaunchUpdate;
            IL.Celeste.Player.WallJumpCheck -= modifyPlayerWallJumpCheck;

            IL.Celeste.GoldenBlock.Awake -= makeGoldenBlocksOrSimilarEntitiesAlwaysLoad;

            silverBlockAwakeHook?.Dispose();
            platinumBlockAwakeHook?.Dispose();

            On.Celeste.Player.Update -= modPlayerUpdate;
            On.Celeste.Player.Jump -= modPlayerJump;
            On.Celeste.Player.Rebound -= modPlayerRebound;
            On.Celeste.Player.ReflectBounce -= modPlayerReflectBounce;
            On.Celeste.Player.PointBounce -= modPlayerPointBounce;
            On.Celeste.Player.OnCollideH -= modPlayerOnCollideH;
            On.Celeste.Player.OnCollideV -= modPlayerOnCollideV;
            On.Celeste.Player.SuperWallJump -= modPlayerSuperWallJump;
            On.Celeste.Player.DreamDashBegin -= modPlayerDreamDashBegin;
            On.Celeste.Player.SideBounce -= modPlayerSideBounce;
            On.Celeste.Player.SuperBounce -= modPlayerSuperBounce;
            On.Celeste.Player.WallJump -= modPlayerWallJump;
            On.Celeste.Player.ClimbJump -= modPlayerClimbJump;
            On.Celeste.Player.StarFlyBegin -= modPlayerStarFlyBegin;
            On.Celeste.Player.ExplodeLaunch_Vector2_bool_bool -= modPlayerExplodeLaunch;
            On.Celeste.Player.FinalBossPushLaunch -= modPlayerFinalBossPushLaunch;
            On.Celeste.Player.AttractBegin -= modPlayerAttractBegin;
            On.Celeste.Player.SwimBegin -= modPlayerSwimBegin;
            On.Celeste.Player.WallJumpCheck -= modPlayerWallJumpCheck;
            On.Celeste.Player.DashBegin -= modPlayerDashBegin;
            On.Celeste.Player.NormalEnd -= modPlayerNormalEnd;
            On.Celeste.Player.SuperJump -= modPlayerSuperJump;
            On.Celeste.Player.ctor -= modPlayerCtor;
            On.Celeste.Player.BeforeUpTransition -= modPlayerBeforeUpTransition;
            On.Celeste.Player.Boost -= modPlayerBoost;
            On.Celeste.Player.RedBoost -= modPlayerRedBoost;
            On.Celeste.Player.Render -= modPlayerRender;
            On.Celeste.Player.SwimCheck -= modPlayerSwimCheck;
            On.Celeste.Player.SwimJumpCheck -= modPlayerSwimJumpCheck;
            On.Celeste.Player.UnderwaterMusicCheck -= modPlayerUnderwaterMusicCheck;
            On.Celeste.Player.Pickup -= modPlayerPickup;
            On.Celeste.Player.NormalBegin -= modPlayerNormalBegin;

            On.Celeste.BounceBlock.WindUpPlayerCheck -= modBounceBlockWindUpPlayerCheck;

            On.Celeste.CrystalStaticSpinner.OnPlayer -= modCrystalStaticSpinnerOnPlayer;

            On.Celeste.Celeste.Freeze -= modCelesteFreeze;

            On.Celeste.Level.LoadLevel -= modLevelLevelLoad;
            On.Celeste.Level.Pause -= modLevelPause;
            On.Celeste.Level.Update -= modLevelUpdate;
            On.Monocle.Scene.BeforeUpdate -= modSceneBeforeUpdate;

            On.Celeste.PlayerDeadBody.Render -= modPlayerDeadBodyRender;

            On.Celeste.Holdable.Release -= modHoldableRelease;
            
            On.Celeste.TheoCrystal.ctor_Vector2 -= modTheoCrystalCtor;

            //code adapted from https://github.com/0x0ade/CelesteNet/blob/405a7e5e4d78727cd35ee679a730400b0a46667a/CelesteNet.Client/Components/CelesteNetMainComponent.cs#L71-L75 (thank you snip for posting this link 8 months ago)
            using (new DetourConfigContext(new DetourConfig(
                "GooberHelper",
                int.MinValue  // this simulates before: "*"
            )).Use()) {
                On.Celeste.Player.SwimUpdate -= modPlayerSwimUpdate;
            }

            using (new DetourConfigContext(new DetourConfig(
                "GooberHelper",
                int.MaxValue
            )).Use()) {
                On.Celeste.PlayerHair.Render -= modPlayerHairRender;
            }
        }

        public override void CreateModMenuSection(TextMenu menu, bool inGame, EventInstance snapshot) {
            base.CreateModMenuSection(menu, inGame, snapshot);

            //add profile saving or something

            menu.Add(new TextMenu.Button(Dialog.Clean("menu_gooberhelper_reset_all_options")).Pressed(() => {
                ResetAll(OptionSetter.User);
            }));
        }

        //thank you everest!!! i stole a lot of your mod options code here; i hope you dont mind
        private void createPauseMenuButton(Level level, TextMenu menu, bool minimal) {
            if(!Settings.ShowOptionsInGame) return;

            TextMenuExt.ButtonExt button = new TextMenuExt.ButtonExt(Dialog.Clean("menu_gooberhelper_options"));

            button.TextColor = GetGlobalColor();

            int index = menu.items.FindIndex(item => item is TextMenu.Button && (item as TextMenu.Button).Label == Dialog.Clean("menu_pause_options"));
            menu.Insert(index, button);
            
            button.OnPressed = () => {
                int returnIndex = menu.IndexOf(button);
                menu.RemoveSelf();

                TextMenu options = OuiGooberHelperOptions.CreateMenu();

                OuiGooberHelperOptions.pauseMenuReturnIndex = returnIndex;
                OuiGooberHelperOptions.pauseMenuMinimal = minimal;

                level.Add(options);
            };
        }

        private void handleVerticalJumpSpeed(Player self, Vector2 originalSpeed, bool isClimbjump = false) {
            bool doDownwardsStuff = GetOptionBool(Option.DownwardsJumpSpeedPreservation) && !isClimbjump && originalSpeed.Y > 0 && Input.MoveY > 0;

            //gaslighting my own mod
            //did you know that gaslighting was invented by john gas?
            if(doDownwardsStuff) {
                self.Speed.Y *= -1;
                originalSpeed.Y *= -1;
            }

            if(GetOptionBool(Option.AdditiveVerticalJumpSpeed)) {
                self.Speed.Y = Math.Min(self.Speed.Y, self.varJumpSpeed + Math.Min(originalSpeed.Y, 0));

                self.varJumpSpeed = self.Speed.Y;
            } else if(
                (originalSpeed.Y < ((false /* IMPLEMENT LATER */) ? -240f : self.varJumpSpeed) && GetOptionBool(Option.UpwardsJumpSpeedPreservation)) ||
                (originalSpeed.Y < -90f && doDownwardsStuff) //remember that this is inverted
            ) {
                self.Speed.Y = originalSpeed.Y + self.LiftBoost.Y;
                self.varJumpSpeed = self.Speed.Y;
            }

            //soliddarking someone elses mod
            if(doDownwardsStuff) {
                self.Speed.Y *= -1;
                originalSpeed.Y *= -1;
                self.varJumpSpeed *= -1;
            }
        }

        private void modPlayerNormalBegin(On.Celeste.Player.orig_NormalBegin orig, Player self) {
            float originalMaxFall = self.maxFall;
            
            orig(self);

            if(GetOptionBool(Option.DownwardsJumpSpeedPreservation)) self.maxFall = originalMaxFall;
        }

        private void modTheoCrystalCtor(On.Celeste.TheoCrystal.orig_ctor_Vector2 orig, TheoCrystal self, Vector2 position) {
            orig(self, position);

            self.Add(new TheoNuclearReactor());
        }

        private void modHoldableRelease(On.Celeste.Holdable.orig_Release orig, Holdable self, Vector2 force) {
            Vector2 playerSpeed = self.Holder.Speed;
            
            orig(self, force);

            if(!GetOptionBool(Option.HoldablesInheritSpeedWhenThrown)) return;

            Vector2 holdableSpeed = self.SpeedGetter.Invoke();
            float newLaunchSpeed = force.X * Math.Max(Math.Abs(holdableSpeed.X), Math.Abs(playerSpeed.X) * 0.8f);

            self.SpeedSetter.Invoke(new Vector2(newLaunchSpeed, holdableSpeed.Y));
        }

        private bool modPlayerPickup(On.Celeste.Player.orig_Pickup orig, Player self, Holdable pickup) {
            bool ducking = self.Ducking;
            
            bool value = orig(self, pickup);

            if(GetOptionBool(Option.AllowCrouchedHoldableGrabbing)) self.Ducking = ducking;

            return value;
        }

        private void allowCrouchedHoldableGrabbing(ILCursor cursor, bool protect1, bool protect2) {
            ILLabel jumpLabel = null;

            if(cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchCallOrCallvirt<Player>("get_Ducking"),
                instr => instr.MatchBrtrue(out jumpLabel)
            )) {
                cursor.Index--;

                cursor.EmitDelegate((bool value) => {
                    if(GetOptionBool(Option.AllowCrouchedHoldableGrabbing)) return false;

                    return value;
                });
            }

            if(protect1) {
                //i was tweaking the fuck out over this
                //i tried to insert my instructions before the block that i just hooked because why wouldnt i
                //but it would always cause an invalid program error upon a cold reload
                //apparently going before this block of instructions puts you at the very end of a finally {} block and causes monomod to shit itself
                //that was a fun one to figure out

                if(cursor.TryGotoNextBestFit(MoveType.After,
                    instr => instr.MatchLdarg0(),
                    instr => instr.MatchLdflda<Player>("Speed"),
                    instr => instr.MatchLdfld<Vector2>("Y"),
                    instr => instr.MatchLdcI4(0),
                    instr => instr.MatchBltUn(out jumpLabel)
                )) {
                    cursor.EmitDelegate(() => {
                        if(GetOptionBool(Option.AllowCrouchedHoldableGrabbing)) return Engine.Scene.Tracker.GetEntity<Player>().Ducking;

                        return false;
                    });

                    cursor.EmitBrtrue(jumpLabel);
                }
            }
            
            if(protect2) {
                if(cursor.TryGotoNextBestFit(MoveType.After,
                    instr => instr.MatchBltUn(out _),
                    instr => instr.MatchLdarg0(),
                    instr => instr.MatchCall<Player>("get_CanUnDuck"),
                    instr => instr.MatchBrfalse(out _),
                    instr => instr.MatchLdarg0(),
                    instr => instr.MatchLdcI4(0)
                )) {
                    cursor.EmitDelegate((bool value) => {
                        if(GetOptionBool(Option.AllowCrouchedHoldableGrabbing)) return Engine.Scene.Tracker.GetEntity<Player>().Ducking;

                        return value;
                    });
                }
            }
        }

        private void allowAllDirectionDreamJumps(ILCursor cursor) {
            if(cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchLdfld<Vector2>("X"),
                instr => instr.MatchLdcR4(0f)
            )) {
                cursor.EmitDelegate((float value) => {
                    return GetOptionBool(Option.AllDirectionDreamJumps) ? 100f : value; //dummy value
                });
            }
        }

        //code stolen from https://github.com/EverestAPI/CelesteTAS-EverestInterop/blob/c3595e5af47bde0bca28e4693c80c180434c218c/CelesteTAS-EverestInterop/Source/EverestInterop/Hitboxes/CycleHitboxColor.cs
        //very helpful resource for this
        private void modSceneBeforeUpdate(On.Monocle.Scene.orig_BeforeUpdate orig, Scene self) {
            float timeActive = self.TimeActive;

            orig(self);

            //super ugly try catch
            //i would work it into the GooberPlayerExtensions.Instance getter but that runs really often and i dont want to cause any performance issues
            try {
                GooberPlayerExtensions c = GooberPlayerExtensions.Instance;

                if (self is Level && Math.Abs(timeActive - self.TimeActive) > 0.000001f && c != null) {
                    c.Counter++;
                }
            } catch {}
        }

        private void modLevelUpdate(On.Celeste.Level.orig_Update orig, Level self) {
            GooberPlayerExtensions c = GooberPlayerExtensions.Instance;

            if(GetOptionBool(Option.LenientStunning) && !self.Paused && c != null && c.StunningWatchTimer > 0f) {
                int offsetGroup = getOffsetGroup(c.StunningOffset);
                bool drifted = offsetGroup != c.StunningGroup;
                //i was going through trials and tribulations while trying to make this account for drift 😭
                //dont worry about the various console logs
                //i should really figure out how to use a debugger huh 

                // Console.WriteLine("-- watching");
                // Console.WriteLine("offset group: " + offsetGroup);
                // Console.WriteLine("drifted: " + drifted);
                // Console.WriteLine("stunning offset: " + c.StunningOffset);
                // Console.WriteLine("group offset: " + getGroupOffset(c.StunningGroup));

                if(drifted) {
                    c.StunningOffset = getGroupOffset(c.StunningGroup);// (c.StunningOffset - Engine.DeltaTime + 0.05f) % 0.05f;
                    
                    setStunnableEntityOffset(c.StunningOffset);
                }

                // Console.WriteLine("new group: " + getOffsetGroup(c.StunningOffset));
                // Console.WriteLine("new offset: " + c.StunningOffset);
                // Console.WriteLine("--");

                c.StunningWatchTimer -= Engine.DeltaTime;
            }

            orig(self);
        }

        //code stolen from https://github.com/EverestAPI/CelesteTAS-EverestInterop/blob/c3595e5af47bde0bca28e4693c80c180434c218c/CelesteTAS-EverestInterop/Source/EverestInterop/Hitboxes/CycleHitboxColor.cs
        private int getOffsetGroup(float offset) {
            float time = Engine.Scene.TimeActive;
            int timeDist = 0;

            while (Math.Floor(((double) time - offset - Engine.DeltaTime) / 0.05f) >= Math.Floor(((double) time - offset) / 0.05f) && timeDist < 3) {
                time += Engine.DeltaTime;
                timeDist++;
            }

            return timeDist < 3 ? (timeDist + GooberPlayerExtensions.Instance.Counter) % 3 : 3;
        }

        private float getGroupOffset(int targetGroup) {
            //terrible
            //terrible
            for(float i = 0; i < 1f; i += Engine.DeltaTime * 0.5f) {
                if(getOffsetGroup(i) == targetGroup) return i;
            }

            return -1;
        }

        private void setStunnableEntityOffset(float offset) {
            //i know using reflection for something like this is freaky as fuck,
            //but afaik this method looks optimized enough given that this method
            //only runs once upon pausing the game. feel free to ping me in modding
            //feedback and suggest an alternative 😭

            //also god damn this language is hot with the variable declaration inside of if statements thing
            //i want that in javascript or typescript so badly
            
            //look at me documenting my own code with comments
            //i should do this more often

            using (List<Component>.Enumerator enumerator = Engine.Scene.Tracker.GetComponents<PlayerCollider>().GetEnumerator()) {
				while (enumerator.MoveNext()) {
                    if (enumerator.Current.Entity is CrystalStaticSpinner spinner && !spinner.Collidable)
                        spinner.offset = offset;

                    if (enumerator.Current.Entity is Lightning lightning && !lightning.Collidable)
                        lightning.toggleOffset = offset;

                    if (enumerator.Current.Entity is DustStaticSpinner dust && !dust.Collidable)
                        dust.offset = offset;
                }
			}
        }

        private void modLevelPause(On.Celeste.Level.orig_Pause orig, Level self, int startIndex, bool minimal, bool quickReset) {
            orig(self, startIndex, minimal, quickReset);

            if(!GetOptionBool(Option.LenientStunning)) return;

            GooberPlayerExtensions c = GooberPlayerExtensions.Instance;
            
            if(c == null) return;

            //dont let the player pause buffer to mimic spinner stunning
            //11 because unpausing time still adds to the counter
            if(c.Counter <= c.LastPauseCounterValue + 11) {
                // Console.WriteLine("zog");
                c.LastPauseCounterValue = c.Counter;

                return;
            }
            c.LastPauseCounterValue = c.Counter;

            float offset = 0f;

            //i dont think it should ever reach 1 but better to be safe than to receive a surprise modding feedback ping
            while (!self.OnInterval(0.05f, offset) && offset < 5f) {
                offset += Engine.DeltaTime / 2f;
            }

            c.StunningWatchTimer = 0.2f;
            c.StunningOffset = offset;
            c.StunningGroup = getOffsetGroup(offset);

            setStunnableEntityOffset(offset);
            
            // Console.WriteLine("offsetGroup: " + c.StunningGroup);
            // Console.WriteLine("offset: " + offset);
        }

        private bool modPlayerSwimCheck(On.Celeste.Player.orig_SwimCheck orig, Player self) {
            if(self.CollideAll<Water>().Any(water => water is Waterfall && (water as Waterfall).nonCollidable)) return false;
            
            return orig(self);
        }

        private bool modPlayerSwimJumpCheck(On.Celeste.Player.orig_SwimJumpCheck orig, Player self) {
            if(self.CollideAll<Water>().Any(water => water is Waterfall && (water as Waterfall).nonCollidable)) return false;
            
            return orig(self);
        }

        private bool modPlayerUnderwaterMusicCheck(On.Celeste.Player.orig_UnderwaterMusicCheck orig, Player self) {
            if(self.CollideAll<Water>().Any(water => water is Waterfall && (water as Waterfall).nonCollidable)) return false;
            
            return orig(self);
        }

        private void modPlayerHairRender(On.Celeste.PlayerHair.orig_Render orig, PlayerHair self) {
            //i need the custom shader to not execute if the startedRendering boolean is false
            //this took way longer than it shouldve to figure out
            //there was a bug where the player trail would be offset from the player for some odd reason
            //i assumed it was a shader problem or more general thing for a while, but i eventually had the idea that it might be specific to the TrailManager (it was)
            //in TrailManager.BeforeRender(), it interrupts the spritebatch and renders specifically the PlayerHair with this method
            //that causes Something to mess up and somehow shift the player trail
            //the startedRendering boolean is set to true when the actual render method is called
            //that Should prevent this method from executing the custom shader code
            //i should document these things more often
            if(!GetOptionBool(Option.PlayerMaskHairOnly) || GetOptionBool(Option.PlayerMask) || !startedRendering) {
                orig(self);

                return;
            }

            if(self.Entity is not Player) return;

            doPlayerMaskStuffBefore(new Vector4((self.Entity as Player).Hair.Color.ToVector3() * (self.Entity as Player).Sprite.Color.ToVector3(), 1), true);

            orig(self);

            doPlayerMaskStuffAfter();

            startedRendering = false;
        }

        private void modPlayerDeadBodyRender(On.Celeste.PlayerDeadBody.orig_Render orig, PlayerDeadBody self) {
            if(!GetOptionBool(Option.PlayerMask)) {
                orig(self);

                return;
            }

            doPlayerMaskStuffBefore(lastPlayerHairColor.ToVector4());

            orig(self);

            doPlayerMaskStuffAfter();
        }
        
        private void modPlayerRender(On.Celeste.Player.orig_Render orig, Player self) {            
            startedRendering = true;

            if(!GetOptionBool(Option.PlayerMask)) {
                orig(self);

                return;
            }

            lastPlayerHairColor = self.Hair.Color;

            doPlayerMaskStuffBefore(new Vector4(self.Hair.Color.ToVector3() * self.Sprite.Color.ToVector3(), 1));

            orig(self);

            doPlayerMaskStuffAfter();
        }

        private void doPlayerMaskStuffBefore(Vector4 color, bool keepOutlines = false) {
            playerMaskEffect = ModIntegration.FrostHelperAPI.GetEffectOrNull.Invoke("playerMask");

            if((Engine.Scene as Level) == null) return;

            GameplayRenderer.End();

            Texture2D tex = GFX.Game["GooberHelper/mask"].Texture.Texture;
            
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, playerMaskEffect, (Engine.Scene as Level).GameplayRenderer.Camera.Matrix);
            playerMaskEffect.CurrentTechnique = playerMaskEffect.Techniques["Grongle"];
            playerMaskEffect.Parameters["CamPos"].SetValue((Engine.Scene as Level).Camera.Position);
            playerMaskEffect.Parameters["HairColor"].SetValue(color);
            playerMaskEffect.Parameters["TextureSize"].SetValue(new Vector2(tex.Width, tex.Height));
            playerMaskEffect.Parameters["Time"].SetValue(Engine.Scene.TimeActive);
            playerMaskEffect.Parameters["KeepOutlines"].SetValue(keepOutlines);
            Engine.Graphics.GraphicsDevice.Textures[1] = tex;
        }

        private void doPlayerMaskStuffAfter() {
            Draw.SpriteBatch.End();
            GameplayRenderer.Begin();
        }

        private void modPlayerRedBoost(On.Celeste.Player.orig_RedBoost orig, Player self, Booster booster) {
            GooberPlayerExtensions.Instance.BoostSpeedPreserved = self.Speed;

            orig(self, booster);
        }

        private void modPlayerBoost(On.Celeste.Player.orig_Boost orig, Player self, Booster booster) {
            GooberPlayerExtensions.Instance.BoostSpeedPreserved = self.Speed;

            orig(self, booster);
        }

        private void modifyDashSpeedThing(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if(cursor.TryGotoNext(MoveType.After, instr => instr.Match(OpCodes.Ldc_R4, 240f))) {
                cursor.EmitDelegate((float value) => {
                    if(!GetOptionBool(Option.BubbleSpeedPreservation)) return value;

                    GooberPlayerExtensions c = GooberPlayerExtensions.Instance;

                    value = Math.Max(c.BoostSpeedPreserved.Length(), value);

                    c.BoostSpeedPreserved = Vector2.Zero;

                    return value;
                });
            }
        }

        private void modPlayerBeforeUpTransition(On.Celeste.Player.orig_BeforeUpTransition orig, Player self) {
            if(!GetOptionBool(Option.KeepSpeedThroughVerticalTransitions)) {
                orig(self);

                return;
            }

            float varJumpTimer = self.varJumpTimer;
            float varJumpSpeed = self.varJumpSpeed;
            Vector2 speed = self.Speed;
            float dashCooldownTimer = self.dashCooldownTimer;

            orig(self);


            self.varJumpTimer = varJumpTimer;
            self.varJumpSpeed = varJumpSpeed;
            self.Speed = speed;
            self.dashCooldownTimer = dashCooldownTimer;
        }

        // public void modHoldableRelease(On.Celeste.Holdable.orig_Release orig, Holdable self, Vector2 force) {
        //     orig(self, force);

        //     Vector2 speed = DynamicData.For(self.Entity).Get<Vector2>("Speed"); 

        //     Player player = Engine.Scene.Tracker.GetEntity<Player>();

        //     DynamicData.For(self.Entity).Set("Speed", new Vector2(Math.Max(Math.Abs(player.Speed.X), Math.Abs(speed.X)) * Math.Sign(speed.X), speed.Y));
        // }

        // public void modifyPlayerThrow(ILContext il) {
        //     ILCursor cursor = new ILCursor(il);

        //     cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Callvirt);

        //     cursor.EmitDelegate(() => {
        //         Player player = Engine.Scene.Tracker.GetEntity<Player>();
        //         Logger.Log(LogLevel.Info, "i", player.Speed.ToString());

                
        //         Vector2 speed = DynamicData.For(player.Holding.Entity).Get<Vector2>("Speed");

        //         Logger.Log(LogLevel.Info, "i", player.Holding.Holder.ToString());
        //         Logger.Log(LogLevel.Info, "f", speed.ToString());
        //         DynamicData.For(player.Holding.Entity).Set("Speed", speed + Vector2.UnitX * Math.Abs(player.Speed.X) * Math.Sign(speed.X));

        //         speed = DynamicData.For(player.Holding.Entity).Get<Vector2>("Speed");

        //         Logger.Log(LogLevel.Info, "g", speed.ToString());
        //     });
        // }
        
        private void allowAllDirectionHypersAndSupers(ILCursor cursor, OpCode nextInstr, bool alwaysRefills) {
            //probably refactor this too

            if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(0), instr => instr.MatchRet(), instr => instr.OpCode == nextInstr)) {
                ILLabel end = cursor.DefineLabel();
                cursor.Emit(OpCodes.Pop);

                cursor.EmitDelegate(() => {
                    if(GetOptionBool(Option.AllDirectionHypersAndSupers)) {
                        Player player = Engine.Scene.Tracker.GetEntity<Player>();

                        float coyote = player.jumpGraceTimer;

                        if(
                            player.CanUnDuck &&
                            Input.Jump.Pressed &&
                            (
                                (
                                    (player.CollideCheck<JumpThru>(player.Position + Vector2.UnitY * player.Collider.Height) && player.CollideCheck<JumpThru>(player.Position + Vector2.UnitY)) ||
                                    player.CollideCheck<Solid>(player.Position + Vector2.UnitY)
                                ) ||
                                (
                                    (coyote > 0f || ExtendedVariants.Variants.JumpCount.GetJumpBuffer() > 0) && 
                                    GetOptionBool(Option.AllDirectionHypersAndSupersWorkWithCoyoteTime)
                                )
                            ) &&
                            (player.Speed.Y <= 0f)
                        ) {
                            if((player.dashRefillCooldownTimer <= 0f && !player.Inventory.NoRefills) || alwaysRefills) {
                                player.RefillDash();
                            }

                            player.SuperJump();

                            return true;
                        }
                    }

                    return false;
                });

                cursor.Emit(OpCodes.Brfalse, end);
                cursor.Emit(OpCodes.Ldc_I4_0);
                cursor.Emit(OpCodes.Ret);
                cursor.MarkLabel(end);
                cursor.Emit(nextInstr);
            } else {
                Logger.Error("GooberHelper", "COULDNT MAKE ALL DIRECTION SUPERS AND HYPERS POSSIBLE; PLEASE CONTACT @ZUCCANIUM");
            }
        }

        private void allowHoldableClimbjumping(ILCursor cursor) {
            for(int i = 0; i < 2; i++) {
                if(
                    cursor.TryGotoNextBestFit(MoveType.After,
                        instr => instr.MatchLdfld<Player>("Stamina"),
                        instr => instr.MatchLdcR4(0),
                        instr => instr.OpCode == OpCodes.Ble_Un_S,
                        instr => instr.MatchLdarg(0),
                        instr => instr.MatchCallvirt<Player>("get_Holding"),
                        instr => instr.OpCode == OpCodes.Brtrue_S
                    )
                ) {
                    cursor.Index--;
                    cursor.EmitDelegate((Holdable value) => {
                        if(!GetOptionBool(Option.AllowHoldableClimbjumping)) return value;

                        return null;
                    });
                } else {
                    Logger.Error("GooberHelper", "COULDNT ALLOW THEO CLIMBJUMPING; PLEASE CONTACT @ZUCCANIUM");
                }
            }
        }

        private void modifyPlayerDreamDashUpdate(ILContext il) {
            allowAllDirectionDreamJumps(new ILCursor(il));
        }
        
        private void modifyPlayerDreamDashEnd(ILContext il) {
            allowAllDirectionDreamJumps(new ILCursor(il));
        }

        private void modifyPlayerLaunchUpdate(ILContext il) {
            allowCrouchedHoldableGrabbing(new ILCursor(il), false, false);
        }

        private void modifyPlayerRedDashUpdate(ILContext il) {
            allowHoldableClimbjumping(new ILCursor(il));
            allowAllDirectionHypersAndSupers(new ILCursor(il), OpCodes.Ldloc_0, true);
        }

        private void modifyPlayerHitSquashUpdate(ILContext il) {
            allowHoldableClimbjumping(new ILCursor(il));
        }

        private void modifyPlayerNormalUpdate(ILContext il) {
            allowHoldableClimbjumping(new ILCursor(il));
            allowCrouchedHoldableGrabbing(new ILCursor(il), true, true);

            ILCursor cursor = new ILCursor(il);

            if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(900f))) {
                cursor.EmitLdarg0();
                cursor.EmitLdloc(8);
                cursor.EmitDelegate((float value, Player player, float fastfallSpeed) => {
                    //400f is default movement-direction-aligned air friction
                    //0.65f is the default multiplier on horizontal air friction while midair
                    //they call me the magic number documenter
                    return GetOptionBool(Option.DownwardsAirFrictionBehavior) && Math.Abs(player.Speed.Y) > fastfallSpeed && Math.Sign(player.Speed.Y) == Input.MoveY ? 400f * 0.65f : value;
                });
            }

            if(cursor.TryGotoNextBestFit(MoveType.After,
                instr => instr.MatchLdarg0(),
                instr => instr.MatchLdfld<Player>("varJumpSpeed"),
                instr => instr.MatchCallOrCallvirt(out _) //math.min
            )) {
                cursor.EmitDelegate((float value) => {
                    if(!GetOptionBool(Option.DownwardsJumpSpeedPreservation)) return value;

                    float varJumpSpeed = Engine.Scene.Tracker.GetEntity<Player>().varJumpSpeed;

                    return varJumpSpeed > 0 ? varJumpSpeed : value;
                });
            }
        }

        private void modifyPlayerDashUpdate(ILContext il) {
            allowHoldableClimbjumping(new ILCursor(il));
            allowAllDirectionHypersAndSupers(new ILCursor(il), OpCodes.Ldarg_0, false);
        }

        private void modifyPlayerPickupCoroutine(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            cursor.TryGotoNext(MoveType.After, instr => instr.MatchStfld(typeof(Player), nameof(Player.Speed)));
            cursor.TryGotoNext(MoveType.After, instr => instr.MatchStfld(typeof(Player), nameof(Player.Speed)));

            cursor.EmitDelegate(() => {
                if(!GetOptionBool(Option.PickupSpeedReversal)) return;

                Player player = Engine.Scene.Tracker.GetEntity<Player>();

                if(-Math.Sign(player.Speed.X) == (int)Input.MoveX) {
                    player.Speed.X *= -1;
                }
            });
        }

        private void makeGoldenBlocksOrSimilarEntitiesAlwaysLoad(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            //iteration #1 is for the flag local boolean on golden and silver blocks
            //iteration #2 is for the flag2 local boolean on platinum blocks
            for(int i = 0; i < 2; i++) {
                if(cursor.TryGotoNext(MoveType.After,
                    instr => instr.OpCode == OpCodes.Ldc_I4_0,
                    instr => instr.OpCode == (i == 0 ? OpCodes.Stloc_0 : OpCodes.Stloc_1)
                )) {
                    cursor.Index--;

                    cursor.EmitDelegate((int value) => {
                        return GetOptionBool(Option.GoldenBlocksAlwaysLoad) ? 1 : value;
                    });
                }
            }
        }

        private void modPlayerSuperJump(On.Celeste.Player.orig_SuperJump orig, Player self) {
            Vector2 origSpeed = self.Speed;
            bool wasDucking = self.Ducking;

            orig(self);

            if(GetOptionBool(Option.VerticalSpeedToHorizontalSpeedOnGroundJump)) {
                GooberPlayerExtensions c = GooberPlayerExtensions.Instance;

                float retainedVerticalSpeed = !c.AwesomeRetentionWasInWater && c.AwesomeRetentionTimer > 0 && c.AwesomeRetentionDirection.X == 0 ? Math.Abs(c.AwesomeRetentionSpeed) : 0;

                self.Speed.X = Math.Sign(self.Speed.X) * Math.Max(Math.Max(Math.Abs(origSpeed.Y), retainedVerticalSpeed), Math.Abs(self.Speed.X));
            }

            if(GetOptionBool(Option.HyperAndSuperSpeedPreservation)) {
                //this exists so that alldirectionsHypersAndSupers can be compatible
                //i dont think it will break anything else                                                                                                                         :cluel:
                float kindaAbsoluteSpeed = origSpeed.Length() == 0 ? self.beforeDashSpeed.Length() : origSpeed.Length();
                
                self.Speed.X = (int)self.Facing * Math.Max(Math.Abs(kindaAbsoluteSpeed), Math.Abs(260f * (wasDucking ? 1.25f : 1f))) + self.LiftBoost.X;
            }

            if(GetOptionBool(Option.AdditiveVerticalJumpSpeed)) {
                self.Speed.Y = Math.Min(self.Speed.Y, self.varJumpSpeed + Math.Min(origSpeed.Y, 0));

                self.varJumpSpeed = self.Speed.Y;
            }  
        }

        private void modPlayerNormalEnd(On.Celeste.Player.orig_NormalEnd orig, Player self) {
            if(GetOptionBool(Option.RemoveNormalEnd)) {
                return;
            }

            //make the method not reset retention timer
            if(GetOptionBool(Option.WallbounceSpeedPreservation) && self.StateMachine.State == 2 && self.wallSpeedRetentionTimer > 0) {
                float retentionTimer = self.wallSpeedRetentionTimer;

                if(retentionTimer > 0) {
                    orig(self);
                    
                    self.wallSpeedRetentionTimer = retentionTimer;
                    
                    return;
                }
            }

            orig(self);
        }

        private void modPlayerDashBegin(On.Celeste.Player.orig_DashBegin orig, Player self) {
            orig(self);

            Vector2 beforeDashSpeed = self.beforeDashSpeed;
            float wallSpeedRetained = self.wallSpeedRetained;

            if(
                GetOptionBool(Option.WallbounceSpeedPreservation) &&
                self.wallSpeedRetentionTimer > 0 &&
                Math.Abs(wallSpeedRetained) > Math.Abs(beforeDashSpeed.X)
            ) {
                self.beforeDashSpeed = new Vector2(wallSpeedRetained, beforeDashSpeed.Y);
                self.wallSpeedRetentionTimer = 0f;
            }
        }

        private void modCrystalStaticSpinnerOnPlayer(On.Celeste.CrystalStaticSpinner.orig_OnPlayer orig, CrystalStaticSpinner self, Player player) {
            if(GetOptionBool(Option.AlwaysExplodeSpinners)) {
                self.Destroy();

                return;
            }

            orig(self, player);
        }

        private void modPlayerCtor(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode) {
            orig(self, position, spriteMode);

            GooberFlingBird.CustomStateId = self.StateMachine.AddState("GooberFlingBird", new Func<int>(GooberFlingBird.CustomStateUpdate), null, null, null);
            // self.Add(new GooberPlayerExtensions());
            
            if(self.level?.Tracker.GetComponent<GooberPlayerExtensions>() == null) {
                self.Add(new GooberPlayerExtensions());
            }
        }

        private bool modPlayerWallJumpCheck(On.Celeste.Player.orig_WallJumpCheck orig, Player self, int dir) {
            if(self.CollideCheck<Water>() && GetOptionBool(Option.CustomSwimming)) {
                return false;
            }

            return orig(self, dir);
        }

        private void modifyPlayerWallJumpCheck(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if(cursor.TryGotoNextBestFit(MoveType.After,
                instr => instr.MatchLdarg0(), //stealing this
                instr => instr.MatchLdarg1(),
                instr => instr.MatchCallOrCallvirt<Player>("ClimbBoundsCheck")
            )) {
                cursor.GotoPrev(MoveType.After, instr => instr.MatchLdarg0());

                cursor.EmitLdloc0();
                cursor.EmitDelegate((Player player, int originalDistance) => {
                    return GetOptionBool(Option.CornerboostBlocksEverywhere) ?
                        Math.Max(originalDistance, (int)Math.Ceiling(Math.Abs(player.Speed.X) * Engine.DeltaTime) + 1) :
                        originalDistance;
                });
                cursor.EmitStloc0();

                cursor.EmitLdarg0(); //giving it back
            }

            //REWORK ALL OF THIS BASED ON BRED'S SUGGESTION

            //this is going to be the worst thing ever
            //i am so sorry

            //okay so essentially what im doing is:
            //if the current collision distance returned false, assume that its overshooting it and subtract the player hitbox width from the collision distance
            //as long as the new collision distance is greater than zero, return back to the start of the evaluation
            //this should work with custom entities such as maddiehelpinghand sideways jumpthrus

            ILLabel startLabel = cursor.DefineLabel();
            ILLabel endLabel = cursor.DefineLabel();

            if(cursor.TryGotoNext(MoveType.Before,
                instr => instr.MatchLdarg0(),
                instr => instr.MatchLdfld<Player>("level")
            )) {
                cursor.MarkLabel(startLabel);
            }

            cursor.TryGotoNext(MoveType.Before, instr => instr.MatchBrtrue(out endLabel));


            //im not gonna bother making these instructions not run without the gooberhelper option enabled
            //nothing here is super expensive
            //its probably fine
            //surely
            //honestly the gooberhelper option check might even be more expensive than this
            if(cursor.TryGotoNext(MoveType.Before, instr => instr.MatchRet())) {
                cursor.EmitBrfalse(endLabel);
                cursor.EmitLdcI4(1);
            }

            if(cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdcI4(0), instr => instr.MatchRet())) {
                cursor.Index++;
                cursor.EmitLdloc0();
                cursor.EmitLdcI4(8);
                cursor.EmitSub();
                cursor.EmitStloc0();
                cursor.EmitLdloc0();
                //this should be a bgt but theres already a zero beneath loc0 on the evaluation stack
                //the logic is just inverted
                cursor.EmitBle(startLabel);
                cursor.EmitLdcI4(0);
            }
        }

        private void modPlayerSwimBegin(On.Celeste.Player.orig_SwimBegin orig, Player self) {
            orig(self);
            
            if(self.Speed.Y > 0 && GetOptionBool(Option.CustomSwimming)) {
                self.Speed.Y *= 2f;

                GooberPlayerExtensions.Instance.AwesomeRetentionSpeed = 0f;
            }
        }

        private void modifyPlayerDashCoroutine(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            Vector2 originalDashSpeed = Vector2.Zero;

            if(cursor.TryGotoNextBestFit(MoveType.After, 
                instr => instr.MatchLdloc2(),
                instr => instr.MatchLdcR4(240),
                instr => instr.MatchCall<Vector2>("op_Multiply"),
                instr => instr.MatchStloc3()
            )) {
                cursor.EmitLdloc3();
                cursor.EmitDelegate((Vector2 value) => {
                    originalDashSpeed = value * Vector2.One; // * Vector2.One to copy it
                });
            }

            if(cursor.TryGotoNextBestFit(MoveType.After, 
                instr => instr.MatchLdloc1(),
                instr => instr.MatchLdloc3(),
                instr => instr.MatchStfld<Player>("Speed")
            )) {
                cursor.TryGotoPrev(MoveType.After, instr => instr.MatchLdloc3());

                cursor.EmitDelegate((Vector2 speed) => {
                    Vector2 newSpeed = speed * Vector2.One;

                    Player player = Engine.Scene.Tracker.GetEntity<Player>();
                    Vector2 beforeDashSpeed = player.beforeDashSpeed;

                    if(
                        GetOptionBool(Option.MagnitudeBasedDashSpeed) ||
                        (GetOptionBool(Option.MagnitudeBasedDashSpeedOnlyCardinal) && Vector2.Dot(originalDashSpeed, Vector2.UnitX) % 1 == 0) //the second part just checks if the vector is cardinal. do you like my commenting?
                    ) {
                        return originalDashSpeed.SafeNormalize() * Math.Max(beforeDashSpeed.Length(), originalDashSpeed.Length());
                    }

                    if(GetOptionBool(Option.VerticalDashSpeedPreservation) && (Math.Sign(beforeDashSpeed.Y) == Math.Sign(speed.Y) || GetOptionBool(Option.ReverseDashSpeedPreservation)) && Math.Abs(beforeDashSpeed.Y) > Math.Abs(speed.Y)) {
                        newSpeed.Y = Math.Abs(beforeDashSpeed.Y) * Math.Sign(originalDashSpeed.Y);
                    }

                    if(GetOptionBool(Option.ReverseDashSpeedPreservation)) {
                        if(Math.Sign(beforeDashSpeed.X) == -Math.Sign(speed.X) && Math.Abs(beforeDashSpeed.X) > Math.Abs(speed.X)) {
                            newSpeed.X = -beforeDashSpeed.X;
                        }
                    }

                    return newSpeed;
                });
            }

            //remove the 0.75x speed multiplier when dashing while in contact with water
            if(cursor.TryGotoNextBestFit(MoveType.After, 
                instr => instr.MatchLdloc1(),
                instr => instr.MatchCallOrCallvirt<Entity>("CollideCheck"), //collidecheck<water>
                instr => instr.MatchBrfalse(out ILLabel buh)
            )) {
                cursor.Index--;

                cursor.EmitDelegate(() => {
                    return !GetOptionBool(Option.CustomSwimming);
                });
                cursor.EmitAnd();
            }

            // if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchStloc(3))) {
            //     cursor.EmitDelegate(() => {
            //         if(GetOptionBool(Option.ReverseDashSpeedPreservation)) {
            //             Player player = Engine.Scene.Tracker.GetEntity<Player>();

            //             Vector2 vector = lastAim");
            //             if (player.OverrideDashDirection != null)
            //             {
            //                 vector = player.OverrideDashDirection.Value;
            //             }
            //             vector = DynamicData.For(player).Invoke<Vector2>("CorrectDashPrecision", vector);

            //             if(vector.X != 0) {
            //                 Vector2 beforeDashSpeed = beforeDashSpeed");
            //                 beforeDashSpeed.X = Math.Sign(vector.X) * Math.Abs(beforeDashSpeed.X);
            //                 beforeDashSpeed", beforeDashSpeed);
            //             }
            //         }
            //     });
            // }

            Func<float, float> makeVerticalDashesNotResetSpeed = value => {
                if(!GetOptionBool(Option.CustomSwimming) && !GetOptionBool(Option.DashesDontResetSpeed)) return value;
                    
                return (
                    (GetOptionBool(Option.CustomSwimming) && Engine.Scene.Tracker.GetEntity<Player>().CollideCheck<Water>()) ||
                    GetOptionBool(Option.DashesDontResetSpeed)
                ) ? float.MinValue : value;
            };

            if(cursor.TryGotoNextBestFit(MoveType.After, 
                instr => instr.MatchLdflda<Player>("DashDir"),
                instr => instr.MatchLdfld<Vector2>("Y"),
                instr => instr.MatchLdcR4(0),
                instr => instr.MatchBgtUn(out _)
            )) {
                cursor.Index--;

                cursor.EmitDelegate(makeVerticalDashesNotResetSpeed);
            }

            if(cursor.TryGotoNextBestFit(MoveType.After, 
                instr => instr.MatchLdflda<Player>("Speed"),
                instr => instr.MatchLdfld<Vector2>("Y"),
                instr => instr.MatchLdcR4(0),
                instr => instr.MatchBgeUn(out _)
            )) {
                cursor.Index--;

                cursor.EmitDelegate(makeVerticalDashesNotResetSpeed);
            }
        }

        private int modPlayerSwimUpdate(On.Celeste.Player.orig_SwimUpdate orig, Player self) {
            if(!GetOptionBool(Option.CustomSwimming)) return orig(self);

            DynamicData data = DynamicData.For(self);

            if (!data.Invoke<bool>("SwimCheck"))
			{
				return 0;
			}
			if (self.CanUnDuck)
			{
				self.Ducking = false;
			}
			if (self.CanDash)
			{
				data.Set("demoDashed", Input.CrouchDashPressed);
				Input.Dash.ConsumeBuffer();
				Input.CrouchDash.ConsumeBuffer();
				return 2;
			}
            bool flag = data.Invoke<bool>("SwimUnderwaterCheck");
			if (!flag && self.Speed.Y >= 0f && Input.GrabCheck && !data.Get<bool>("IsTired") && self.CanUnDuck && Math.Sign(self.Speed.X) != (int)(-(int)self.Facing) && self.ClimbCheck((int)self.Facing, 0))
			{
				if (SaveData.Instance.Assists.NoGrabbing)
				{
					self.ClimbTrigger((int)self.Facing);
				}
				else if (!self.MoveVExact(-1, null, null))
				{
					self.Ducking = false;
					return 1;
				}

			}
            Vector2 vector = Input.Feather.Value.SafeNormalize(Vector2.Zero);

            float speed = 90;

            if(Vector2.Dot(self.Speed.SafeNormalize(Vector2.Zero), vector) < -0.5 || vector.Length() == 0 || self.Speed.Length() <= 90) {
                self.Speed.X = Calc.Approach(self.Speed.X, speed * vector.X, 350f * Engine.DeltaTime);
                self.Speed.Y = Calc.Approach(self.Speed.Y, speed * vector.Y, 350f * Engine.DeltaTime);
            } else {
                self.Speed = self.Speed.RotateTowards(vector.Angle(), 10f * Engine.DeltaTime);
                // if(self.Speed.Length() < speed) {
                //     self.Speed = self.Speed.SafeNormalize() * speed;
                // } 
            }

            self.wallSpeedRetentionTimer = 0f;

            GooberPlayerExtensions c = GooberPlayerExtensions.Instance;

            if(Input.Jump.Pressed) {
                if(!self.CollideCheck<Water>(self.Position + Vector2.UnitY * (-10f + Math.Min(self.Speed.Y * Engine.DeltaTime, 0)))) {
                    if (self.Speed.Y >= 0) {
                        // self.Jump(true, true);

                        return 0;
                    }

                    if(self.Speed.Y < -130f) {
                        self.Speed += 80f * self.Speed.SafeNormalize(Vector2.Zero);
                        
                        data.Set("launched", true);

                        // self.Scene.Add(Engine.Pooler.Create<SpeedRing>().Init(self.Center, self.Speed.Angle(), Color.White));
                        Dust.Burst(self.Position, (vector * -1f).Angle(), 4);

                        Input.Jump.ConsumeBuffer();
                    }
                }

                float redirectSpeed = Math.Max(self.Speed.Length(), c.AwesomeRetentionSpeed) + 20;

                // Console.WriteLine(customWaterRetentionTimer);
                // Console.WriteLine(redirectSpeed);
                // Console.WriteLine(customWaterRetentionDirection);

                if(c.AwesomeRetentionTimer <= 0 || !c.AwesomeRetentionWasInWater) {
                    redirectSpeed = 0;
                }

                Vector2 v = c.AwesomeRetentionDirection * -1;

                if(v != Vector2.Zero && redirectSpeed != 0) {
                    // Console.WriteLine("boiyoyoyoing");

                    Input.Jump.ConsumeBuffer();

                    self.Speed = v.SafeNormalize() * redirectSpeed;
                }
            }   

            // if (Input.Jump.Pressed && data.Invoke<bool>("SwimJumpCheck")) {
            //     self.Jump(true, true);

            //     return 0;
            // }

			// Vector2 vector = Input.Feather.Value;
			// vector = vector.SafeNormalize();
			// float num = (flag ? 60f : 80f) * 10f;
			// float num2 = 80f * 10f;
			// if (Math.Abs(self.Speed.X) > 80f && Math.Sign(self.Speed.X) == Math.Sign(vector.X))
			// {
			// 	self.Speed.X = Calc.Approach(self.Speed.X, num * vector.X, 400f * Engine.DeltaTime);
			// }
			// else
			// {
			// 	self.Speed.X = Calc.Approach(self.Speed.X, num * vector.X, 600f * Engine.DeltaTime);
			// }
			// if (vector.Y == 0f && data.Invoke<bool>("SwimRiseCheck"))
			// {
			// 	self.Speed.Y = Calc.Approach(self.Speed.Y, -60f, 600f * Engine.DeltaTime);
			// }
			// else if (vector.Y >= 0f || data.Invoke<bool>("SwimUnderwaterCheck"))
			// {
			// 	if (Math.Abs(self.Speed.Y) > 80f && Math.Sign(self.Speed.Y) == Math.Sign(vector.Y))
			// 	{
			// 		self.Speed.Y = Calc.Approach(self.Speed.Y, num2 * vector.Y, 400f * Engine.DeltaTime);
			// 	}
			// 	else
			// 	{
			// 		self.Speed.Y = Calc.Approach(self.Speed.Y, num2 * vector.Y, 600f * Engine.DeltaTime);
			// 	}
			// }
			if (!flag && data.Get<int>("moveX") != 0 && self.CollideCheck<Solid>(self.Position + Vector2.UnitX * (float)data.Get<int>("moveX")) && !self.CollideCheck<Solid>(self.Position + new Vector2((float)data.Get<int>("moveX"), -3f)))
			{
				data.Invoke("ClimbHop");
			}
			return 3;
        }

        private void modLevelLevelLoad(On.Celeste.Level.orig_LoadLevel orig, Level level, Player.IntroTypes playerIntro, bool isFromLoader) {
            if(level.Tracker.GetEntity<GooberIconThing>() == null) {
                level.Add(new GooberIconThing());
            }

            if(level.Tracker.GetEntity<GooberSettingsList>() == null) {
                level.Add(new GooberSettingsList());
            }

            orig(level, playerIntro, isFromLoader);

            if(level.Tracker.GetComponent<GooberPlayerExtensions>() == null) {
                level.Tracker.GetEntity<Player>()?.Add(new GooberPlayerExtensions());
            }
        }

        private Player modBounceBlockWindUpPlayerCheck(On.Celeste.BounceBlock.orig_WindUpPlayerCheck orig, BounceBlock self) {
            if(!GetOptionBool(Option.AlwaysActivateCoreBlocks)) {
                return orig(self);
            }

            Player player = self.CollideFirst<Player>(self.Position - Vector2.UnitY);

            if (player != null && player.Speed.Y < 0f)
            {
                player = null;
            }
            if (player == null)
            {
                player = self.CollideFirst<Player>(self.Position + Vector2.UnitX);
                if (player == null || player.Facing != Facings.Left)
                {
                    player = self.CollideFirst<Player>(self.Position - Vector2.UnitX);
                    if (player == null || player.Facing != Facings.Right)
                    {
                        player = null;
                    }
                }
            }

            return player;
        }

        private void modPlayerAttractBegin(On.Celeste.Player.orig_AttractBegin orig, Player self) {
            GooberPlayerExtensions.Instance.AttractSpeedPreserved = self.Speed;

            orig(self);
        }

        private void modPlayerFinalBossPushLaunch(On.Celeste.Player.orig_FinalBossPushLaunch orig, Player self, int dir) {
            orig(self, dir);

            if(GetOptionBool(Option.BadelineBossSpeedReversing)) {
                self.Speed.X = dir * Math.Max(Math.Abs(self.Speed.X), GooberPlayerExtensions.Instance.AttractSpeedPreserved.Length());
            }
        }

        private Vector2 modPlayerExplodeLaunch(On.Celeste.Player.orig_ExplodeLaunch_Vector2_bool_bool orig, Player self, Vector2 from, bool snapUp, bool sidesOnly) {
            if(!GetOptionBool(Option.ExplodeLaunchSpeedPreservation)) {
                return orig(self, from, snapUp, sidesOnly);
            }

            Vector2 originalSpeed = self.Speed;
            Vector2 returnValue = orig(self, from, snapUp, sidesOnly);

            self.Speed.X = Math.Sign(self.Speed.X) * Math.Max(Math.Abs(originalSpeed.X) * (Input.MoveX.Value == Math.Sign(self.Speed.X) ? 1.2f : 1f), Math.Abs(self.Speed.X)); 

            if (Input.MoveX.Value != Math.Sign(self.Speed.X)) {
                self.explodeLaunchBoostSpeed = self.Speed.X * 1.2f;
            }

            //hi
            if((Engine.Scene as Level).Session.Area.SID == "alex21/Dashless+/1A Dashless but Spikier" && (Engine.Scene as Level).Session.Level == "b-06") {
                self.Speed.X = 0;
                self.Speed.Y = -330;
            }

            return returnValue;
        }

        private void modifyPlayerOnCollideH(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if(cursor.TryGotoNextBestFit(MoveType.After,
                instr => instr.MatchDup(),
                instr => instr.MatchLdindR4(),
                instr => instr.MatchLdcR4(-0.5f)
            )) {
                cursor.EmitDelegate((float value) => {
                    return GetOptionBool(Option.CustomFeathers) ? -1f : value;
                });
            }

            if(cursor.TryGotoNextBestFit(MoveType.After, instr => instr.MatchLdcR4(0.06f))) {
                cursor.EmitDelegate((float value) => {
                    float newTime = GetOptionValue(Option.RetentionFrames);

                    return newTime != 4 ? newTime / 60f : value;
                });
            }
        }
        
        private void modifyPlayerOnCollideV(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if(cursor.TryGotoNextBestFit(MoveType.After,
                instr => instr.MatchDup(),
                instr => instr.MatchLdindR4(),
                instr => instr.MatchLdcR4(-0.5f)
            )) {
                cursor.EmitDelegate((float value) => {
                    return GetOptionBool(Option.CustomFeathers) ? -1f : value;
                });
            }
        }

        private void modPlayerStarFlyBegin(On.Celeste.Player.orig_StarFlyBegin orig, Player self) {
            GooberPlayerExtensions.Instance.StarFlySpeedPreserved = self.Speed;

            orig(self);
        }

        private void modifyPlayerStarFlyUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            int start = cursor.Index;

            float lowMult = 0.65f;
            float midMult = 0.90f;
            float highMult = 1.05f;

            //destroyer of reality
            // cursor.EmitDelegate(() => {
            //     Player player = Engine.Scene.Tracker.GetEntity<Player>();

            //     (Engine.Scene as Level).Displacement.AddBurst(player.Center, 2f, 8f, 1000f, 1f, null, null);
            // });

            float[] matches = [91, 140, 190, 140, 140, 140];
            float[] replaceMults = [lowMult, midMult, highMult, midMult, midMult, 0.75f];

            for(int i = 0; i < matches.Length; i++) {
                //fsr i have to put this in a variable instead of just accessing the array from within the delegate. that took way longer to figure out than it shouldve
                float replaceMult = replaceMults[i];

                if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(matches[i]))) {
                    cursor.EmitDelegate((float value) => {
                        return GetOptionBool(Option.CustomFeathers) ?
                            Math.Max(Engine.Scene.Tracker.GetComponent<GooberPlayerExtensions>().StarFlySpeedPreserved.Length() * replaceMult, value) :
                            value;
                    });
                }
            }

            cursor.Index = start;

            if(cursor.TryGotoNextBestFit(MoveType.After,
                instr => instr.MatchLdfld<Player>("starFlyTimer"),
                instr => instr.MatchLdcR4(0f),
                instr => instr.OpCode == OpCodes.Bgt_Un
            )) {
                ILLabel label = cursor.DefineLabel();
                int index = cursor.Index;

                if(cursor.TryGotoNextBestFit(MoveType.Before, 
                    instr => instr.MatchLdcI4(1),
                    instr => instr.MatchLdcI4(1),
                    instr => instr.MatchCall(typeof(Input).GetMethod("Rumble"))
                )) {

                    cursor.MarkLabel(label);
                    cursor.Index = index;

                    cursor.EmitDelegate(() => {
                        if(GetOptionBool(Option.FeatherEndSpeedPreservation)) {
                            Player player = Engine.Scene.Tracker.GetEntity<Player>();

                            //free feather end boosts
                            if(player.Speed.Y <= 0f) {
                                player.varJumpSpeed = player.Speed.Y;
                                player.AutoJump = true;
                                player.AutoJumpTimer = 0f;
                                player.varJumpTimer = 0.2f;
                            }

                            return true;
                        }

                        return false;
                    });
                    cursor.EmitBrtrue(label);
                }
            }

            // if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(91))) {
            //     cursor.EmitDelegate((float value) => { return ((Settings.Physics.CustomFeathers && !Settings.DisableSettings) || Session.CustomFeathers) ? Math.Max(c.StarFlySpeedPreserved.Length() * lowMult, value) : value; });
            // }

            // if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(140))) {
            //     cursor.EmitDelegate((float value) => { return ((Settings.Physics.CustomFeathers && !Settings.DisableSettings) || Session.CustomFeathers) ? Math.Max(c.StarFlySpeedPreserved.Length() * midMult, value) : value; });
            // }

            // if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(190))) {
            //     cursor.EmitDelegate((float value) => { return ((Settings.Physics.CustomFeathers && !Settings.DisableSettings) || Session.CustomFeathers) ? Math.Max(c.StarFlySpeedPreserved.Length() * highMult, value) : value; });
            // }

            // if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(140))) {
            //     cursor.EmitDelegate((float value) => { return ((Settings.Physics.CustomFeathers && !Settings.DisableSettings) || Session.CustomFeathers) ? Math.Max(c.StarFlySpeedPreserved.Length() * midMult, value) : value; });
            // }

            // if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(140))) {
            //     cursor.EmitDelegate((float value) => { return ((Settings.Physics.CustomFeathers && !Settings.DisableSettings) || Session.CustomFeathers) ? Math.Max(c.StarFlySpeedPreserved.Length() * midMult, value) : value; });
            // }
            
            // if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(140))) {
            //     cursor.EmitDelegate((float value) => { return ((Settings.Physics.CustomFeathers && !Settings.DisableSettings) || Session.CustomFeathers) ? Math.Max(c.StarFlySpeedPreserved.Length() * 0.75f, value) : value; });
            // }
        }

        private void modifyPlayerStarFlyCoroutine(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            int startPosition = cursor.Index;

            if(cursor.TryGotoNextBestFit(MoveType.After,
                instr => instr.MatchLdarg0(),
                instr => instr.MatchLdcI4(-1),
                instr => instr.OpCode == OpCodes.Stfld
            )) {
                startPosition = cursor.Index;
            }

            if(
                cursor.TryGotoNext(MoveType.After, 
                    instr => instr.MatchLdcR4(0.1f)
                ) &&
                cursor.TryGotoNext(MoveType.After, 
                    instr => instr.MatchLdcI4(1),
                    instr => instr.MatchRet()
                )
            ) {
                ILLabel afterStarFlyStartLabel = cursor.MarkLabel();

                cursor.Index = startPosition;
                cursor.EmitDelegate(() => {
                    if(!GetOptionBool(Option.CustomFeathers)) return false;

                    Engine.Scene.Tracker.GetEntity<Player>().Sprite.Play("starFly", false, false);

                    return true;
                });
                cursor.Emit(OpCodes.Brtrue_S, afterStarFlyStartLabel);
            }

            // ILLabel start = cursor.DefineLabel();

            // if(cursor.TryGotoNext(instr => instr.OpCode == OpCodes.Stfld)) {
            //     start = cursor.MarkLabel();
            // }

            // if(cursor.TryGotoNext(MoveType.Before, 
            //     instr => instr.MatchLdcR4(0.1f)
            // )) {
            //     //probably change this
            //     cursor.Index += 8;

            //     ILLabel afterStarFlyStartLabel = cursor.MarkLabel();

            //     cursor.GotoLabel(start, MoveType.After);
            //     cursor.EmitDelegate(() => { return GetOptionBool(Option.CustomFeathers); });
            //     cursor.Emit(OpCodes.Brtrue_S, afterStarFlyStartLabel);
            // }
            
            if(cursor.TryGotoNextBestFit(MoveType.After,
                instr => instr.MatchLdcR4(250),
                instr => instr.OpCode == OpCodes.Call,
                instr => instr.OpCode == OpCodes.Stfld
            )) {
                cursor.EmitDelegate(() => {
                    if(GetOptionBool(Option.CustomFeathers)) {
                        Player player = Engine.Scene.Tracker.GetEntity<Player>();

                        player.Speed = GooberPlayerExtensions.Instance.StarFlySpeedPreserved.SafeNormalize() * Math.Max(GooberPlayerExtensions.Instance.StarFlySpeedPreserved.Length(), 250);
                    }
                });
            }
        }

        private void modPlayerClimbJump(On.Celeste.Player.orig_ClimbJump orig, Player self) {
            Vector2 originalSpeed = self.Speed;

            int beforeJumpCount = SaveData.Instance.TotalJumps;

            if(self.wallSpeedRetentionTimer > 0f && GetOptionBool(Option.GetClimbJumpSpeedInRetainedFrames)) {
                self.Speed.X = self.wallSpeedRetained;
            }

            orig(self);

            if(self.wallSpeedRetentionTimer > 0f && GetOptionBool(Option.GetClimbJumpSpeedInRetainedFrames)) {
                self.wallSpeedRetained = self.Speed.X;
            }

            //the method didnt run; dont do anything else
            if(beforeJumpCount == SaveData.Instance.TotalJumps) return;

            // self.Speed.X = Math.Abs(originalSpeed.Y) * Math.Sign(self.Speed.X);
            // self.Speed.Y = -Math.Max(Math.Max(Math.Abs(originalSpeed.X), Math.Abs(self.wallSpeedRetained) * (self.wallSpeedRetentionTimer > 0f ? 1f : 0f)), Math.Abs(self.Speed.Y));
            // self.varJumpSpeed = self.Speed.Y;
            // self.wallSpeedRetentionTimer = 0;

            if(GetOptionBool(Option.WallBoostDirectionBasedOnOppositeSpeed)) {
                if(Input.MoveX == 0) {
                    self.wallBoostDir = Math.Sign(-self.Speed.X);
                }
            }

            //dont do it with additive vertical jump speed because that already modifies jump
            if(
                !GetOptionBool(Option.AdditiveVerticalJumpSpeed) &&
                originalSpeed.Y < ((false /* IMPLEMENT LATER */) ? -240f : -105f) && GetOptionBool(Option.UpwardsJumpSpeedPreservation)
            ) {
                self.Speed.Y = originalSpeed.Y + self.LiftSpeed.Y;
                self.varJumpSpeed = self.Speed.Y;
            }
        }

        private void modPlayerWallJump(On.Celeste.Player.orig_WallJump orig, Player self, int dir) {
            Vector2 originalSpeed = self.Speed;

            int beforeJumpCount = SaveData.Instance.TotalWallJumps;

            orig(self, dir);
            
            //the method didnt run; dont do anything else
            if(beforeJumpCount == SaveData.Instance.TotalWallJumps) return;

            if(GetOptionBool(Option.SwapHorizontalAndVerticalSpeedOnWallJump)) {
                self.Speed.X = Math.Max(Math.Abs(originalSpeed.Y), Math.Abs(self.Speed.X)) * Math.Sign(self.Speed.X);
                self.Speed.Y = -Math.Max(Math.Max(Math.Abs(originalSpeed.X), Math.Abs(self.wallSpeedRetained) * (self.wallSpeedRetentionTimer > 0f ? 1f : 0f)), Math.Abs(self.Speed.Y));
                self.varJumpSpeed = self.Speed.Y;

                return;
            }

            handleVerticalJumpSpeed(self, originalSpeed);

            if(GetOptionBool(Option.WallJumpSpeedInversion)) {
                self.Speed.X = Math.Sign(self.Speed.X) * Math.Max(
                    Math.Max(
                        Math.Abs(self.Speed.X),
                        Math.Abs(originalSpeed.X)
                    ),
                    Math.Abs(self.wallSpeedRetained) * (self.wallSpeedRetentionTimer > 0f ? 1f : 0f)
                );
            } else if(Math.Sign(self.Speed.X - self.LiftSpeed.X) == Math.Sign(originalSpeed.X) && GetOptionBool(Option.WallJumpSpeedPreservation)) {
                self.Speed.X = Math.Sign(originalSpeed.X) * Math.Max(Math.Abs(self.Speed.X), Math.Abs(originalSpeed.X) - (Input.MoveX == 0 ? 0.0f : 40.0f)) + self.LiftSpeed.X;
            }
        }

        private void modPlayerDreamDashBegin(On.Celeste.Player.orig_DreamDashBegin orig, Player self) {
            Vector2 originalSpeed = self.Speed;
            Vector2 intendedSpeed = self.DashDir * 240f;

            orig(self);

            if(GetOptionBool(Option.DreamBlockSpeedPreservation)) {
                self.Speed.X = originalSpeed.X;

                self.Speed.X = Math.Sign(intendedSpeed.X) * Math.Max(Math.Abs(intendedSpeed.X), Math.Abs(self.Speed.X));
            }
        }

        private void modPlayerSuperWallJump(On.Celeste.Player.orig_SuperWallJump orig, Player self, int dir) {
            float originalSpeedY = self.Speed.Y;

            int beforeJumpCount = SaveData.Instance.TotalWallJumps;

            orig(self, dir);

            //the method didnt run; dont do anything else
            if(beforeJumpCount == SaveData.Instance.TotalWallJumps) return;

            if(GetOptionBool(Option.AdditiveVerticalJumpSpeed)) {
                self.Speed.Y = Math.Min(self.Speed.Y, self.varJumpSpeed + Math.Min(originalSpeedY, 0));

                self.varJumpSpeed = self.Speed.Y;
            } else if(originalSpeedY < ((false /* IMPLEMENT LATER */) ? -240f : -105f) && GetOptionBool(Option.UpwardsJumpSpeedPreservation)) {
                self.Speed.Y = originalSpeedY + self.LiftSpeed.Y;
                self.varJumpSpeed = self.Speed.Y;
            }

            if(!GetOptionBool(Option.WallbounceSpeedPreservation)) {
                return;
            }


            float absoluteBeforeDashSpeed = Math.Abs(self.beforeDashSpeed.X);
            float absoluteRetainedSpeed = Math.Abs(self.wallSpeedRetained);

            if(self.wallSpeedRetentionTimer <= 0) {
                absoluteRetainedSpeed = 0;
            }

            self.Speed.X = dir * Math.Max(Math.Max(absoluteBeforeDashSpeed, absoluteRetainedSpeed) + self.LiftBoost.X, Math.Abs(self.Speed.X));

            return;
        }

        private void modCelesteFreeze(On.Celeste.Celeste.orig_Freeze orig, float time) {
            float newTime = GetOptionValue(Option.RefillFreezeLength);
            
            //as long as all refill freeze freezeframe callers have "refillroutine" in their names and nothing else then this should work
            if(newTime != 3f && refillRoutineRegex.IsMatch(new System.Diagnostics.StackTrace().ToString())) {
                time = newTime / 60f;
            }

            orig(time);
        }

        private void modPlayerPointBounce(On.Celeste.Player.orig_PointBounce orig, Player self, Vector2 from) {
            if(!GetOptionBool(Option.ReboundInversion)) {
                orig(self, from);

                return;
            }

            Vector2 originalSpeed = self.Speed;

            orig(self, from);

            self.Speed = self.Speed.SafeNormalize() * originalSpeed.Length();
        }

        private void doSpeedReverseStuff(float originalSpeed, Player self, float givenSpeed, int dir = 0) {
            self.Speed.X *= Math.Abs(originalSpeed) / givenSpeed; //divide by the given speed so i multiply by the original speed and get my speed back im so good at talking holy fuck
            

            if(self.Speed.X == 0) self.Speed.X = Input.MoveX * Math.Abs(originalSpeed); //in case the direction was 0
            if(self.Speed.X == 0) self.Speed.X = originalSpeed; //in case Input.MoveX is 0
            if(self.Speed.X == 0) self.Speed.X = dir; //just have it be givenSpeed

            self.Speed.X = Math.Sign(self.Speed.X) * Math.Max(Math.Abs(self.Speed.X), givenSpeed);
        }

        private void modPlayerRebound(On.Celeste.Player.orig_Rebound orig, Player self, int direction = 0) {
            if(!GetOptionBool(Option.ReboundInversion)) {
                orig(self, direction);

                return;
            }

            Vector2 originalSpeed = self.Speed;

            orig(self, direction);

            //i dont know why i had it do this
            // if(((Settings.Physics.CustomSwimming && !Settings.DisableSettings) || Session.CustomSwimming) && self.CollideCheck<Water>()) {
            //     self.Speed = originalSpeed * 1.1f;

            //     // self.Speed = -Math.Max(originalSpeed.Length(), 120) * originalSpeed.SafeNormalize(Vector2.Zero);

            //     return;
            // }

            doSpeedReverseStuff(originalSpeed.X, self, 120);
        }

        private void modPlayerReflectBounce(On.Celeste.Player.orig_ReflectBounce orig, Player self, Vector2 direction) {
            if(!GetOptionBool(Option.ReboundInversion) || direction.X == 0) {
                orig(self, direction);

                return;
            }

            float originalSpeed = self.Speed.X;

            orig(self, direction);

            doSpeedReverseStuff(originalSpeed, self, 220);
        }

        private bool modPlayerSideBounce(On.Celeste.Player.orig_SideBounce orig, Player self, int dir, float fromX, float fromY) {
            if(!GetOptionBool(Option.SpringSpeedPreservation)) {
                return orig(self, dir, fromX, fromY);
            }

            float originalSpeed = self.Speed.X;

            bool res = orig(self, dir, fromX, fromY);

            doSpeedReverseStuff(originalSpeed, self, 240, dir);

            return res;
        }

        private void modPlayerSuperBounce(On.Celeste.Player.orig_SuperBounce orig, Player self, float fromY) {
            if(!GetOptionBool(Option.SpringSpeedPreservation)) {
                orig(self, fromY);

                return;
            }

            float originalSpeed = self.Speed.X;

            orig(self, fromY);

            self.Speed.X = Math.Abs(originalSpeed) * Input.MoveX;
            if(self.Speed.X == 0) self.Speed.X = Math.Abs(originalSpeed); //in case movex is 0
        }

        private void modPlayerOnCollideH(On.Celeste.Player.orig_OnCollideH orig, Player self, CollisionData data) {
            if(!GetOptionBool(Option.KeepDashAttackOnCollision) && !UseAwesomeRetention) {
                orig(self, data);

                return;
            }

            if(UseAwesomeRetention) {
                GooberPlayerExtensions c = GooberPlayerExtensions.Instance;

                if(c.AwesomeRetentionTimer <= 0.0f) {
                    c.AwesomeRetentionSpeed = self.Speed.Length();
                    c.AwesomeRetentionTimer = 0.06f;
                    c.AwesomeRetentionWasInWater = self.CollideCheck<Water>();

                    c.AwesomeRetentionDirection = new Vector2(self.CollideCheck<Solid>(self.Position + Vector2.UnitX * -1) ? -1 : 1, c.AwesomeRetentionDirection.Y);
                }
            }
            
            float originalDashAttack = self.dashAttackTimer;

            orig(self, data);

            if(GetOptionBool(Option.KeepDashAttackOnCollision)) {
                self.dashAttackTimer = originalDashAttack;
            }
        }

        private void modPlayerOnCollideV(On.Celeste.Player.orig_OnCollideV orig, Player self, CollisionData data) {
            if(!GetOptionBool(Option.KeepDashAttackOnCollision) && !UseAwesomeRetention) {
                orig(self, data);

                return;
            }

            if(UseAwesomeRetention) {
                GooberPlayerExtensions c = GooberPlayerExtensions.Instance;

                if(c.AwesomeRetentionTimer <= 0.0f) {
                    c.AwesomeRetentionSpeed = self.Speed.Length();
                    c.AwesomeRetentionTimer = 0.06f;
                    c.AwesomeRetentionWasInWater = self.CollideCheck<Water>();

                    c.AwesomeRetentionDirection = new Vector2(c.AwesomeRetentionDirection.X, self.CollideCheck<Solid>(self.Position + Vector2.UnitY * -1) ? -1 : 1);
                }
            }

            float originalDashAttack = self.dashAttackTimer;

            orig(self, data);

            if(GetOptionBool(Option.KeepDashAttackOnCollision)) {
                self.dashAttackTimer = originalDashAttack;
            }
        }

        private void modPlayerJump(On.Celeste.Player.orig_Jump orig, Player self, bool particles, bool playSfx) {
            bool isClimbjump = particles == false && playSfx == false;
            Vector2 originalSpeed = self.Speed;

            if(GetOptionBool(Option.JumpInversion)) {
                if (!(isClimbjump && !GetOptionBool(Option.AllowClimbJumpInversion))) {
                    if ((float)self.moveX == -Math.Sign(self.Speed.X)) {
                        self.Speed.X *= -1;
                    }
                }
            }

            bool doVerticalSpeedToHorizontalSpeed = GetOptionBool(Option.VerticalSpeedToHorizontalSpeedOnGroundJump) && !isClimbjump;
            
            if(doVerticalSpeedToHorizontalSpeed) {
                GooberPlayerExtensions c = GooberPlayerExtensions.Instance;

                float retainedVerticalSpeed = !c.AwesomeRetentionWasInWater && c.AwesomeRetentionTimer > 0 && c.AwesomeRetentionDirection.X == 0 ? Math.Abs(c.AwesomeRetentionSpeed) : 0;

                self.Speed.X = Math.Sign(self.moveX) * Math.Max(Math.Max(Math.Abs(self.Speed.Y), retainedVerticalSpeed), Math.Abs(self.Speed.X));
            }

            int beforeJumpCount = SaveData.Instance.TotalJumps;

            orig(self, particles, playSfx);

            //the method didnt run; dont do anything else
            if(beforeJumpCount == SaveData.Instance.TotalJumps) return;

            if(!doVerticalSpeedToHorizontalSpeed) handleVerticalJumpSpeed(self, originalSpeed, isClimbjump);
        }

        private void modPlayerUpdate(On.Celeste.Player.orig_Update orig, Player self) {
            if(GetOptionBool(Option.HorizontalTurningSpeedInversion) && Input.MoveX != Math.Sign(self.Speed.X) && Input.MoveX != 0) {
                self.Speed.X *= -1;
            }

            //weird as hell
            if(GetOptionBool(Option.VerticalTurningSpeedInversion) && Input.MoveY != Math.Sign(self.Speed.Y) && Input.MoveY != 0) {
                if(self.varJumpTimer > 0 && self.Speed.Y < 0f) {
                    self.varJumpTimer = 0f;
                }

                self.Speed.Y *= -1;
            }

            if(UseAwesomeRetention) {
                GooberPlayerExtensions c = GooberPlayerExtensions.Instance;

                if(c.AwesomeRetentionTimer > 0) {
                    c.AwesomeRetentionTimer -= Engine.DeltaTime;
                } else {
                    c.AwesomeRetentionDirection = Vector2.Zero;
                }
            }

            orig(self);

            if(GetOptionBool(Option.PickupSpeedReversal) && self.StateMachine.State == 8) {
                self.Facing = self.moveX == 0 ? self.Facing : (Facings)self.moveX;
            }
        }

        private void modifyPlayerUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            //both of these variables dont need to be part of the session
            //theyre both assigned and accessed in the same method
            bool upwardsCoyote = false;

            if(cursor.TryGotoNextBestFit(MoveType.Before,
                //ldarg.0 here (im stealing it)
                instr => instr.MatchLdfld<Player>("StateMachine"),
                instr => instr.MatchCallOrCallvirt<StateMachine>("get_State"),
                instr => instr.MatchLdcI4(9),
                instr => instr.MatchBneUn(out _)
            )) {
                cursor.EmitDelegate((Player player) => {
                    if(player.Speed.Y > 0 || !GetOptionBool(Option.AllowUpwardsCoyote)) {
                        upwardsCoyote = false;

                        return;
                    }

                    upwardsCoyote = 
                        player.CollideFirst<Solid>(player.Position + Vector2.UnitY) != null ||
                        (player.CollideCheck<JumpThru>(player.Position + Vector2.UnitY * player.Collider.Height) && player.CollideCheck<JumpThru>(player.Position + Vector2.UnitY));
                });
                cursor.EmitLdarg0(); //im giving it back
            }

            float cobwob_originalSpeed = 0;

            //[BEFORE] this.Speed.X = 130f * (float)this.moveX;
            if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(130f))) {
                cursor.EmitDelegate<Func<float, float>>(orig => {
                    if(!GetOptionBool(Option.CobwobSpeedInversion)) return orig;

                    Player player = Engine.Scene.Tracker.GetEntity<Player>();
                    if (player == null) return orig;

                    cobwob_originalSpeed = player.Speed.X;

                    return orig;
                });
            }

            //[BEFORE] this.Stamina += 27.5f;
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(27.5f))) {
                cursor.EmitDelegate<Func<float, float>>(orig => {
                    if(!GetOptionBool(Option.CobwobSpeedInversion) && !GetOptionBool(Option.WallBoostSpeedIsAlwaysOppositeSpeed)) return orig;

                    Player player = Engine.Scene.Tracker.GetEntity<Player>();
                    if (player == null) return orig;

                    float dir = Math.Sign(player.Speed.X);
                    float newAbsoluteSpeed = Math.Max(130f, Math.Abs(cobwob_originalSpeed));

                    if(
                        GetOptionBool(Option.WallBoostSpeedIsAlwaysOppositeSpeed) &&
                        !GetOptionBool(Option.WallBoostDirectionBasedOnOppositeSpeed) &&
                        player.wallBoostDir == Math.Sign(cobwob_originalSpeed - 11f * Math.Sign(cobwob_originalSpeed))
                    ) {
                        dir = -Math.Sign(cobwob_originalSpeed);
                    }
                    
                    if(player.wallSpeedRetentionTimer > 0.0 && GetOptionBool(Option.AllowRetentionReverse)) {
                        float retainedSpeed = player.wallSpeedRetained;

                        newAbsoluteSpeed = Math.Max(130f, Math.Abs(retainedSpeed));
                    }

                    player.Speed.X = dir * newAbsoluteSpeed;

                    return orig;
                });
            }

            ILLabel beforeStaminaRefillLabel = cursor.DefineLabel();
            ILLabel beforeCoyoteRefillLabel = cursor.DefineLabel();

            for(int i = 0; i < 2; i++) {
                if(cursor.TryGotoNextBestFit(MoveType.Before,
                    //ldarg.0 here (im also stealing it)
                    instr => instr.MatchLdfld<Player>("onGround"),
                    instr => instr.MatchBrfalse(out _)
                )) {
                    cursor.EmitDelegate((Player player) => {
                        return upwardsCoyote;
                    });
                    cursor.EmitBrtrue(i == 0 ? beforeStaminaRefillLabel : beforeCoyoteRefillLabel);
                    cursor.EmitLdarg0(); //also giving it back
                }
                
                if(i == 0) {
                    if(cursor.TryGotoNextBestFit(MoveType.Before,
                        instr => instr.MatchLdarg0(),
                        instr => instr.MatchLdcR4(110),
                        instr => instr.MatchStfld<Player>("Stamina")
                    )) {
                        cursor.MarkLabel(beforeStaminaRefillLabel);
                    }
                } else {
                    if(cursor.TryGotoNextBestFit(MoveType.Before,
                        instr => instr.MatchLdarg0(),
                        instr => instr.MatchLdcR4(0.1f),
                        instr => instr.MatchStfld<Player>("jumpGraceTimer")
                    )) {
                        cursor.MarkLabel(beforeCoyoteRefillLabel);
                    }
                }
            }
        }
    }
}
