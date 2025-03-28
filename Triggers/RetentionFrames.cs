using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;

namespace Celeste.Mod.GooberHelper.Entities {

    [CustomEntity("GooberHelper/RetentionFrames")]
    public class RetentionFrames : Trigger {
        private float RetentionFramesValue;

        public RetentionFrames(EntityData data, Vector2 offset) : base(data, offset) {
            RetentionFramesValue = data.Float("retentionFrames", 4f);
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            GooberHelperModule.Session.RetentionFrames = RetentionFramesValue;
        }
    }
}