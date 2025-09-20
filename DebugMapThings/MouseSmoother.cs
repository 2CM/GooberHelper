using Celeste.Editor;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.GooberHelper {
    public class MouseSmoother {
        private static Vector2[] mouseSmoothingBuffer = new Vector2[5];
        private static int mouseSmoothingBufferIndex = 0;

        public static void Load() {
            On.Celeste.Editor.MapEditor.UpdateMouse += modMapEditorUpdateMouse;
        }

        public static void Unload() {
            On.Celeste.Editor.MapEditor.UpdateMouse -= modMapEditorUpdateMouse;
        }

        public static void modMapEditorUpdateMouse(On.Celeste.Editor.MapEditor.orig_UpdateMouse orig, MapEditor self) {
            Vector2 previousMousePosition = self.mousePosition + Vector2.Zero; //dont just copy the pointer

            orig(self);
        
            Vector2 mouseOffset = self.mousePosition - previousMousePosition;
            
            mouseSmoothingBuffer[mouseSmoothingBufferIndex] = mouseOffset;
            mouseSmoothingBufferIndex++;
            mouseSmoothingBufferIndex %= mouseSmoothingBuffer.Length;
        }

        public Vector2 GetOffset() {
            Vector2 sum = Vector2.Zero;

            foreach(Vector2 vector in mouseSmoothingBuffer) {
                sum += vector;
            }

            return sum / mouseSmoothingBuffer.Length;
        }
    }
}