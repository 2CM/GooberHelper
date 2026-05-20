using System;
using Celeste.Mod.GooberHelper.Attributes;

namespace Celeste.Mod.GooberHelper.Options.Physics.Jumping {
    [GooberHelperOption]
    public class HyperAndSuperSpeedPreservation : AbstractOption {
        public static void InSuperJump(Player player, Vector2 originalSpeed) {
            var hyperAndSuperSpeedPreservationValue = 0f;

            if(GetOptionBool(Option.HyperAndSuperSpeedPreservation))
                hyperAndSuperSpeedPreservationValue = originalSpeed.Length();

            player.Speed.X = Utils.SignedAbsMax(
                player.Speed.X,
                hyperAndSuperSpeedPreservationValue
            );
        }
    }
}