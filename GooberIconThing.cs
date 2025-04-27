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
            bool draw = false;

            if(GooberHelperModule.Settings.DisableSettings) return;

            foreach(PropertyInfo prop1 in typeof(GooberHelperModuleSettings).GetProperties()) {
                object value1 = typeof(GooberHelperModuleSettings).GetProperty(prop1.Name).GetValue(GooberHelperModule.Settings);

                if(prop1.Name == "ShowActiveSettings" || prop1.Name == "Visuals") continue;

                foreach(PropertyInfo prop2 in value1.GetType().GetProperties()) {
                    object value2 = value1.GetType().GetProperty(prop2.Name).GetValue(value1);

                    if(
                        (value2.GetType() == typeof(int) && (int)value2 != -1) ||
                        (value2.GetType() == typeof(bool) && (bool)value2 == true)
                    ) {
                        draw = true;

                        break;
                    }
                }
            }

            if(draw) {
                icon.Draw(new Vector2(-32, 1080-32), Vector2.Zero, new Color(1,1,1,0.2f));
            }
        }
    }
}