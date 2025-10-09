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
using Celeste.Mod.Entities;
using System.Dynamic;

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
                return GetOptionBool(Option.CustomSwimming) || GetOptionValue(Option.VerticalToHorizontalSpeedOnGroundJump) != (int)VerticalToHorizontalSpeedOnGroundJumpValue.None;
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
            ModIntegration.ExtendedVariantModeAPI.Load();

            FluidSimulation.Load();
            AbstractTrigger<GooberPhysicsOptions>.Load();
            AbstractTrigger<GooberMiscellaneousOptions>.Load();
            AbstractTrigger<RefillFreezeLength>.Load();
            AbstractTrigger<RetentionFrames>.Load();

            GooberHelperOptions.Load();

            BufferOffsetIndicator.Load();

            DebugMapThings.Load();

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
            IL.Celeste.Player.SwimUpdate += modifyPlayerSwimUpdate;

            On.Celeste.Player.Update += modPlayerUpdate;
            On.Celeste.Player.Jump += modPlayerJump;
            On.Celeste.Player.Rebound += modPlayerRebound;
            On.Celeste.Player.ReflectBounce += modPlayerReflectBounce;
            On.Celeste.Player.PointBounce += modPlayerPointBounce;
            On.Celeste.Player.OnCollideH += modPlayerOnCollideH;
            On.Celeste.Player.OnCollideV += modPlayerOnCollideV;
            On.Celeste.Player.SuperWallJump += modPlayerSuperWallJump;
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

            // //code adapted from https://github.com/0x0ade/CelesteNet/blob/405a7e5e4d78727cd35ee679a730400b0a46667a/CelesteNet.Client/Components/CelesteNetMainComponent.cs#L71-L75 (thank you snip for posting this link 8 months ago)
            // using (new DetourConfigContext(new DetourConfig(
            //     "GooberHelper",
            //     int.MinValue  // this simulates before: "*"
            // )).Use()) {
            //     On.Celeste.Player.SwimUpdate += modPlayerSwimUpdate;
            // }


            using (new DetourConfigContext(new DetourConfig(
                "GooberHelper",
                int.MaxValue
            )).Use()) {
                On.Celeste.PlayerHair.Render += modPlayerHairRender;
                On.Celeste.Player.DreamDashBegin += modPlayerDreamDashBegin;
            }

            using (new DetourConfigContext(new DetourConfig(
                "GooberHelper",
                int.MinValue
            )).Use()) {
                On.Celeste.Player.DreamDashUpdate += modPlayerDreamDashUpdate;
            }
        }

        public override void Unload() {
            FluidSimulation.Unload();
            AbstractTrigger<GooberPhysicsOptions>.Unload();
            AbstractTrigger<GooberMiscellaneousOptions>.Unload();
            AbstractTrigger<RefillFreezeLength>.Unload();
            AbstractTrigger<RetentionFrames>.Unload();


            GooberHelperOptions.Unload();

            BufferOffsetIndicator.Unload();
            
            DebugMapThings.Unload();

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
            IL.Celeste.Player.SwimUpdate -= modifyPlayerSwimUpdate;

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

            // //code adapted from https://github.com/0x0ade/CelesteNet/blob/405a7e5e4d78727cd35ee679a730400b0a46667a/CelesteNet.Client/Components/CelesteNetMainComponent.cs#L71-L75 (thank you snip for posting this link 8 months ago)
            // using (new DetourConfigContext(new DetourConfig(
            //     "GooberHelper",
            //     int.MinValue  // this simulates before: "*"
            // )).Use()) {
            //     On.Celeste.Player.SwimUpdate -= modPlayerSwimUpdate;
            // }

            using (new DetourConfigContext(new DetourConfig(
                "GooberHelper",
                int.MaxValue
            )).Use()) {
                On.Celeste.PlayerHair.Render -= modPlayerHairRender;
                On.Celeste.Player.DreamDashBegin -= modPlayerDreamDashBegin;
            }

            using (new DetourConfigContext(new DetourConfig(
                "GooberHelper",
                int.MinValue
            )).Use()) {
                On.Celeste.Player.DreamDashUpdate -= modPlayerDreamDashUpdate;
            }
        }

        public override void CreateModMenuSection(TextMenu menu, bool inGame, EventInstance snapshot) {
            base.CreateModMenuSection(menu, inGame, snapshot);

            int debugMapPhysicsIndex = menu.items.FindIndex(item => item is TextMenu.OnOff && (item as TextMenu.OnOff).Label == "Debug Map Physics");

            var debugMapPhysics = menu.items[debugMapPhysicsIndex] as TextMenu.OnOff;
            debugMapPhysics.Label = Dialog.Clean("GooberHelper_DebugMapPhysics");
            debugMapPhysics.AddExplodingDescription(menu, Dialog.Clean("GooberHelper_DebugMapPhysics_description"));

            var explodingDescription = menu.items[debugMapPhysicsIndex + 1] as ExplodingDescription;

            debugMapPhysics.OnValueChange += value => {
                if(value) explodingDescription.Explode(); else explodingDescription.Unexplode();
            };

            menu.Add(OuiGooberHelperOptions.CreateOptionsButton(menu, false, inGame));

            menu.Add(new TextMenu.Button(Dialog.Clean("menu_gooberhelper_reset_all_options")).Pressed(() => {
                ResetAll(OptionSetter.User);
            }));
        }

        //thank you everest!!! i stole a lot of your mod options code here; i hope you dont mind
        private void createPauseMenuButton(Level level, TextMenu menu, bool minimal) {
            if(!Settings.ShowOptionsInGame) return;

            int index = menu.items.FindIndex(item => item is TextMenu.Button && (item as TextMenu.Button).Label == Dialog.Clean("menu_pause_options"));
            menu.Insert(index, OuiGooberHelperOptions.CreateOptionsButton(menu, true));
        }

        private void handleVerticalSpeedToHorizontal(Player self, Vector2 originalSpeed) {
            float verticalToHorizontalSpeedOnGroundJumpValue = GetOptionValue(Option.VerticalToHorizontalSpeedOnGroundJump);
            
            if(verticalToHorizontalSpeedOnGroundJumpValue != (int)VerticalToHorizontalSpeedOnGroundJumpValue.None) {
                GooberPlayerExtensions c = GooberPlayerExtensions.Instance;

                float retainedVerticalSpeed = !c.AwesomeRetentionWasInWater && c.AwesomeRetentionTimer > 0 && c.AwesomeRetentionDirection.X == 0 ? Math.Abs(c.AwesomeRetentionSpeed.Y) : 0;

                float dir = Math.Sign(self.Speed.X);

                if(dir == 0) dir = self.moveX;
                if(dir == 0) dir = (int)self.Facing;

                float speedToConvert = Math.Max(Math.Abs(originalSpeed.Y), retainedVerticalSpeed);
                
                if(verticalToHorizontalSpeedOnGroundJumpValue == (int)VerticalToHorizontalSpeedOnGroundJumpValue.Magnitude) {
                    speedToConvert = new Vector2(speedToConvert, originalSpeed.X).Length();
                }

                self.Speed.X = dir * Math.Max(speedToConvert, Math.Abs(self.Speed.X));
            }
        }

        private void handleVerticalJumpSpeed(Player self, Vector2 originalSpeed) {
            float downwardsOptionValue = GetOptionValue(Option.DownwardsJumpSpeedPreservationThreshold);
            float upwardsOptionValue = GetOptionValue(Option.UpwardsJumpSpeedPreservationThreshold);

            bool doDownwardsStuff = Input.MoveY > 0 && (
                downwardsOptionValue == (int)VerticalJumpSpeedPreservationHybridValue.None ? false :
                downwardsOptionValue == (int)VerticalJumpSpeedPreservationHybridValue.DashSpeed ? self.StateMachine.state == Player.StDash :
                originalSpeed.Y >= downwardsOptionValue
            );

            bool doUpwardsStuff = (
                upwardsOptionValue == (int)VerticalJumpSpeedPreservationHybridValue.None ? false :
                upwardsOptionValue == (int)VerticalJumpSpeedPreservationHybridValue.DashSpeed ? self.StateMachine.state == Player.StDash :
                originalSpeed.Y <= -upwardsOptionValue
            );

            //probably add something to allow conversion between the two

            //gaslighting my own mod
            //did you know that gaslighting was invented by john gas?
            if(doDownwardsStuff) {
                self.Speed.Y *= -1;
                originalSpeed.Y *= -1;
            }

            if(GetOptionBool(Option.AdditiveVerticalJumpSpeed)) {
                self.Speed.Y = Math.Min(self.Speed.Y, self.varJumpSpeed + Math.Min(originalSpeed.Y, 0));

                self.varJumpSpeed = self.Speed.Y;
            } else {
                if(doDownwardsStuff || doUpwardsStuff) {
                    self.Speed.Y = Math.Min(originalSpeed.Y + self.LiftBoost.Y, self.Speed.Y);
                    self.varJumpSpeed = self.Speed.Y;
                }
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

            if(GetOptionValue(Option.DownwardsJumpSpeedPreservationThreshold) != -1) self.maxFall = originalMaxFall;
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
                    cursor.EmitLdarg0();
                    cursor.EmitDelegate((Player player) => {
                        if(GetOptionBool(Option.AllowCrouchedHoldableGrabbing)) return player.Ducking;

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
                    cursor.EmitLdarg0();
                    cursor.EmitDelegate((bool value, Player player) => {
                        if(GetOptionBool(Option.AllowCrouchedHoldableGrabbing)) return player.Ducking;

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
            if(self is not Level) {
                orig(self);

                return;
            }

            float timeActive = self.TimeActive;
            GooberPlayerExtensions c = GooberPlayerExtensions.Instance;

            orig(self);

            if(Math.Abs(timeActive - self.TimeActive) > 0.000001f && c != null) {
                c.Counter++;
            }
        }

        private void modLevelUpdate(On.Celeste.Level.orig_Update orig, Level self) {
            GooberPlayerExtensions c = GooberPlayerExtensions.Instance;

            if(c == null) {
                orig(self);

                return;
            }

            if(GetOptionBool(Option.RefillFreezeGameSuspension) && c.FreezeFrameFrozen) {
                var newInputs = new Utils.InputState();
                
                if(c.FreezeFrameFrozenInputs.FarEnoughFrom(newInputs)) {
                    c.FreezeFrameFrozen = false;

                    Celeste.Freeze(0.01f);
                } else {
                    self.Camera.Position = self.Camera.position + ((c.Entity as Player).CameraTarget - self.Camera.position) * (1f - (float)Math.Pow(0.01f, Engine.DeltaTime));

                    //CODE DIRECTLY COPIED FROM SPEEDRUNTOOL StateManager.cs
                    self.Wipe?.Update(self);
                    self.HiresSnow?.Update(self);
                    self.Foreground.Update(self);
                    self.Background.Update(self);
                    Engine.Scene.Tracker.GetEntity<CassetteBlockManager>()?.Update();
                    foreach(var entity in Engine.Scene.Tracker.GetEntities<CassetteBlock>()) {
                        entity.Update();
                    }

                    foreach(var listener in Engine.Scene.Tracker.GetComponents<CassetteListener>()) {
                        listener.Entity.Update();
                    }

                    GameSuspensionIgnore.UpdateEntities();

                    return;
                }
            }

            if(GetOptionBool(Option.LenientStunning) && !self.Paused && c.StunningWatchTimer > 0f) {
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
            if(GetOptionValue(Option.PlayerShaderMask) != (int)PlayerShaderMaskValue.HairOnly || !startedRendering) {
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
            if(GetOptionValue(Option.PlayerShaderMask) != (int)PlayerShaderMaskValue.Cover) {
                orig(self);

                return;
            }

            doPlayerMaskStuffBefore(lastPlayerHairColor.ToVector4());

            orig(self);

            doPlayerMaskStuffAfter();
        }
        
        private void modPlayerRender(On.Celeste.Player.orig_Render orig, Player self) {            
            startedRendering = true;

            if(GetOptionValue(Option.PlayerShaderMask) != (int)PlayerShaderMaskValue.Cover) {
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
            if(!GetOptionBool(Option.UpwardsTransitionSpeedPreservation)) {
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

                cursor.EmitLdarg0();
                cursor.EmitDelegate((Player player) => {
                    if(GetOptionBool(Option.AllDirectionHypersAndSupers)) {
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
                                    (coyote > 0f || ModIntegration.ExtendedVariantModeAPI.GetJumpCount?.Invoke() > 0) && 
                                    GetOptionValue(Option.AllDirectionHypersAndSupers) == (int)AllDirectionHypersAndSupersValue.WorkWithCoyoteTime
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
                cursor.EmitLdarg0();
                cursor.EmitDelegate((float value, Player player) => {
                    if(GetOptionValue(Option.DownwardsJumpSpeedPreservationThreshold) == -1) return value;

                    float varJumpSpeed = player.varJumpSpeed;

                    return varJumpSpeed > 0 && !player.onGround ? varJumpSpeed : value;
                });
            }
        }

        private void modifyPlayerDashUpdate(ILContext il) {
            allowHoldableClimbjumping(new ILCursor(il));
            allowAllDirectionHypersAndSupers(new ILCursor(il), OpCodes.Ldarg_0, false);

            ILCursor cursor = new ILCursor(il);

            if(cursor.TryGotoNextBestFit(MoveType.Before, 
                instr => instr.MatchLdarg0(),
                instr => instr.MatchCallOrCallvirt<Player>("get_SuperWallJumpAngleCheck"),
                instr => instr.MatchBrfalse(out _)
            )) {
                ILLabel afterReturnLabel = cursor.DefineLabel();

                cursor.MoveAfterLabels();

                //if(customswimming && tryCustomSwimmingJump(this, this.DashDir))
                cursor.EmitLdarg0();
                cursor.EmitDelegate((Player player) => {
                    return GetOptionBool(Option.CustomSwimming) && Input.Jump && tryCustomSwimmingWalljump(player, player.DashDir);
                });

                cursor.EmitBrfalse(afterReturnLabel);

                //return StSwim;
                cursor.EmitLdcI4(Player.StSwim);
                cursor.EmitRet();

                cursor.MarkLabel(afterReturnLabel);
            }
        }

        private void modifyPlayerPickupCoroutine(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            cursor.TryGotoNext(MoveType.After, instr => instr.MatchStfld(typeof(Player), nameof(Player.Speed)));
            cursor.TryGotoNext(MoveType.After, instr => instr.MatchStfld(typeof(Player), nameof(Player.Speed)));

            cursor.EmitLdarg0();
            cursor.EmitDelegate((Player player) => {
                if(!GetOptionBool(Option.PickupSpeedInversion)) return;

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
            Vector2 originalSpeed = self.Speed;
            bool wasDucking = self.Ducking;

            orig(self);

            handleVerticalSpeedToHorizontal(self, originalSpeed);

            if(GetOptionBool(Option.HyperAndSuperSpeedPreservation)) {
                //this exists so that alldirectionsHypersAndSupers can be compatible
                //i dont think it will break anything else                                                                                                                         :cluel:
                float kindaAbsoluteSpeed = originalSpeed.Length() == 0 ? self.beforeDashSpeed.Length() : originalSpeed.Length();
                
                self.Speed.X = (int)self.Facing * Math.Max(Math.Abs(kindaAbsoluteSpeed), Math.Abs(260f * (wasDucking ? 1.25f : 1f))) + self.LiftBoost.X;
            }

            if(GetOptionBool(Option.AdditiveVerticalJumpSpeed)) {
                self.Speed.Y = Math.Min(self.Speed.Y, self.varJumpSpeed + Math.Min(originalSpeed.Y, 0));

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

            //approach based on hitbox width extension. it doesnt work
            //method #1: extension towards the wall
            //the player will be unable to collide with sideways jumpthroughs because they call 
            //Entity.CollideFirstOutside<T>(Vector2 position), a method that requires the unshifted
            //player hitbox to NOT be in the entity of collision. extending the hitbox would force
            //it to always return false if the player is close enough to the sideways jumpthrough
            //method #2: extension away from the wall
            //this would have the same problem. if you extend the player hitbox such that it grows
            //away from the wall, the same thing will happen as method #1 except it will collide
            //with an entity on the other side of the wall check. imagine a scenario where the
            //player has a really high horizontal speed but is trapped in a 2 tile wide hole with
            //a sideways jumpthrough that they're supposed to cornerboost off of. i know this is
            //extemely unlikely, but i dont want to have that edge case.
            //ill take my scuffed inefficient solution that actually works

            // int extension = 0;

            // if(cursor.TryGotoNextBestFit(MoveType.After,
            //     instr => instr.MatchLdarg0(), //stealing this
            //     instr => instr.MatchLdarg1(),
            //     instr => instr.MatchCallOrCallvirt<Player>("ClimbBoundsCheck")
            // )) {
            //     cursor.GotoPrev(MoveType.After, instr => instr.MatchLdarg0());

            //     cursor.EmitLdloc0();
            //     cursor.EmitLdarg1();
            //     cursor.EmitDelegate((Player player, int originalDistance, int dir) => {
            //         extension = 0;

            //         if(GetOptionBool(Option.CornerboostBlocksEverywhere)) {
            //             extension = Math.Max(originalDistance, (int)Math.Ceiling(Math.Abs(player.Speed.X) * Engine.DeltaTime) + 1) - originalDistance;

            //             player.Collider.Width += extension;
            //             if(dir == -1) player.Collider.Position.X -= extension;
            //         }
            //     });

            //     cursor.EmitLdarg0(); //giving it back
            // }

            // while(cursor.TryGotoNext(MoveType.Before, instr => instr.MatchRet())) {
            //     cursor.EmitLdarg0();
            //     cursor.EmitLdarg1();
            //     cursor.EmitDelegate((Player player, int dir) => {
            //         player.Collider.Width -= extension;

            //         if(dir == -1) player.Collider.Position.X += extension;
            //     });

            //     cursor.Index++;
            // }

            if(cursor.TryGotoNextBestFit(MoveType.Before,
                instr => instr.MatchLdarg0(),
                instr => instr.MatchLdarg1(),
                instr => instr.MatchCallOrCallvirt<Player>("ClimbBoundsCheck")
            )) {
                cursor.MoveAfterLabels();

                cursor.EmitLdarg0();
                cursor.EmitLdloc0();
                cursor.EmitDelegate((Player player, int originalDistance) => {
                    return GetOptionBool(Option.CornerboostBlocksEverywhere) ?
                        Math.Max(originalDistance, (int)Math.Ceiling(Math.Abs(player.Speed.X) * Engine.DeltaTime) + 1) :
                        originalDistance;
                });
                cursor.EmitStloc0();
            }

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

                GooberPlayerExtensions.Instance.AwesomeRetentionSpeed = Vector2.Zero;
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
                cursor.Index--;

                cursor.EmitLdloc1();
                cursor.EmitDelegate((Vector2 speed, Player player) => {
                    Vector2 newSpeed = speed * Vector2.One;

                    Vector2 beforeDashSpeed = player.beforeDashSpeed;

                    if(
                        GetOptionValue(Option.MagnitudeBasedDashSpeed) == (int)MagnitudeBasedDashSpeedValue.All ||
                        (
                            GetOptionValue(Option.MagnitudeBasedDashSpeed) == (int)MagnitudeBasedDashSpeedValue.OnlyCardinal &&
                            Vector2.Dot(originalDashSpeed, Vector2.UnitX) % 1 == 0 //the second part just checks if the vector is cardinal. do you like my commenting?
                        )
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

            Func<float, Player, float> makeVerticalDashesNotResetSpeed(DashesDontResetSpeedValue minimum) {
                return (value, player) => {
                    return (
                        (GetOptionBool(Option.CustomSwimming) && player.CollideCheck<Water>()) ||
                        GetOptionValue(Option.DashesDontResetSpeed) >= (int)minimum
                    ) ? float.MinValue : value;
                };
            };

            if(cursor.TryGotoNextBestFit(MoveType.After, 
                instr => instr.MatchLdflda<Player>("DashDir"),
                instr => instr.MatchLdfld<Vector2>("Y"),
                instr => instr.MatchLdcR4(0),
                instr => instr.MatchBgtUn(out _)
            )) {
                cursor.Index--;

                cursor.EmitLdloc1();
                cursor.EmitDelegate(makeVerticalDashesNotResetSpeed(DashesDontResetSpeedValue.Legacy));
            }

            if(cursor.TryGotoNextBestFit(MoveType.After, 
                instr => instr.MatchLdflda<Player>("Speed"),
                instr => instr.MatchLdfld<Vector2>("Y"),
                instr => instr.MatchLdcR4(0),
                instr => instr.MatchBgeUn(out _)
            )) {
                cursor.Index--;

                cursor.EmitLdloc1();
                cursor.EmitDelegate(makeVerticalDashesNotResetSpeed(DashesDontResetSpeedValue.On));
            }
        }

        private bool tryCustomSwimmingWalljump(Player self, Vector2 vector) {
            GooberPlayerExtensions c = GooberPlayerExtensions.Instance;

            float redirectSpeed = Math.Max(self.Speed.Length(), c.AwesomeRetentionSpeed.Length()) + 20;

            if(c.AwesomeRetentionTimer <= 0 || !c.AwesomeRetentionWasInWater) {
                return false;
            }

            Vector2 redirectDirection = -c.AwesomeRetentionDirection;

            if(redirectDirection != Vector2.Zero && redirectSpeed != 0) {
                Input.Jump.ConsumeBuffer();
                self.Speed = redirectDirection.SafeNormalize() * redirectSpeed;

                int index = c.AwesomeRetentionPlatform.GetWallSoundIndex(self, Math.Sign(c.AwesomeRetentionDirection.X));

                Dust.Burst(self.Center - redirectDirection * self.Collider.Size / 2, redirectDirection.Angle(), 4, self.DustParticleFromSurfaceIndex(index));
                self.Play(SurfaceIndex.GetPathFromIndex(index) + "/landing", "surface_index", index);
            }

            return true;
        }

        private bool doCustomSwimMovement(Player self, Vector2 vector) {
            float defaultSpeed = 90f;

            if(Vector2.Dot(self.Speed.SafeNormalize(Vector2.Zero), vector) < -0.5f || vector.Length() == 0 || self.Speed.Length() <= 90) {
                self.Speed = Calc.Approach(self.Speed, defaultSpeed * vector, 350f * Engine.DeltaTime);
            } else {
                self.Speed = self.Speed.RotateTowards(vector.Angle(), 10f * Engine.DeltaTime);
            }

            //awesome retention is used while underwater
            //i need to think of a better name for that
            self.wallSpeedRetentionTimer = 0f;

            if(Input.Jump.Pressed) {
                float distance = Math.Min(self.Speed.Y * Engine.DeltaTime, 0) - 10f;

                if(!self.CollideCheck<Water>(self.Position + Vector2.UnitY * distance)) {
                    if (self.Speed.Y >= 0) {
                        // self.Jump(true, true);

                        return true;
                    }

                    if(self.Speed.Y < -130f) {
                        Input.Jump.ConsumeBuffer();
                        self.Speed += 80f * self.Speed.SafeNormalize(Vector2.Zero);
                        
                        self.launched = true;
                        Dust.Burst(self.Position, (vector * -1f).Angle(), 4);

                        self.Play(SFX.char_mad_jump_super);
                    }
                }

                tryCustomSwimmingWalljump(self, vector);
            }

            return false;
        }

        private void modifyPlayerSwimUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            ILLabel originalMovementEndLabel = cursor.DefineLabel();
            ILLabel customMovementEndLabel = cursor.DefineLabel();
            ILLabel swimJumpEndLabel = cursor.DefineLabel();

            void LogError() {
                Logger.Error("GooberHelper", "Failed to find il while making custom swimming work");
            }

            if(cursor.TryGotoNextBestFit(MoveType.After, 
                instr => instr.MatchLdloc1(),
                instr => instr.MatchCallOrCallvirt(typeof(Calc).GetMethods().Where(method => method.Name == "SafeNormalize").First()),
                instr => instr.MatchStloc1()
            )) {
                ILLabel afterReturnLabel = cursor.DefineLabel();

                //if(GetOptionBool(Option.CustomSwimming))
                cursor.EmitDelegate(() => {
                    return GetOptionBool(Option.CustomSwimming);
                });
                cursor.EmitBrfalse(customMovementEndLabel);

                //if(doCustomSwimMovement(this, vector))
                cursor.EmitLdarg0();
                cursor.EmitLdloc1();
                cursor.EmitDelegate(doCustomSwimMovement);
                cursor.EmitBrfalse(afterReturnLabel);
                
                //return 0;
                cursor.EmitLdcI4(0);
                cursor.EmitRet();

                cursor.MarkLabel(afterReturnLabel);

                cursor.EmitBr(originalMovementEndLabel);

                cursor.MarkLabel(customMovementEndLabel);
            } else {
                LogError();

                return;
            }

            if(cursor.TryGotoNextBestFit(MoveType.Before, 
                instr => instr.MatchLdloc0(),
                instr => instr.MatchBrtrue(out _),
                instr => instr.MatchLdarg0(),
                instr => instr.MatchLdfld<Player>("moveX")
            )) {
                cursor.MarkLabel(originalMovementEndLabel);
            } else {
                LogError();

                return;
            }

            if(cursor.TryGotoNextBestFit(MoveType.Before,
                instr => instr.MatchLdsfld(typeof(Input).GetField("Jump")),
                instr => instr.MatchCallOrCallvirt<VirtualButton>("get_Pressed"),
                instr => instr.MatchBrfalse(out swimJumpEndLabel)
            )) {
                cursor.MoveAfterLabels();
                cursor.EmitDelegate(() => {
                    return GetOptionBool(Option.CustomSwimming);
                });
                cursor.EmitBrtrue(swimJumpEndLabel);
            } else {
                LogError();

                return;
            }
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
            if(!GetOptionBool(Option.CoreBlockAllDirectionActivation)) {
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

            if(GetOptionBool(Option.BadelineBossSpeedPreservation)) {
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
                    float newTime = GetOptionValue(Option.RetentionLength);

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

                    cursor.EmitLdarg0();
                    cursor.EmitDelegate((Player player) => {
                        if(GetOptionBool(Option.FeatherEndSpeedPreservation)) {
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
                cursor.EmitLdloc1();
                cursor.EmitDelegate((Player player) => {
                    if(GetOptionValue(Option.CustomFeathers) == (int)CustomFeathersValue.SkipIntro) {
                        player.Sprite.Play("starFly", false, false);

                        return true;
                    }

                    return false;
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
                cursor.EmitLdloc1();
                cursor.EmitDelegate((Player player) => {
                    if(GetOptionBool(Option.CustomFeathers)) {
                        player.Speed = GooberPlayerExtensions.Instance.StarFlySpeedPreserved.SafeNormalize() * Math.Max(GooberPlayerExtensions.Instance.StarFlySpeedPreserved.Length(), 250);
                    }
                });
            }
        }

        private void modPlayerClimbJump(On.Celeste.Player.orig_ClimbJump orig, Player self) {
            Vector2 originalSpeed = self.Speed;

            int beforeJumpCount = SaveData.Instance.TotalJumps;

            if(self.wallSpeedRetentionTimer > 0f && GetOptionBool(Option.GetClimbjumpSpeedInRetention)) {
                self.Speed.X = self.wallSpeedRetained;
            }

            orig(self);

            if(self.wallSpeedRetentionTimer > 0f && GetOptionBool(Option.GetClimbjumpSpeedInRetention)) {
                self.wallSpeedRetained = self.Speed.X;
            }

            //the method didnt run; dont do anything else
            if(beforeJumpCount == SaveData.Instance.TotalJumps) return;

            // self.Speed.X = Math.Abs(originalSpeed.Y) * Math.Sign(self.Speed.X);
            // self.Speed.Y = -Math.Max(Math.Max(Math.Abs(originalSpeed.X), Math.Abs(self.wallSpeedRetained) * (self.wallSpeedRetentionTimer > 0f ? 1f : 0f)), Math.Abs(self.Speed.Y));
            // self.varJumpSpeed = self.Speed.Y;
            // self.wallSpeedRetentionTimer = 0;

            if(GetOptionBool(Option.WallboostDirectionIsOppositeSpeed)) {
                if(Input.MoveX == 0) {
                    self.wallBoostDir = Math.Sign(-self.Speed.X);
                }
            }

            //dont do it with additive vertical jump speed because that already modifies jump
            // if(!GetOptionBool(Option.AdditiveVerticalJumpSpeed)) {
            //     float upwardsThreshold = Math.Min(-GetOptionValue(Option.UpwardsJumpSpeedPreservationThreshold), self.varJumpSpeed);
            //     float downwardsThreshold = Math.Min(-GetOptionValue(Option.DownwardsJumpSpeedPreservationThreshold), self.varJumpSpeed);

            //     if(
            //         (upwardsThreshold != 1 && originalSpeed.Y < upwardsThreshold) || //1 because its inverted
            //         (doDownwardsStuff && originalSpeed.Y < downwardsThreshold)
            //     ) {
            //         self.Speed.Y = originalSpeed.Y + self.LiftBoost.Y;
            //         self.varJumpSpeed = self.Speed.Y;
            //     }
            // }
        }

        private void modPlayerWallJump(On.Celeste.Player.orig_WallJump orig, Player self, int dir) {
            Vector2 originalSpeed = self.Speed;

            int beforeJumpCount = SaveData.Instance.TotalWallJumps;

            orig(self, dir);
            
            //the method didnt run; dont do anything else
            if(beforeJumpCount == SaveData.Instance.TotalWallJumps) return;

            if(GetOptionBool(Option.SwapHorizontalAndVerticalSpeedOnWalljump)) {
                self.Speed.X = Math.Max(Math.Abs(originalSpeed.Y), Math.Abs(self.Speed.X)) * Math.Sign(self.Speed.X);
                self.Speed.Y = -Math.Max(Math.Max(Math.Abs(originalSpeed.X), Math.Abs(self.wallSpeedRetained) * (self.wallSpeedRetentionTimer > 0f ? 1f : 0f)), Math.Abs(self.Speed.Y));
                self.varJumpSpeed = self.Speed.Y;

                return;
            }

            handleVerticalJumpSpeed(self, originalSpeed);

            WalljumpSpeedPreservationValue wallJumpSpeedPreservationValue = (WalljumpSpeedPreservationValue)GetOptionValue(Option.WalljumpSpeedPreservation);

            if(wallJumpSpeedPreservationValue == WalljumpSpeedPreservationValue.Invert) {
                self.Speed.X = Math.Sign(self.Speed.X) * Math.Max(
                    Math.Max(
                        Math.Abs(self.Speed.X),
                        Math.Abs(originalSpeed.X)
                    ),
                    Math.Abs(self.wallSpeedRetained) * (self.wallSpeedRetentionTimer > 0f ? 1f : 0f)
                ) + self.LiftSpeed.X;
            } else if(Math.Sign(self.Speed.X - self.LiftSpeed.X) == Math.Sign(originalSpeed.X) && wallJumpSpeedPreservationValue != WalljumpSpeedPreservationValue.None) {
                self.Speed.X = Math.Sign(originalSpeed.X) * Math.Max(
                    Math.Abs(self.Speed.X), 
                    Math.Abs(originalSpeed.X) - (Input.MoveX == 0 || wallJumpSpeedPreservationValue == WalljumpSpeedPreservationValue.Preserve ? 0f : 40f)
                ) + self.LiftSpeed.X;
            }
        }

        private int modPlayerDreamDashUpdate(On.Celeste.Player.orig_DreamDashUpdate orig, Player self) {
            if(GetOptionBool(Option.DreamBlockSpeedPreservation)) {
                Vector2 correctSpeed = GooberPlayerExtensions.Instance.PreservedDreamBlockSpeedMagnitude;

                if(self.Speed.X == -correctSpeed.X && Math.Abs(self.Speed.X) > 0) correctSpeed.X *= -1; else 
                if(self.Speed.Y == -correctSpeed.Y && Math.Abs(self.Speed.Y) > 0) correctSpeed.Y *= -1; else
                {
                    string dreamBlockType = self.dreamBlock.GetType().Name;
                    DynamicData data = DynamicData.For(self.dreamBlock);

                    //i know this is evil but also putting code to update the player speed to anything constant is evil too so it cancels out and its fine
                    if(dreamBlockType == "ConnectedDreamBlock" && data.Get<bool>("FeatherMode")) {
                        self.Speed = self.Speed.SafeNormalize() * correctSpeed.Length();
                    } else {
                        self.Speed = correctSpeed;
                    }
                }
            }

            return orig(self);
        }

        private void modPlayerDreamDashBegin(On.Celeste.Player.orig_DreamDashBegin orig, Player self) {
            Vector2 originalSpeed = self.Speed;

            orig(self);

            var optionValue = (DreamBlockSpeedPreservationValue)GetOptionValue(Option.DreamBlockSpeedPreservation);

            if(optionValue != DreamBlockSpeedPreservationValue.None) {
                Vector2 componentMax = self.Speed.Sign() * Vector2.Max(self.Speed.Abs(), originalSpeed.Abs());

                switch(optionValue) {
                    case DreamBlockSpeedPreservationValue.Horizontal:self.Speed.X = componentMax.X; break;
                    case DreamBlockSpeedPreservationValue.Vertical: self.Speed.Y = componentMax.Y; break;
                    case DreamBlockSpeedPreservationValue.Both: self.Speed = componentMax; break;
                    case DreamBlockSpeedPreservationValue.Magnitude:
                        self.Speed = self.Speed.SafeNormalize() * Math.Max(originalSpeed.Length(), self.Speed.Length());
                    break;
                }
                
                GooberPlayerExtensions.Instance.PreservedDreamBlockSpeedMagnitude = self.Speed;
            }
        }

        private void modPlayerSuperWallJump(On.Celeste.Player.orig_SuperWallJump orig, Player self, int dir) {
            Vector2 originalSpeed = self.Speed;

            int beforeJumpCount = SaveData.Instance.TotalWallJumps;

            orig(self, dir);

            //the method didnt run; dont do anything else
            if(beforeJumpCount == SaveData.Instance.TotalWallJumps) return;

            handleVerticalJumpSpeed(self, originalSpeed);

            // if(GetOptionBool(Option.AdditiveVerticalJumpSpeed)) {
            //     self.Speed.Y = Math.Min(self.Speed.Y, self.varJumpSpeed + Math.Min(originalSpeed.Y, 0));

            //     self.varJumpSpeed = self.Speed.Y;
            // } else {
            //     float upwardsThreshold = Math.Min(-GetOptionValue(Option.UpwardsJumpSpeedPreservationThreshold), self.varJumpSpeed);

            //     if(upwardsThreshold != 1 && originalSpeed.Y < upwardsThreshold) {
            //         self.Speed.Y = originalSpeed.Y + self.LiftSpeed.Y;
            //         self.varJumpSpeed = self.Speed.Y;
            //     }
            // }

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
            if(refillRoutineRegex.IsMatch(new System.Diagnostics.StackTrace().ToString())) {
                if(newTime != 3f) time = newTime / 60f;

                if(GetOptionBool(Option.RefillFreezeGameSuspension)) {
                    GooberPlayerExtensions c = GooberPlayerExtensions.Instance;

                    c.FreezeFrameFrozen = true;
                    c.FreezeFrameFrozenInputs = new Utils.InputState();

                    return;
                }
            }

            orig(time);
        }

        private void modPlayerPointBounce(On.Celeste.Player.orig_PointBounce orig, Player self, Vector2 from) {
            if(!GetOptionBool(Option.ReboundSpeedPreservation)) {
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
            if(!GetOptionBool(Option.ReboundSpeedPreservation)) {
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
            if(!GetOptionBool(Option.ReboundSpeedPreservation) || direction.X == 0) {
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
            Vector2 originalSpeed = self.Speed;

            orig(self, fromY);

            float springSpeedPreservationValue = GetOptionValue(Option.SpringSpeedPreservation);

            if(springSpeedPreservationValue != (int)SpringSpeedPreservationValue.None) {
                self.Speed.X = originalSpeed.X;

                if(self.moveX == -Math.Sign(self.Speed.X) && springSpeedPreservationValue == (int)SpringSpeedPreservationValue.Invert) {
                    self.Speed.X *= -1;
                }
            }
        }

        private void modPlayerOnCollideH(On.Celeste.Player.orig_OnCollideH orig, Player self, CollisionData data) {
            if(UseAwesomeRetention) {
                GooberPlayerExtensions c = GooberPlayerExtensions.Instance;

                if(c.AwesomeRetentionTimer <= 0.0f) {
                    c.AwesomeRetentionSpeed = self.Speed;
                    c.AwesomeRetentionTimer = 0.06f;
                    c.AwesomeRetentionWasInWater = self.CollideCheck<Water>();
                    c.AwesomeRetentionPlatform = data.Hit;

                    c.AwesomeRetentionDirection = new Vector2(data.Direction.X, c.AwesomeRetentionDirection.Y);
                }
            }
            
            float originalDashAttack = self.dashAttackTimer;

            orig(self, data);

            if(GetOptionBool(Option.KeepDashAttackOnCollision)) {
                self.dashAttackTimer = originalDashAttack;
            }
        }

        private void modPlayerOnCollideV(On.Celeste.Player.orig_OnCollideV orig, Player self, CollisionData data) {
            if(UseAwesomeRetention) {
                GooberPlayerExtensions c = GooberPlayerExtensions.Instance;

                if(c.AwesomeRetentionTimer <= 0.0f) {
                    c.AwesomeRetentionSpeed = self.Speed;
                    c.AwesomeRetentionTimer = 0.06f;
                    c.AwesomeRetentionWasInWater = self.CollideCheck<Water>();
                    c.AwesomeRetentionPlatform = data.Hit;

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

            if((float)self.moveX == -Math.Sign(self.Speed.X)) {
                if(
                    GetOptionValue(Option.JumpInversion) == (int)JumpInversionValue.All ||
                    (!isClimbjump && GetOptionValue(Option.JumpInversion) == (int)JumpInversionValue.GroundJumps)
                ) {
                    self.Speed.X *= -1;
                }
            }

            if(!isClimbjump) handleVerticalSpeedToHorizontal(self, originalSpeed);

            int beforeJumpCount = SaveData.Instance.TotalJumps;

            orig(self, particles, playSfx);

            //the method didnt run; dont do anything else
            if(beforeJumpCount == SaveData.Instance.TotalJumps) return;

            handleVerticalJumpSpeed(self, originalSpeed);
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

            if(GetOptionBool(Option.PickupSpeedInversion) && self.StateMachine.State == 8) {
                self.Facing = self.moveX == 0 ? self.Facing : (Facings)self.moveX;
            }
        }

        private void modifyPlayerUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            //both of these variables dont need to be part of the session
            //theyre both assigned and accessed in the same method
            bool upwardsCoyote = false;

            if(cursor.TryGotoNextBestFit(MoveType.Before,
                instr => instr.MatchLdarg0(),
                instr => instr.MatchLdfld<Player>("StateMachine"),
                instr => instr.MatchCallOrCallvirt<StateMachine>("get_State"),
                instr => instr.MatchLdcI4(9),
                instr => instr.MatchBneUn(out _)
            )) {
                cursor.MoveAfterLabels();

                cursor.EmitLdarg0();
                cursor.EmitDelegate((Player player) => {
                    if(!GetOptionBool(Option.AllowUpwardsCoyote) || player.Speed.Y > 0) {
                        upwardsCoyote = false;

                        return;
                    }

                    upwardsCoyote = 
                        player.CollideFirst<Solid>(player.Position + Vector2.UnitY) != null ||
                        (player.CollideCheck<JumpThru>(player.Position + Vector2.UnitY * player.Collider.Height) && player.CollideCheck<JumpThru>(player.Position + Vector2.UnitY));
                });
            }

            float cobwob_originalSpeed = 0;

            //[BEFORE] this.Speed.X = 130f * (float)this.moveX;
            if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(130f))) {
                cursor.EmitLdarg0();
                cursor.EmitDelegate((float orig, Player player) => {
                    if(GetOptionValue(Option.CobwobSpeedInversion) == (int)CobwobSpeedInversionValue.None) return orig;

                    if (player == null) return orig;

                    cobwob_originalSpeed = player.Speed.X;

                    return orig;
                });
            }

            //[BEFORE] this.Stamina += 27.5f;
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(27.5f))) {
                cursor.EmitLdarg0();
                cursor.EmitDelegate((float orig, Player player) => {
                    if(
                        GetOptionValue(Option.CobwobSpeedInversion) == (int)CobwobSpeedInversionValue.None &&
                        !GetOptionBool(Option.WallboostSpeedIsOppositeSpeed)
                    ) return orig;

                    if (player == null) return orig;

                    float dir = Math.Sign(player.Speed.X);
                    float newAbsoluteSpeed = Math.Max(130f, Math.Abs(cobwob_originalSpeed));

                    if(
                        GetOptionBool(Option.WallboostSpeedIsOppositeSpeed) &&
                        !GetOptionBool(Option.WallboostDirectionIsOppositeSpeed) &&
                        player.wallBoostDir == Math.Sign(cobwob_originalSpeed - 11f * Math.Sign(cobwob_originalSpeed))
                    ) {
                        dir = -Math.Sign(cobwob_originalSpeed);
                    }
                    
                    if(player.wallSpeedRetentionTimer > 0.0 && GetOptionValue(Option.CobwobSpeedInversion) == (int)CobwobSpeedInversionValue.WorkWithRetention) {
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
                    instr => instr.MatchLdarg0(),
                    instr => instr.MatchLdfld<Player>("onGround"),
                    instr => instr.MatchBrfalse(out _)
                )) {
                    cursor.MoveAfterLabels();
                    cursor.EmitDelegate(() => {
                        return upwardsCoyote;
                    });
                    cursor.EmitBrtrue(i == 0 ? beforeStaminaRefillLabel : beforeCoyoteRefillLabel);
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
