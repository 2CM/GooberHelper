using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Celeste.Mod.GooberHelper.Entities {

    [CustomEntity("GooberHelper/Waterfall")]
    [TrackedAs(typeof(Water))]
    public class Waterfall : Water {

        private MTexture splashTexture = GFX.Game["objs/waterfall/fade"];
        private MTexture noiseTexture;

        bool playerInside = false;
        float speed = 200f;

        List<Vector4> splashes = new List<Vector4>();

        public Waterfall(EntityData data, Vector2 offset) : base(data.Position + offset, false, false, data.Width, data.Height) {
            this.Depth = -9999;

            // Collider = new Hitbox(16f, 16f, -8f, -8f);
            Add(new PlayerCollider(onPlayer, null, Collider));
        }

        public override void Added(Scene scene)
        {
            for(int i = 0; i < this.Width / 2; i++) {
                float angle = Random.Shared.NextAngle();

                this.splashes.Add(new Vector4((float)i * 2f, (float)Math.Cos(angle), (float)Math.Sin(angle), Random.Shared.NextFloat()));
            }

            base.Added(scene);
        }

        private void onPlayer(Player player) {
            player.MoveV(speed * Engine.DeltaTime);
        }

        public override void Update() {
            if(!base.CollideCheck<Player>() && playerInside) {
                Engine.Scene.Tracker.GetEntity<Player>().Speed.Y += speed;
            }

            base.Update();

            playerInside = base.CollideCheck<Player>();
        }

        public override void Render() {
            base.Render();

            int scroll = 128 - ((int)(Scene.TimeActive * 96) % 128);
            int scrollOverlay = 128 - ((int)(Scene.TimeActive * 192) % 128);

            int padding = 3;

            for(int i = 0; i < Math.Ceiling(this.Height/128) + 1; i++) {
                noiseTexture = new MTexture(GFX.Game["objs/waterfall/noiseOverlay"], null, new Rectangle(0, scrollOverlay - i * 128, 128, (int)this.Height + padding), new Vector2(0, 0), 128, (int)this.Height + padding);

                for(int j = 0; j < Math.Ceiling(this.Width/128); j++) {
                    if(j == Math.Floor(this.Width/128)) {
                        noiseTexture = new MTexture(GFX.Game["objs/waterfall/noiseOverlay"], null, new Rectangle(0, scrollOverlay - i * 128, (int)this.Width % 128, (int)this.Height + padding), new Vector2(0, 0), (int)this.Width % 128, (int)this.Height + padding);
                    }

                    noiseTexture.DrawJustified(base.Position + new Vector2(j * 128, i * 128 - (i > 0 ? scrollOverlay : 0)), Vector2.Zero);
                }
            }

            foreach(Vector4 splash in splashes) {
                Vector2 basePos = base.Position + new Vector2(splash.X, base.Height);
                float len = 1.5f;
                float fac = (base.Scene.TimeActive * 4f + splash.W * len) % len;

                Vector2 offset = new Vector2(splash.Y, splash.Z) * 32 * fac;

                float a = (len-fac) * 0.5f;

                splashTexture.DrawCentered(basePos + offset, new Color(a,a,a,a), 0.75f);
            }
        }

        // public class Splash {
        //     public Vector2 Position;
        //     public float Angle;

        //     public Splash(Vector2 position, float angle, float time) {
        //         this.Position = position;
        //         this.Angle = angle;
        //     }

        //     public void Draw() {

        //     }
        // }
    }
}