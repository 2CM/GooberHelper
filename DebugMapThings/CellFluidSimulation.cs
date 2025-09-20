using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.GooberHelper {
    //REFERENCED CODE FROM https://github.com/jongallant/LiquidSimulator/blob/master/Assets/Scripts/LiquidSimulator.cs A LOT
    //THANK YOU
    public class CellFluidSimulation {
        public VirtualMap<float> Cells;
        public VirtualMap<float> Diffs;
        public VirtualMap<bool> Tiles;
        public bool PlacedAnything = false;

        public static float MinValue = 0f;
        public static float MaxValue = 1f;

        public static float MaxCompression = 0f;

        public static float MinFlow = 0f;
        public static float MaxFlow = 4f;

        public static float FlowSpeed = 2f;


        public CellFluidSimulation(VirtualMap<bool> tiles) {
            this.Cells = new VirtualMap<float>(tiles.Columns, tiles.Rows, 0);
            this.Diffs = new VirtualMap<float>(tiles.Columns, tiles.Rows, 0);
            this.Tiles = tiles;
        }

        public void AddFluid(Vector2 position, float amount, int radius = 2) {
            PlacedAnything = true;

            for(int x = -radius; x <= radius; x += 1) {
                for(int y = -radius; y <= radius; y += 1) {
                    int nx = (int)position.X + x;
                    int ny = (int)position.Y + y;

                    if(new Vector2(x, y).Length() > radius || Tiles[nx, ny]) continue;

                    Cells[nx, ny] = Math.Min(Cells[nx, ny] + amount, MaxValue);
                }
            }
        }

        public float CalculateVerticalFlow(float source, float destination) {
            float sum = source + destination;
            float value = 0;

            if(sum >= MaxValue) {
                value = MaxValue;
            } else if(sum < 2f * MaxValue + MaxCompression) {
                value = (MaxValue * MaxValue + sum * MaxCompression) / (MaxValue + MaxCompression);
            } else {
                value = (sum + MaxCompression) / 2f;
            }

            return value;
        }

        public float CalculateHorizontalFlow(float source, float destination) {
            return (source - destination) / 4f;
        }

        public void Update() {
            if(!PlacedAnything) return;

            for(int x = 0; x < Cells.Columns; x++) {
                for(int y = 0; y < Cells.Rows; y++) {
                    Diffs[x, y] = 0;
                }
            }

            for(int x = 0; x < Cells.Columns; x++) {
                for(int y = 0; y < Cells.Rows; y++) {
                    float startValue = Cells[x, y];

                    if(startValue == 0) continue;

                    if(startValue < MinValue) startValue = 0;

                    float remainingValue = startValue;

                    void updateStuff(float flow, int xOffset, int yOffset) {
                        if(flow > MinFlow) flow *= FlowSpeed;
                        flow = Math.Clamp(flow, 0f, MathF.Min(MaxFlow, remainingValue));

                        remainingValue -= flow;
                        Diffs[x, y] -= flow;
                        Diffs[x + xOffset, y + yOffset] += flow;
                    }

                    bool noMoreFluid() {
                        if(remainingValue < MinValue) {
                            Diffs[x, y] -= remainingValue;

                            return true;
                        }
                        
                        return false;
                    }

                    //flowing down
                    if(!Tiles[x, y + 1] && y + 1 < this.Cells.Rows) {
                        float cellUnder = Cells[x, y + 1];
                        float flow = CalculateVerticalFlow(remainingValue, cellUnder) - cellUnder;

                        updateStuff(flow, 0, 1);

                        if(noMoreFluid()) continue;
                    }


                    //flowing to the side
                    for(int dir = -1; dir <= 1; dir += 2) {
                        if(!Tiles[x + dir, y] && x + dir < this.Cells.Columns && x + dir >= 0) {
                            float cellSide = Cells[x + dir, y];

                            float flow = CalculateHorizontalFlow(remainingValue, cellSide);

                            updateStuff(flow, dir, 0);

                            if(noMoreFluid()) break;
                        }
                    }

                    if(noMoreFluid()) continue;

                    //flowing up
                    if(!Tiles[x, y - 1]) {
                        float cellAbove = Cells[x, y - 1];
                        float flow = remainingValue - CalculateVerticalFlow(remainingValue, cellAbove);

                        updateStuff(flow, 0, -1);

                        if(noMoreFluid()) continue;
                    }

                    if(noMoreFluid()) continue;
                }
            }

            for(int x = 0; x < Diffs.Columns; x++) {
                for(int y = 0; y < Diffs.Rows; y++) {
                    Cells[x, y] = Math.Clamp(Cells[x, y] + Diffs[x, y], MinValue, MaxValue);
                }
            }
        }

        public void Render(Vector2 offset, Color color1, Color color2) {
            if(!this.PlacedAnything) return;

            for(int x = 0; x < Cells.Columns; x++) {
                for(int y = 0; y < Cells.Rows; y++) {
                    Color blendedColor = Color.Lerp(color1, color2, Math.Abs(Diffs[x, y]) * 2f) * (1 - MathF.Pow(1 - Cells[x, y], 5)) * MaxValue;

                    Draw.Rect(offset.X + x, offset.Y + y, 1, 1, blendedColor);
                }
            }
        }
    }
}