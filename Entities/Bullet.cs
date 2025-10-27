using System;
using Celeste.Mod.GooberHelper.ModIntegration;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using NLua;

namespace Celeste.Mod.GooberHelper.Entities {
    [Tracked(false)]
    public class Bullet : Actor {
        public class HighResolutionBulletRenderer : HiresRenderer {
            public static bool DontRender = false;

            public override void RenderContent(Scene scene) {
                BeginRender();
                DontRender = false;

                foreach(var entity in scene.Tracker.GetEntities<Bullet>()) {
                    if(entity.Visible) {
                        entity.Render();
                    }
                }

                EndRender();
            }

            public static void Load() {
                On.Celeste.LevelLoader.LoadingThread += modLevelLoadingThread;
                IL.Celeste.Level.Render += modifyLevelRender;
            }

            public static void Unload() {
                On.Celeste.LevelLoader.LoadingThread -= modLevelLoadingThread;
                IL.Celeste.Level.Render -= modifyLevelRender;
            }

            public static void modLevelLoadingThread(On.Celeste.LevelLoader.orig_LoadingThread orig, LevelLoader self) {
                orig(self);

                var renderer = new HighResolutionBulletRenderer();

                self.Level.Add(renderer);
                DynamicData.For(self.Level).Set("HighResolutionBulletRenderer", renderer);
            }

            public static void modifyLevelRender(ILContext il) {
                ILCursor cursor = new ILCursor(il);

                //dont let the gameplay renderer render high resolution bullets
                cursor.EmitDelegate(() => { DontRender = true; });

                if(cursor.TryGotoNextBestFit(MoveType.AfterLabel,
                    instr => instr.MatchLdarg0(),
                    instr => instr.MatchLdfld<Scene>("Paused"),
                    instr => instr.MatchBrfalse(out var _)
                )) {
                    cursor.EmitLdarg0();
                    cursor.EmitDelegate((Level level) => {
                        DynamicData.For(level).Get<HighResolutionBulletRenderer>("HighResolutionBulletRenderer").Render(level);
                    });
                }
            }
        }

        public BulletActivator Parent;
        public Vector2 Velocity = Vector2.Zero;
        public Vector2 Acceleration = Vector2.Zero;
        public Color Color = Color.White;
        public string Texture = "bullets/GooberHelper/arrow";
        public float Scale = 1f;
        public string Effect = "coloredBullet";
        public bool Additive = false;
        public bool HighResolution = true;


        public Bullet(BulletActivator parent, LuaTable props) : base(parent.BulletFieldCenter + (Vector2)(props["position"] ?? Vector2.Zero)) {
            parent.Scene.Add(this);
            Parent = parent;

            if(props["velocity"] is Vector2 velocity) Velocity = velocity;
            if(props["acceleration"] is Vector2 acceleration) Acceleration = acceleration;
            if(props["color"] is Color color) Color = color;
            if(props["texture"] is string texture) Texture = texture;
            if(props["scale"] is double scale) Scale = (float)scale;
            if(props["effect"] is string effect) Effect = effect;
            if(props["additive"] is bool additive) Additive = additive;
            if(props["highResolution"] is bool highResolution) HighResolution = highResolution;

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

        //todo please optimize this
        public override void Render() {
            base.Render();

            if((Engine.Scene as Level) == null || HighResolutionBulletRenderer.DontRender) return;

            var bulletEffect = ModIntegration.FrostHelperAPI.GetEffectOrNull.Invoke(Effect);

            //init effect
            if(HighResolution)
                Draw.SpriteBatch.End();
            else
                GameplayRenderer.End();

            Matrix matrix = (Engine.Scene as Level).GameplayRenderer.Camera.Matrix;

            if(HighResolution) 
                matrix *= Matrix.CreateScale(6);

            Draw.SpriteBatch.Begin(
                SpriteSortMode.Deferred,
                Additive ? BlendState.Additive : BlendState.AlphaBlend,
                SamplerState.PointWrap,
                DepthStencilState.None,
                RasterizerState.CullNone,
                bulletEffect,
                matrix
            );
            bulletEffect.CurrentTechnique = bulletEffect.Techniques["Shader"];

            //actual rendering
            GFX.Game[Texture].DrawCentered(this.Position, this.Color, this.Scale, this.Velocity.Angle() + MathF.PI/2);
            
            //uninit effect
            Draw.SpriteBatch.End();
            
            if(HighResolution)
                HudRenderer.BeginRender();
            else
                GameplayRenderer.Begin();
        }
    }
}