using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;

namespace Celeste.Mod.GooberHelper.Entities {
    [CustomEntity("GooberHelper/RefillFreezeLength")]
    [Tracked(false)]
    public class RefillFreezeLength : AbstractTrigger<RefillFreezeLength> {
        public RefillFreezeLength(EntityData data, Vector2 offset) : base(data, offset, -1f, ["RefillFreezeLength"]) {}
    }
}