using Celeste.Mod.GooberHelper.Attributes.Hooks;
using Celeste.Mod.GooberHelper.Extensions;
using Celeste.Mod.GooberHelper.Options.Physics.Jumping;
using Celeste.Mod.Helpers;
using MonoMod.Cil;

namespace Celeste.Mod.GooberHelper.Options.GeneralHooks {
    public static class SuperJump {
        private static Vector2 originalSpeed;

        [ILHook]
        private static void patch_Player_SuperJump(ILContext il) {
            var cursor = new ILCursor(il);

            cursor.EmitLdarg0();
            cursor.EmitDelegate(setOriginalSpeed);

            if(cursor.TryGotoNextBestFit(MoveType.AfterLabel,
                instr => instr.MatchLdarg0(),
                instr => instr.MatchLdfld<Player>("Speed"),
                instr => instr.MatchLdarg0(),
                instr => instr.MatchCallOrCallvirt<Player>("get_LiftBoost")
            )) {
                cursor.EmitLdarg0();
                cursor.EmitDelegate(setPlayerSpeed);
            }
        }

        private static void setOriginalSpeed(Player player)
            => originalSpeed = player.GetConservedSpeed(
                null,
                player.Speed == Vector2.Zero
                    ? player.beforeDashSpeed
                    : Vector2.Zero
            );

        private static void setPlayerSpeed(Player player) {
            if(player.Ducking)
                player.Speed *= new Vector2(1.25f, 0.5f);

            HyperAndSuperSpeedPreservation.InSuperJump(player, originalSpeed);

            if(player.Ducking)
                player.Speed /= new Vector2(1.25f, 0.5f);
        }
    }
}