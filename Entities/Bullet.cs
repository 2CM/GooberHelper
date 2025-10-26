using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.GooberHelper.Entities {
    public class Bullet : Actor {
        public BulletActivator Parent;
        public Vector2 Velocity;
        public Vector2 Acceleration;
        public Color Color;
        public Sprite Sprite;
        public float Size;


        public Bullet(BulletActivator parent, Vector2 position) : base(parent.BulletFieldCenter + position) {
            parent.Scene.Add(this);

            Console.WriteLine("hii");
        }

        public override void Update() {
            base.Update();

            Console.WriteLine(this.Position);

            Position += Velocity * Engine.DeltaTime;
            Velocity += Acceleration * Engine.DeltaTime;
        }

        public override void Render() {
            Draw.Circle(this.Position, 10, Color.Red, 10);

            base.Render();
        }
    }
}