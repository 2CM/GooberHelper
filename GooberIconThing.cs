using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;
using System.Reflection;
using System.Linq;
using static Celeste.Mod.GooberHelper.OptionsManager;

namespace Celeste.Mod.GooberHelper.Entities {
    [Tracked(false)]
    public class GooberIconThing : Entity {
        MTexture icon;

        public GooberIconThing() {
            base.Tag = Tags.HUD | Tags.Global;
            base.Depth = -1000;

            icon = GFX.Gui["ourple"];
        }

        public override void Render() {
            float x = 0f;

            if(GetUserEnabledEvilOption()) { icon.Draw(new Vector2(x, 1080 - 32), Vector2.Zero, UserDefinedEvilColor); x += 32f; }
            if(GetUserEnabledCoolOption()) { icon.Draw(new Vector2(x, 1080 - 32), Vector2.Zero, UserDefinedCoolColor); }
        }
    }
}