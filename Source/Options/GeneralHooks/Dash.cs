using System;
using Celeste.Mod.GooberHelper.Attributes.Hooks;
using Celeste.Mod.GooberHelper.Extensions;
using Celeste.Mod.GooberHelper.Options.Physics.Dashing;
using Celeste.Mod.GooberHelper.Options.Physics.Other;
using Celeste.Mod.Helpers;
using MonoMod.Cil;

namespace Celeste.Mod.GooberHelper.Options.GeneralHooks {
    public static class Dash {
        private static Vector2 originalDashSpeed;
        private static float normalEndWallSpeedRetained;
        private static float normalEndWallSpeedRetentionTimer;

        [ILHook]
        private static void patch_Player_DashCoroutine(ILContext il) {
            var cursor = new ILCursor(il);

            if(cursor.TryGotoNextBestFit(MoveType.After, 0x20,
                instr => instr.MatchLdloc2(),
                instr => instr.MatchLdcR4(240),
                instr => instr.MatchCall<Vector2>("op_Multiply"),
                instr => instr.MatchStloc3()
            )) {
                cursor.EmitLdloc3();
                cursor.EmitDelegate(setOriginalDashSpeed);
            }

            if(cursor.TryGotoNextBestFit(MoveType.After, 
                instr => instr.MatchLdloc1(),
                instr => instr.MatchLdloc3(),
                instr => instr.MatchStfld<Player>("Speed")
            )) {
                cursor.Index--;

                cursor.EmitLdloc1();
                cursor.EmitDelegate(overrideDashSpeed);
            }
        }

        [ILHook]
        private static void patch_Player_DashBegin(ILContext il) {
            var cursor = new ILCursor(il);

            cursor.EmitLdarg0();
            cursor.EmitDelegate(setConservedSpeed);
        }

        [OnHook]
        private static void patch_Player_NormalEnd(On.Celeste.Player.orig_NormalEnd orig, Player self) {
            if(self.StateMachine.State == Player.StDash) {
                normalEndWallSpeedRetained = self.wallSpeedRetained;
                normalEndWallSpeedRetentionTimer = self.wallSpeedRetentionTimer;
            } else {
                normalEndWallSpeedRetained = 0;
                normalEndWallSpeedRetentionTimer = 0;
            }

            orig(self);
        }

        private static void setConservedSpeed(Player player) {
            var extraThing = Vector2.Zero;

            if(Math.Abs(normalEndWallSpeedRetained) > Math.Abs(player.wallSpeedRetained) && normalEndWallSpeedRetentionTimer > 0f)
                extraThing = new Vector2(normalEndWallSpeedRetained, 0);

            player.GetExtensionFields().BeforeDashSpeedConserved = player.GetConservedSpeed(null, extraThing);
        }

        private static void setOriginalDashSpeed(Vector2 orig)
            => originalDashSpeed = orig;

        private static Vector2 overrideDashSpeed(Vector2 orig, Player player) {
            var beforeDashSpeedConserved = player.GetExtensionFields().BeforeDashSpeedConserved;

            if(MagnitudeBasedDashSpeed.OverrideDashSpeed(player, originalDashSpeed, beforeDashSpeedConserved, ref orig))
                return orig;

            VerticalDashSpeedPreservation.OverrideDashSpeed(player, originalDashSpeed, beforeDashSpeedConserved, ref orig);
            ReverseDashSpeedPreservation.OverrideDashSpeed(player, originalDashSpeed, beforeDashSpeedConserved, ref orig);
            AllowWindWhileDashing.OverrideDashSpeed(player, originalDashSpeed, beforeDashSpeedConserved, ref orig);
            
            return orig;
        }
    }
}