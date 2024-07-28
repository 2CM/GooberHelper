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
        public float CullDist = 0;
        public float GroupId = 0;
        public float BounceAmplitude = 0;
        public float Size = 1;
        public bool Special = false;
        public float Friction = 1f;
        public Vector2 Acceleration = Vector2.Zero;

        public Collision onCollide;
        public Collider playerCollider;


        public Bullet(BulletSource parent, Vector2 position, Vector2 speed, Color color, float cullDist, float size, float groupId, float bounceAmplitude) : base(parent.Center + position) {
            this.Parent = parent;
            this.Speed = speed;
            this.Color = color;
            this.CullDist = cullDist;
            this.Size = size;
            this.GroupId = groupId;
            this.BounceAmplitude = bounceAmplitude;
        }

        public Bullet(BulletSource parent, Vector2 position, Vector2 speed, Color color, float cullDist, float size, float groupId, float bounceAmplitude, float friction, Vector2 acceleration) : base(parent.Center + position) {
            this.Special = true;
            this.Parent = parent;
            this.Speed = speed;
            this.Color = color;
            this.CullDist = cullDist;
            this.Size = size;
            this.GroupId = groupId;
            this.BounceAmplitude = bounceAmplitude;
            this.Friction = friction;
            this.Acceleration = acceleration;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            base.Collider = this.BounceAmplitude == 0f ? null : new Hitbox(this.Size * 4, this.Size * 4, this.Size * -2, this.Size * -2);

            this.playerCollider = new Circle(this.Size * 2 * Parent.hitboxScale);
            base.Add(new PlayerCollider(OnCollidePlayer, this.playerCollider));

            this.onCollide = new Collision(this.OnCollide);

            base.Depth = Depths.FGParticles;
        }

        public void SetPosition(Vector2 position) { Position = (Parent.Center + position).Round(); }
        public Vector2 GetPosition() { return Position - Parent.Center; }
        public void SetSpeed(Vector2 speed) { Speed = speed; }
        public Vector2 GetSpeed() { return Speed; }
        public void SetColor(Color color) { Color = color; }
        public Color GetColor() { return Color; }
        public void SetSize(float size) { Size = size; base.Collider = new Hitbox(this.Size * 4, this.Size * 4, this.Size * -2, this.Size * -2); this.playerCollider = new Circle(this.Size * 2 * Parent.hitboxScale); } //bro
        public float GetSize() { return Size; } 
        public void SetFriction(float friction) { Friction = friction; }
        public float GetFriction() { return Friction; }
        public void SetAcceleration(Vector2 acceleration) { Acceleration = acceleration; }
        public Vector2 GetAcceleration() { return Acceleration; }
        public void SetBounceAmplitude(float bounceAmplitude) { BounceAmplitude = bounceAmplitude; if(this.BounceAmplitude == 0f) this.Collider = null; }
        public float GetBounceAmplitude() { return BounceAmplitude; }

        // public Bullet() : base(Engine.Scene.Tracker.GetEntity<BulletSource>().Position) {
        //     this.Speed = Vector2.UnitY * -50;
        // }

        public static LuaTable GetGroup(float groupId) {
            IEnumerable<Bullet> bullets = Engine.Scene.Tracker.GetEntities<Bullet>().OfType<Bullet>().Where(bullet => bullet.GroupId == groupId);

            return LuaHelper.ListToLuaTable(bullets.ToList());
        }

        public bool InRoom() {
            return (
                (this.Position.X >= (Scene as Level).Bounds.Left - CullDist) &&
                (this.Position.X <= (Scene as Level).Bounds.Right + CullDist) &&
                (this.Position.Y >= (Scene as Level).Bounds.Top - CullDist) &&
                (this.Position.Y <= (Scene as Level).Bounds.Bottom + CullDist)
            );
        }

        public override void Update() {
            base.Update();

            this.Position = this.Position.Floor();

            if(CullDist != -1 && !InRoom()) {
                RemoveSelf();
            }

            if(Special) {
                this.Speed *= MathF.Exp(Engine.DeltaTime * -Friction);
                this.Speed += this.Acceleration * Engine.DeltaTime;
            }

            this.MoveH(this.Speed.X * Engine.DeltaTime, onCollide);
            this.MoveV(this.Speed.Y * Engine.DeltaTime, onCollide);
        }

        public void OnCollide(CollisionData data) {
            if(this.BounceAmplitude == -1) {
                this.RemoveSelf();
            } else {
                if(data.Direction.Y != 0) {
                    this.Speed.Y *= -BounceAmplitude;
                } else if(data.Direction.X != 0) {
                    this.Speed.X *= -BounceAmplitude;
                }
            }
        }

        public void OnCollidePlayer(Player player) {
            player.Die((player.Position - this.Position).SafeNormalize(), false);
        }

        public override void Render()
        {
            base.Render();
            GFX.Game["bullet"].DrawCentered(this.Position, Color, Size);
        }
    }
}