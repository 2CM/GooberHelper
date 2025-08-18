using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.GooberHelper.Entities {

    [CustomEntity("GooberHelper/GooberVisualOptions")]
    [Tracked(false)]
    public class GooberVisualOptions : AbstractTrigger<GooberVisualOptions> {
        public GooberVisualOptions(EntityData data, Vector2 offset) : base(data, offset, false, [
            "PlayerMask",
            "PlayerMaskHairOnly",
            "TheoNuclearReactor",
        ]) {}
    }
}