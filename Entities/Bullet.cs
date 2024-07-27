using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using Monocle;
using System.Diagnostics;

namespace Celeste.Mod.GooberHelper.Entities {

    [CustomEntity("GooberHelper/Bullet")]
    [Tracked(true)]
    public class Bullet : Actor {
        Vector2 Speed;
        Color Color = Color.White;

        public Bullet(BulletSource parent, Vector2 position, Vector2 speed, Color color) : base(parent.Center + position) {
            this.Speed = speed;
            this.Color = color;
        }

        // public Bullet() : base(Engine.Scene.Tracker.GetEntity<BulletSource>().Position) {
        //     this.Speed = Vector2.UnitY * -50;
        // }

        public bool InRoom() {
            return (
                (this.Position.X > (Scene as Level).Bounds.Left) &&
                (this.Position.X < (Scene as Level).Bounds.Right) &&
                (this.Position.Y > (Scene as Level).Bounds.Top) &&
                (this.Position.Y < (Scene as Level).Bounds.Bottom)
            );
        }

        public override void Update() {
            base.Update();

            this.MoveH(this.Speed.X * Engine.DeltaTime);
            this.MoveV(this.Speed.Y * Engine.DeltaTime);

            if(!InRoom()) {
                RemoveSelf();
            }
        }

        public override void Render()
        {
            base.Render();
            GFX.Game["bullet"].DrawCentered(this.Position, Color);
        }
    }
}