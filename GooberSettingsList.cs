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
            
            foreach(PropertyInfo settingGroupProperty in typeof(GooberHelperModuleSettings).GetProperties()) {
                object settingGroupValue = typeof(GooberHelperModuleSettings).GetProperty(settingGroupProperty.Name).GetValue(GooberHelperModule.Settings);

                // if(settingGroupProperty.Name == "ShowActiveSettings" || settingGroupProperty.Name == "Visuals") continue;
                if(settingGroupProperty.Name == "ShowActiveSettings") continue;

                foreach(PropertyInfo settingProperty in settingGroupValue.GetType().GetProperties()) {
                    // object settingValue = settingGroupValue.GetType().GetProperty(settingProperty.Name).GetValue(settingGroupValue);

                    object settingValue = settingProperty.GetValue(settingGroupValue);
                    object sessionValue = null;
                    try {
                        sessionValue = typeof(GooberHelperModuleSession).GetProperty(settingProperty.Name).GetValue(GooberHelperModule.Session);
                    } catch {}

                    if(
                        (settingValue.GetType() == typeof(float) && (float)settingValue != -1f && !GooberHelperModule.Settings.DisableSettings) || 
                        (settingValue.GetType() == typeof(bool) && (bool)settingValue == true && !GooberHelperModule.Settings.DisableSettings) ||
                        (sessionValue?.GetType() == typeof(float) && (float)sessionValue != -1f) || 
                        (sessionValue?.GetType() == typeof(bool) && (bool)sessionValue == true)
                    ) {
                        string str = settingProperty.Name.ToString();

                        if(sessionValue?.GetType() == typeof(float)) {
                            str += $" ({((float)settingValue == -1f ? (float)sessionValue : (float)settingValue)})";
                        }

                        ActiveFont.Draw(str, new Vector2(0, offset + 128), new Vector2(0, 0), new Vector2(0.4f), new Color(1, 1, 1, 0.8f));

                        offset += ActiveFont.FontSize.LineHeight / 2;
                    }
                }
            }
        }
    }
}