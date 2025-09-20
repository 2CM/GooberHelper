using System;
using System.Collections.Generic;
using Celeste.Editor;
using Celeste.Mod.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste.Mod.GooberHelper {
    public static class DebugMapThings {
        public static MouseSmoother MouseSmoother = new MouseSmoother();

        public static float AttractStrength;

        public static Color FluidStaticColor;
        public static Color FluidMovingColor;
        public static float FluidBlobSize;

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

        public static void RandomizeFluidColor() {
            FluidStaticColor = Calc.HsvToColor(Random.Shared.NextFloat(), Random.Shared.NextFloat(), Random.Shared.NextFloat()) * 0.5f;
        }

        public static void ResetThings() {
            AttractStrength = 0;

            FluidStaticColor = new Color(0, 0.5f, 1f, 1f) * 0.5f;
            FluidMovingColor = new Color(1f, 1f, 1f, 1f) * 0.5f;
            FluidBlobSize = 2;
        }
    }
}