using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;
using System.Reflection;
using System.Collections.Generic;
using MonoMod.Utils;
using static Celeste.Mod.GooberHelper.OptionsManager;

namespace Celeste.Mod.GooberHelper.Entities {
    [Tracked(false)]
    public class GooberSettingsList : Entity {
        public static Dictionary<string, FastReflectionHelper.FastInvoker> optionGetters;

        public GooberSettingsList() {
            base.Tag = Tags.HUD | Tags.Global;
            base.Depth = 10000;
        }

        public override void Render() {
            if(!GetOptionBool(Option.ShowActiveOptions)) return;

            int pad = 8;

            ActiveFont.Draw(
                GetEnabledOptionsString(),
                new Vector2(1920 - pad, pad),
                new Vector2(1f, 0),
                new Vector2(0.4f),
                new Color(1, 1, 1, 0.8f)
            );
        }
    }
}