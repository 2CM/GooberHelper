using Celeste.Editor;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste.Mod.GooberHelper {
    public static class MapEditorHooks {
        public static void Load() {
            On.Celeste.Editor.MapEditor.Update += modMapEditorUpdate;
            On.Celeste.Editor.MapEditor.ctor += modMapEditorCtor;
        }

        public static void Unload() {
            On.Celeste.Editor.MapEditor.Update -= modMapEditorUpdate;
            On.Celeste.Editor.MapEditor.ctor -= modMapEditorCtor;
        }

        private static void modMapEditorCtor(On.Celeste.Editor.MapEditor.orig_ctor orig, MapEditor self, AreaKey area, bool reloadMapData) {
            orig(self, area, reloadMapData);

            DebugMapThings.AttractStrength = 0;
        }

        private static void modMapEditorUpdate(On.Celeste.Editor.MapEditor.orig_Update orig, MapEditor self) {
            MapEditor.MouseModes previousMouseMode = self.mouseMode;
            float previousZoom = MapEditor.Camera.Zoom;

            orig(self);

            foreach(LevelTemplate level in self.levels) {
                level.Update();
            }


            if(self.mouseMode == MapEditor.MouseModes.Move) {
                foreach(var level in self.selection) {
                    var extensions = level.GetExtraData();

                    extensions.Velocity = DebugMapThings.MouseSmoother.GetOffset() * Engine.DeltaTime;
                    extensions.BeingDragged = true;
                }
            }

            if(previousMouseMode == MapEditor.MouseModes.Move && self.mouseMode == MapEditor.MouseModes.Hover) {
                foreach(var level in self.selection) {
                    var extensions = level.GetExtraData();

                    extensions.BeingDragged = false;
                }
            }

            if(MInput.Keyboard.Check(Keys.P)) {
                LevelTemplate oggy = self.TestCheck(self.mousePosition);
                
                oggy?.GetExtraData().Fluid.AddFluid(self.mousePosition - oggy.Rect.Location.ToVector2(), 0.05f, 3);
            }

            if(MInput.Mouse.CheckRightButton) {
                DebugMapThings.AttractStrength += MInput.Mouse.WheelDelta * 0.5f;

                foreach(LevelTemplate level in self.levels) {
                    level.ApproachMouse(self.mousePosition, DebugMapThings.AttractStrength);
                }

                MapEditor.Camera.Zoom = previousZoom;
            }
        }
    }
}