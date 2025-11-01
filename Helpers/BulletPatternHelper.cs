using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;
using NLua;

namespace Celeste.Mod.GooberHelper {
    public static class BulletPatternHelper {
        public static LuaTable CreateStar(Vector2 center, float r1, float r2, float initialRotation, int tips = 5, int resolution = 8) {
            LuaTable table = Everest.LuaLoader.Context.DoString("return {}").FirstOrDefault() as LuaTable;
            int ptr = 1;
            
            float initialRotationRadians = initialRotation / 180 * MathF.PI;     

            for(float j = 0; j < tips; j++) {
                var a = center + Calc.AngleToVector(2f * (j + 0.0f) / tips * MathF.PI + initialRotationRadians, r1);
                var b = center + Calc.AngleToVector(2f * (j + 0.5f) / tips * MathF.PI + initialRotationRadians, r2);
                var c = center + Calc.AngleToVector(2f * (j + 1.0f) / tips * MathF.PI + initialRotationRadians, r1);

                for(float i = 0; i < resolution; i++) {
                    table[ptr] = Vector2.Lerp(a, b, i / resolution);
                    table[ptr + resolution] = Vector2.Lerp(b, c, i / resolution);

                    ptr++;
                }

                ptr += resolution;
            }

            return table;
        }
    }
}