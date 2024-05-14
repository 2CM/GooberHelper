using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;
using System.Reflection;

namespace Celeste.Mod.GooberHelper.Entities {
    public class GooberIconThing : Entity {
        MTexture icon;

        public GooberIconThing() {
            base.Tag = Tags.HUD | Tags.Global;
            base.Depth = 10000;

            icon = GFX.Gui["ourple"];
        }

        public override void Render() {
            bool draw = false;

            foreach(PropertyInfo prop in typeof(GooberHelperModuleSettings).GetProperties()) {
                object value = typeof(GooberHelperModuleSettings).GetProperty(prop.Name).GetValue(GooberHelperModule.Settings);

                if(value.GetType() == typeof(Int32)) {
                    if((prop.Name == "RefillFreezeLength" || prop.Name == "RetentionFrames") && (Int32)value != -1) {
                        draw = true;

                        break;
                    }
                } else {
                    if((bool)value == true) {
                        draw = true;

                        break;
                    }
                }
            }

            if(draw) {
                icon.Draw(new Vector2(0, 1080-64), Vector2.Zero, new Color(1,1,1,1f));
            }
        }
    }
}