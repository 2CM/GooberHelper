using System;
using System.Collections.Generic;
using Celeste.Editor;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.GooberHelper {
    public static class LevelTemplateExtensions {
        public const float AreaMassContribution = 0.5f;

        public class ExtraData {
            public Vector2 Velocity = Vector2.Zero;
            public Vector2 MovementCounter = Vector2.One / 2;
            public CellFluidSimulation Fluid;
            public bool BeingDragged;
            public float Mass;

            public ExtraData(LevelTemplate template) {
                var grid = template.Grid?.Data;
                Mass = template.Width * template.Height * AreaMassContribution + 1f;

                if(grid == null) {
                    grid = new VirtualMap<bool>(template.Width, template.Height, true);
                } else {
                    // foreach(var jumpthru in template.Jumpthrus) {
                    //     for(int x = jumpthru.Left; x < jumpthru.Right; x++) {
                    //         grid[x, jumpthru.Y] = true;
                    //     }
                    // }
                }

                Fluid = new CellFluidSimulation(grid);
            }
        }

        public static ExtraData GetExtraData(this LevelTemplate self) {
            return DynamicData.For(self).Get<ExtraData>("extraData");
        }

        public static void InitializeExtraData(this LevelTemplate self) {
            DynamicData.For(self).Set("extraData", new ExtraData(self));
        }

        #region Movement
        public static void MoveH(this LevelTemplate self, float moveH, ExtraData extraData) {
            float newX = self.X + extraData.MovementCounter.X + moveH;
            
            self.X = (int)Math.Floor(newX);
            extraData.MovementCounter.X = ((newX % 1) + 1) % 1;
        }

        public static void MoveH(this LevelTemplate self, float moveH) {
            self.MoveH(moveH, self.GetExtraData());
        }

        public static void MoveV(this LevelTemplate self, float moveV, ExtraData extraData) {
            float newY = self.Y + extraData.MovementCounter.Y + moveV;
            
            self.Y = (int)Math.Floor(newY);
            extraData.MovementCounter.Y = ((newY % 1) + 1) % 1;
        }

        public static void MoveV(this LevelTemplate self, float moveV) {
            self.MoveV(moveV, self.GetExtraData());
        }

        public static void Move(this LevelTemplate self, Vector2 move, ExtraData extraData) {
            self.MoveH(move.X, extraData);
            self.MoveV(move.Y, extraData);
        }

        public static void Move(this LevelTemplate self, Vector2 move) {
            self.Move(move, self.GetExtraData());
        }
        #endregion

        public static List<LevelTemplate> CollideAll(this LevelTemplate self) {
            List<LevelTemplate> collisions = [];
            
            foreach(LevelTemplate level in (Engine.Scene as MapEditor).levels) {
                if(level.Rect.Intersects(self.Rect) && level != self) {
                    collisions.Add(level);
                }
            }

            return collisions;
        }

        public static void Update(this LevelTemplate self) {
            var extraData = self.GetExtraData();
            extraData.Fluid.Update();

            if(!extraData.BeingDragged) self.Move(extraData.Velocity / Engine.DeltaTime);
            if(extraData.Velocity.Length() == 0) return;

            foreach(LevelTemplate level in self.CollideAll()) {
                int horizontalSide = self.Rect.Center.X < level.Rect.Center.X ? -1 : 1;
                int verticalSide = self.Rect.Center.Y < level.Rect.Center.Y ? -1 : 1;

                int horizontalInset = Math.Min(
                    self.Right - self.Left,
                    horizontalSide == -1 ?
                        self.Right - level.Left :
                        level.Right - self.Left
                );

                int verticalInset = Math.Min(
                    self.Bottom - self.Top,
                    verticalSide == -1 ?
                        self.Bottom - level.Top :
                        level.Bottom - self.Top
                );

                // Console.WriteLine($"horizontal side: {horizontalSide}");
                // Console.WriteLine($"vertical side: {verticalSide}");
                // Console.WriteLine($"horizontal inset: {horizontalInset}");
                // Console.WriteLine($"vertical inset: {verticalInset}");

                var otherExtraData = level.GetExtraData();

                float selfFraction = extraData.Mass / (extraData.Mass + otherExtraData.Mass);
                float otherFraction = 1 - selfFraction;

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
            var extraData = self.GetExtraData();

            Vector2 offset = mousePosition - new Vector2(self.X, self.Y);

            extraData.Velocity += offset.SafeNormalize()/MathF.Max(offset.LengthSquared(), 0.01f) * strength;
        }
    }
}