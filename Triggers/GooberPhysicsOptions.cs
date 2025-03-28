using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.GooberHelper.Entities {

    [CustomEntity("GooberHelper/GooberPhysicsOptions")]
    [Tracked(false)]
    public class GooberPhysicsOptions : AbstractTrigger {
        StackThing stackThing;

        public GooberPhysicsOptions(EntityData data, Vector2 offset) : base(data, offset, [
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
        ]) {

        }

        public override void OnLeave(Player player)
        {
            base.OnLeave(player);

            stack.Remove(this.stackThing);

            this.updateStack();
        }

        public override void OnEnter(Player player)
        {
            // this.settingValuesBefore = typeof(GooberHelperModuleSession).GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(GooberHelperModule.Session));

            // foreach(GooberPhysicsOptions item in Engine.Scene.Tracker.GetEntities<GooberPhysicsOptions>().Cast<GooberPhysicsOptions>().Where(a => a.GetType() == this.GetType() && a.Activated)) {
            //     // item.reversionEntity = this;
            //     // item.hasReversionEntity = true;
            //     this.reversionEntity = item;
            //     this.hasReversionEntity = true;
            // }

            // this.Activated = true;

            // foreach(var item in this.settingValues) {
            //     typeof(GooberHelperModuleSession).GetProperty(item.Key).SetValue(GooberHelperModule.Session, item.Value);
            // }

            base.OnEnter(player);

            this.stackThing = new StackThing(this.settingValues, this);

            stack.Add(this.stackThing);

            this.updateStack();
        }
    }
}