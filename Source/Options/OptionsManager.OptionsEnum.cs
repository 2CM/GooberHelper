//this file is literally just for the enum
//lmao

namespace Celeste.Mod.GooberHelper.Options {
    public static partial class OptionsManager {
        public enum Option {
            //jumping
            CobwobSpeedInversion,
            JumpInversion,
            WalljumpSpeedPreservation,
            WallbounceSpeedPreservation,
            HyperAndSuperSpeedPreservation,
            UpwardsJumpSpeedPreservationThreshold,
            DownwardsJumpSpeedPreservationThreshold,
            BounceHelperBounceSpeedPreservation,

            GetClimbjumpSpeedInRetention,
            AdditiveVerticalJumpSpeed,
            SwapHorizontalAndVerticalSpeedOnWalljump,
            VerticalToHorizontalSpeedOnGroundJump,
            CornerboostBlocksEverywhere,

            AllDirectionHypersAndSupers,
            AllowUpwardsCoyote,
            AllDirectionDreamJumps,
            AllowHoldableClimbjumping,

            //dashing
            VerticalDashSpeedPreservation,
            ReverseDashSpeedPreservation,

            MagnitudeBasedDashSpeed,

            DashesDontResetSpeed,
            KeepDashAttackOnCollision,
            DownDemoDashing,

            //moving
            HorizontalTurningSpeedInversion,
            VerticalTurningSpeedInversion,

            WallboostDirectionIsOppositeSpeed,
            WallboostSpeedIsOppositeSpeed,
            DownwardsAirFrictionBehavior,
            IgnoreForcemove,

            //entities
            RefillFreezeLength,

            DreamBlockSpeedPreservation,
            SpringSpeedPreservation,
            ReboundSpeedPreservation,
            PointBounceSpeedPreservation,
            ReflectBounceSpeedInversion,
            ExplodeLaunchSpeedPreservation,
            PickupSpeedInversion,
            BubbleSpeedPreservation,
            FeatherEndSpeedPreservation,
            BadelineBossSpeedPreservation,

            CustomFeathers,
            CustomSwimming,
            CustomSwimmingWalljumpSpeed,
            CustomSwimmingLaunchSpeed,
            CustomSwimmingLaunchThreshold,
            LenientStunning,
            HoldableSpeedInheritanceHorizontal,
            HoldableSpeedInheritanceVertical,
            ReverseBackboosts,
            
            AllowCrouchedHoldableGrabbing,
            CoreBlockAllDirectionActivation,

            //other
            RetentionLength,

            ConserveBeforeDashSpeed,
            ClimbingSpeedPreservation,
            UpwardsTransitionSpeedPreservation,

            FastFallHitboxSquish,
            LiftboostAdditionHorizontal,
            LiftboostAdditionVertical,
            AdvantageousLiftboost,
            StackableUltras,

            AllowUpwardsClimbGrabbing,
            AllowCrouchedClimbGrabbing,
            AllowClimbingInDashState,
            AllowWindWhileDashing,
            RemoveNormalEnd,

            //visuals
            PlayerShaderMask,
            TheoNuclearReactor,
            RotatePlayerToSpeed,
            CustomSwimmingAnimation,

            //miscellaneous
            AlwaysExplodeSpinners,
            GoldenBlocksAlwaysLoad,
            RefillFreezeGameSuspension,
            BufferDelayVisualization,
            Ant,

            //general
            ShowActiveOptions,
        }
    }
}