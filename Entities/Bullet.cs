using System;
using Microsoft.Xna.Framework;
using Monocle;
using NLua;

namespace Celeste.Mod.GooberHelper.Entities {
    public class Bullet : Actor {
        public BulletActivator Parent;
        public Vector2 Velocity = Vector2.Zero;
        public Vector2 Acceleration = Vector2.Zero;
        public Color Color = Color.White;
        public string Texture = "bullets/GooberHelper/arrow";
        public float Scale = 1f;


        public Bullet(BulletActivator parent, LuaTable props) : base(parent.BulletFieldCenter + (Vector2)(props["position"] ?? Vector2.Zero)) {
            parent.Scene.Add(this);
            Parent = parent;

            if(props["velocity"] is Vector2 velocity) Velocity = velocity;
            if(props["acceleration"] is Vector2 acceleration) Acceleration = acceleration;
            if(props["color"] is Color color) Color = color;
            if(props["texture"] is string texture) Texture = texture;
            if(props["scale"] is double scale) Scale = (float)scale;

            Add(new PlayerCollider(onCollidePlayer, new Hitbox(2, 2, -1, -1)));
        }

        public override void Update() {
            base.Update();

            Position += Velocity * Engine.DeltaTime;
            Velocity += Acceleration * Engine.DeltaTime;

            if((this.Position - this.Parent.BulletFieldCenter).Length() > 200) this.RemoveSelf();
        }

        private void onCollidePlayer(Player player) {
            // player.Die((player.Position - Position).SafeNormalize());

            player.Play("event:/char/madeline/death");

            RemoveSelf();
        }

        public override void Render() {
            GFX.Game[Texture].DrawCentered(this.Position, this.Color, this.Scale, this.Velocity.Angle() + MathF.PI/2);

            // Draw.Circle(this.Position, 10, Color.Red, 10);

            base.Render();
        }
    }
}