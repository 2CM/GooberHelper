using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.GooberHelper.Entities {

    [CustomEntity("GooberHelper/GooberPhysicsOptions")]
    [Tracked(false)]
    public class GooberPhysicsOptions : AbstractTrigger<GooberPhysicsOptions> {
        public GooberPhysicsOptions(EntityData data, Vector2 offset) : base(data, offset, false, [
            "CobwobSpeedInversion",
            "AllowRetentionReverse",
            "JumpInversion",
            "AllowClimbJumpInversion",
            "KeepDashAttackOnCollision",
            "ReboundInversion",
            "WallbounceSpeedPreservation",
            "DreamBlockSpeedPreservation",
            "SpringSpeedPreservation",
            "WallJumpSpeedPreservation",
            "GetClimbJumpSpeedInRetainedFrames",
            "CustomFeathers",
            "FeatherEndSpeedPreservation",
            "ExplodeLaunchSpeedPreservation",
            "BadelineBossSpeedReversing",
            "AlwaysActivateCoreBlocks",
            "CustomSwimming",
            "VerticalDashSpeedPreservation",
            "DashesDontResetSpeed",
            "HyperAndSuperSpeedPreservation",
            "RemoveNormalEnd",
            "PickupSpeedReversal",
            "AllowHoldableClimbjumping",
            "WallBoostDirectionBasedOnOppositeSpeed",
            "WallBoostSpeedIsAlwaysOppositeSpeed",
            "ReverseDashSpeedPreservation",
            "KeepSpeedThroughVerticalTransitions",
            "BubbleSpeedPreservation",
            "AdditiveVerticalJumpSpeed",
            "WallJumpSpeedInversion",
            "AllDirectionHypersAndSupers",
            "AllDirectionHypersAndSupersWorkWithCoyoteTime",
        ]) {}
    }
}