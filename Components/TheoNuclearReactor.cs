using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Celeste.Mod.GooberHelper.Components {
    [Tracked(false)]
    public class TheoNuclearReactor : Component {
        public TheoNuclearReactor() : base(true, true) {
            
        }

        public override void Update() {
            base.Update();

            if(!OptionsManager.TheoNuclearReactor) return;

            if(this.Scene.OnInterval(0.02f)) {
                (this.Scene as Level).Displacement.AddBurst(
                    this.Entity.Center + Vector2.One.Rotate(Random.Shared.NextAngle()) * 10.0f,
                    Random.Shared.Range(0.5f, 1f),
                    Random.Shared.Range(0f, 100f),
                    Random.Shared.Range(300f, 1000f),
                    Random.Shared.Range(0.2f, 0.3f)
                );
            }
        }

        // public override void Removed(Entity entity) {
        //     Console.WriteLine(entity.Position);

        //     if(entity.Left - 8f > (Engine.Scene as Level).Bounds.Right) {
        //         entity.Components.Remove(this);

        //         Entity dummy = new Entity(this.Entity.Position);
        //             dummy.Components.Add(this);
        //             Engine.Scene.Add(dummy);
                
        //         Console.WriteLine(dummy.Position);
        //     }

        //     base.Removed(entity);
        // }

        public override void Render() {
            base.Render();

            if(!OptionsManager.TheoNuclearReactor) return;

            Effect nuclearReactor = ModIntegration.FrostHelperAPI.GetEffectOrNull.Invoke("nuclearReactor");
            if(nuclearReactor == null || Engine.Scene is not Level) return;

            MTexture noiseTexture = GFX.Game["GooberHelper/noise"];

            GameplayRenderer.End();

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, nuclearReactor, (Engine.Scene as Level).Camera.Matrix);
            nuclearReactor.CurrentTechnique = nuclearReactor.Techniques["Grongle"];
            nuclearReactor.Parameters["time"]?.SetValue(Engine.Scene.TimeActive);
            Engine.Graphics.GraphicsDevice.Textures[1] = GFX.Game["GooberHelper/theoidle00"].Texture.Texture; //using the actual texture is terrible because of the whole texture atlas thing

            noiseTexture.DrawCentered(this.Entity.Center, Color.White, 8f);

            Draw.SpriteBatch.End();
            GameplayRenderer.Begin();
        }
    }
}