using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.GooberHelper.Entities {

    [CustomEntity("GooberHelper/RetentionFrames")]
    [Tracked(false)]
    public class RetentionFrames : AbstractTrigger<RetentionFrames> {
        public RetentionFrames(EntityData data, Vector2 offset) : base(data, offset, OptionsManager.OptionType.Float, ["RetentionFrames"], new Dictionary<string, string>() {{"RetentionFrames", "RetentionLength"}}) {}
    }
}