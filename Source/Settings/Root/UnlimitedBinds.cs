using System;
using System.Collections.Generic;
using System.Reflection;
using Celeste.Mod.GooberHelper.Attributes;
using Celeste.Mod.GooberHelper.Attributes.Hooks;
using Celeste.Mod.Helpers;
using MonoMod.Cil;

namespace Celeste.Mod.GooberHelper.Settings.Root {
    /// <summary>
    /// uncaps the default bind limit of 8.
    /// </summary>
    [GooberHelperSetting]
    public class UnlimitedBinds : AbstractToggle {
        public const int ArbitrarilyLargeAmount = int.MaxValue;
        
        // i hate hardcoding this but ¯\_(ツ)_/¯
        public const int DefaultMaxBindings = 8;

        public const float MaxWidth = 900f;

        // used by Render
        private static float cachedWidth;
        private static int lineCount;
        private static int currentLine;
        
        // used by RightWidth
        private static int wrapCount;
        private static float currentRowWidth;
        
        private static readonly FieldInfo f_Input_MaxBindings
            = typeof(Input)
                .GetField(nameof(Input.MaxBindings))!;
        
        public override void OnValueChange(bool value) {
            base.OnValueChange(value);
            
            updateBindCount();
        }

        private static void updateBindCount()
            => f_Input_MaxBindings
                .SetValue(
                    null,
                    GooberHelperModule.Settings.UnlimitedBinds
                        ? ArbitrarilyLargeAmount
                        : DefaultMaxBindings
                );

        [OnHook]
        private static void patch_Input_Initialize(On.Celeste.Input.orig_Initialize orig) {
            orig();
            
            updateBindCount();
        }

        [OnHook]
        private static float patch_TextMenu_Setting_Height(On.Celeste.TextMenu.Setting.orig_Height orig, TextMenu.Setting self) {
            // compute wrapCount
            self.RightWidth();
            
            return orig(self) * (1f + wrapCount);
        }

        [ILHook]
        private static void patch_TextMenu_Setting_Render(ILContext il) {
            var cursor = new ILCursor(il);

            cursor.EmitLdarg0();
            cursor.EmitDelegate(resetStaticVariables);
            
            for(var i = 0; i < 2; i++) {
                if(cursor.TryGotoNext(MoveType.Before,
                    instr => instr.MatchLdcR4(0),
                    instr => instr.MatchNewobj<Vector2>()
                )) {
                    cursor.Index++;

                    cursor.EmitDelegate(overrideYOffset);
                }
            }

            cursor.Index = 0;

            void emitUpdateOffset() {
                cursor.EmitLdloc3();
                cursor.EmitLdarg0();
                cursor.EmitDelegate(updateOffset);
                cursor.EmitStloc3();
            }

            // after MTexture mtexture = obj as MTexture;
            if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchStloc(6))) {
                cursor.EmitLdloc(6);
                cursor.EmitDelegate(getTextureWidth);
                emitUpdateOffset();
            }
            
            // after the measurement of the text
            if(cursor.TryGotoNext(MoveType.After, instr => instr.MatchStloc(7))) {
                cursor.EmitLdloc(7);
                emitUpdateOffset();
            }

            return;

            static void resetStaticVariables(TextMenu.Setting self) {
                cachedWidth = self.RightWidth();
                lineCount = wrapCount;
                currentLine = 0;
            }

            static float getTextureWidth(MTexture texture)
                => texture.Width;

            static float updateOffset(float itemWidth, float offset, TextMenu.Setting self) {
                if(GooberHelperModule.Settings.UnlimitedBinds && offset - itemWidth < 0) {
                    offset = cachedWidth;
                    
                    currentLine++;
                }

                return offset;
            }

            static float overrideYOffset(float orig)
                => GooberHelperModule.Settings.UnlimitedBinds
                    ? (currentLine - lineCount * 0.5f) * ActiveFont.LineHeight
                    : orig;
        }

        [OnHook]
        private static float patch_TextMenu_Setting_RightWidth(On.Celeste.TextMenu.Setting.orig_RightWidth orig, TextMenu.Setting self)
            => MathF.Min(MaxWidth, orig(self));
        
        [ILHook]
        private static void patch_TextMenu_Setting_RightWidth(ILContext il) {
            var cursor = new ILCursor(il);

            cursor.EmitDelegate(resetStaticVariables);

            for(var i = 0; i < 2; i++) {
                // theres a bunch of "num += [something]"s
                // get go before the num += and yoink the [something]s
                if(cursor.TryGotoNext(MoveType.AfterLabel,
                       instr => instr.MatchAdd(),
                       instr => instr.MatchStloc0()
                   )) {
                    cursor.EmitDelegate(interceptWidth);
                    cursor.Index += 2;
                }
            }

            // replace the return value with the best width
            if(cursor.TryGotoNext(MoveType.Before, instr => instr.MatchRet())) {
                cursor.EmitDelegate(overrideReturn);
            }

            return;
            
            static void resetStaticVariables()
                => currentRowWidth = wrapCount = 0;
            
            static float interceptWidth(float orig) {
                if(GooberHelperModule.Settings.UnlimitedBinds) {
                    if(currentRowWidth + orig > MaxWidth) {
                        wrapCount++;
                        currentRowWidth = 0;
                    }

                    currentRowWidth += orig;
                }

                return orig;
            }

            static float overrideReturn(float orig)
                => GooberHelperModule.Settings.UnlimitedBinds
                    ? Math.Min(orig, MaxWidth)
                    : orig;
        }
    }
}