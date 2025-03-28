using System;
using System.Reflection;
using Celeste;
using Celeste.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Monocle;
using Microsoft.Xna.Framework;
using System.Collections;
using System.Text.RegularExpressions;
using System.Diagnostics;
using IL.Celeste;
using Celeste.Mod.GooberHelper.Entities;
using System.Collections.Generic;
using Mono.Cecil;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
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
        private static ILHook playerStarFlyUpdateHook;
        private static ILHook playerOnCollideHHook;
        private static ILHook playerOnCollideVHook;
        private static ILHook playerDashCoroutineHook;
        private static ILHook playerPickupCoroutineHook;
        private static ILHook playerDashUpdateHook;
        private static ILHook playerNormalUpdateHook;
        private static ILHook playerRedDashUpdateHook;
        private static ILHook playerHitSquashUpdateHook;
        private static ILHook playerRedDashCoroutineHook;
        private static ILHook playerDashCoroutineHook2;

        private static Vector2 beforeStarFlySpeed = Vector2.Zero;

        private static float beforeAttractSpeed = 0;

        private static DynamicData selfDyn;
            private static void setSelfDyn() { selfDyn = DynamicData.For(Engine.Scene.Tracker.GetEntity<Player>()); }

        private static float customWaterRetentionSpeed {
            get { if(!selfDyn.TryGet("customWaterRetentionSpeed", out float? value)) selfDyn.Set("customWaterRetentionSpeed", 0f); return value ?? 0f; }
            set { selfDyn.Set("customWaterRetentionSpeed", value); }
        }

        private static float customWaterRetentionTimer {
            get { if(!selfDyn.TryGet("customWaterRetentionTimer", out float? value)) selfDyn.Set("customWaterRetentionTimer", 0f); return value ?? 0f; }
            set { selfDyn.Set("customWaterRetentionTimer", value); }
        }

        private static Vector2 customWaterRetentionDirection {
            get { if(!selfDyn.TryGet("customWaterRetentionDirection", out Vector2? value)) selfDyn.Set("customWaterRetentionDirection", new Vector2(0,0)); return value ?? new Vector2(0,0); }
            set { selfDyn.Set("customWaterRetentionDirection", value); }
        }

        static Effect playerMask = FluidSimulation.TryGetEffect("playerMask");


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
            playerMask = FluidSimulation.TryGetEffect("playerMask");

            playerUpdateHook = new ILHook(typeof(Player).GetMethod("orig_Update"), modifyPlayerUpdate);
            playerStarFlyCoroutineHook = new ILHook(typeof(Player).GetMethod("StarFlyCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), modifyPlayerStarFlyCoroutine);
            playerStarFlyUpdateHook = new ILHook(typeof(Player).GetMethod("StarFlyUpdate", BindingFlags.NonPublic | BindingFlags.Instance), modifyPlayerStarFlyUpdate);
            playerOnCollideHHook = new ILHook(typeof(Player).GetMethod("OnCollideH", BindingFlags.NonPublic | BindingFlags.Instance), modifyPlayerOnCollideH);
            playerOnCollideVHook = new ILHook(typeof(Player).GetMethod("OnCollideV", BindingFlags.NonPublic | BindingFlags.Instance), modifyPlayerOnCollideV);
            playerDashCoroutineHook = new ILHook(typeof(Player).GetMethod("DashCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), modifyPlayerDashCoroutine);
            playerPickupCoroutineHook = new ILHook(typeof(Player).GetMethod("PickupCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), modifyPlayerPickupCoroutine);
            playerDashUpdateHook = new ILHook(typeof(Player).GetMethod("DashUpdate", BindingFlags.NonPublic | BindingFlags.Instance), modifyPlayerDashUpdate);
            playerNormalUpdateHook = new ILHook(typeof(Player).GetMethod("NormalUpdate", BindingFlags.NonPublic | BindingFlags.Instance), modifyPlayerNormalUpdate);
            playerRedDashUpdateHook = new ILHook(typeof(Player).GetMethod("RedDashUpdate", BindingFlags.NonPublic | BindingFlags.Instance), modifyPlayerRedDashAndHitSquashUpdate);
            playerHitSquashUpdateHook = new ILHook(typeof(Player).GetMethod("HitSquashUpdate", BindingFlags.NonPublic | BindingFlags.Instance), modifyPlayerRedDashAndHitSquashUpdate);
            playerRedDashCoroutineHook = new ILHook(typeof(Player).GetMethod("RedDashCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), modifyDashSpeedThing);
            playerDashCoroutineHook2 = new ILHook(typeof(Player).GetMethod("DashCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), modifyDashSpeedThing);

            IL.Celeste.GoldenBlock.Awake += modifyGoldenBlockAwake;

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
            On.Celeste.Player.SwimUpdate += modPlayerSwimUpdate;
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
        }

        public override void Unload() {
            FluidSimulation.Unload();

            playerUpdateHook.Dispose();
            playerStarFlyCoroutineHook.Dispose();
            playerStarFlyUpdateHook.Dispose();
            playerOnCollideHHook.Dispose();
            playerOnCollideVHook.Dispose();
            playerDashCoroutineHook.Dispose();
            playerPickupCoroutineHook.Dispose();
            playerDashUpdateHook.Dispose();
            playerNormalUpdateHook.Dispose();
            playerRedDashUpdateHook.Dispose();
            playerHitSquashUpdateHook.Dispose();
            playerRedDashCoroutineHook.Dispose();
            playerDashCoroutineHook2.Dispose();

            IL.Celeste.GoldenBlock.Awake -= modifyGoldenBlockAwake;

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
            On.Celeste.Player.SwimUpdate -= modPlayerSwimUpdate;
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
        }

        public bool modPlayerSwimCheck(On.Celeste.Player.orig_SwimCheck orig, Player self) {
            if(self.CollideAll<Water>().Any(water => water is Waterfall && (water as Waterfall).nonCollidable)) return false;
            
            return orig(self);
        }

        public bool modPlayerSwimJumpCheck(On.Celeste.Player.orig_SwimJumpCheck orig, Player self) {
            if(self.CollideAll<Water>().Any(water => water is Waterfall && (water as Waterfall).nonCollidable)) return false;
            
            return orig(self);
        }

        public bool modPlayerUnderwaterMusicCheck(On.Celeste.Player.orig_UnderwaterMusicCheck orig, Player self) {
            if(self.CollideAll<Water>().Any(water => water is Waterfall && (water as Waterfall).nonCollidable)) return false;
            
            return orig(self);
        }
        
        public void modPlayerRender(On.Celeste.Player.orig_Render orig, Player self) {
            if(playerMask == null) {
                playerMask = FluidSimulation.TryGetEffect("playerMask");
            }
            
            if(!Settings.Visuals.PlayerMask || playerMask == null) {
                orig(self);

                return;
            }

            GameplayRenderer.End();
            
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, playerMask, (Engine.Scene as Level).Camera.Matrix);
            playerMask.CurrentTechnique = playerMask.Techniques["Grongle"];
            playerMask.Parameters["CamPos"].SetValue((Engine.Scene as Level).Camera.Position);
            playerMask.Parameters["HairColor"].SetValue(new Vector4(self.Hair.Color.ToVector3() * self.Sprite.Color.ToVector3(), 1));
            Engine.Graphics.GraphicsDevice.Textures[1] = GFX.Game["guhcat"].Texture.Texture;

            orig(self);

            Draw.SpriteBatch.End();
            GameplayRenderer.Begin();
        }

        public void modPlayerRedBoost(On.Celeste.Player.orig_RedBoost orig, Player self, Booster booster) {
            DynamicData.For(self).Set("bubbleSpeedPreserved", self.Speed);

            orig(self, booster);
        }

        public void modPlayerBoost(On.Celeste.Player.orig_Boost orig, Player self, Booster booster) {
            DynamicData.For(self).Set("bubbleSpeedPreserved", self.Speed);

            orig(self, booster);
        }

        public void modifyDashSpeedThing(ILContext context) {
            ILCursor cursor = new ILCursor(context);

            if(cursor.TryGotoNext(MoveType.After, instr => instr.Match(OpCodes.Ldc_R4, 240f))) {
                // cursor.Emit(OpCodes.Pop); 
                // cursor.Emit()
                cursor.EmitDelegate((float v) => {
                    float value = v;
                    if(!(Session.BubbleSpeedPreservation || Settings.Physics.BubbleSpeedPreservation)) return value;

                    Player player = Engine.Scene.Tracker.GetEntity<Player>();

                    try {
                        value = Math.Max(DynamicData.For(player).Get<Vector2>("bubbleSpeedPreserved").Length(), v);
                    } catch {}

                    DynamicData.For(player).Set("bubbleSpeedPreserved", Vector2.Zero);

                    return value;
                });
            }
        }

        public void modPlayerBeforeUpTransition(On.Celeste.Player.orig_BeforeUpTransition orig, Player self) {
            float varJumpTimer = DynamicData.For(self).Get<float>("varJumpTimer");
            float varJumpSpeed = DynamicData.For(self).Get<float>("varJumpSpeed");
            Vector2 speed = self.Speed;
            float dashCooldownTimer = DynamicData.For(self).Get<float>("dashCooldownTimer");

            orig(self);

            if(!(Settings.Physics.KeepSpeedThroughVerticalTransitions || Session.KeepSpeedThroughVerticalTransitions)) {
                return;
            }

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

        public void allowTheoClimbjumping(ILCursor cursor, int skipCount) {
            for(int i = 0; i < skipCount; i++) {
                cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldarg_0, instr => instr.OpCode == OpCodes.Callvirt, instr => instr.OpCode == OpCodes.Brtrue_S);
            }

            for(int i = 0; i < 2; i++) {
                if(
                    cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldarg_0, instr => instr.OpCode == OpCodes.Callvirt, instr => instr.OpCode == OpCodes.Brtrue_S)
                ) {
                    cursor.Index--;
                    // cursor.EmitDelegate<Func<Holdable, Holdable>>((Holdable h) => {
                    //     Player player = Engine.Scene.Tracker.GetEntity<Player>();

                    //     return null;

                    //     // if((Settings.Physics.AllowHoldableClimbjumping || Session.AllowHoldableClimbjumping) && !player.CollideCheck<EnforceNormalHoldableClimbjumps>()) {
                    //     //     return null;
                    //     // } else {
                    //     //     return player.Holding;
                    //     // }
                    // });
                    cursor.Emit(OpCodes.Pop);
                    cursor.EmitDelegate(() => {
                        Player player = Engine.Scene.Tracker.GetEntity<Player>();

                        if((Settings.Physics.AllowHoldableClimbjumping || Session.AllowHoldableClimbjumping) && !player.CollideCheck<EnforceNormalHoldableClimbjumps>()) {
                            return false;
                        } else {
                            return player.Holding != null;
                        }
                    });
                }
            }
        }

        public void modifyPlayerRedDashAndHitSquashUpdate(ILContext il) { allowTheoClimbjumping(new ILCursor(il), 0); }
        public void modifyPlayerNormalUpdate(ILContext il) { allowTheoClimbjumping(new ILCursor(il), 2); }
        public void modifyPlayerDashUpdate(ILContext il) { allowTheoClimbjumping(new ILCursor(il), 1); }

        public void modifyPlayerPickupCoroutine(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            cursor.TryGotoNext(MoveType.After, instr => instr.MatchStfld(typeof(Player), nameof(Player.Speed)));
            cursor.TryGotoNext(MoveType.After, instr => instr.MatchStfld(typeof(Player), nameof(Player.Speed)));

            cursor.EmitDelegate(() => {
                Player player = Engine.Scene.Tracker.GetEntity<Player>();

                if(-Math.Sign(player.Speed.X) == (int)Input.MoveX && (Settings.Physics.PickupSpeedReversal || Session.PickupSpeedReversal)) {
                    player.Speed.X *= -1;
                }
            });
        }

        public void modifyGoldenBlockAwake(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if(cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Stloc_0)) {
                cursor.Index--;

                cursor.Emit(OpCodes.Pop);

                cursor.EmitDelegate(() => {
                    return Settings.Miscellaneous.GoldenBlocksAlwaysLoad;
                });
            }
        }

        private void modPlayerSuperJump(On.Celeste.Player.orig_SuperJump orig, Player self) {
            float origSpeed = self.Speed.X;
            bool wasDucking = self.Ducking;

            orig(self);

            if(!(Settings.Physics.HyperAndSuperSpeedPreservation || Session.HyperAndSuperSpeedPreservation)) {
                return;
            }

            self.Speed.X = (int)self.Facing * Math.Max(Math.Abs(origSpeed), Math.Abs(260f * (wasDucking ? 1.25f : 1f))) + DynamicData.For(self).Get<Vector2>("LiftBoost").X;
        }

        private void modPlayerNormalEnd(On.Celeste.Player.orig_NormalEnd orig, Player self) {
            if (Settings.Physics.RemoveNormalEnd || Session.RemoveNormalEnd) {
                return;
            }

            if((Settings.Physics.WallbounceSpeedPreservation || Session.WallbounceSpeedPreservation) && self.StateMachine.State == 2 && DynamicData.For(self).Get<float>("wallSpeedRetentionTimer") > 0) {
                DynamicData.For(self).Set("wallBoostTimer", 0f);
                DynamicData.For(self).Set("hopWaitX", 0f);

                return;
            }

            orig(self);
        }

        private void modPlayerDashBegin(On.Celeste.Player.orig_DashBegin orig, Player self) {
            orig(self);

            Vector2 beforeDashSpeed = DynamicData.For(self).Get<Vector2>("beforeDashSpeed");
            float wallSpeedRetained = DynamicData.For(self).Get<float>("wallSpeedRetained");

            if((Settings.Physics.WallbounceSpeedPreservation || Session.WallbounceSpeedPreservation) && DynamicData.For(self).Get<float>("wallSpeedRetentionTimer") > 0 && Math.Abs(wallSpeedRetained) > Math.Abs(beforeDashSpeed.X)) {
                DynamicData.For(self).Set("beforeDashSpeed", new Vector2(wallSpeedRetained, beforeDashSpeed.Y));
                DynamicData.For(self).Set("wallSpeedRetentionTimer", 0f);
            }
        }

        private void modCrystalStaticSpinnerOnPlayer(On.Celeste.CrystalStaticSpinner.orig_OnPlayer orig, CrystalStaticSpinner self, Player player) {
            if(Settings.Miscellaneous.AlwaysExplodeSpinners || Session.AlwaysExplodeSpinners) {
                self.Destroy();

                return;
            }

            orig(self, player);
        }

        private void modPlayerCtor(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode) {
            orig(self, position, spriteMode);

            GooberFlingBird.CustomStateId = self.StateMachine.AddState("GooberFlingBird", new Func<int>(GooberFlingBird.CustomStateUpdate), null, null, null);
        }

        private bool modPlayerWallJumpCheck(On.Celeste.Player.orig_WallJumpCheck orig, Player self, int dir) {
            if(self.CollideCheck<Water>() && (Settings.Physics.CustomSwimming || Session.CustomSwimming)) {
                return false;
            }

            return orig(self, dir);
        }

        private void modPlayerSwimBegin(On.Celeste.Player.orig_SwimBegin orig, Player self) {
            orig(self);
            
            if(self.Speed.Y > 0 && (Settings.Physics.CustomSwimming || Session.CustomSwimming)) {
                self.Speed.Y *= 2f;

                setSelfDyn();

                // DynamicData.For(self).Set("customWaterRetentionSpeed", 0.0f);
                customWaterRetentionSpeed = 0f;
            }
        }

        private void modifyPlayerDashCoroutine(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchStloc(3))) {
                cursor.EmitDelegate(() => {
                    if(Settings.Physics.ReverseDashSpeedPreservation || Session.ReverseDashSpeedPreservation) {
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

            if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchStfld<Player>("AutoJump"))) {
                for(int i = 0; i < 2; i++) {
                    cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(0.0f));
                }

                cursor.Emit(OpCodes.Pop);
                cursor.EmitDelegate(() => {
                    Player player = Engine.Scene.Tracker.GetEntity<Player>();

                    return ((Settings.Physics.CustomSwimming || Session.CustomSwimming) && player.CollideCheck<Water>()) || (Settings.Physics.DashesDontResetSpeed || Session.DashesDontResetSpeed) ? -100f : 0;
                });
            }
        }

        private void modPlayerCallDashEvents(On.Celeste.Player.orig_CallDashEvents orig, Player self) {
            if((Settings.Physics.CustomSwimming || Session.CustomSwimming) && self.CollideCheck<Water>() && self.StateMachine.State == 2) {
                self.Speed /= 0.75f;
            }

            if((Settings.Physics.VerticalDashSpeedPreservation || Session.VerticalDashSpeedPreservation) && self.StateMachine.State == 2) {
                DynamicData data = DynamicData.For(self);

                float beforeDashSpeedY = data.Get<Vector2>("beforeDashSpeed").Y;
                Vector2 vector2 = data.Invoke<Vector2>("CorrectDashPrecision", data.Get<Vector2>("lastAim")) * 240f;

                if(vector2.Y != 0 && (Settings.Physics.ReverseDashSpeedPreservation || Session.ReverseDashSpeedPreservation)) {
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
            if(!(Settings.Physics.CustomSwimming || Session.CustomSwimming)) return orig(self);

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

            if(self.CollideCheck<Solid>(self.Position)) {
                // if(!DynamicData.For(self).TryGet("customWaterRetentionSpeed", out float customWaterRetentionSpeed)) DynamicData.For(self).Set("customWaterRetentionSpeed", 0f);
                // if(!DynamicData.For(self).TryGet("customWaterRetentionTimer", out float customWaterRetentionTimer)) DynamicData.For(self).Set("customRetentionTimer", 0f);
                // if(!DynamicData.For(self).TryGet("customWaterRetentionDirection", out Vector2 customWaterRetentionDirection)) DynamicData.For(self).Set("customRetentionDirection", new Vector2(0,0));

                setSelfDyn();

                if(customWaterRetentionSpeed > 0 && customWaterRetentionTimer > 0) {
                    if(Vector2.Dot(self.Speed, customWaterRetentionDirection) < 0) {
                        // DynamicData.For(self).Set("customWaterRetentionTimer", 0f);
                        customWaterRetentionTimer = 0;
                    } else {
                        // self.Speed = vector * DynamicData.For(self).Get<float>("customWaterRetentionSpeed");
                        self.Speed = vector * customWaterRetentionSpeed;

                        // DynamicData.For(self).Set("customWaterRetentionSpeed", 0f);
                        customWaterRetentionSpeed = 0f;
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

                //maybe make them go on a cardinal direction if you have 2 walls or a floor and ceiling

                // float redirectSpeed = Math.Max(self.Speed.Length(), DynamicData.For(self).Get<float>("customWaterRetentionSpeed")) + 20;
                float redirectSpeed = Math.Max(self.Speed.Length(), customWaterRetentionSpeed) + 20;

                // if(DynamicData.For(self).Get<float>("customWaterRetentionTimer") <= 0) {
                if(customWaterRetentionTimer <= 0) {
                    redirectSpeed = 0;
                }

                // Vector2 v = (
                //     self.CollideCheck<Solid>(self.Position + Vector2.UnitX * -1) ? new Vector2(1, vector.Y) :
                //     self.CollideCheck<Solid>(self.Position + Vector2.UnitX * 1) ? new Vector2(-1, vector.Y) :
                //     self.CollideCheck<Solid>(self.Position + Vector2.UnitY * -1) ? new Vector2(vector.X, 1) :
                //     self.CollideCheck<Solid>(self.Position + Vector2.UnitY * 1) ? new Vector2(vector.X, -1) : Vector2.Zero
                // );

                
                // Vector2 v = DynamicData.For(self).Get<Vector2>("customWaterRetentionDirection") * -1;
                Vector2 v = customWaterRetentionDirection * -1;

                // if(self.CollideCheck<Solid>(self.Position + Vector2.UnitX * -1)) {
                //     self.Speed = new Vector2(1, vector.Y).SafeNormalize() * redirectSpeed;

                //     jumped = true;
                // }

                // if(self.CollideCheck<Solid>(self.Position + Vector2.UnitX * 1)) {
                //     self.Speed = new Vector2(-1, vector.Y).SafeNormalize() * redirectSpeed;

                //     jumped = true;
                // }

                // if(self.CollideCheck<Solid>(self.Position + Vector2.UnitY * 1)) {
                //     self.Speed = new Vector2(vector.X, -1).SafeNormalize() * redirectSpeed;

                //     jumped = true;
                // }

                // if(self.CollideCheck<Solid>(self.Position + Vector2.UnitY * -1)) {
                //     self.Speed = new Vector2(vector.X, 1).SafeNormalize() * redirectSpeed;

                //     jumped = true;
                // }

                if(v != Vector2.Zero && redirectSpeed != 0) {
                    Input.Jump.ConsumeBuffer();

                    self.Speed = v.SafeNormalize() * redirectSpeed;
                }

                // Logger.Log(LogLevel.Info, "left", );
                // Logger.Log(LogLevel.Info, "right", self.CollideCheck<Solid>(self.Position + Vector2.UnitX * 1).ToString());
                // Logger.Log(LogLevel.Info, "down", self.CollideCheck<Solid>(self.Position + Vector2.UnitY * 1).ToString());
                // Logger.Log(LogLevel.Info, "up", self.CollideCheck<Solid>(self.Position + Vector2.UnitY * -1).ToString());
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
            if(!(Settings.Physics.AlwaysActivateCoreBlocks || Session.AlwaysActivateCoreBlocks)) {
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
            beforeAttractSpeed = self.Speed.X;

            orig(self);
        }

        private void modPlayerFinalBossPushLaunch(On.Celeste.Player.orig_FinalBossPushLaunch orig, Player self, int dir) {
            orig(self, dir);

            if(Settings.Physics.BadelineBossSpeedReversing || Session.BadelineBossSpeedReversing) {
                self.Speed.X = dir * Math.Max(Math.Abs(self.Speed.X), Math.Abs(beforeAttractSpeed));
            }
        }

        private Vector2 modPlayerExplodeLaunch(On.Celeste.Player.orig_ExplodeLaunch_Vector2_bool_bool orig, Player self, Vector2 from, bool snapUp, bool sidesOnly) {
            if(!(Settings.Physics.ExplodeLaunchSpeedPreservation || Session.ExplodeLaunchSpeedPreservation)) {
                return orig(self, from, snapUp, sidesOnly);
            }

            Vector2 originalSpeed = self.Speed;

            Vector2 returnValue = orig(self, from, snapUp, sidesOnly);

            self.Speed.X = Math.Sign(self.Speed.X) * Math.Max(Math.Abs(originalSpeed.X) * (Input.MoveX.Value == Math.Sign(self.Speed.X) ? 1.2f : 1f), Math.Abs(self.Speed.X)); 
            
            // // if(Input.MoveX.Value == Math.Sign(self.Speed.X)) {
            //     DynamicData.For(self).Set("explodeLaunchBoostSpeed", self.Speed.X);
            //     // DynamicData.For(self).Set("explodeLaunchBoostTimer", 0f);
            // // }

            if (Input.MoveX.Value != Math.Sign(self.Speed.X)) {
                DynamicData.For(self).Set("explodeLaunchBoostSpeed", self.Speed.X * 1.2f);
            }

            if((Engine.Scene as Level).Session.Area.SID == "alex21/Dashless+/1A Dashless but Spikier" && (Engine.Scene as Level).Session.Level == "b-06") {
                self.Speed.X = 0;
                self.Speed.Y = -330;
            }

            return returnValue;
        }

        private void modifyPlayerOnCollideH(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-0.5f))) {
                cursor.Emit(OpCodes.Pop);
                cursor.EmitDelegate(() => {return (Settings.Physics.CustomFeathers || Session.CustomFeathers) ? -1f : -0.5f;});
            }

            if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(0.06f))) {
                cursor.Emit(OpCodes.Pop);
                cursor.EmitDelegate(() => {
                    if(Session.RetentionFrames != -1) return Session.RetentionFrames / 60f;
                    if(Settings.Physics.RetentionFrames != -1) return Settings.Physics.RetentionFrames / 60f;

                    return 0.06f;
                });
            }
        }
        
        private void modifyPlayerOnCollideV(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-0.5f))) {
                cursor.Emit(OpCodes.Pop);
                cursor.EmitDelegate(() => {return (Settings.Physics.CustomFeathers || Session.CustomFeathers) ? -1f : -0.5f;});
            }
        }

        private void modPlayerStarFlyBegin(On.Celeste.Player.orig_StarFlyBegin orig, Player self) {
            beforeStarFlySpeed = self.Speed;

            orig(self);

            //DynamicData.For(self).Set("starFlyTransforming", false);
        }

        private void modifyPlayerStarFlyUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            float lowMult = 0.65f;
            float midMult = 0.90f;
            float highMult = 1.05f;

            //IM SO FUCKING SORRY

            if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(91))) {
                cursor.Emit(OpCodes.Pop);
                cursor.EmitDelegate(() => {return (Settings.Physics.CustomFeathers || Session.CustomFeathers) ? Math.Max(beforeStarFlySpeed.Length() * lowMult, 91) : 91;});
            }

            if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(140))) {
                cursor.Emit(OpCodes.Pop);
                cursor.EmitDelegate(() => {return (Settings.Physics.CustomFeathers || Session.CustomFeathers) ? Math.Max(beforeStarFlySpeed.Length() * midMult, 140) : 140;});
            }

            if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(190))) {
                cursor.Emit(OpCodes.Pop);
                cursor.EmitDelegate(() => {return (Settings.Physics.CustomFeathers || Session.CustomFeathers) ? Math.Max(beforeStarFlySpeed.Length() * highMult, 190) : 190;});
            }

            if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(140))) {
                cursor.Emit(OpCodes.Pop);
                cursor.EmitDelegate(() => {return (Settings.Physics.CustomFeathers || Session.CustomFeathers) ? Math.Max(beforeStarFlySpeed.Length() * midMult, 140) : 140;});
            }

            // if(cursor.TryGotoNext(MoveType.Before, 
            //     instr => instr.Match(OpCodes.Ldc_I4_M1)
            // )) {
            //     cursor.Index += 0;

            //     cursor.EmitDelegate(() => {
            //         Player player = Engine.Scene.Tracker.GetEntity<Player>();

            //         if(Settings.Physics.CustomFeathers && player.CanUnDuck && player.Facing == Facings.Left && Input.GrabCheck && !SaveData.Instance.Assists.NoGrabbing && player.Stamina > 0f && !ClimbBlocker.Check(player.Scene, player, player.Position + Vector2.UnitX * -3f)) {
            //             DynamicData.For(player).Invoke("ClimbJump");
            //         }
            //     });
            // }

            if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(140))) {
                cursor.Emit(OpCodes.Pop);
                cursor.EmitDelegate(() => {return (Settings.Physics.CustomFeathers || Session.CustomFeathers) ? Math.Max(beforeStarFlySpeed.Length() * midMult, 140) : 140;});
            }
            
            if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(140))) {
                cursor.Emit(OpCodes.Pop);
                cursor.EmitDelegate(() => {return (Settings.Physics.CustomFeathers || Session.CustomFeathers) ? Math.Max(beforeStarFlySpeed.Length() * 0.75f, 140) : 140;});
            }
        }

        private void modifyPlayerStarFlyCoroutine(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            ILLabel start = cursor.DefineLabel();

            if(cursor.TryGotoNext(instr => instr.OpCode == OpCodes.Stfld)) {
                start = cursor.MarkLabel();
            }

            if(cursor.TryGotoNext(MoveType.Before, 
                instr => instr.MatchLdcR4(0.1f)
            )) {
                cursor.Index += 8;

                ILLabel afterStarFlyStartLabel = cursor.MarkLabel();

                cursor.GotoLabel(start, MoveType.After);
                cursor.EmitDelegate(() => {return (Settings.Physics.CustomFeathers || Session.CustomFeathers);});
                cursor.Emit(OpCodes.Brtrue_S, afterStarFlyStartLabel);
            }
            
            if(cursor.TryGotoNext(MoveType.After, 
                instr => instr.MatchLdcR4(250),
                instr => instr.OpCode == OpCodes.Call,
                instr => instr.OpCode == OpCodes.Stfld
            )) {
                cursor.EmitDelegate(() => {
                    if(Settings.Physics.CustomFeathers || Session.CustomFeathers) {
                        Player player = Engine.Scene.Tracker.GetEntity<Player>();

                        player.Speed = beforeStarFlySpeed.SafeNormalize() * Math.Max(beforeStarFlySpeed.Length(), 250);
                    }
                });
            }
        }

        private void modPlayerClimbJump(On.Celeste.Player.orig_ClimbJump orig, Player self) {
            float originalSpeedY = self.Speed.Y;

            orig(self);

            if(Settings.Physics.WallBoostDirectionBasedOnOppositeSpeed || Session.WallBoostDirectionBasedOnOppositeSpeed) {
                if(Input.MoveX == 0) {
                    DynamicData.For(self).Set("wallBoostDir", Math.Sign(-self.Speed.X));
                }
            }

            if(originalSpeedY < -240f && (Settings.Physics.VerticalDashSpeedPreservation || Session.VerticalDashSpeedPreservation)) {
                self.Speed.Y = originalSpeedY + self.LiftSpeed.Y;
                DynamicData.For(self).Set("varJumpSpeed", self.Speed.Y);
            }

            if(DynamicData.For(self).Get<float>("wallSpeedRetentionTimer") > 0.0 && (Settings.Physics.GetClimbJumpSpeedInRetainedFrames || Session.GetClimbJumpSpeedInRetainedFrames)) {
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

            orig(self, dir);

            if(originalSpeed.Y < -240f && (Settings.Physics.VerticalDashSpeedPreservation || Session.VerticalDashSpeedPreservation)) {
                self.Speed.Y = originalSpeed.Y + self.LiftSpeed.Y;
                DynamicData.For(self).Set("varJumpSpeed", self.Speed.Y);
            }

            if(Math.Sign(self.Speed.X - self.LiftSpeed.X) == Math.Sign(originalSpeed.X) && (Settings.Physics.WallJumpSpeedPreservation || Session.WallJumpSpeedPreservation)) {
                self.Speed.X = Math.Sign(originalSpeed.X) * Math.Max(Math.Abs(self.Speed.X), Math.Abs(originalSpeed.X) - (Input.MoveX == 0 ? 0.0f : 40.0f)) + self.LiftSpeed.X;
            }
        }

        private void modPlayerDreamDashBegin(On.Celeste.Player.orig_DreamDashBegin orig, Player self) {
            Vector2 originalSpeed = self.Speed;
            Vector2 intendedSpeed = self.DashDir * 240f;

            orig(self);

            if(Settings.Physics.DreamBlockSpeedPreservation || Session.DreamBlockSpeedPreservation) {
                self.Speed.X = originalSpeed.X;

                self.Speed.X = Math.Sign(intendedSpeed.X) * Math.Max(Math.Abs(intendedSpeed.X), Math.Abs(self.Speed.X));
            }
        }

        private void modPlayerSuperWallJump(On.Celeste.Player.orig_SuperWallJump orig, Player self, int dir) {
            float originalSpeedY = self.Speed.Y;

            orig(self, dir);

            if(originalSpeedY < -240f && (Settings.Physics.VerticalDashSpeedPreservation || Session.VerticalDashSpeedPreservation)) {
                self.Speed.Y = originalSpeedY + self.LiftSpeed.Y;
                DynamicData.For(self).Set("varJumpSpeed", self.Speed.Y);
            }

            if(!(Settings.Physics.WallbounceSpeedPreservation || Session.WallbounceSpeedPreservation)) {
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
            //as long as all refill freeze freezeframe callers have "refillroutine" in their names and nothing else then this should work
            if(Regex.Matches(new System.Diagnostics.StackTrace().ToString(), "RefillRoutine").Count > 0) {
                if(Session.RefillFreezeLength != -1) time = Session.RefillFreezeLength / 60f;
                if(Settings.Physics.RefillFreezeLength != -1) time = Settings.Physics.RefillFreezeLength / 60f;
            }

            orig(time);
        }

        private void modPlayerPointBounce(On.Celeste.Player.orig_PointBounce orig, Player self, Vector2 from) {
            if(!(Settings.Physics.ReboundInversion || Session.ReboundInversion)) {
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
            if(!(Settings.Physics.ReboundInversion || Session.ReboundInversion) && !(Settings.Physics.CustomSwimming || Session.CustomSwimming)) {
                orig(self, direction);

                return;
            }

            Vector2 originalSpeed = self.Speed;

            orig(self, direction);

            if((Settings.Physics.CustomSwimming || Session.CustomSwimming) && self.CollideCheck<Water>()) {
                self.Speed = originalSpeed * 1.1f;

                // self.Speed = -Math.Max(originalSpeed.Length(), 120) * originalSpeed.SafeNormalize(Vector2.Zero);

                return;
            }

            doSpeedReverseStuff(originalSpeed.X, self, 120);
        }

        private void modPlayerReflectBounce(On.Celeste.Player.orig_ReflectBounce orig, Player self, Vector2 direction) {
            if(!(Settings.Physics.ReboundInversion || Session.ReboundInversion) || direction.X == 0) {
                orig(self, direction);

                return;
            }

            float originalSpeed = self.Speed.X;

            orig(self, direction);

            doSpeedReverseStuff(originalSpeed, self, 220);
        }

        private bool modPlayerSideBounce(On.Celeste.Player.orig_SideBounce orig, Player self, int dir, float fromX, float fromY) {
            if(!(Settings.Physics.SpringSpeedPreservation || Session.SpringSpeedPreservation)) {
                return orig(self, dir, fromX, fromY);
            }

            float originalSpeed = self.Speed.X;

            bool res = orig(self, dir, fromX, fromY);

            doSpeedReverseStuff(originalSpeed, self, 240, dir);

            return res;
        }

        private void modPlayerSuperBounce(On.Celeste.Player.orig_SuperBounce orig, Player self, float fromY) {
            if(!(Settings.Physics.SpringSpeedPreservation || Session.SpringSpeedPreservation)) {
                orig(self, fromY);

                return;
            }

            float originalSpeed = self.Speed.X;

            orig(self, fromY);

            self.Speed.X = Math.Abs(originalSpeed) * Input.MoveX;
            if(self.Speed.X == 0) self.Speed.X = Math.Abs(originalSpeed); //in case movex is 0
        }

        private void modPlayerOnCollideH(On.Celeste.Player.orig_OnCollideH orig, Player self, CollisionData data) {
            if(!(Settings.Physics.KeepDashAttackOnCollision || Session.KeepDashAttackOnCollision) && !(Settings.Physics.CustomSwimming || Session.CustomSwimming)) {
                orig(self, data);

                return;
            };

            if((Settings.Physics.CustomSwimming || Session.CustomSwimming) && self.CollideCheck<Water>()) {
                // if(DynamicData.For(self).Get<float>("customWaterRetentionTimer") <= 0.0f) {
                //     DynamicData.For(self).Set("customWaterRetentionSpeed", self.Speed.Length());
                //     DynamicData.For(self).Set("customWaterRetentionTimer", 0.06f);

                //     Vector2 customWaterRetentionDirection = DynamicData.For(self).Get<Vector2>("customWaterRetentionDirection");

                //     customWaterRetentionDirection.X = self.CollideCheck<Solid>(self.Position + Vector2.UnitX * -1) ? -1 : 1;

                //     DynamicData.For(self).Set("customWaterRetentionDirection", customWaterRetentionDirection);
                // }

                setSelfDyn();

                if(customWaterRetentionTimer <= 0.0f) {
                    customWaterRetentionSpeed = self.Speed.Length();
                    customWaterRetentionTimer = 0.06f;

                    customWaterRetentionDirection = new Vector2(self.CollideCheck<Solid>(self.Position + Vector2.UnitX * -1) ? -1 : 1, customWaterRetentionDirection.Y);
                }
            }
            
            float originalDashAttack = DynamicData.For(self).Get<float>("dashAttackTimer");

            orig(self, data);

            DynamicData.For(self).Set("dashAttackTimer", originalDashAttack);
        }

        private void modPlayerOnCollideV(On.Celeste.Player.orig_OnCollideV orig, Player self, CollisionData data) {
            if(!(Settings.Physics.KeepDashAttackOnCollision || Session.KeepDashAttackOnCollision) && !(Settings.Physics.CustomSwimming || Session.CustomSwimming)) {
                orig(self, data);

                return;
            };

            if((Settings.Physics.CustomSwimming || Session.CustomSwimming) && self.CollideCheck<Water>()) {
                // if(DynamicData.For(self).Get<float>("customWaterRetentionTimer") <= 0.0f) {
                //     DynamicData.For(self).Set("customWaterRetentionSpeed", self.Speed.Length());
                //     DynamicData.For(self).Set("customWaterRetentionTimer", 0.06f);

                //     Vector2 customWaterRetentionDirection = DynamicData.For(self).Get<Vector2>("customWaterRetentionDirection");

                //     customWaterRetentionDirection.Y = self.CollideCheck<Solid>(self.Position + Vector2.UnitY * -1) ? -1 : 1;

                //     DynamicData.For(self).Set("customWaterRetentionDirection", customWaterRetentionDirection);
                // }

                setSelfDyn();

                if(customWaterRetentionTimer <= 0.0f) {
                    customWaterRetentionSpeed = self.Speed.Length();
                    customWaterRetentionTimer = 0.06f;

                    customWaterRetentionDirection = new Vector2(customWaterRetentionDirection.X, self.CollideCheck<Solid>(self.Position + Vector2.UnitY * -1) ? -1 : 1);
                }
            }

            float originalDashAttack = DynamicData.For(self).Get<float>("dashAttackTimer");

            orig(self, data);

            DynamicData.For(self).Set("dashAttackTimer", originalDashAttack);
        }

        private void modPlayerJump(On.Celeste.Player.orig_Jump orig, Player self, bool particles, bool playSfx) {
            if(!(Settings.Physics.JumpInversion || Session.JumpInversion)) {
                orig(self, particles, playSfx);

                Player player = Engine.Scene.Tracker.GetEntity<Player>();

                float varJumpTimer = DynamicData.For(player).Get<float>("varJumpTimer");
                
                return;
            }


            if(!((particles && playSfx) || (Settings.Physics.AllowClimbJumpInversion || Session.AllowClimbJumpInversion))) {
                orig(self, particles, playSfx);

                return;    
            }


            int moveX = DynamicData.For(self).Get<int>("moveX");

            if((float)moveX == -Math.Sign(self.Speed.X)) {
                self.Speed.X *= -1;
            }

            orig(self, particles, playSfx);
        }

        private void modPlayerUpdate(On.Celeste.Player.orig_Update orig, Player self) {
            // if(Input.MoveX != Math.Sign(self.Speed.X) && Input.MoveX != 0) {
            //     self.Speed.X *= -1;
            // }

            if((Settings.Physics.CustomSwimming || Session.CustomSwimming) && self.StateMachine.State == 3) {
                // DynamicData.For(self).Set("customWaterRetentionTimer", DynamicData.For(self).Get<float>("customWaterRetentionTimer") - Engine.DeltaTime);
                setSelfDyn();

                customWaterRetentionTimer -= Engine.DeltaTime;
            }

            // if(Settings.Physics.AlwaysExplodeSpinners || Session.AlwaysExplodeSpinners) {
            //     System.Collections.Generic.List<CrystalStaticSpinner> spinners = self.Scene.CollideAll<CrystalStaticSpinner>(new Rectangle((int)(self.CenterX + self.Speed.X * Engine.DeltaTime - 10f), (int)(self.Center.Y + self.Speed.Y * Engine.DeltaTime - 10f), 20, 20));

            //     foreach(CrystalStaticSpinner spinner in spinners) {
            //         spinner.Destroy(false);
            //     }
            // }
            

            orig(self);

            

            /*
            if(resetSweatSpriteTimer > 0f) {
                resetSweatSpriteTimer -= Engine.DeltaTime;
                Logger.Log(LogLevel.Info, "GooberHelper", $"{resetSweatSpriteTimer}");

                if(resetSweatSpriteTimer <= 0f) {
                    DynamicData.For(self).Get<Sprite>("sweatSprite").SetColor(Color.White);
                    Logger.Log(LogLevel.Info, "GooberHelper", "reset");

                    resetSweatSpriteTimer = 0f;
                }
            }
            */
        }

        private void modifyPlayerUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            float cobwob_originalSpeed = 0;

            //[BEFORE] this.Speed.X = 130f * (float)this.moveX;
            if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(130f))) {
                cursor.EmitDelegate<Func<float, float>>(orig => {
                    if(!(Settings.Physics.CobwobSpeedInversion || Session.CobwobSpeedInversion)) return orig;

                    Player player = Engine.Scene.Tracker.GetEntity<Player>();
                    if (player == null) return orig;

                    cobwob_originalSpeed = player.Speed.X;

                    return orig;
                });
            }

            //[BEFORE] this.Stamina += 27.5f;
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(27.5f))) {
                cursor.EmitDelegate<Func<float, float>>(orig => {
                    if(!(Settings.Physics.CobwobSpeedInversion || Session.CobwobSpeedInversion)) return orig;

                    Player player = Engine.Scene.Tracker.GetEntity<Player>();
                    if (player == null) return orig;

                    float dir = Math.Sign(player.Speed.X);
                    float newAbsoluteSpeed = Math.Max(130f, Math.Abs(cobwob_originalSpeed));

                    if(
                        (
                            Settings.Physics.WallBoostSpeedIsAlwaysOppositeSpeed ||
                            Session.WallBoostSpeedIsAlwaysOppositeSpeed
                        ) &&
                        (
                            !Settings.Physics.WallBoostDirectionBasedOnOppositeSpeed ||
                            !Session.WallBoostDirectionBasedOnOppositeSpeed
                        ) &&
                        DynamicData.For(player).Get<int>("wallBoostDir") == Math.Sign(cobwob_originalSpeed - 11f * Math.Sign(cobwob_originalSpeed))
                    ) {
                        dir *= -1;
                    }
                    
                    if(DynamicData.For(player).Get<float>("wallSpeedRetentionTimer") > 0.0 && (Settings.Physics.AllowRetentionReverse || Session.AllowRetentionReverse)) {
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
