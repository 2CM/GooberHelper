using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.GooberHelper.Entities {
    [CustomEntity("GooberHelper/RefillFreezeLength")]
    [Tracked(false)]
    public class RefillFreezeLength : AbstractTrigger<RefillFreezeLength> {
        public RefillFreezeLength(EntityData data, Vector2 offset) : base(data, offset, OptionsManager.OptionType.Float, ["RefillFreezeLength"], new Dictionary<string, string>()) {}
    }
}