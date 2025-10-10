using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;
using static Celeste.Mod.GooberHelper.OptionsManager;

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
        public bool KeepSpeedThroughVerticalTransitionsValue;
        public bool BubbleSpeedPreservationValue;
        public bool ShowActiveSettings;

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
            KeepSpeedThroughVerticalTransitionsValue = data.Bool("keepSpeedThroughVerticalTransitions", false);
            BubbleSpeedPreservationValue = data.Bool("bubbleSpeedPreservation", false);
            ShowActiveSettings = data.Bool("showActiveSettings", false);
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            SetOptionValue(Option.CobwobSpeedInversion, CobwobSpeedInversionValue ? AllowRetentionReverseValue ? 2 : 1 : 0, OptionSetter.Map);
            SetOptionValue(Option.JumpInversion, JumpInversionValue ? AllowClimbJumpInversionValue ? 2 : 1 : 0, OptionSetter.Map);
            SetOptionValue(Option.KeepDashAttackOnCollision, KeepDashAttackOnCollisionValue ? 1 : 0, OptionSetter.Map);
            SetOptionValue(Option.ReboundSpeedPreservation, ReboundInversionValue ? 1 : 0, OptionSetter.Map);
            SetOptionValue(Option.WallbounceSpeedPreservation, WallbounceSpeedPreservationValue ? 1 : 0, OptionSetter.Map);
            SetOptionValue(Option.DreamBlockSpeedPreservation, DreamBlockSpeedPreservationValue ? 1 : 0, OptionSetter.Map);
            SetOptionValue(Option.SpringSpeedPreservation, SpringSpeedPreservationValue ? 2 : 0, OptionSetter.Map);
            SetOptionValue(Option.WalljumpSpeedPreservation, WallJumpSpeedPreservationValue ? 1 : 0, OptionSetter.Map);
            SetOptionValue(Option.GetClimbjumpSpeedInRetention, GetClimbJumpSpeedInRetainedFramesValue ? 1 : 0, OptionSetter.Map);
            SetOptionValue(Option.CustomFeathers, CustomFeathersValue ? 2 : 0, OptionSetter.Map);
            SetOptionValue(Option.ExplodeLaunchSpeedPreservation, ExplodeLaunchSpeedPreservationValue ? 1 : 0, OptionSetter.Map);
            SetOptionValue(Option.BadelineBossSpeedPreservation, BadelineBossSpeedReversingValue ? 1 : 0, OptionSetter.Map);
            SetOptionValue(Option.CoreBlockAllDirectionActivation, AlwaysActivateCoreBlocksValue ? 1 : 0, OptionSetter.Map);
            SetOptionValue(Option.AlwaysExplodeSpinners, AlwaysExplodeSpinnersValue ? 1 : 0, OptionSetter.Map);
            SetOptionValue(Option.CustomSwimming, CustomSwimmingValue ? 1 : 0, OptionSetter.Map);
            SetOptionValue(Option.VerticalDashSpeedPreservation, VerticalDashSpeedPreservationValue ? 1 : 0, OptionSetter.Map);
            SetOptionValue(Option.DashesDontResetSpeed, DashesDontResetSpeedValue ? 1 : 0, OptionSetter.Map);
            SetOptionValue(Option.HyperAndSuperSpeedPreservation, HyperAndSuperSpeedPreservationValue ? 1 : 0, OptionSetter.Map);
            SetOptionValue(Option.RemoveNormalEnd, RemoveNormalEndValue ? 1 : 0, OptionSetter.Map);
            SetOptionValue(Option.PickupSpeedInversion, PickupSpeedReversalValue ? 1 : 0, OptionSetter.Map);
            SetOptionValue(Option.AllowHoldableClimbjumping, AllowHoldableClimbjumpingValue ? 1 : 0, OptionSetter.Map);
            SetOptionValue(Option.WallboostDirectionIsOppositeSpeed, WallBoostDirectionBasedOnOppositeSpeedValue ? 1 : 0, OptionSetter.Map);
            SetOptionValue(Option.WallboostDirectionIsOppositeSpeed, WallBoostSpeedIsAlwaysOppositeSpeedValue ? 1 : 0, OptionSetter.Map);
            SetOptionValue(Option.ReverseDashSpeedPreservation, ReverseDashSpeedPreservationValue ? 1 : 0, OptionSetter.Map);
            SetOptionValue(Option.UpwardsTransitionSpeedPreservation, KeepSpeedThroughVerticalTransitionsValue ? 1 : 0, OptionSetter.Map);
            SetOptionValue(Option.BubbleSpeedPreservation, BubbleSpeedPreservationValue ? 1 : 0, OptionSetter.Map);
            SetOptionValue(Option.ShowActiveOptions, ShowActiveSettings ? 1 : 0, OptionSetter.Map);

            //backwards compatibility!!!!
            SetOptionValue(Option.UpwardsJumpSpeedPreservationThreshold, VerticalDashSpeedPreservationValue ? 240 : Options[Option.UpwardsJumpSpeedPreservationThreshold].DefaultValue, OptionSetter.Map);
        }
    }
}