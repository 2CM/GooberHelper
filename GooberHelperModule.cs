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

        private static Effect playerMask = null;
        private static Color lastPlayerHairColor = Player.NormalHairColor;

        private static Regex refillRoutineRegex = new Regex("RefillRoutine");


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
            FluidSimulation.Load();
            //i really gotta refactor this man
            AbstractTrigger<GooberPhysicsOptions>.Load();
            AbstractTrigger<GooberVisualOptions>.Load();
            AbstractTrigger<GooberMiscellaneousOptions>.Load();
            AbstractTrigger<RefillFreezeLength>.Load();
            AbstractTrigger<RetentionFrames>.Load();

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
            On.Celeste.Player.CallDashEvents += modPlayerCallDashEvents;
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

            On.Celeste.BounceBlock.WindUpPlayerCheck += modBounceBlockWindUpPlayerCheck;

            On.Celeste.CrystalStaticSpinner.OnPlayer += modCrystalStaticSpinnerOnPlayer;

            On.Celeste.Celeste.Freeze += modCelesteFreeze;

            On.Celeste.Level.LoadLevel += modLevelLevelLoad;

            On.Celeste.PlayerDeadBody.Render += modPlayerDeadBodyRender;

            //code adapted from https://github.com/0x0ade/CelesteNet/blob/405a7e5e4d78727cd35ee679a730400b0a46667a/CelesteNet.Client/Components/CelesteNetMainComponent.cs#L71-L75 (thank you snip for posting this link 8 months ago)
            using (new DetourConfigContext(new DetourConfig(
                "GooberHelper",
                int.MinValue  // this simulates before: "*"
            )).Use()) {
                On.Celeste.Player.SwimUpdate += modPlayerSwimUpdate;
            }
        }

        public override void Unload() {
            FluidSimulation.Unload();
            AbstractTrigger<GooberPhysicsOptions>.Unload();
            AbstractTrigger<GooberVisualOptions>.Unload();
            AbstractTrigger<GooberMiscellaneousOptions>.Unload();
            AbstractTrigger<RefillFreezeLength>.Unload();
            AbstractTrigger<RetentionFrames>.Unload();

            playerUpdateHook.Dispose();
            playerStarFlyCoroutineHook.Dispose();
            playerDashCoroutineHook.Dispose();
            playerPickupCoroutineHook.Dispose();
            playerRedDashCoroutineHook.Dispose();
            playerDashCoroutineHook2.Dispose();

            IL.Celeste.Player.Update -= modifyPlayerUpdate;
            IL.Celeste.Player.OnCollideH -= modifyPlayerOnCollideH;
            IL.Celeste.Player.OnCollideV -= modifyPlayerOnCollideV;
            IL.Celeste.Player.StarFlyUpdate -= modifyPlayerStarFlyUpdate;
            IL.Celeste.Player.DashUpdate -= modifyPlayerDashUpdate;
            IL.Celeste.Player.NormalUpdate -= modifyPlayerNormalUpdate;
            IL.Celeste.Player.RedDashUpdate -= modifyPlayerRedDashUpdate;
            IL.Celeste.Player.HitSquashUpdate -= modifyPlayerHitSquashUpdate;

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
            On.Celeste.Player.CallDashEvents -= modPlayerCallDashEvents;
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

            On.Celeste.BounceBlock.WindUpPlayerCheck -= modBounceBlockWindUpPlayerCheck;

            On.Celeste.CrystalStaticSpinner.OnPlayer -= modCrystalStaticSpinnerOnPlayer;

            On.Celeste.Celeste.Freeze -= modCelesteFreeze;

            On.Celeste.Level.LoadLevel -= modLevelLevelLoad;

            On.Celeste.PlayerDeadBody.Render -= modPlayerDeadBodyRender;

            //code adapted from https://github.com/0x0ade/CelesteNet/blob/405a7e5e4d78727cd35ee679a730400b0a46667a/CelesteNet.Client/Components/CelesteNetMainComponent.cs#L71-L75 (thank you snip for posting this link 8 months ago)
            using (new DetourConfigContext(new DetourConfig(
                "GooberHelper",
                int.MinValue  // this simulates before: "*"
            )).Use()) {
                On.Celeste.Player.SwimUpdate -= modPlayerSwimUpdate;
            }
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

        private void modPlayerDeadBodyRender(On.Celeste.PlayerDeadBody.orig_Render orig, PlayerDeadBody self) {
            if(!OptionsManager.PlayerMask) {
                orig(self);

                return;
            }

            doPlayerMaskStuff(lastPlayerHairColor.ToVector4());

            orig(self);

            Draw.SpriteBatch.End();
            GameplayRenderer.Begin();
        }
        
        private void modPlayerRender(On.Celeste.Player.orig_Render orig, Player self) {            
            if(!OptionsManager.PlayerMask) {
                orig(self);

                return;
            }

            lastPlayerHairColor = self.Hair.Color;

            doPlayerMaskStuff(new Vector4(self.Hair.Color.ToVector3() * self.Sprite.Color.ToVector3(), 1));

            orig(self);

            Draw.SpriteBatch.End();
            GameplayRenderer.Begin();
        }

        private void doPlayerMaskStuff(Vector4 color) {
            if(playerMask == null) {
                playerMask = FluidSimulation.TryGetEffect("playerMask");
            }
            
            if((Engine.Scene as Level) == null) return;

            GameplayRenderer.End();
            
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, playerMask, (Engine.Scene as Level).Camera.Matrix);
            playerMask.CurrentTechnique = playerMask.Techniques["Grongle"];
            playerMask.Parameters["CamPos"].SetValue((Engine.Scene as Level).Camera.Position);
            playerMask.Parameters["HairColor"].SetValue(color);
            //todo MAKE THIS NOT HARDCODED
            Engine.Graphics.GraphicsDevice.Textures[1] = GFX.Game["guhcat"].Texture.Texture;
        }

        private void modPlayerRedBoost(On.Celeste.Player.orig_RedBoost orig, Player self, Booster booster) {
            GooberPlayerExtensions.Instance.BoostSpeedPreserved = self.Speed;

            orig(self, booster);
        }

        private void modPlayerBoost(On.Celeste.Player.orig_Boost orig, Player self, Booster booster) {
            GooberPlayerExtensions.Instance.BoostSpeedPreserved = self.Speed;

            orig(self, booster);
        }

        private void modifyDashSpeedThing(ILContext context) {
            ILCursor cursor = new ILCursor(context);

            if(cursor.TryGotoNext(MoveType.After, instr => instr.Match(OpCodes.Ldc_R4, 240f))) {
                cursor.EmitDelegate((float value) => {
                    if(!OptionsManager.BubbleSpeedPreservation) return value;

                    GooberPlayerExtensions c = GooberPlayerExtensions.Instance;

                    value = Math.Max(c.BoostSpeedPreserved.Length(), value);

                    c.BoostSpeedPreserved = Vector2.Zero;

                    return value;
                });
            }
        }

        private void modPlayerBeforeUpTransition(On.Celeste.Player.orig_BeforeUpTransition orig, Player self) {
            if(!OptionsManager.KeepSpeedThroughVerticalTransitions) {
                orig(self);

                return;
            }

            float varJumpTimer = DynamicData.For(self).Get<float>("varJumpTimer");
            float varJumpSpeed = DynamicData.For(self).Get<float>("varJumpSpeed");
            Vector2 speed = self.Speed;
            float dashCooldownTimer = DynamicData.For(self).Get<float>("dashCooldownTimer");

            orig(self);


            DynamicData.For(self).Set("varJumpTimer", varJumpTimer);
            DynamicData.For(self).Set("varJumpSpeed", varJumpSpeed);
            self.Speed = speed;
            DynamicData.For(self).Set("dashCooldownTimer", dashCooldownTimer);
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
                    if(OptionsManager.AllDirectionHypersAndSupers) {
                        Player player = Engine.Scene.Tracker.GetEntity<Player>();

                        float coyote = DynamicData.For(player).Get<float>("jumpGraceTimer");

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
                                    OptionsManager.AllDirectionHypersAndSupersWorkWithCoyoteTime
                                )
                            ) &&
                            (player.Speed.Y <= 0f)
                        ) {
                            if((DynamicData.For(player).Get<float>("dashRefillCooldownTimer") <= 0f && !player.Inventory.NoRefills) || alwaysRefills) {
                                player.RefillDash();
                            }

                            DynamicData.For(player).Invoke("SuperJump");

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

        private void allowTheoClimbjumping(ILCursor cursor) {
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
                        if(!OptionsManager.AllowHoldableClimbjumping) return value;

                        return null;
                    });
                } else {
                    Logger.Error("GooberHelper", "COULDNT ALLOW THEO CLIMBJUMPING; PLEASE CONTACT @ZUCCANIUM");
                }
            }
        }

        private void modifyPlayerRedDashUpdate(ILContext il) { allowTheoClimbjumping(new ILCursor(il)); allowAllDirectionHypersAndSupers(new ILCursor(il), OpCodes.Ldloc_0, true); }
        private void modifyPlayerHitSquashUpdate(ILContext il) { allowTheoClimbjumping(new ILCursor(il)); }
        private void modifyPlayerNormalUpdate(ILContext il) { allowTheoClimbjumping(new ILCursor(il)); }
        private void modifyPlayerDashUpdate(ILContext il) { allowTheoClimbjumping(new ILCursor(il)); allowAllDirectionHypersAndSupers(new ILCursor(il), OpCodes.Ldarg_0, false); }

        private void modifyPlayerPickupCoroutine(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            cursor.TryGotoNext(MoveType.After, instr => instr.MatchStfld(typeof(Player), nameof(Player.Speed)));
            cursor.TryGotoNext(MoveType.After, instr => instr.MatchStfld(typeof(Player), nameof(Player.Speed)));

            cursor.EmitDelegate(() => {
                if(!OptionsManager.PickupSpeedReversal) return;

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
                        return OptionsManager.GoldenBlocksAlwaysLoad ? 1 : value;
                    });
                }
            }
        }

        private void modPlayerSuperJump(On.Celeste.Player.orig_SuperJump orig, Player self) {
            Vector2 origSpeed = self.Speed;
            bool wasDucking = self.Ducking;

            orig(self);

            if(OptionsManager.HyperAndSuperSpeedPreservation) {
                //this exists so that alldirectionsHypersAndSupers can be compatible
                //i dont think it will break anything else                                                                                                                         :cluel:
                float kindaAbsoluteSpeed = origSpeed.Length() == 0 ? DynamicData.For(self).Get<Vector2>("beforeDashSpeed").Length() : origSpeed.Length();
                
                self.Speed.X = (int)self.Facing * Math.Max(Math.Abs(kindaAbsoluteSpeed), Math.Abs(260f * (wasDucking ? 1.25f : 1f))) + DynamicData.For(self).Get<Vector2>("LiftBoost").X;
            }

            if(OptionsManager.AdditiveVerticalJumpSpeed) {
                self.Speed.Y = Math.Min(self.Speed.Y, DynamicData.For(self).Get<float>("varJumpSpeed") + Math.Min(origSpeed.Y, 0));

                DynamicData.For(self).Set("varJumpSpeed", self.Speed.Y);
            }  
        }

        private void modPlayerNormalEnd(On.Celeste.Player.orig_NormalEnd orig, Player self) {
            if(OptionsManager.RemoveNormalEnd) {
                return;
            }

            //make the method not reset retention timer
            if(OptionsManager.WallbounceSpeedPreservation && self.StateMachine.State == 2 && DynamicData.For(self).Get<float>("wallSpeedRetentionTimer") > 0) {
                float retentionTimer = DynamicData.For(self).Get<float>("wallSpeedRetentionTimer");

                if(retentionTimer > 0) {
                    orig(self);
                    
                    DynamicData.For(self).Set("wallSpeedRetentionTimer", retentionTimer);
                    
                    return;
                }
            }

            orig(self);
        }

        private void modPlayerDashBegin(On.Celeste.Player.orig_DashBegin orig, Player self) {
            orig(self);

            Vector2 beforeDashSpeed = DynamicData.For(self).Get<Vector2>("beforeDashSpeed");
            float wallSpeedRetained = DynamicData.For(self).Get<float>("wallSpeedRetained");

            if(
                OptionsManager.WallbounceSpeedPreservation &&
                DynamicData.For(self).Get<float>("wallSpeedRetentionTimer") > 0 &&
                Math.Abs(wallSpeedRetained) > Math.Abs(beforeDashSpeed.X)
            ) {
                DynamicData.For(self).Set("beforeDashSpeed", new Vector2(wallSpeedRetained, beforeDashSpeed.Y));
                DynamicData.For(self).Set("wallSpeedRetentionTimer", 0f);
            }
        }

        private void modCrystalStaticSpinnerOnPlayer(On.Celeste.CrystalStaticSpinner.orig_OnPlayer orig, CrystalStaticSpinner self, Player player) {
            if(OptionsManager.AlwaysExplodeSpinners) {
                self.Destroy();

                return;
            }

            orig(self, player);
        }

        private void modPlayerCtor(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode) {
            orig(self, position, spriteMode);

            GooberFlingBird.CustomStateId = self.StateMachine.AddState("GooberFlingBird", new Func<int>(GooberFlingBird.CustomStateUpdate), null, null, null);
            self.Add(new GooberPlayerExtensions());
        }

        private bool modPlayerWallJumpCheck(On.Celeste.Player.orig_WallJumpCheck orig, Player self, int dir) {
            if(self.CollideCheck<Water>() && OptionsManager.CustomSwimming) {
                return false;
            }

            return orig(self, dir);
        }

        private void modPlayerSwimBegin(On.Celeste.Player.orig_SwimBegin orig, Player self) {
            orig(self);
            
            if(self.Speed.Y > 0 && OptionsManager.CustomSwimming) {
                self.Speed.Y *= 2f;

                GooberPlayerExtensions.Instance.WaterRetentionSpeed = 0f;
            }
        }

        private void modifyPlayerDashCoroutine(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchStloc(3))) {
                cursor.EmitDelegate(() => {
                    if(OptionsManager.ReverseDashSpeedPreservation) {
                        Player player = Engine.Scene.Tracker.GetEntity<Player>();

                        Vector2 vector = DynamicData.For(player).Get<Vector2>("lastAim");
                        if (player.OverrideDashDirection != null)
                        {
                            vector = player.OverrideDashDirection.Value;
                        }
                        vector = DynamicData.For(player).Invoke<Vector2>("CorrectDashPrecision", vector);

                        if(vector.X != 0) {
                            Vector2 beforeDashSpeed = DynamicData.For(player).Get<Vector2>("beforeDashSpeed");
                            beforeDashSpeed.X = Math.Sign(vector.X) * Math.Abs(beforeDashSpeed.X);
                            DynamicData.For(player).Set("beforeDashSpeed", beforeDashSpeed);
                        }
                    }
                });
            }

            if(cursor.TryGotoNextBestFit(MoveType.After, 
                instr => instr.MatchLdflda<Player>("DashDir"),
                instr => instr.MatchLdfld<Vector2>("Y"),
                instr => instr.MatchLdcR4(0),
                instr => instr.OpCode == OpCodes.Bgt_Un_S
            )) {
                cursor.Index--;

                cursor.EmitDelegate((float value) => {
                    Player player = Engine.Scene.Tracker.GetEntity<Player>();

                    return (
                        (OptionsManager.CustomSwimming && player.CollideCheck<Water>()) ||
                        OptionsManager.DashesDontResetSpeed
                    ) ? -100f : value; //-100f is an arbitrary value
                });
            }
        }

        private void modPlayerCallDashEvents(On.Celeste.Player.orig_CallDashEvents orig, Player self) {
            if(OptionsManager.CustomSwimming && self.CollideCheck<Water>() && self.StateMachine.State == 2) {
                self.Speed /= 0.75f;
            }

            if(OptionsManager.VerticalDashSpeedPreservation && self.StateMachine.State == 2) {
                DynamicData data = DynamicData.For(self);

                float beforeDashSpeedY = data.Get<Vector2>("beforeDashSpeed").Y;
                Vector2 vector2 = data.Invoke<Vector2>("CorrectDashPrecision", data.Get<Vector2>("lastAim")) * 240f;

                if(vector2.Y != 0 && OptionsManager.ReverseDashSpeedPreservation) {
                    beforeDashSpeedY = Math.Sign(vector2.Y) * Math.Abs(beforeDashSpeedY);
                }

                if (Math.Sign(beforeDashSpeedY) == Math.Sign(vector2.Y) && Math.Abs(beforeDashSpeedY) > Math.Abs(vector2.Y))
                {
                    self.Speed.Y = beforeDashSpeedY;
                }
            }
            
            orig(self);
        }

        private int modPlayerSwimUpdate(On.Celeste.Player.orig_SwimUpdate orig, Player self) {
            if(!OptionsManager.CustomSwimming) return orig(self);

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

            DynamicData.For(self).Set("wallSpeedRetentionTimer", 0.0f);

            GooberPlayerExtensions c = GooberPlayerExtensions.Instance;

            if(self.CollideCheck<Solid>(self.Position)) {
                if(c.WaterRetentionSpeed > 0 && c.WaterRetentionTimer > 0) {
                    if(Vector2.Dot(self.Speed, c.WaterRetentionDirection) < 0) {
                        c.WaterRetentionTimer = 0;
                        c.WaterRetentionDirection = Vector2.Zero;
                    } else {
                        self.Speed = vector * c.WaterRetentionSpeed;

                        c.WaterRetentionSpeed = 0f;
                    }
                }
            }

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

                float redirectSpeed = Math.Max(self.Speed.Length(), c.WaterRetentionSpeed) + 20;

                // Console.WriteLine(customWaterRetentionTimer);
                // Console.WriteLine(redirectSpeed);
                // Console.WriteLine(customWaterRetentionDirection);

                if(c.WaterRetentionTimer <= 0) {
                    redirectSpeed = 0;
                }

                Vector2 v = c.WaterRetentionDirection * -1;

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
            if(level.Entities.FindFirst<GooberIconThing>() == null) {
                level.Add(new GooberIconThing());
            }

            if(level.Entities.FindFirst<GooberSettingsList>() == null) {
                level.Add(new GooberSettingsList());
            }

            orig(level, playerIntro, isFromLoader);
        }

        private Player modBounceBlockWindUpPlayerCheck(On.Celeste.BounceBlock.orig_WindUpPlayerCheck orig, BounceBlock self) {
            if(!OptionsManager.AlwaysActivateCoreBlocks) {
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

            if(OptionsManager.BadelineBossSpeedReversing) {
                self.Speed.X = dir * Math.Max(Math.Abs(self.Speed.X), GooberPlayerExtensions.Instance.AttractSpeedPreserved.Length());
            }
        }

        private Vector2 modPlayerExplodeLaunch(On.Celeste.Player.orig_ExplodeLaunch_Vector2_bool_bool orig, Player self, Vector2 from, bool snapUp, bool sidesOnly) {
            if(!OptionsManager.ExplodeLaunchSpeedPreservation) {
                return orig(self, from, snapUp, sidesOnly);
            }

            Vector2 originalSpeed = self.Speed;
            Vector2 returnValue = orig(self, from, snapUp, sidesOnly);

            self.Speed.X = Math.Sign(self.Speed.X) * Math.Max(Math.Abs(originalSpeed.X) * (Input.MoveX.Value == Math.Sign(self.Speed.X) ? 1.2f : 1f), Math.Abs(self.Speed.X)); 

            if (Input.MoveX.Value != Math.Sign(self.Speed.X)) {
                DynamicData.For(self).Set("explodeLaunchBoostSpeed", self.Speed.X * 1.2f);
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
                    return OptionsManager.CustomFeathers ? -1f : value;
                });
            }

            if(cursor.TryGotoNextBestFit(MoveType.After, instr => instr.MatchLdcR4(0.06f))) {
                cursor.EmitDelegate((float value) => {
                    float newTime = OptionsManager.RetentionFrames;

                    return newTime != -1 ? newTime / 60f : value;
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
                    return OptionsManager.CustomFeathers ? -1f : value;
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
                        return OptionsManager.CustomFeathers ?
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
                        if(OptionsManager.FeatherEndSpeedPreservation) {
                            Player player = Engine.Scene.Tracker.GetEntity<Player>();

                            //free feather end boosts
                            if(player.Speed.Y <= 0f) {
                                DynamicData.For(player).Set("varJumpSpeed", player.Speed.Y);
                                player.AutoJump = true;
                                player.AutoJumpTimer = 0f;
                                DynamicData.For(player).Set("varJumpTimer", 0.2f);
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
                    if(!OptionsManager.CustomFeathers) return false;

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
            //     cursor.EmitDelegate(() => { return OptionsManager.CustomFeathers; });
            //     cursor.Emit(OpCodes.Brtrue_S, afterStarFlyStartLabel);
            // }
            
            if(cursor.TryGotoNextBestFit(MoveType.After,
                instr => instr.MatchLdcR4(250),
                instr => instr.OpCode == OpCodes.Call,
                instr => instr.OpCode == OpCodes.Stfld
            )) {
                cursor.EmitDelegate(() => {
                    if(OptionsManager.CustomFeathers) {
                        Player player = Engine.Scene.Tracker.GetEntity<Player>();

                        player.Speed = GooberPlayerExtensions.Instance.StarFlySpeedPreserved.SafeNormalize() * Math.Max(GooberPlayerExtensions.Instance.StarFlySpeedPreserved.Length(), 250);
                    }
                });
            }
        }

        private void modPlayerClimbJump(On.Celeste.Player.orig_ClimbJump orig, Player self) {
            float originalSpeedY = self.Speed.Y;

            int beforeJumpCount = SaveData.Instance.TotalJumps;

            orig(self);

            //the method didnt run; dont do anything else
            if(beforeJumpCount == SaveData.Instance.TotalJumps) return;

            if(OptionsManager.WallBoostDirectionBasedOnOppositeSpeed) {
                if(Input.MoveX == 0) {
                    DynamicData.For(self).Set("wallBoostDir", Math.Sign(-self.Speed.X));
                }
            }

            //dont do it with additive vertical jump speed because that already modifies jump
            if(
                !OptionsManager.AdditiveVerticalJumpSpeed &&
                originalSpeedY < -240f && OptionsManager.VerticalDashSpeedPreservation
            ) {
                self.Speed.Y = originalSpeedY + self.LiftSpeed.Y;
                DynamicData.For(self).Set("varJumpSpeed", self.Speed.Y);
            }

            if(DynamicData.For(self).Get<float>("wallSpeedRetentionTimer") > 0.0 && OptionsManager.GetClimbJumpSpeedInRetainedFrames) {
                float retainedSpeed = DynamicData.For(self).Get<float>("wallSpeedRetained");

                DynamicData.For(self).Set("wallSpeedRetained", retainedSpeed + (float)DynamicData.For(self).Get<int>("moveX") * 40f);

                /*
                //this feature can be used to cheat without being noticed easily so i need to make a watermark
                DynamicData.For(self).Get<Sprite>("sweatSprite").SetColor(Color.BlueViolet);

                resetSweatSpriteTimer = 0.45f;
                */
            }
        }

        private void modPlayerWallJump(On.Celeste.Player.orig_WallJump orig, Player self, int dir) {
            Vector2 originalSpeed = self.Speed;

            int beforeJumpCount = SaveData.Instance.TotalWallJumps;

            orig(self, dir);

            //the method didnt run; dont do anything else
            if(beforeJumpCount == SaveData.Instance.TotalWallJumps) return;

            if(OptionsManager.AdditiveVerticalJumpSpeed) {
                self.Speed.Y = Math.Min(self.Speed.Y, DynamicData.For(self).Get<float>("varJumpSpeed") + Math.Min(originalSpeed.Y, 0));

                DynamicData.For(self).Set("varJumpSpeed", self.Speed.Y);
            } else if(originalSpeed.Y < -240f && OptionsManager.VerticalDashSpeedPreservation) {
                self.Speed.Y = originalSpeed.Y + self.LiftSpeed.Y;
                DynamicData.For(self).Set("varJumpSpeed", self.Speed.Y);
            }

            if(OptionsManager.WallJumpSpeedInversion) {
                self.Speed.X = Math.Sign(self.Speed.X) * Math.Max(
                    Math.Max(
                        Math.Abs(self.Speed.X),
                        Math.Abs(originalSpeed.X)
                    ),
                    Math.Abs(DynamicData.For(self).Get<float>("wallSpeedRetained")) * (DynamicData.For(self).Get<float>("wallSpeedRetentionTimer") > 0f ? 1f : 0f)
                );
            } else if(Math.Sign(self.Speed.X - self.LiftSpeed.X) == Math.Sign(originalSpeed.X) && OptionsManager.WallJumpSpeedPreservation) {
                self.Speed.X = Math.Sign(originalSpeed.X) * Math.Max(Math.Abs(self.Speed.X), Math.Abs(originalSpeed.X) - (Input.MoveX == 0 ? 0.0f : 40.0f)) + self.LiftSpeed.X;
            }
        }

        private void modPlayerDreamDashBegin(On.Celeste.Player.orig_DreamDashBegin orig, Player self) {
            Vector2 originalSpeed = self.Speed;
            Vector2 intendedSpeed = self.DashDir * 240f;

            orig(self);

            if(OptionsManager.DreamBlockSpeedPreservation) {
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

            if(OptionsManager.AdditiveVerticalJumpSpeed) {
                self.Speed.Y = Math.Min(self.Speed.Y, DynamicData.For(self).Get<float>("varJumpSpeed") + Math.Min(originalSpeedY, 0));

                DynamicData.For(self).Set("varJumpSpeed", self.Speed.Y);
            } else if(originalSpeedY < -240f && OptionsManager.VerticalDashSpeedPreservation) {
                self.Speed.Y = originalSpeedY + self.LiftSpeed.Y;
                DynamicData.For(self).Set("varJumpSpeed", self.Speed.Y);
            }

            if(!OptionsManager.WallbounceSpeedPreservation) {
                return;
            }


            float absoluteBeforeDashSpeed = Math.Abs(DynamicData.For(self).Get<Vector2>("beforeDashSpeed").X);
            float absoluteRetainedSpeed = Math.Abs(DynamicData.For(self).Get<float>("wallSpeedRetained"));

            if(DynamicData.For(self).Get<float>("wallSpeedRetentionTimer") <= 0) {
                absoluteRetainedSpeed = 0;
            }

            self.Speed.X = dir * Math.Max(Math.Max(absoluteBeforeDashSpeed, absoluteRetainedSpeed) + DynamicData.For(self).Get<Vector2>("LiftBoost").X, Math.Abs(self.Speed.X));

            return;
        }

        private void modCelesteFreeze(On.Celeste.Celeste.orig_Freeze orig, float time) {
            int newTime = OptionsManager.RefillFreezeLength;
            
            //as long as all refill freeze freezeframe callers have "refillroutine" in their names and nothing else then this should work
            if(newTime != -1 && refillRoutineRegex.IsMatch(new System.Diagnostics.StackTrace().ToString())) {
                time = newTime / 60f;
            }

            orig(time);
        }

        private void modPlayerPointBounce(On.Celeste.Player.orig_PointBounce orig, Player self, Vector2 from) {
            if(!OptionsManager.ReboundInversion) {
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
            if(!OptionsManager.ReboundInversion) {
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
            if(!OptionsManager.ReboundInversion || direction.X == 0) {
                orig(self, direction);

                return;
            }

            float originalSpeed = self.Speed.X;

            orig(self, direction);

            doSpeedReverseStuff(originalSpeed, self, 220);
        }

        private bool modPlayerSideBounce(On.Celeste.Player.orig_SideBounce orig, Player self, int dir, float fromX, float fromY) {
            if(!OptionsManager.SpringSpeedPreservation) {
                return orig(self, dir, fromX, fromY);
            }

            float originalSpeed = self.Speed.X;

            bool res = orig(self, dir, fromX, fromY);

            doSpeedReverseStuff(originalSpeed, self, 240, dir);

            return res;
        }

        private void modPlayerSuperBounce(On.Celeste.Player.orig_SuperBounce orig, Player self, float fromY) {
            if(!OptionsManager.SpringSpeedPreservation) {
                orig(self, fromY);

                return;
            }

            float originalSpeed = self.Speed.X;

            orig(self, fromY);

            self.Speed.X = Math.Abs(originalSpeed) * Input.MoveX;
            if(self.Speed.X == 0) self.Speed.X = Math.Abs(originalSpeed); //in case movex is 0
        }

        private void modPlayerOnCollideH(On.Celeste.Player.orig_OnCollideH orig, Player self, CollisionData data) {
            if(!(OptionsManager.KeepDashAttackOnCollision || OptionsManager.CustomSwimming)) {
                orig(self, data);

                return;
            };

            if(OptionsManager.CustomSwimming && self.CollideCheck<Water>()) {
                GooberPlayerExtensions c = GooberPlayerExtensions.Instance;

                if(c.WaterRetentionTimer <= 0.0f) {
                    c.WaterRetentionSpeed = self.Speed.Length();
                    c.WaterRetentionTimer = 0.06f;

                    c.WaterRetentionDirection = new Vector2(self.CollideCheck<Solid>(self.Position + Vector2.UnitX * -1) ? -1 : 1, c.WaterRetentionDirection.Y);
                }
            }
            
            float originalDashAttack = DynamicData.For(self).Get<float>("dashAttackTimer");

            orig(self, data);

            if(OptionsManager.KeepDashAttackOnCollision) {
                DynamicData.For(self).Set("dashAttackTimer", originalDashAttack);
            }
        }

        private void modPlayerOnCollideV(On.Celeste.Player.orig_OnCollideV orig, Player self, CollisionData data) {
            if(!(OptionsManager.KeepDashAttackOnCollision || OptionsManager.CustomSwimming)) {
                orig(self, data);

                return;
            };

            if(OptionsManager.CustomSwimming && self.CollideCheck<Water>()) {
                GooberPlayerExtensions c = GooberPlayerExtensions.Instance;

                if(c.WaterRetentionTimer <= 0.0f) {
                    c.WaterRetentionSpeed = self.Speed.Length();
                    c.WaterRetentionTimer = 0.06f;

                    c.WaterRetentionDirection = new Vector2(c.WaterRetentionDirection.X, self.CollideCheck<Solid>(self.Position + Vector2.UnitY * -1) ? -1 : 1);
                }
            }

            float originalDashAttack = DynamicData.For(self).Get<float>("dashAttackTimer");

            orig(self, data);

            if(OptionsManager.KeepDashAttackOnCollision) {
                DynamicData.For(self).Set("dashAttackTimer", originalDashAttack);
            }
        }

        private void modPlayerJump(On.Celeste.Player.orig_Jump orig, Player self, bool particles, bool playSfx) {
            bool isClimbjump = particles == false && playSfx == false;
            float originalSpeedY = self.Speed.Y;

            if(OptionsManager.JumpInversion) {
                if (!(isClimbjump && !OptionsManager.AllowClimbJumpInversion)) {
                    int moveX = DynamicData.For(self).Get<int>("moveX");

                    if ((float)moveX == -Math.Sign(self.Speed.X)) {
                        self.Speed.X *= -1;
                    }
                }
            }
            
            int beforeJumpCount = SaveData.Instance.TotalJumps;

            orig(self, particles, playSfx);

            //the method didnt run; dont do anything else
            if(beforeJumpCount == SaveData.Instance.TotalJumps) return;

            if(OptionsManager.AdditiveVerticalJumpSpeed) {
                self.Speed.Y = Math.Min(self.Speed.Y, DynamicData.For(self).Get<float>("varJumpSpeed") + Math.Min(originalSpeedY, 0));

                DynamicData.For(self).Set("varJumpSpeed", self.Speed.Y);
            }
        }

        private void modPlayerUpdate(On.Celeste.Player.orig_Update orig, Player self) {
            // if(Input.MoveX != Math.Sign(self.Speed.X) && Input.MoveX != 0) {
            //     self.Speed.X *= -1;
            // }

            if(OptionsManager.CustomSwimming) {
                GooberPlayerExtensions c = GooberPlayerExtensions.Instance;

                c.WaterRetentionTimer -= Engine.DeltaTime;

                if(c.WaterRetentionTimer <= 0) {
                    c.WaterRetentionDirection = Vector2.Zero;
                }
            }

            orig(self);

            if(OptionsManager.PickupSpeedReversal && self.StateMachine.State == 8) {
                int buh = DynamicData.For(self).Get<int>("moveX");

                self.Facing = buh == 0 ? self.Facing : (Facings)buh;
            }
        }

        private void modifyPlayerUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            float cobwob_originalSpeed = 0;

            //[BEFORE] this.Speed.X = 130f * (float)this.moveX;
            if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(130f))) {
                cursor.EmitDelegate<Func<float, float>>(orig => {
                    if(!OptionsManager.CobwobSpeedInversion) return orig;

                    Player player = Engine.Scene.Tracker.GetEntity<Player>();
                    if (player == null) return orig;

                    cobwob_originalSpeed = player.Speed.X;

                    return orig;
                });
            }

            //[BEFORE] this.Stamina += 27.5f;
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(27.5f))) {
                cursor.EmitDelegate<Func<float, float>>(orig => {
                    if(!OptionsManager.CobwobSpeedInversion && !OptionsManager.WallBoostSpeedIsAlwaysOppositeSpeed) return orig;

                    Player player = Engine.Scene.Tracker.GetEntity<Player>();
                    if (player == null) return orig;

                    float dir = Math.Sign(player.Speed.X);
                    float newAbsoluteSpeed = Math.Max(130f, Math.Abs(cobwob_originalSpeed));

                    if(
                        OptionsManager.WallBoostSpeedIsAlwaysOppositeSpeed &&
                        !OptionsManager.WallBoostDirectionBasedOnOppositeSpeed &&
                        DynamicData.For(player).Get<int>("wallBoostDir") == Math.Sign(cobwob_originalSpeed - 11f * Math.Sign(cobwob_originalSpeed))
                    ) {
                        dir = -Math.Sign(cobwob_originalSpeed);
                    }
                    
                    if(DynamicData.For(player).Get<float>("wallSpeedRetentionTimer") > 0.0 && OptionsManager.AllowRetentionReverse) {
                        float retainedSpeed = DynamicData.For(player).Get<float>("wallSpeedRetained");

                        newAbsoluteSpeed = Math.Max(130f, Math.Abs(retainedSpeed));
                    }

                    player.Speed.X = dir * newAbsoluteSpeed;

                    return orig;
                });
            }
        }
    }
}
