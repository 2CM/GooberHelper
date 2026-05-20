using System;
using System.Linq;
using Celeste.Mod.GooberHelper.Attributes;
using Celeste.Mod.GooberHelper.Attributes.Hooks;
using Celeste.Mod.GooberHelper.Extensions;
using Celeste.Mod.GooberHelper.Helpers;
using Celeste.Mod.GooberHelper.Options.Visuals;
using Celeste.Mod.Helpers;
using MonoMod.Cil;

namespace Celeste.Mod.GooberHelper.Options.Physics.Entities {
    [GooberHelperOption]
    public class CustomSwimming : AbstractOption {
        //look at me putting all my constants at the top of the file instead of just scattering magic numbers everywhere
        //i deserve an award
        public static readonly float DefaultSwimmingSpeed = 90f;

        public static readonly float SlowAcceleration = 350f;
        public static readonly float FastTurningSpeed = 10f;
        
        [ILHook]
        private static void patch_Player_Update(ILContext il) {
            var cursor = new ILCursor(il);

            cursor.EmitLdarg0();
            cursor.EmitDelegate(updateRetention);
        }

        [ILHook(typeof(Player), "OnCollideH")]
        [ILHook(typeof(Player), "OnCollideV")]
        private static void patchCollisions(ILContext il) {
            var cursor = new ILCursor(il);

            cursor.EmitLdarg0();
            cursor.EmitLdarg1();
            cursor.EmitDelegate(handleCollision);
        }

        private static void updateRetention(Player player) {
            var ext = player.GetExtensionFields();

            if(ext.SwimmingRetentionTimer <= 0f) {
                ext.SwimmingRetentionSpeed = Vector2.Zero;

                return;
            }
            
            Utils.Log($"[swimming retention info] speed: {ext.SwimmingRetentionSpeed}, time: {ext.SwimmingRetentionTimer}");

            ext.SwimmingRetentionTimer -= Engine.DeltaTime;
        }

        private static void handleCollision(Player player, CollisionData data) {
            if(!GetOptionBool(Option.CustomSwimming) || !player.CollideCheck<Water>())
                return;

            var ext = player.GetExtensionFields();

            var newCollision = false;

            if(data.Direction.Y == 0 && Math.Abs(player.Speed.X) > Math.Abs(ext.SwimmingRetentionSpeed.X)) {
                ext.SwimmingRetentionSpeed.X = player.Speed.X;
                
                newCollision = true;
            }

            if(data.Direction.X == 0 && Math.Abs(player.Speed.Y) > Math.Abs(ext.SwimmingRetentionSpeed.Y)) {
                ext.SwimmingRetentionSpeed.Y = player.Speed.Y;
                
                newCollision = true;
            }

            if(newCollision) {
                ext.SwimmingRetentionTimer = 0.06f;
                ext.SwimmingRetentionPlatform = data.Hit;
            }
        }

        //returns true if it worked
        private static bool trySwimWalljump(Player player, PlayerExtensions.PlayerExtensionFields ext = null) {
            ext ??= player.GetExtensionFields();

            var optionValue = GetOptionValue(Option.CustomSwimmingWalljumpSpeed);

            if(optionValue == (int)CustomSwimmingWalljumpSpeed.Value.Off)
                return false;
            
            if(!Input.Jump.Pressed || ext.SwimmingRetentionTimer <= 0f || !GetOptionBool(Option.CustomSwimming))
                return false;
            
            var conservedSpeed = player.GetConservedSpeed();

            var swimWallJumpSpeed = new Vector2(
                Utils.SignedAbsMax(conservedSpeed.X, ext.SwimmingRetentionSpeed.X),
                Utils.SignedAbsMax(conservedSpeed.Y, ext.SwimmingRetentionSpeed.Y)
            );

            var swimWalljumpSpeed = swimWallJumpSpeed.Length() + optionValue;
            var swimWalljumpDirection = -ext.SwimmingRetentionSpeed.SafeNormalize();

            if(swimWalljumpDirection == Vector2.Zero)
                return false;

            Input.Jump.ConsumeBuffer();
            player.Speed = swimWalljumpDirection * swimWalljumpSpeed;

            var index = ext.SwimmingRetentionPlatform.GetWallSoundIndex(player, -Math.Sign(swimWalljumpDirection.X));

            Dust.Burst(player.Center + swimWalljumpDirection * player.Collider.Size / 2, swimWalljumpDirection.Angle(), 4, player.DustParticleFromSurfaceIndex(index));
            player.Play(SurfaceIndex.GetPathFromIndex(index) + "/landing", "surface_index", index);

            return true;
        }

        private static bool trySwimWalljumpFromDash(Player player) {
            if(!player.CollideCheck<Water>())
                return false;
        
            return trySwimWalljump(player, player.GetExtensionFields());
        }

        private static bool trySwimLaunch(Player player, PlayerExtensions.PlayerExtensionFields ext = null) {
            var thresholdOptionValue = GetOptionValue(Option.CustomSwimmingLaunchThreshold);

            if(thresholdOptionValue == (int)CustomSwimmingLaunchThreshold.Value.Off)
                return false;
            
            ext ??= player.GetExtensionFields();

            if(!Input.Jump.Pressed || ext.SwimLaunchCooldownTimer > 0f)
                return false;

            var upwardsDisplacement = Math.Min(player.Speed.Y * Engine.DeltaTime, 0);
            var inWaterNextFrame = player.CollideCheck<Water>(player.Position + Vector2.UnitY * (upwardsDisplacement - 10f)); //8 because you stop being in stswim a bit before the surface

            if(inWaterNextFrame || player.Speed.Y > -GetOptionValue(Option.CustomSwimmingLaunchThreshold))
                return false;

            Input.Jump.ConsumeBuffer();

            player.Speed += GetOptionValue(Option.CustomSwimmingLaunchSpeed) * player.Speed.SafeNormalize();
            player.launched = true;
            player.Play(SFX.char_mad_jump_super);
            
            Dust.Burst(player.Position, MathF.PI + player.Speed.Angle(), 4);

            CustomSwimmingAnimation.ParticleBurst(player);
            
            ext.IsDolphin = true;
            ext.SwimLaunchCooldownTimer = 0.2f;

            return true;
        }

        private static bool doCustomSwimMovement(Player player, Vector2 vector) {
            var ext = player.GetExtensionFields();

            ext.SwimLaunchCooldownTimer -= Engine.DeltaTime;

            //lenient all direction retention is used underwater
            player.wallSpeedRetentionTimer = 0f;

            var retention = ext.SwimmingRetentionSpeed;

            if(Math.Abs(retention.X) > Math.Abs(player.Speed.X) && !player.CollideCheck<Solid>(player.Position + Math.Sign(retention.X) * Vector2.UnitX))
                player.Speed.X = ext.SwimmingRetentionSpeed.X;

            if(Math.Abs(retention.Y) > Math.Abs(player.Speed.Y) && !player.CollideCheck<Solid>(player.Position + Math.Sign(retention.Y) * Vector2.UnitY))
                player.Speed.Y = ext.SwimmingRetentionSpeed.Y;

            if(Input.Jump.Pressed) {
                if(trySwimWalljump(player, ext))
                    return false;
                
                if(trySwimLaunch(player, ext))
                    return true;
            }

            var useSlowMovement =
                Vector2.Dot(player.Speed.SafeNormalize(Vector2.Zero), vector) < -0.5f ||
                vector.Length() == 0 ||
                player.Speed.Length() <= DefaultSwimmingSpeed;

            player.Speed = useSlowMovement
                ? Calc.Approach(player.Speed, DefaultSwimmingSpeed * vector, SlowAcceleration * Engine.DeltaTime)
                : player.Speed.RotateTowards(vector.Angle(), FastTurningSpeed * Engine.DeltaTime);

            // if(useSlowMovement && Input.Aim != Vector2.Zero)
            //     ext.PlayerRotationTarget = ext.PlayerRotation = player.Speed.Angle() + MathF.PI / 2;

            return false;
        }

        [ILHook]
        private static void patch_Player_SwimUpdate(ILContext il) {
            var cursor = new ILCursor(il);

            var originalMovementEndLabel = cursor.DefineLabel();
            var customMovementEndLabel = cursor.DefineLabel();
            var swimJumpEndLabel = cursor.DefineLabel();
            var afterReturnLabel = cursor.DefineLabel();

            HookHelper.Begin(cursor, "making custom swimming work");

            HookHelper.Move("going after safenormalize", () => {
                cursor.GotoNextBestFit(MoveType.After, 
                    instr => instr.MatchLdloc1(),
                    instr => instr.MatchCallOrCallvirt(((Func<Vector2, Vector2>)Calc.SafeNormalize).Method),
                    instr => instr.MatchStloc1()
                );
            });

            HookHelper.Do(() => {
                //if(GetOptionBool(Option.CustomSwimming))
                cursor.EmitDelegate(getOptionBool);
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
            });

            HookHelper.Move("going before moveX", () => {
                cursor.GotoNextBestFit(MoveType.Before, 0x20,
                    instr => instr.MatchLdloc0(),
                    instr => instr.MatchBrtrue(out _),
                    instr => instr.MatchLdarg0(),
                    instr => instr.MatchLdfld<Player>("moveX")
                );
            });

            HookHelper.Do(() => {
                cursor.MarkLabel(originalMovementEndLabel);
            });

            HookHelper.Move("going before Jump.Pressed", () => {
                cursor.GotoNextBestFit(MoveType.Before,
                    instr => instr.MatchLdsfld(typeof(Input).GetField("Jump")),
                    instr => instr.MatchCallOrCallvirt<VirtualButton>("get_Pressed"),
                    instr => instr.MatchBrfalse(out swimJumpEndLabel)
                );
            });

            HookHelper.Do(() => {
                cursor.MoveAfterLabels();
                cursor.EmitDelegate(getOptionBool);
                cursor.EmitBrtrue(swimJumpEndLabel);
            });

            HookHelper.End();
        }

        [ILHook]
        private static void patch_Player_NormalUpdate(ILContext il) {
            var cursor = new ILCursor(il);

            HookHelper.Begin(cursor, "making the custom swimming launch thing work in normalupdate");

            var afterJumpLabel = cursor.DefineLabel();

            HookHelper.Move("going to the water check", () => {
                var collideFirstMethod = typeof(Entity)
                    .GetMethods()
                    .Where(method =>
                        method.Name == "CollideFirst" &&
                        method.IsGenericMethodDefinition &&
                        method.GetParameters().FirstOrDefault()?.ParameterType == typeof(Vector2)
                    )
                    .First()
                    .MakeGenericMethod(typeof(Water));

                cursor.GotoNextBestFit(MoveType.After,
                    instr => instr.MatchCallOrCallvirt(collideFirstMethod),
                    instr => instr.MatchDup(),
                    instr => instr.MatchStloc(out _),
                    instr => instr.MatchBrfalse(out _)
                );
            });

            HookHelper.Do(() => {
                cursor.EmitLdarg0();
                cursor.EmitLdnull();
                cursor.EmitDelegate(trySwimLaunch);
                cursor.EmitBrtrue(afterJumpLabel);
            });

            HookHelper.Move("going after the jump call", () => {
                cursor.GotoNextBestFit(MoveType.After, instr => instr.MatchCallOrCallvirt<Player>("Jump"));
            });

            HookHelper.Do(() => {
                cursor.MarkLabel(afterJumpLabel);
            });

            HookHelper.End();
        }

        [ILHook]
        private static void patch_Player_DashUpdate(ILContext il) {
            var cursor = new ILCursor(il);

            if(cursor.TryGotoNextBestFit(MoveType.Before, 
                instr => instr.MatchLdarg0(),
                instr => instr.MatchCallOrCallvirt<Player>("get_SuperWallJumpAngleCheck"),
                instr => instr.MatchBrfalse(out _)
            )) {
                var afterReturnLabel = cursor.DefineLabel();

                cursor.MoveAfterLabels();

                //if(customswimming && trySwimWalljumpFromDash(this))
                cursor.EmitLdarg0();
                cursor.EmitDelegate(trySwimWalljumpFromDash);

                cursor.EmitBrfalse(afterReturnLabel);

                //return StSwim;
                cursor.EmitLdcI4(Player.StSwim);
                cursor.EmitRet();

                cursor.MarkLabel(afterReturnLabel);
            }
        }

        [ILHook]
        private static void patch_Player_DashCoroutine(ILContext il) {
            var cursor = new ILCursor(il);

            //remove the 0.75x speed multiplier when dashing while in contact with water
            if(cursor.TryGotoNextBestFit(MoveType.After, 
                instr => instr.MatchLdloc1(),
                instr => instr.MatchCallOrCallvirt<Entity>("CollideCheck"), //collidecheck<water>
                instr => instr.MatchBrfalse(out var _)
            )) {
                cursor.Index--;

                cursor.EmitDelegate(getOptionBool);
                cursor.EmitNot();
                cursor.EmitAnd();
            }
        }

        [OnHook]
        private static bool patch_Player_WallJumpCheck(On.Celeste.Player.orig_WallJumpCheck orig, Player self, int dir) {
            if(self.CollideCheck<Water>() && GetOptionBool(Option.CustomSwimming))
                return false;

            return orig(self, dir);
        }

        [ILHook]
        private static void patch_Player_SwimBegin(ILContext il) {
            var cursor = new ILCursor(il);

            if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(0.5f)))
                cursor.EmitDelegate(overrideHalfSpeed);
        }

        private static bool getOptionBool()
            => GetOptionBool(Option.CustomSwimming);

        private static float overrideHalfSpeed(float orig)
            => GetOptionBool(Option.CustomSwimming)
                ? 1f
                : orig; //AAAA THIS WAS ALMOST A 0F BUT I CAUGHT ITTTTTTTTTT
    }
}