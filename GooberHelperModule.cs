﻿using System;
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

namespace Celeste.Mod.GooberHelper {
    public class GooberHelperModule : EverestModule {
        public static GooberHelperModule Instance { get; private set; }


        public override Type SettingsType => typeof(GooberHelperModuleSettings);
        public static GooberHelperModuleSettings Settings => (GooberHelperModuleSettings) Instance._Settings;

        public override Type SessionType => typeof(GooberHelperModuleSession);
        public static GooberHelperModuleSession Session => (GooberHelperModuleSession) Instance._Session;

        private static ILHook playerUpdateHook;

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
            playerUpdateHook = new ILHook(typeof(Player).GetMethod("orig_Update"), modifyPlayerUpdate);
            
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
            
            On.Celeste.Celeste.Freeze += modCelesteFreeze;
        }

        public override void Unload() {
            playerUpdateHook.Dispose();

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

            On.Celeste.Celeste.Freeze -= modCelesteFreeze;
        }

        private void modPlayerDreamDashBegin(On.Celeste.Player.orig_DreamDashBegin orig, Player self) {
            Vector2 originalSpeed = self.Speed;
            Vector2 intendedSpeed = self.DashDir * 240f;

            orig(self);

            if(Settings.DreamBlockSpeedPreservation) {
                self.Speed.X = originalSpeed.X;

                self.Speed.X = Math.Sign(intendedSpeed.X) * Math.Max(Math.Abs(intendedSpeed.X), Math.Abs(self.Speed.X));
            }
        }

        private void modPlayerSuperWallJump(On.Celeste.Player.orig_SuperWallJump orig, Player self, int dir) {
            orig(self, dir);

            if(!Settings.WallbounceSpeedPreservation && !Session.WallbounceSpeedPreservation) {
                return;
            }

            Vector2 beforeDashSpeed = DynamicData.For(self).Get<Vector2>("beforeDashSpeed");
            float absoluteBeforeDashSpeed = Math.Abs(beforeDashSpeed.X);

            self.Speed.X = dir * Math.Max(absoluteBeforeDashSpeed + DynamicData.For(self).Get<Vector2>("LiftBoost").X, Math.Abs(self.Speed.X));

            return;
        }

        private void modCelesteFreeze(On.Celeste.Celeste.orig_Freeze orig, float time) {
            //as long as all refill freeze freezeframe callers have "refillroutine" in their names and nothing else then this should work
            if(Regex.Matches(new System.Diagnostics.StackTrace().ToString(), "RefillRoutine").Count > 0) {
                if(Session.RefillFreezeLength != -1) time = Session.RefillFreezeLength / 60f;
                if(Settings.RefillFreezeLength != -1) time = Settings.RefillFreezeLength / 60f;
            }

            orig(time);
        }

        private void modPlayerPointBounce(On.Celeste.Player.orig_PointBounce orig, Player self, Vector2 from) {
            if(!Settings.ReboundInversion && !Session.ReboundInversion) {
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
            if(!Settings.ReboundInversion && !Session.ReboundInversion) {
                orig(self, direction);

                return;
            }

            float originalSpeed = self.Speed.X;

            orig(self, direction);

            doSpeedReverseStuff(originalSpeed, self, 120);
        }

        private void modPlayerReflectBounce(On.Celeste.Player.orig_ReflectBounce orig, Player self, Vector2 direction) {
            if((!Settings.ReboundInversion && !Session.ReboundInversion) || direction.X == 0) {
                orig(self, direction);

                return;
            }

            float originalSpeed = self.Speed.X;

            orig(self, direction);

            doSpeedReverseStuff(originalSpeed, self, 220);
        }

        private bool modPlayerSideBounce(On.Celeste.Player.orig_SideBounce orig, Player self, int dir, float fromX, float fromY) {
            if(!Settings.SpringSpeedPreservation) {
                return orig(self, dir, fromX, fromY);
            }

            float originalSpeed = self.Speed.X;

            bool res = orig(self, dir, fromX, fromY);

            doSpeedReverseStuff(originalSpeed, self, 240, dir);

            return res;
        }

        private void modPlayerSuperBounce(On.Celeste.Player.orig_SuperBounce orig, Player self, float fromY) {
            if(!Settings.SpringSpeedPreservation) {
                orig(self, fromY);

                return;
            }

            float originalSpeed = self.Speed.X;

            orig(self, fromY);

            self.Speed.X = Math.Abs(originalSpeed) * Input.MoveX;
            if(self.Speed.X == 0) self.Speed.X = Math.Abs(originalSpeed); //in case movex is 0
        }

        private void modPlayerOnCollideH(On.Celeste.Player.orig_OnCollideH orig, Player self, CollisionData data) {
            if(!Settings.KeepDashAttackOnCollision && !Session.KeepDashAttackOnCollision) {
                orig(self, data);

                return;
            };
            
            float originalDashAttack = DynamicData.For(self).Get<float>("dashAttackTimer");

            orig(self, data);

            DynamicData.For(self).Set("dashAttackTimer", originalDashAttack);
        }

        private void modPlayerOnCollideV(On.Celeste.Player.orig_OnCollideV orig, Player self, CollisionData data) {
            if(!Settings.KeepDashAttackOnCollision && !Session.KeepDashAttackOnCollision) {
                orig(self, data);

                return;
            };

            float originalDashAttack = DynamicData.For(self).Get<float>("dashAttackTimer");

            orig(self, data);

            DynamicData.For(self).Set("dashAttackTimer", originalDashAttack);
        }

        private void modPlayerJump(On.Celeste.Player.orig_Jump orig, Player self, bool particles, bool playSfx) {
            if(!Settings.JumpInversion && !Session.JumpInversion) {
                orig(self, particles, playSfx);

                return;
            }


            if(!((particles && playSfx) || Settings.AllowClimbJumpInversion || Session.AllowClimbJumpInversion)) {
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
            orig(self);
        }

        private void modifyPlayerUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            float cobwob_originalSpeed = 0;

            //[BEFORE] this.Speed.X = 130f * (float)this.moveX;
            if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(130f))) {
                cursor.EmitDelegate<Func<float, float>>(orig => {
                    if(!Settings.CobwobSpeedInversion && !Session.CobwobSpeedInversion) return orig;

                    Player player = Engine.Scene.Tracker.GetEntity<Player>();
                    if (player == null) return orig;

                    cobwob_originalSpeed = player.Speed.X;

                    return orig;
                });
            }

            //[BEFORE] this.Stamina += 27.5f;
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(27.5f))) {
                cursor.EmitDelegate<Func<float, float>>(orig => {
                    if(!Settings.CobwobSpeedInversion && !Session.CobwobSpeedInversion) return orig;

                    Player player = Engine.Scene.Tracker.GetEntity<Player>();
                    if (player == null) return orig;

                    float dir = Math.Sign(player.Speed.X);
                    float newAbsoluteSpeed = Math.Max(130f, Math.Abs(cobwob_originalSpeed));

                    
                    if(DynamicData.For(player).Get<float>("wallSpeedRetentionTimer") > 0.0 && (Settings.AllowRetentionReverse || Session.AllowRetentionReverse)) {
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
