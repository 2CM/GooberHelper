using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;
using System.Reflection;
using System.Collections.Generic;
using MonoMod.Utils;

namespace Celeste.Mod.GooberHelper.Entities {
    [Tracked(false)]
    public class GooberSettingsList : Entity {
        public static Dictionary<string, FastReflectionHelper.FastInvoker> optionGetters;

        public GooberSettingsList() {
            if(optionGetters == null) {
                optionGetters = new();

                foreach(MethodInfo method in typeof(OptionsManager).GetMethods()) {
                    if(!method.IsPrivate && method.IsStatic) {
                        optionGetters.Add(method.Name.Split("_")[1], method.GetFastInvoker());
                    }
                }
            }

            base.Tag = Tags.HUD | Tags.Global;
            base.Depth = 10000;
        }

        public override void Render() {
            if(!(GooberHelperModule.Settings.ShowActiveSettings || GooberHelperModule.Session.ShowActiveSettings)) return;
            
            string text = "";

            foreach(KeyValuePair<string, FastReflectionHelper.FastInvoker> pair in optionGetters) {
                object value = pair.Value.Invoke(new object(), []);

                if(value is int && (int)value == -1) continue;
                if(value is bool && (bool)value == false) continue;

                text += value is int ? 
                    pair.Key + $" ({value})\n" :
                    pair.Key + "\n";

                // offset += ActiveFont.FontSize.LineHeight / 2;
            }

            int pad = 8;

            ActiveFont.Draw(
                text,
                new Vector2(1920 - pad, pad),
                new Vector2(1f, 0),
                new Vector2(0.4f),
                new Color(1, 1, 1, 0.8f)
            );

            // if(!(GooberHelperModule.Settings.ShowActiveSettings || GooberHelperModule.Session.ShowActiveSettings)) return;
            
            // foreach(KeyValuePair<string, FastReflectionHelper.FastInvoker> pair in optionGetters) {
            //     object value = true;//pair.Value.Invoke(new object(), []);

            //     if(value is int && (int)value == -1) continue;
            //     if(value is bool && (bool)value == false) continue;

            //     ActiveFont.Draw(
            //         value is int ? 
            //             pair.Key + $" ({value})" :
            //             pair.Key,
            //         new Vector2(0, offset + 128),
            //         new Vector2(0, 0),
            //         new Vector2(0.4f),
            //         new Color(1, 1, 1, 0.8f)
            //     );

            //     offset += ActiveFont.FontSize.LineHeight / 2;
            // }

            // foreach(PropertyInfo settingGroupProperty in typeof(GooberHelperModuleSettings).GetProperties()) {
            //     object settingGroupValue = typeof(GooberHelperModuleSettings).GetProperty(settingGroupProperty.Name).GetValue(GooberHelperModule.Settings);

            //     // if(settingGroupProperty.Name == "ShowActiveSettings" || settingGroupProperty.Name == "Visuals") continue;
            //     if(settingGroupProperty.Name == "ShowActiveSettings") continue;

            //     foreach(PropertyInfo settingProperty in settingGroupValue.GetType().GetProperties()) {
            //         // object settingValue = settingGroupValue.GetType().GetProperty(settingProperty.Name).GetValue(settingGroupValue);

            //         object settingValue = settingProperty.GetValue(settingGroupValue);
            //         object sessionValue = null;
            //         try {
            //             sessionValue = typeof(GooberHelperModuleSession).GetProperty(settingProperty.Name).GetValue(GooberHelperModule.Session);
            //         } catch {}

            //         if(
            //             (settingValue.GetType() == typeof(int) && (int)settingValue != -1 && !GooberHelperModule.Settings.DisableSettings) || 
            //             (settingValue.GetType() == typeof(bool) && (bool)settingValue == true && !GooberHelperModule.Settings.DisableSettings) ||
            //             (sessionValue?.GetType() == typeof(int) && (int)sessionValue != -1) || 
            //             (sessionValue?.GetType() == typeof(bool) && (bool)sessionValue == true)
            //         ) {
            //             string str = settingProperty.Name.ToString();

            //             if(sessionValue?.GetType() == typeof(int)) {
            //                 str += $" ({((int)settingValue == -1 ? (int)sessionValue : (int)settingValue)})";
            //             }

            //             ActiveFont.Draw(str, new Vector2(0, offset + 128), new Vector2(0, 0), new Vector2(0.4f), new Color(1, 1, 1, 0.8f));

            //             offset += ActiveFont.FontSize.LineHeight / 2;
            //         }
            //     }
            // }
        }
    }
}