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
            
            foreach(PropertyInfo prop in typeof(GooberHelperModuleSettings).GetProperties()) {
                if(prop.Name == "ShowActiveSettings") continue;

                object value1 = prop.GetValue(GooberHelperModule.Settings);
                object value2 = null;
                try {
                    value2 = prop.GetValue(GooberHelperModule.Session);
                } catch {}

                if(
                    (value1.GetType() == typeof(int) && (int)value1 != -1) || 
                    (value1.GetType() == typeof(bool) && (bool)value1 == true) ||
                    (value2?.GetType() == typeof(int) && (int)value2 != -1) || 
                    (value2?.GetType() == typeof(bool) && (bool)value2 == true)
                ) {
                    string str = prop.Name.ToString();

                    if(value1.GetType() == typeof(int)) {
                        str += $" ({((int)value1 == -1 ? (int)value2 : (int)value1)})";
                    }

                    ActiveFont.Draw(str, new Vector2(0,offset + 128), new Vector2(0,0), new Vector2(0.4f), new Color(1,1,1,0.8f));

                    offset += ActiveFont.FontSize.LineHeight / 2;
                }
            }
        }
    }
}