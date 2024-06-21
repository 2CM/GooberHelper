using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;

namespace Celeste.Mod.GooberHelper.Entities {

    [CustomEntity("GooberHelper/GooberPhysics")]
    public class GooberPhysics : Trigger {
        public bool CobwobSpeedInversionValue;
        public bool AllowRetentionReverseValue;
        public bool JumpInversionValue;
        public bool AllowClimbJumpInversionValue;
        public bool KeepDashAttackOnCollisionValue;
        public bool ReboundInversionValue;
        public bool WallbounceSpeedPreservationValue;
        public bool DreamBlockSpeedPreservationValue;
        public bool SpringSpeedPreservationValue;
        public bool WallJumpSpeedPreservationValue;
        public bool GetClimbJumpSpeedInRetainedFramesValue;
        public bool CustomFeathersValue;
        public bool ExplodeLaunchSpeedPreservationValue;
        public bool BadelineBossSpeedReversingValue;
        public bool AlwaysActivateCoreBlocksValue;
        public bool AlwaysExplodeSpinnersValue;
        public bool CustomSwimmingValue;
        public bool VerticalDashSpeedPreservationValue;
        public bool DashesDontResetSpeedValue;
        public bool HyperAndSuperSpeedPreservationValue;
        public bool RemoveNormalEndValue;
        public bool PickupSpeedReversalValue;
        public bool AllowHoldableClimbjumpingValue;
        public bool WallBoostDirectionBasedOnOppositeSpeedValue;
        public bool WallBoostSpeedIsAlwaysOppositeSpeedValue;
        public bool ReverseDashSpeedPreservationValue;


        public GooberPhysics(EntityData data, Vector2 offset) : base(data, offset) {
            CobwobSpeedInversionValue = data.Bool("cobwobSpeedInversion", false);
            AllowRetentionReverseValue = data.Bool("allowRetentionReverse", false);
            JumpInversionValue = data.Bool("jumpInversion", false);
            AllowClimbJumpInversionValue = data.Bool("allowClimbJumpInversion", false);
            KeepDashAttackOnCollisionValue = data.Bool("keepDashAttackOnCollision", false);
            ReboundInversionValue = data.Bool("reboundInversion", false);
            WallbounceSpeedPreservationValue = data.Bool("wallbounceSpeedPreservation", false);
            DreamBlockSpeedPreservationValue = data.Bool("dreamBlockSpeedPreservation", false);
            SpringSpeedPreservationValue = data.Bool("springSpeedPreservation", false);
            WallJumpSpeedPreservationValue = data.Bool("wallJumpSpeedPreservation", false);
            GetClimbJumpSpeedInRetainedFramesValue = data.Bool("getClimbJumpSpeedInRetainedFrames", false);
            CustomFeathersValue = data.Bool("customFeathers", false);
            ExplodeLaunchSpeedPreservationValue = data.Bool("explodeLaunchSpeedPreservation", false);
            BadelineBossSpeedReversingValue = data.Bool("badelineBossSpeedReversing", false);
            AlwaysActivateCoreBlocksValue = data.Bool("alwaysActivateCoreBlocks", false);
            AlwaysExplodeSpinnersValue = data.Bool("alwaysExplodeSpinners", false);
            CustomSwimmingValue = data.Bool("customSwimming", false);
            VerticalDashSpeedPreservationValue = data.Bool("verticalDashSpeedPreservation", false);
            DashesDontResetSpeedValue = data.Bool("dashesDontResetSpeed", false);
            HyperAndSuperSpeedPreservationValue = data.Bool("hyperAndSuperSpeedPreservation", false);
            RemoveNormalEndValue = data.Bool("removeNormalEnd", false);
            PickupSpeedReversalValue = data.Bool("pickupSpeedReversal", false);
            AllowHoldableClimbjumpingValue = data.Bool("allowHoldableClimbjumping", false);  
            WallBoostDirectionBasedOnOppositeSpeedValue = data.Bool("wallBoostDirectionBasedOnOppositeSpeed", false);
            WallBoostSpeedIsAlwaysOppositeSpeedValue = data.Bool("wallBoostSpeedIsAlwaysOppositeSpeed", false);
            ReverseDashSpeedPreservationValue = data.Bool("reverseDashSpeedPreservation", false);
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
            GooberHelperModule.Session.DreamBlockSpeedPreservation = DreamBlockSpeedPreservationValue;
            GooberHelperModule.Session.SpringSpeedPreservation = SpringSpeedPreservationValue;
            GooberHelperModule.Session.WallJumpSpeedPreservation = WallJumpSpeedPreservationValue;
            GooberHelperModule.Session.GetClimbJumpSpeedInRetainedFrames = GetClimbJumpSpeedInRetainedFramesValue;
            GooberHelperModule.Session.CustomFeathers = CustomFeathersValue;
            GooberHelperModule.Session.ExplodeLaunchSpeedPreservation = ExplodeLaunchSpeedPreservationValue;
            GooberHelperModule.Session.BadelineBossSpeedReversing = BadelineBossSpeedReversingValue;
            GooberHelperModule.Session.AlwaysActivateCoreBlocks = AlwaysActivateCoreBlocksValue;
            GooberHelperModule.Session.AlwaysExplodeSpinners = AlwaysExplodeSpinnersValue;
            GooberHelperModule.Session.CustomSwimming = CustomSwimmingValue;
            GooberHelperModule.Session.VerticalDashSpeedPreservation = VerticalDashSpeedPreservationValue;
            GooberHelperModule.Session.DashesDontResetSpeed = DashesDontResetSpeedValue;
            GooberHelperModule.Session.HyperAndSuperSpeedPreservation = HyperAndSuperSpeedPreservationValue;
            GooberHelperModule.Session.RemoveNormalEnd = RemoveNormalEndValue;
            GooberHelperModule.Session.PickupSpeedReversal = PickupSpeedReversalValue;
            GooberHelperModule.Session.AllowHoldableClimbjumping = AllowHoldableClimbjumpingValue;
            GooberHelperModule.Session.WallBoostDirectionBasedOnOppositeSpeed = WallBoostDirectionBasedOnOppositeSpeedValue;
            GooberHelperModule.Session.WallBoostSpeedIsAlwaysOppositeSpeed = WallBoostSpeedIsAlwaysOppositeSpeedValue;
            GooberHelperModule.Session.ReverseDashSpeedPreservation = ReverseDashSpeedPreservationValue;
        }
    }
}