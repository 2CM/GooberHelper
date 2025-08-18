using System;
using System.Collections.Generic;
using Celeste.Mod.GooberHelper.Entities;

namespace Celeste.Mod.GooberHelper {
    public class GooberHelperModuleSession : EverestModuleSession {
        
        public Dictionary<string, List<StackItem>> Stacks { get; set; } = [];

        public bool CobwobSpeedInversion { get; set; } = false;
        public bool AllowRetentionReverse { get; set; } = false;
        public bool JumpInversion { get; set; } = false;
        public bool AllowClimbJumpInversion { get; set; } = false;
        public bool KeepDashAttackOnCollision { get; set; } = false;
        public bool ReboundInversion { get; set; } = false;
        public int RefillFreezeLength { get; set; } = -1;
        public bool WallbounceSpeedPreservation { get; set; } = false;
        public bool DreamBlockSpeedPreservation { get; set; } = false;
        public bool SpringSpeedPreservation { get; set; } = false;
        public bool WallJumpSpeedPreservation { get; set; } = false;
        public bool GetClimbJumpSpeedInRetainedFrames { get; set; } = false;
        public bool CustomFeathers { get; set; } = false;
        public bool FeatherEndSpeedPreservation { get; set; } = false;
        public bool ExplodeLaunchSpeedPreservation { get; set; } = false;
        public bool BadelineBossSpeedReversing { get; set; } = false;
        public bool AlwaysActivateCoreBlocks { get; set; } = false;
        public bool CustomSwimming { get; set; } = false;
        public bool VerticalDashSpeedPreservation { get; set; } = false;
        public bool ReverseDashSpeedPreservation { get; set; } = false;
        public bool DashesDontResetSpeed { get; set; } = false;
        public bool MagnitudeBasedDashSpeed { get; set; } = false;
        public bool MagnitudeBasedDashSpeedOnlyCardinal { get; set; } = false;
        public int RetentionFrames { get; set; } = -1;
        public bool RemoveNormalEnd { get; set; } = false;
        public bool HyperAndSuperSpeedPreservation { get; set; } = false;
        public bool PickupSpeedReversal { get; set; } = false;
        public bool AllowHoldableClimbjumping { get; set; } = false;
        public bool WallBoostDirectionBasedOnOppositeSpeed { get; set; } = false;
        public bool WallBoostSpeedIsAlwaysOppositeSpeed { get; set; } = false;
        public bool KeepSpeedThroughVerticalTransitions { get; set; } = false;
        public bool BubbleSpeedPreservation { get; set; } = false;
        public bool AdditiveVerticalJumpSpeed { get; set; } = false;
        public bool WallJumpSpeedInversion { get; set; } = false;
        public bool AllDirectionHypersAndSupers { get; set; } = false;
        public bool AllDirectionHypersAndSupersWorkWithCoyoteTime { get; set; } = false;
        public bool AllowUpwardsCoyote { get; set; } = false;
        public bool AllDirectionDreamJumps { get; set; } = false;
        public bool LenientStunning { get; set; } = false;
        public bool HorizontalTurningSpeedInversion { get; set; } = false;
        public bool VerticalTurningSpeedInversion { get; set; } = false;
        public bool AllowCrouchedHoldableGrabbing { get; set; } = false;
        public bool HoldablesInheritSpeedWhenThrown { get; set; } = false;
        public bool UpwardsJumpSpeedPreservation { get; set; } = false;
        public bool DownwardsJumpSpeedPreservation { get; set; } = false;
        public bool DownwardsAirFrictionBehavior { get; set; } = false;
        public bool CornerboostBlocksEverywhere { get; set; } = false;
        public bool SwapHorizontalAndVerticalSpeedOnWallJump { get; set; } = false;
        public bool VerticalSpeedToHorizontalSpeedOnGroundJump { get; set; } = false;

        public bool PlayerMask { get; set; } = false;
        public bool PlayerMaskHairOnly { get; set; } = false;
        public bool TheoNuclearReactor { get; set; } = false;

        public bool GoldenBlocksAlwaysLoad { get; set; } = false;
        public bool AlwaysExplodeSpinners { get; set; } = false;

        public bool ShowActiveSettings { get; set; } = false;
        public bool VerticalDashSpeedPreservation_old { get; set; } = false;
    }
}
