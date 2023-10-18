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

namespace Celeste.Mod.GooberHelper {
    public class GooberHelperModule : EverestModule {
        public static GooberHelperModule Instance { get; private set; }


        public override Type SettingsType => typeof(GooberHelperModuleSettings);
        public static GooberHelperModuleSettings Settings => (GooberHelperModuleSettings) Instance._Settings;

        public override Type SessionType => typeof(GooberHelperModuleSession);
        public static GooberHelperModuleSession Session => (GooberHelperModuleSession) Instance._Session;

        private static ILHook playerUpdateHook;
        private static ILHook playerDashUpdateHook;

        private bool changeNextFreezeLength = false;

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
            playerDashUpdateHook = new ILHook(typeof(Player).GetMethod("DashUpdate", BindingFlags.NonPublic | BindingFlags.Instance), modifyPlayerDashUpdate);
            
            On.Celeste.Player.Update += modPlayerUpdate;
            On.Celeste.Player.Jump += modPlayerJump;
            On.Celeste.Player.Rebound += modPlayerRebound;
            On.Celeste.Player.ReflectBounce += modPlayerReflectBounce;
            On.Celeste.Player.PointBounce += modPlayerPointBounce;
            On.Celeste.Player.OnCollideH += modPlayerOnCollideH;
            On.Celeste.Player.OnCollideV += modPlayerOnCollideV;

            On.Celeste.Refill.RefillRoutine += modRefillRefillRoutine;

            On.Celeste.Celeste.Freeze += modCelesteFreeze;
        }

        public override void Unload() {
            playerUpdateHook.Dispose();
            playerDashUpdateHook.Dispose();

            On.Celeste.Player.Update -= modPlayerUpdate;
            On.Celeste.Player.Jump -= modPlayerJump;
            On.Celeste.Player.Rebound -= modPlayerRebound;
            On.Celeste.Player.ReflectBounce -= modPlayerReflectBounce;
            On.Celeste.Player.PointBounce -= modPlayerPointBounce;
            On.Celeste.Player.OnCollideH -= modPlayerOnCollideH;
            On.Celeste.Player.OnCollideV -= modPlayerOnCollideV;

            On.Celeste.Refill.RefillRoutine -= modRefillRefillRoutine;

            On.Celeste.Celeste.Freeze -= modCelesteFreeze;
        }

        void logshit() {
            Logger.Log(LogLevel.Info, "GooberHelper", "gaygaygay");
        }

        private void modifyPlayerDashUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if(cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Call, instr => instr.MatchLdcR4(0.1f))) {
                Logger.Log(LogLevel.Info, "GooberHelper", $"FOUND IT AT {cursor.Index} {cursor.DefineLabel().Target}");

                cursor.Index -= 4;

                cursor.EmitDelegate(logshit);
                cursor.EmitDelegate(() => {
                    Player player = Engine.Scene.Tracker.GetEntity<Player>();

                    Logger.Log(LogLevel.Info, "GooberHelper", "before if");

                    if (player.CanUnDuck && Input.Jump.Pressed && DynamicData.For(player).Get<float>("jumpGraceTimer") > 0f)
                    {  
                        Logger.Log(LogLevel.Info, "GooberHelper", "during if");

                        DynamicData.For(player).Invoke("SuperJump");
                    }
                });
                //cursor.Emit(OpCodes.Ret, 0);
            }
        }

        private IEnumerator modRefillRefillRoutine(On.Celeste.Refill.orig_RefillRoutine orig, Refill self, Player player) {
            changeNextFreezeLength = true;

            return orig(self, player);
        }

        private void modCelesteFreeze(On.Celeste.Celeste.orig_Freeze orig, float time) {
            if(changeNextFreezeLength) {
                if(Session.RefillFreezeLength != -1) time = Session.RefillFreezeLength / 60;
                if(Settings.RefillFreezeLength != -1) time = Settings.RefillFreezeLength / 60f;

                changeNextFreezeLength = false;
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

        private void doSpeedReverseStuff(float originalSpeed, Player self, float givenSpeed) {
            self.Speed.X *= Math.Abs(originalSpeed) / givenSpeed;

            if(self.Speed.X == 0) self.Speed.X = Input.MoveX * Math.Abs(originalSpeed); //in case the direction was 0
            if(self.Speed.X == 0) self.Speed.X = originalSpeed; //in case Input.MoveX is 0

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
            changeNextFreezeLength = false;

            orig(self);
        }

        private void modifyPlayerUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            float cobwob_originalSpeed = 0;

            //[BEFORE] this.Speed.X = 130f * (float)this.moveX;
            if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(130f))) {
                cursor.EmitDelegate<Func<float, float>>(orig => {
                    if(!Settings.CobwobSpeedInversion && Session.CobwobSpeedInversion) return orig;

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