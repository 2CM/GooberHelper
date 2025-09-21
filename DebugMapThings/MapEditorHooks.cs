using System;
using System.Linq;
using Celeste.Editor;
using Celeste.Mod.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;

namespace Celeste.Mod.GooberHelper {
    public static class MapEditorHooks {
        public static void Load() {
            On.Celeste.Editor.MapEditor.Update += modMapEditorUpdate;
            On.Celeste.Editor.MapEditor.ctor += modMapEditorCtor;
            On.Celeste.Editor.MapEditor.Render += modMapEditorRender;
        }

        public static void Unload() {
            On.Celeste.Editor.MapEditor.Update -= modMapEditorUpdate;
            On.Celeste.Editor.MapEditor.ctor -= modMapEditorCtor;
            On.Celeste.Editor.MapEditor.Render -= modMapEditorRender;
        }
        
        private static string[] generateInfoText() {
            return [
                $"Left Click:           Fling rooms: \n" + 
                $"Right Click:          Attract to mouse\n" + 
                $"Right Click + Scroll: Change attraction\n" + 
                $"\n" + 
                $"P:                    Place fluid\n" +
                $"P + Scroll:           Change fluid blob size\n" + 
                $"O:                    Randomize fluid color",

                $"Attraction:           {string.Format("{0:N2}", DebugMapThings.AttractStrength)}\n" +
                $"Fluid Blob Size:      {string.Format("{0:N0}", DebugMapThings.FluidBlobSize)}"
            ];
        }

        private static void modMapEditorCtor(On.Celeste.Editor.MapEditor.orig_ctor orig, MapEditor self, AreaKey area, bool reloadMapData) {
            orig(self, area, reloadMapData);

            DebugMapThings.ResetThings();
        }

        private static void modMapEditorUpdate(On.Celeste.Editor.MapEditor.orig_Update orig, MapEditor self) {
            MapEditor.MouseModes previousMouseMode = self.mouseMode;
            float previousZoom = MapEditor.Camera.Zoom;

            orig(self);

            if(!GooberHelperModule.Settings.DebugMapPhysics) return;

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
                DebugMapThings.FluidBlobSize = Math.Max(0, DebugMapThings.FluidBlobSize + Math.Sign(MInput.Mouse.WheelDelta));
                MapEditor.Camera.Zoom = previousZoom;

                LevelTemplate oggy = self.TestCheck(self.mousePosition);
                
                oggy?.GetExtraData().Fluid.AddFluid(self.mousePosition - oggy.Rect.Location.ToVector2(), 0.05f, (int)DebugMapThings.FluidBlobSize);
            }

            if(MInput.Keyboard.Pressed(Keys.O)) {
                DebugMapThings.RandomizeFluidColor();
            }

            if(MInput.Mouse.CheckRightButton) {
                DebugMapThings.AttractStrength += MInput.Mouse.WheelDelta * 0.5f * (1 + Math.Abs(DebugMapThings.AttractStrength)/1000);
                MapEditor.Camera.Zoom = previousZoom;

                foreach(LevelTemplate level in self.levels) {
                    level.ApproachMouse(self.mousePosition, DebugMapThings.AttractStrength);
                }
            }
        }

        private static void modMapEditorRender(On.Celeste.Editor.MapEditor.orig_Render orig, MapEditor self) {
            orig(self);

            if(!CoreModule.Settings.ShowManualTextOnDebugMap || !GooberHelperModule.Settings.DebugMapPhysics) return;

            Draw.SpriteBatch.Begin();

            int offset = 72;
            int padding = 10;

            string[] panels = generateInfoText();
            Vector2[] panelSizes = panels.Select(panel => Draw.DefaultFont.MeasureString(panel)).ToArray();

            int maxWidth = (int)panelSizes.Select(panel => panel.X).Max();

            for(int i = 0; i < panels.Length; i++) {
                Vector2 size = panelSizes[i];
                string text = panels[i];

                Draw.Rect(1920 - maxWidth, offset, maxWidth, size.Y, Color.Black * 0.8f);
                Draw.Text(Draw.DefaultFont, text, new Vector2(1920 - maxWidth, offset), Color.White);

                offset += (int)size.Y + padding;
            }
            
            Draw.SpriteBatch.End();
        }
    }
}