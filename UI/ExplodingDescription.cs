using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.GooberHelper.UI {
    public static class TextMenuItemExtensions {
        //modified version of everest.extensions.adddescription on textmenu.item
        public static TextMenu.Item AddExplodingDescription(this TextMenu.Item option, TextMenu containingMenu, string description) {
            ExplodingDescription descriptionText = new(description, initiallyVisible: false, containingMenu) {
                TextColor = Color.Gray,
                HeightExtra = 0f
            };

            List<TextMenu.Item> items = containingMenu.Items;

            if(items.Contains(option)) {
                containingMenu.Insert(items.IndexOf(option) + 1, descriptionText);
            }

            option.OnEnter = (Action)Delegate.Combine(option.OnEnter, (Action)delegate {
                descriptionText.FadeVisible = true;
            });

            option.OnLeave = (Action)Delegate.Combine(option.OnLeave, (Action)delegate {
                descriptionText.FadeVisible = false;
            });

            return option;
        }
    }
    public class ExplodingDescription : TextMenuExt.EaseInSubHeaderExt {
        private const float Gravity = 400f;
        private const float Cooling = 0.95f;
        private const float SizeMultiplier = 0.6f;
        private static Color HotColor1 = new Color(1f, 0f, 0f); 
        private static Color HotOutlineColor1 = new Color(0.5f, 0f, 0f); 

        private static Color HotColor2 = new Color(1f, 0.7f, 0f); 
        private static Color HotOutlineColor2 = new Color(1f, 1f, 1f); 

        private class Chunk {
            public string Text;
            public Vector2 OriginalPosition;
            public Vector2 Position;
            public Vector2 Velocity;
            public Vector2 Size;
            public float Temperature;
            public float TemperatureColorLerp;

            public Chunk(string text, Vector2 position, Vector2 velocity, Vector2 size) {
                Text = text;
                Position = position;
                OriginalPosition = position + Vector2.Zero;
                Velocity = velocity;
                Size = size;

                SetTemperatureStuff();
            }

            public void Update() {
                Position += Velocity * Engine.DeltaTime;
                Velocity += new Vector2(0, Gravity) * Engine.DeltaTime;

                Temperature *= Cooling; //yeah ik theres no delta time its probably fine

                if(Position.Y > 1080 - Size.Y || Position.Y < 0) {
                    Velocity.Y *= -1;
                    SetTemperatureStuff();

                    Position.Y = Math.Clamp(Position.Y, 0, 1080 - Size.Y);
                }

                if(Position.X > 1920 - Size.X || Position.X < 0) {
                    Velocity.X *= -1;
                    SetTemperatureStuff();

                    Position.X = Math.Clamp(Position.X, 0, 1920 - Size.X);
                }
            }

            public void SetTemperatureStuff() {
                Temperature = 1f;
                TemperatureColorLerp = Random.Shared.NextFloat();
            }
        }

        private List<Chunk> chunks;
        private bool exploded = false;
        private bool retracting = false;
        private Vector2 position;
        private Coroutine unexplodeCoroutineInstance;

        private static HashSet<char> evilCharacters = new HashSet<char>([' ', '\n']); 

        public ExplodingDescription(string title, bool initiallyVisible, TextMenu containingMenu, string icon = null) : base(title, initiallyVisible, containingMenu, icon) {}

        public void Explode() {
            unexplodeCoroutineInstance?.Entity?.RemoveSelf();
            retracting = false;

            chunks = new List<Chunk>(Title.Length);
            
            //copied from the decomp; i would never write such terrible code surely
            Vector2 textPosition = position + ((Container.InnerContent == TextMenu.InnerContentMode.TwoColumn && !AlwaysCenter) ? 
                new Vector2(0f, MathHelper.Max(0f, -16f + HeightExtra)) :
                new Vector2(Container.Width * 0.5f, MathHelper.Max(0f, -16f + HeightExtra)));
            
            textPosition.Y -= 0.5f * ActiveFont.HeightOf(Title) * SizeMultiplier;

            Vector2 originalTextPosition = textPosition + Vector2.Zero; //dont just copy the pointer
            Vector2 chunkPosition = textPosition + Vector2.Zero;

            string chunkText = "";

            for(int i = 0; i < Title.Length; i++) {
                char character = Title[i];
                bool endSegment = Random.Shared.NextFloat() < 0.4f || i == Title.Length - 1;

                chunkText += character;

                textPosition.X += ActiveFont.Measure(character).X * SizeMultiplier;

                while(i + 1 < Title.Length && evilCharacters.Contains(Title[i + 1])) {
                    endSegment = true;
                    i++;

                    if(Title[i] == '\n') {
                        textPosition.X = originalTextPosition.X;
                        textPosition.Y += ActiveFont.LineHeight * SizeMultiplier;
                    } else {
                        textPosition.X += ActiveFont.Measure(Title[i]).X * SizeMultiplier;
                    }
                }

                if(endSegment) {
                    Vector2 size = ActiveFont.Measure(chunkText) * SizeMultiplier;
                    
                    chunks.Add(new Chunk(
                        text: chunkText,
                        position: chunkPosition,
                        velocity: Vector2.UnitX.Rotate(Random.Shared.NextAngle()) * Random.Shared.Range(200f, 1000f),
                        size: size
                    ));

                    chunkPosition = textPosition;
                    chunkText = "";
                }
            }

            exploded = true;
        }

        public IEnumerator UnexplodeCoroutine() {
            retracting = true;

            float retractTimer = 0.3f;
            
            while(retractTimer > 0) {
                retractTimer -= Engine.DeltaTime;

                foreach(Chunk chunk in chunks) {
                    chunk.Position = Vector2.Lerp(chunk.Position, chunk.OriginalPosition, 0.3f);
                    chunk.Temperature = MathHelper.Lerp(chunk.Temperature, 0, 0.3f);
                }

                yield return null;
            }

            retracting = false;
            exploded = false;
        }

        public void Unexplode() {
            if(!exploded) return;

            unexplodeCoroutineInstance = new Coroutine(UnexplodeCoroutine(), true);

            var entity = new Entity();

            entity.Tag += Tags.PauseUpdate;
            entity.Add(unexplodeCoroutineInstance);

            Engine.Scene.Add(entity);
        }

        public override void Update() {
            base.Update();

            if(!exploded || retracting) return;

            foreach(Chunk chunk in chunks) {
                chunk.Update();
            }
        }

        public override void Render(Vector2 position, bool highlighted) {
            this.position = position + Offset;

            if(!exploded) {
                base.Render(position, highlighted);
            
                return;
            }

            float alpha = Container.Alpha * Alpha;

            foreach(var chunk in chunks) {
                ActiveFont.DrawOutline(
                    chunk.Text,
                    chunk.Position,
                    new Vector2(0, 0),
                    Vector2.One * SizeMultiplier,
                    Color.Lerp(TextColor, Color.Lerp(HotColor1, HotColor2, chunk.TemperatureColorLerp), chunk.Temperature) * alpha,
                    2f,
                    Color.Lerp(Color.Black, Color.Lerp(HotOutlineColor1, HotOutlineColor2, chunk.TemperatureColorLerp), chunk.Temperature) * alpha * alpha * alpha
                );
            }
        }
    }
}