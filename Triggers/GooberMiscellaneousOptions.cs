using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.GooberHelper.Entities {

    [CustomEntity("GooberHelper/GooberMiscellaneousOptions")]
    [Tracked(false)]
    public class GooberMiscellaneousOptions : AbstractTrigger<GooberMiscellaneousOptions> {
        public GooberMiscellaneousOptions(EntityData data, Vector2 offset) : base(data, offset, OptionsManager.OptionType.Boolean, [
            "AlwaysExplodeSpinners",
            "GoldenBlocksAlwaysLoad",
            "ShowActiveSettings",
        ], new Dictionary<string, string>()) {}
    }
}