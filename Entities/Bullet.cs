using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using Monocle;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using NLua;
using System;

namespace Celeste.Mod.GooberHelper.Entities {

    [CustomEntity("GooberHelper/Bullet")]
    [Tracked(true)]
    public class Bullet : Actor {
        private BulletSource Parent;
        public Vector2 Speed;
        public Color Color = Color.White;
        public float GroupId = 0;


        public Bullet(BulletSource parent, Vector2 position, Vector2 speed, Color color, float groupId) : base(parent.Center + position) {
            this.Parent = parent;
            this.Speed = speed;
            this.Color = color;
            this.GroupId = groupId;

            new Vector2().SafeNormalize();
        }

        public void SetPosition(Vector2 position) { Position = (Parent.Center + position).Round(); }
        public Vector2 GetPosition() { return Position - Parent.Center; }
        public void SetSpeed(Vector2 speed) { Speed = speed; }
        public Vector2 GetSpeed() { return Speed; }
        public void SetColor(Color color) { Color = color; }
        public Color GetColor() { return Color; }

        // public Bullet() : base(Engine.Scene.Tracker.GetEntity<BulletSource>().Position) {
        //     this.Speed = Vector2.UnitY * -50;
        // }

        public static LuaTable GetGroup(float groupId) {
            IEnumerable<Bullet> bullets = Engine.Scene.Tracker.GetEntities<Bullet>().OfType<Bullet>().Where(bullet => bullet.GroupId == groupId);

            return LuaHelper.ListToLuaTable(bullets.ToList());
        }

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

            new Vector2().Normalize();
        }

        public override void Render()
        {
            base.Render();
            GFX.Game["bullet"].DrawCentered(this.Position, Color);
        }
    }
}