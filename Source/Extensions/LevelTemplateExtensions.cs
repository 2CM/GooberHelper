using System;
using System.Collections.Generic;
using Celeste.Editor;
using Celeste.Mod.GooberHelper.Attributes.Hooks;
using Celeste.Mod.GooberHelper.Settings.Root.DebugMapPhysicsUtils;
using MonoMod.Utils;

namespace Celeste.Mod.GooberHelper.Extensions {
    //i cant really make this not use dynamicdata
    //sorry
    public static class LevelTemplateExtensions {
        public class LevelTemplateExtensionFields {
            public Vector2 Velocity = Vector2.Zero;
            public Vector2 MovementCounter = Vector2.One / 2;
            public CellFluidSimulation Fluid;
            public bool BeingDragged;
            public float Mass;
        }

        private static readonly string f_LevelTemplate_GooberHelperExtensionFields = nameof(f_LevelTemplate_GooberHelperExtensionFields);

        public static LevelTemplateExtensionFields GetExtensionFields(this LevelTemplate self)
            => DynamicData.For(self).Get<LevelTemplateExtensionFields>(f_LevelTemplate_GooberHelperExtensionFields);

        [OnHook]
        private static void patch_LevelTemplate_ctor_int_int_int_int(On.Celeste.Editor.LevelTemplate.orig_ctor_int_int_int_int orig, LevelTemplate self, int x, int y, int w, int h) {
            orig(self, x, y, w, h);
            
            DynamicData.For(self).Set(f_LevelTemplate_GooberHelperExtensionFields, new LevelTemplateExtensionFields());
            self.InitializeExtensionFields();
        }

        [OnHook]
        private static void patch_LevelTemplate_ctor_LevelData(On.Celeste.Editor.LevelTemplate.orig_ctor_LevelData orig, LevelTemplate self, LevelData data) {
            orig(self, data);
            
            DynamicData.For(self).Set(f_LevelTemplate_GooberHelperExtensionFields, new LevelTemplateExtensionFields());
            self.InitializeExtensionFields();
        }

#region Movement
        public static void MoveH(this LevelTemplate self, float moveH, LevelTemplateExtensionFields extraData) {
            var newX = self.X + extraData.MovementCounter.X + moveH;
            
            self.X = (int)Math.Floor(newX);
            extraData.MovementCounter.X = ((newX % 1) + 1) % 1;
        }

        public static void MoveH(this LevelTemplate self, float moveH)
            => self.MoveH(moveH, self.GetExtensionFields());

        public static void MoveV(this LevelTemplate self, float moveV, LevelTemplateExtensionFields extraData) {
            var newY = self.Y + extraData.MovementCounter.Y + moveV;
            
            self.Y = (int)Math.Floor(newY);
            extraData.MovementCounter.Y = ((newY % 1) + 1) % 1;
        }

        public static void MoveV(this LevelTemplate self, float moveV)
            => self.MoveV(moveV, self.GetExtensionFields());

        public static void Move(this LevelTemplate self, Vector2 move, LevelTemplateExtensionFields extraData) {
            self.MoveH(move.X, extraData);
            self.MoveV(move.Y, extraData);
        }

        public static void Move(this LevelTemplate self, Vector2 move)
            => self.Move(move, self.GetExtensionFields());

#endregion

#region General
        public static void InitializeExtensionFields(this LevelTemplate self) {
            if(!GooberHelperModule.Settings.DebugMapPhysics)
                return;

            var ext = self.GetExtensionFields();

            ext.Mass = self.Width * self.Height * 0.5f + 1f;

            var grid = self.Grid?.Data
                ?? new VirtualMap<bool>(self.Width, self.Height, true);

            //make jumpthroughs interact with water (disabled)
            //holy indentation
            // if(self.Jumpthrus != null)
            //     foreach(var jumpthru in self.Jumpthrus)
            //         for(var x = jumpthru.Left; x < jumpthru.Right; x++)
            //             grid[x, jumpthru.Y] = true;

            ext.Fluid = new CellFluidSimulation(grid);
        }

        public static List<LevelTemplate> CollideAll(this LevelTemplate self) {
            var collisions = new List<LevelTemplate>();
            
            foreach(var level in (Engine.Scene as MapEditor).levels)
                if(level.Rect.Intersects(self.Rect) && level != self)
                    collisions.Add(level);

            return collisions;
        }

        public static void Update(this LevelTemplate self) {
            var extraData = self.GetExtensionFields();
            extraData.Fluid.Update();

            if(!extraData.BeingDragged)
                self.Move(extraData.Velocity / Engine.DeltaTime);

            if(extraData.Velocity.Length() == 0)
                return;

            foreach(var level in self.CollideAll()) {
                var horizontalSide = self.Rect.Center.X < level.Rect.Center.X ? -1 : 1;
                var verticalSide = self.Rect.Center.Y < level.Rect.Center.Y ? -1 : 1;

                var horizontalInset = Math.Min(
                    self.Right - self.Left,
                    horizontalSide == -1 ?
                        self.Right - level.Left :
                        level.Right - self.Left
                );

                var verticalInset = Math.Min(
                    self.Bottom - self.Top,
                    verticalSide == -1 ?
                        self.Bottom - level.Top :
                        level.Bottom - self.Top
                );

                var otherExtraData = level.GetExtensionFields();

                var selfFraction = extraData.Mass / (extraData.Mass + otherExtraData.Mass);
                var otherFraction = 1 - selfFraction;

                otherExtraData.Velocity += extraData.Velocity * selfFraction;
                
                extraData.Velocity *= otherFraction;

                if(horizontalInset > verticalInset) {
                    extraData.Velocity.Y *= -1;

                    self.Y += verticalInset * verticalSide;
                } else {
                    extraData.Velocity.X *= -1;

                    self.X += horizontalInset * horizontalSide;
                }
            }
        }

        public static void ApproachMouse(this LevelTemplate self, Vector2 mousePosition, float strength) {
            var extraData = self.GetExtensionFields();

            var offset = mousePosition - new Vector2(self.X, self.Y);

            extraData.Velocity += offset.SafeNormalize()/MathF.Max(offset.LengthSquared(), 0.01f) * strength;
        }
#endregion

    }
}