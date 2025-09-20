using System.Collections.Generic;
using Celeste.Editor;
using Celeste.Mod.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste.Mod.GooberHelper {
    public static class DebugMapThings {
        public static float AttractStrength = 0;
        public static MouseSmoother MouseSmoother = new MouseSmoother();
        public static Color FluidStaticColor = new Color(0, 0.5f, 1f, 1f) * 0.5f;
        public static Color FluidMovingColor = new Color(1f, 1f, 1f, 1f) * 0.5f;

        public static void Load() {
            MapEditorHooks.Load();
            LevelTemplateHooks.Load();
            MouseSmoother.Load();
        }

        public static void Unload() {
            MapEditorHooks.Unload();
            LevelTemplateHooks.Unload();
            MouseSmoother.Unload();
        }
    }
}