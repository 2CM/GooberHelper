using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;

namespace Celeste.Mod.GooberHelper.Entities {

    [CustomEntity("GooberHelper/RetentionFrames")]
    [Tracked(false)]
    public class RetentionFrames : AbstractTrigger<RetentionFrames> {
        public RetentionFrames(EntityData data, Vector2 offset) : base(data, offset, -1, ["RetentionFrames"]) {}
    }
}