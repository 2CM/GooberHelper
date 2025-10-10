using System.Collections.Generic;
using Celeste.Editor;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.GooberHelper {
    public static class LevelTemplateHooks {
        public static void Load() {
            On.Celeste.Editor.LevelTemplate.ctor_LevelData += modLevelTemplateCtorLevelData;
            On.Celeste.Editor.LevelTemplate.ctor_int_int_int_int += modLevelTemplateCtorIntIntIntInt;
            On.Celeste.Editor.LevelTemplate.RenderContents += modLevelTemplateRenderContents;
            On.Celeste.Editor.LevelTemplate.Move += modLevelTemplateMove;
        }

        public static void Unload() {
            On.Celeste.Editor.LevelTemplate.ctor_LevelData -= modLevelTemplateCtorLevelData;
            On.Celeste.Editor.LevelTemplate.ctor_int_int_int_int -= modLevelTemplateCtorIntIntIntInt;
            On.Celeste.Editor.LevelTemplate.RenderContents -= modLevelTemplateRenderContents;
            On.Celeste.Editor.LevelTemplate.Move -= modLevelTemplateMove;
        }

        private static void modLevelTemplateRenderContents(On.Celeste.Editor.LevelTemplate.orig_RenderContents orig, LevelTemplate self, Camera camera, List<LevelTemplate> allLevels) {
            orig(self, camera, allLevels);

            if(!GooberHelperModule.Settings.DebugMapPhysics) return;

            if(
                self.Right < camera.Left ||
                self.Left > camera.Right ||
                self.Bottom < camera.Top ||
                self.Top > camera.Bottom
            ) return;

            self.GetExtraData().Fluid.Render(self.Rect.Location.ToVector2(), DebugMapThings.FluidStaticColor, DebugMapThings.FluidMovingColor);
        }

        private static void modLevelTemplateCtorLevelData(On.Celeste.Editor.LevelTemplate.orig_ctor_LevelData orig, LevelTemplate self, LevelData data) {
            orig(self, data);

            self.InitializeExtraData();
        }

        private static void modLevelTemplateCtorIntIntIntInt(On.Celeste.Editor.LevelTemplate.orig_ctor_int_int_int_int orig, LevelTemplate self, int x, int y, int w, int h) {
            orig(self, x, y, w, h);

            self.InitializeExtraData();
        }

        private static void modLevelTemplateMove(On.Celeste.Editor.LevelTemplate.orig_Move orig, LevelTemplate self, Vector2 relativeMove, List<LevelTemplate> allLevels, bool snap) {
            orig(self, relativeMove, allLevels, !GooberHelperModule.Settings.DebugMapPhysics);
        }
    }
}