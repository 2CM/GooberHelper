using System;
using System.Collections.Generic;
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
            "ReverseDashSpeedPreservation",
            "MagnitudeBasedDashSpeed",
            "MagnitudeBasedDashSpeedOnlyCardinal",
            "DashesDontResetSpeed",
            "HyperAndSuperSpeedPreservation",
            "RemoveNormalEnd",
            "PickupSpeedReversal",
            "AllowHoldableClimbjumping",
            "WallBoostDirectionBasedOnOppositeSpeed",
            "WallBoostSpeedIsAlwaysOppositeSpeed",
            "KeepSpeedThroughVerticalTransitions",
            "BubbleSpeedPreservation",
            "AdditiveVerticalJumpSpeed",
            "WallJumpSpeedInversion",
            "AllDirectionHypersAndSupers",
            "AllDirectionHypersAndSupersWorkWithCoyoteTime",
            "AllowUpwardsCoyote",
            "AllDirectionDreamJumps",
            "LenientStunning",
            "HorizontalTurningSpeedInversion",
            "VerticalTurningSpeedInversion",
            "AllowCrouchedHoldableGrabbing",
            "HoldablesInheritSpeedWhenThrown",
            "UpwardsJumpSpeedPreservation",
            "DownwardsJumpSpeedPreservation",
            "DownwardsAirFrictionBehavior",
            "CornerboostBlocksEverywhere",
            "SwapHorizontalAndVerticalSpeedOnWallJump",
            "VerticalSpeedToHorizontalSpeedOnGroundJump",
        ]) {
            //backwards compatibility!!!!
            if(data.Bool("verticalDashSpeedPreservation") && !data.Has("upwardsJumpSpeedPreservation")) {
                this.settingValues["VerticalDashSpeedPreservation_old"] = true;
                this.settingValues["UpwardsJumpSpeedPreservation"] = true;
            }
        }
    }
}