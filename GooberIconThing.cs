using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;
using System.Reflection;
using System.Linq;

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
            bool drawGoldenBlocksIndicator = GooberHelperModule.Settings.UserDefinedOptions.TryGetValue(OptionsManager.Option.GoldenBlocksAlwaysLoad, out float value) ? value == 1 : false;
            bool drawMainIndicator = GooberHelperModule.Settings.UserDefinedOptions.Any(a =>
                OptionsManager.Options[a.Key].Category != "Visuals" &&
                a.Key != OptionsManager.Option.GoldenBlocksAlwaysLoad &&
                a.Key != OptionsManager.Option.ShowActiveSettings
            );

            float x = 0f;

            if(drawMainIndicator) { icon.Draw(new Vector2(x, 1080 - 32), Vector2.Zero, new Color(0.5f,0.5f,1f,0.2f)); x += 32f; }
            if(drawGoldenBlocksIndicator) { icon.Draw(new Vector2(x, 1080 - 32), Vector2.Zero, new Color(1f,0.5f,0f,0.2f)); x += 32f; }
        }
    }
}