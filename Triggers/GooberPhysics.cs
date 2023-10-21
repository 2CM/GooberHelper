using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;

namespace Celeste.Mod.GooberHelper.Entities {

    [CustomEntity("GooberHelper/GooberPhysics")]
    public class GooberPhysics : Trigger {
        private bool CobwobSpeedInversionValue;
        private bool AllowRetentionReverseValue;
        private bool JumpInversionValue;
        private bool AllowClimbJumpInversionValue;
        private bool KeepDashAttackOnCollisionValue;
        private bool ReboundInversionValue;
        private bool WallbounceSpeedPreservationValue;


        public GooberPhysics(EntityData data, Vector2 offset) : base(data, offset) {
            CobwobSpeedInversionValue = data.Bool("cobwobSpeedInversion", false);
            AllowRetentionReverseValue = data.Bool("allowRetentionReverse", false);
            JumpInversionValue = data.Bool("jumpInversion", false);
            AllowClimbJumpInversionValue = data.Bool("allowClimbJumpInversion", false);
            KeepDashAttackOnCollisionValue = data.Bool("keepDashAttackOnCollision", false);
            ReboundInversionValue = data.Bool("reboundInversion", false);
            WallbounceSpeedPreservationValue = data.Bool("wallbounceSpeedPreservation", false);
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            GooberHelperModule.Session.CobwobSpeedInversion = CobwobSpeedInversionValue;
            GooberHelperModule.Session.AllowRetentionReverse = AllowRetentionReverseValue;
            GooberHelperModule.Session.JumpInversion = JumpInversionValue;
            GooberHelperModule.Session.AllowClimbJumpInversion = AllowClimbJumpInversionValue;
            GooberHelperModule.Session.KeepDashAttackOnCollision = KeepDashAttackOnCollisionValue;
            GooberHelperModule.Session.ReboundInversion = ReboundInversionValue;
            GooberHelperModule.Session.WallbounceSpeedPreservation = WallbounceSpeedPreservationValue;
        }
    }
}