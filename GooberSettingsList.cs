using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;
using System.Reflection;

namespace Celeste.Mod.GooberHelper.Entities {
    public class GooberSettingsList : Entity {
        public GooberSettingsList() {
            base.Tag = Tags.HUD | Tags.Global;
            base.Depth = 10000;
        }

        public override void Render() {
            int offset = 0;

            if(!(GooberHelperModule.Settings.ShowActiveSettings || GooberHelperModule.Session.ShowActiveSettings)) return;
            
            foreach(PropertyInfo prop1 in typeof(GooberHelperModuleSettings).GetProperties()) {
                object value1 = typeof(GooberHelperModuleSettings).GetProperty(prop1.Name).GetValue(GooberHelperModule.Settings);

                if(prop1.Name == "ShowActiveSettings" || prop1.Name == "Visuals") continue;

                foreach(PropertyInfo prop2 in value1.GetType().GetProperties()) {
                    object value2 = value1.GetType().GetProperty(prop2.Name).GetValue(value1);

                    object value3 = prop2.GetValue(value1);
                    object value4 = null;
                    try {
                        value4 = typeof(GooberHelperModuleSession).GetProperty(prop2.Name).GetValue(GooberHelperModule.Session);
                    } catch {}

                    if(
                        (value3.GetType() == typeof(float) && (float)value3 != -1f) || 
                        (value3.GetType() == typeof(bool) && (bool)value3 == true) ||
                        (value4?.GetType() == typeof(float) && (float)value4 != -1f) || 
                        (value4?.GetType() == typeof(bool) && (bool)value4 == true)
                    ) {
                        string str = prop2.Name.ToString();

                        if(value3.GetType() == typeof(float)) {
                            str += $" ({((float)value3 == -1f ? (float)value4 : (float)value3)})";
                        }

                        ActiveFont.Draw(str, new Vector2(0,offset + 128), new Vector2(0,0), new Vector2(0.4f), new Color(1,1,1,0.8f));

                        offset += ActiveFont.FontSize.LineHeight / 2;
                    }
                }
            }
        }
    }
}