using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;
using System.Reflection;
using System.Linq;

namespace Celeste.Mod.GooberHelper.Entities {
    public class GooberIconThing : Entity {
        MTexture icon;

        public GooberIconThing() {
            base.Tag = Tags.HUD | Tags.Global;
            base.Depth = -1000;

            icon = GFX.Gui["ourple"];
        }

        public override void Render() {
            bool drawMain = false;
            bool drawGoldenBlocks = GooberHelperModule.Settings.Miscellaneous.GoldenBlocksAlwaysLoad;

            if(GooberHelperModule.Settings.DisableSettings) return;

            foreach(PropertyInfo prop1 in typeof(GooberHelperModuleSettings).GetProperties()) {
                object value1 = typeof(GooberHelperModuleSettings).GetProperty(prop1.Name).GetValue(GooberHelperModule.Settings);

                if(prop1.Name == "ShowActiveSettings" || prop1.Name == "Visuals") continue;

                foreach(PropertyInfo prop2 in value1.GetType().GetProperties()) {
                    object value2 = value1.GetType().GetProperty(prop2.Name).GetValue(value1);
                    
                    if(prop2.Name == "GoldenBlocksAlwaysLoad") continue;

                    if(
                        (value2.GetType() == typeof(int) && (int)value2 != -1) ||
                        (value2.GetType() == typeof(bool) && (bool)value2 == true)
                    ) {
                        drawMain = true;

                        break;
                    }
                }
            }

            float x = 0f;

            if(drawMain) { icon.Draw(new Vector2(x, 1080 - 32), Vector2.Zero, new Color(0.5f,0.5f,1f,0.2f)); x += 32f; }
            if(drawGoldenBlocks) { icon.Draw(new Vector2(x, 1080 - 32), Vector2.Zero, new Color(1f,0.5f,0f,0.2f)); x += 32f; }
        }
    }
}