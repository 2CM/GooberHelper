using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using static Celeste.Mod.GooberHelper.OptionsManager;

namespace Celeste.Mod.GooberHelper.Entities {

    [CustomEntity("GooberHelper/GooberPhysicsOptions")]
    [Tracked(false)]
    public class GooberPhysicsOptions : AbstractTrigger<GooberPhysicsOptions> {
        public GooberPhysicsOptions(EntityData data, Vector2 offset) : base(data, offset, OptionType.Boolean, [
            "CobwobSpeedInversion",
            "JumpInversion",
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
            "AlwaysActivateCoreBlocks",
            "CustomSwimming",
            "ReverseDashSpeedPreservation",
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
            "AllDirectionHypersAndSupers",
            "VerticalDashSpeedPreservation",
            "BadelineBossSpeedReversing",
        ],
        new Dictionary<string, string>() {
            {"ReboundInversion", "ReboundSpeedPreservation"},
            {"GetClimbJumpSpeedInRetainedFrames", "GetClimbjumpSpeedInRetention"},
            {"AlwaysActivateCoreBlocks", "CoreBlockAllDirectionActivation"},
            {"WallBoostDirectionBasedOnOppositeSpeed", "WallboostDirectionIsOppositeSpeed"},
            {"WallBoostSpeedIsAlwaysOppositeSpeed", "WallboostSpeedIsOppositeSpeed"},
            {"KeepSpeedThroughVerticalTransitions", "UpwardsTransitionSpeedPreservation"},
            {"PickupSpeedReversal", "PickupSpeedInversion"},
            {"WallJumpSpeedPreservation", "WalljumpSpeedPreservation"},
            {"ShowActiveSettings", "ShowActiveOptions"},
            {"BadelineBossSpeedReversing", "BadelineBossSpeedPreservation"},
        }) {
            //backwards compatibility!!!!
            this.SettingValues[Option.UpwardsJumpSpeedPreservationThreshold] = data.Bool("verticalDashSpeedPreservation") ? 240f : Options[Option.UpwardsJumpSpeedPreservationThreshold].DefaultValue;
            if(data.Bool("cobwobSpeedInversion") && data.Bool("allowRetentionReverse")) this.SettingValues[Option.CobwobSpeedInversion] = (float)CobwobSpeedInversionValue.WorkWithRetention;
            if(data.Bool("jumpInversion") && data.Bool("allowClimbJumpInversion")) this.SettingValues[Option.JumpInversion] = (float)JumpInversionValue.All;
            if(data.Bool("allDirectionHypersAndSupers") && data.Bool("allDirectionHypersAndSupersWorkWithCoyoteTime")) this.SettingValues[Option.AllDirectionHypersAndSupers] = (float)AllDirectionHypersAndSupersValue.WorkWithCoyoteTime;
            if(data.Bool("wallJumpSpeedInversion")) this.SettingValues[Option.WalljumpSpeedPreservation] = (float)WalljumpSpeedPreservationValue.Invert;
            if(data.Bool("customFeathers")) this.SettingValues[Option.CustomFeathers] = (float)CustomFeathersValue.SkipIntro;
            if(data.Bool("springSpeedPreservation")) this.SettingValues[Option.SpringSpeedPreservation] = (float)SpringSpeedPreservationValue.Invert;
        }
    }
}