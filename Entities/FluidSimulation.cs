using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Utils;
using System.Collections;

namespace Celeste.Mod.GooberHelper.Entities {

    [CustomEntity("GooberHelper/FluidSimulation")]
    [Tracked(false)]
    public class FluidSimulation : Entity {
        public DoubleRenderTarget2D source;
		public DoubleRenderTarget2D velocity;
		public DoubleRenderTarget2D pressure;
		public RenderTarget2D divergenceCurl;
		public RenderTarget2D display;

		public Effect displayShader = null;
		public Effect advectionShader = null;
		public Effect baseVelocityShader = null;
		public Effect jacobiShader = null;
		public Effect divergenceCurlShader = null;
		public Effect gradientShader = null;
		public Effect diffuseShader = null;
		public Effect vorticityShader = null;

        public Rectangle bounds;

		public MeshData plane;

        public float playerVelocityInfluence;
        public float playerSizeInfluence;
        public string textureName;
        public float velocityDiffusion;
        public float colorDiffusion;
        public float playerHairDyeFactor;
        public List<Color> dyeColors;
        public float dyeBrightness;
		public float dyeCycleSpeed;
		public bool onlyDyeWhileDashing;
		public bool onlyInfluenceWhileDashing;
		public float playerSpeedForFullBrightness;
		public int pressureIterations;
		public float vorticity;
		public bool doExplosionShockwave;
		public float shockwaveSize;
		public float shockwaveForce;

		public float dyeCycleTime = 0;

		public bool duplicate = false;	

		public int EntityId;

		// public bool Initialized = false;

        public FluidSimulation(EntityData data, Vector2 offset) : base(data.Position + offset) {
			// Logger.Log(LogLevel.Info, "f", "IM BEING CONSTRUCTED");

			foreach(FluidSimulation sim in Engine.Scene.Tracker.GetEntities<FluidSimulation>()) {
				if(data.ID == sim.EntityId) {
					// Logger.Log(LogLevel.Info, "f", "another one");

					RemoveSelf();

					this.duplicate = true;

					return;
				}
			}
			
            this.Tag = Tags.Persistent;
			this.EntityId = data.ID;
            this.bounds = new Rectangle((int)(data.Position.X + offset.X), (int)(data.Position.Y + offset.Y), data.Width, data.Height);
            this.plane = MeshData.CreatePlane(data.Width, data.Height);

            this.playerVelocityInfluence = data.Float("playerVelocityInfluence", -0.1f);
            this.playerSizeInfluence = data.Float("playerSizeInfluence", 15.0f);
            this.textureName = data.Attr("texture", "");
            this.velocityDiffusion = data.Float("velocityDiffusion", 0.95f);
            this.colorDiffusion = data.Float("colorDiffusion", 0.95f);
            this.playerHairDyeFactor = data.Float("playerHairDyeFactor", 0.0f);
        	this.dyeBrightness = data.Attr("dyeColor", "00ffff,ffffff,ff44ff|0.5").Contains('|') ? float.Parse(data.Attr("dyeColor", "000000").Split("|")[1]) : 1f; //code programming glumbsup
        	this.dyeColors = new List<Color>(); foreach(string str in data.Attr("dyeColor", "00ffff,ffffff,ff44ff|0.5").Split('|')[0].Split(",")) { this.dyeColors.Add(Calc.HexToColor(str)); }
            this.dyeCycleSpeed = data.Float("dyeCycleSpeed", 4.0f);
            this.onlyDyeWhileDashing = data.Bool("onlyDyeWhileDashing", false);
            this.onlyInfluenceWhileDashing = data.Bool("onlyInfluenceWhileDashing", false);
            this.Depth = data.Int("depth", 10001);
            this.playerSpeedForFullBrightness = data.Float("playerSpeedForFullBrightness", 90);
            this.pressureIterations = data.Int("pressureIterations", 50);
            this.vorticity = data.Float("vorticity", 0f);
            this.doExplosionShockwave = data.Bool("doExplosionShockwave", false);
            this.shockwaveSize = data.Float("shockwaveSize", 20);
            this.shockwaveForce = data.Float("shockwaveForce", 10);

			displayShader          = TryGetEffect("display");
			advectionShader        = TryGetEffect("advection");
			baseVelocityShader     = TryGetEffect("baseVelocity");
			jacobiShader           = TryGetEffect("jacobi");
			divergenceCurlShader   = TryGetEffect("divergenceCurl");
			gradientShader         = TryGetEffect("gradient");
			diffuseShader          = TryGetEffect("diffuse");
			vorticityShader        = TryGetEffect("vorticity");

			ClearBuffers();
        }

		public static void Load() {
			On.Celeste.Puffer.Explode += modPufferExplode;
			On.Celeste.Seeker.RegenerateCoroutine += modSeekerRegenerateCoroutine;
		}

		public static void Unload() {
			On.Celeste.Puffer.Explode -= modPufferExplode;
			On.Celeste.Seeker.RegenerateCoroutine -= modSeekerRegenerateCoroutine;
		}

		public static void modPufferExplode(On.Celeste.Puffer.orig_Explode orig, Puffer self) {
			orig(self);

			foreach(FluidSimulation sim in Engine.Scene.Tracker.GetEntities<FluidSimulation>()) {
				sim.Shockwave(self.Center);
			}
		}

		public static IEnumerator modSeekerRegenerateCoroutine(On.Celeste.Seeker.orig_RegenerateCoroutine orig, Seeker self) {
			IEnumerator origEnum = orig(self);
            while (origEnum.MoveNext()) {
                yield return origEnum.Current;
            }

			foreach(FluidSimulation sim in Engine.Scene.Tracker.GetEntities<FluidSimulation>()) {
				sim.Shockwave(self.Center);
			}
		}

        public static void BeginSpriteBatch() {
			Draw.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null);
		}

		public static void EndSpriteBatch() {
			Draw.SpriteBatch.End();
		}

		public static Effect TryGetEffect(string id) {
			//CODE DIRECTLY COPIED FROM FROSTHELPER

			if (Everest.Content.TryGet($"Effects/{id}.cso", out var effectAsset, true)) {
				try {
					Effect effect = new Effect(Engine.Graphics.GraphicsDevice, effectAsset.Data);

					return effect;
				} catch (Exception ex) {
					Logger.Log(LogLevel.Error, "GooberHelper", "Failed to load the shader " + id);
					Logger.Log(LogLevel.Error, "GooberHelper", "Exception: \n" + ex.ToString());
				}
			}

			return null;
		}

		public bool EnsureRenderTarget2D(ref RenderTarget2D renderTarget) {
			if(renderTarget == null) {
				renderTarget = new RenderTarget2D(Engine.Instance.GraphicsDevice, bounds.Width, bounds.Height, false, SurfaceFormat.Vector4, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

				return true;
			}

			return false;
		}

		public bool EnsureDoubleRenderTarget2D(ref DoubleRenderTarget2D renderTarget) {
			if(renderTarget == null) {
				renderTarget = new DoubleRenderTarget2D(bounds.Width, bounds.Height);

				return true;
			}

			return false;
		}

		public void ClearDoubleRenderTarget2D(ref DoubleRenderTarget2D renderTarget) {
			if(renderTarget != null) {
				Engine.Graphics.GraphicsDevice.SetRenderTarget(renderTarget.write);
				Engine.Instance.GraphicsDevice.Clear(Color.Transparent);

				Engine.Graphics.GraphicsDevice.SetRenderTarget(renderTarget.read);
				Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
			}
		}

		public void ClearRenderTarget2D(ref RenderTarget2D renderTarget) {
			Engine.Graphics.GraphicsDevice.SetRenderTarget(renderTarget);
			Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
		}

		public void ClearBuffers() {
			ClearDoubleRenderTarget2D(ref source);
			ClearDoubleRenderTarget2D(ref velocity);
			ClearDoubleRenderTarget2D(ref pressure);
			ClearRenderTarget2D(ref divergenceCurl);

			if(source != null && textureName != "") {
				Engine.Graphics.GraphicsDevice.SetRenderTarget(source.read);
				Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
				BeginSpriteBatch();
				Engine.Graphics.GraphicsDevice.Textures[0] = GFX.Game[textureName].Texture.Texture;
				EndSpriteBatch();
				RenderEffect(displayShader);
			}
		}
        
        public override void Update()
        {
            base.Update();

			if(this.duplicate) return;

			// Logger.Log(LogLevel.Info, "f", "updating at frame " + Engine.Scene.TimeActive * 60);

			dyeCycleTime += Engine.DeltaTime * dyeCycleSpeed;

			bool hadToReload = false;
			
			hadToReload |= EnsureRenderTarget2D(ref display);
			hadToReload |= EnsureDoubleRenderTarget2D(ref source);
			hadToReload |= EnsureDoubleRenderTarget2D(ref velocity);
			hadToReload |= EnsureDoubleRenderTarget2D(ref pressure);
			hadToReload |= EnsureRenderTarget2D(ref divergenceCurl);
			if(hadToReload) {
				// Logger.Log(LogLevel.Info, "f", "had to reload");

				ClearBuffers();

				return;
			}

			// EnsureRenderTarget2D(ref display);
			// EnsureDoubleRenderTarget2D(ref source);
			// EnsureDoubleRenderTarget2D(ref velocity);
			// EnsureDoubleRenderTarget2D(ref pressure);
			// EnsureRenderTarget2D(ref divergence);

			// if(hadToReload) {
			// if(Input.Talk.Pressed) {
			// 	// bounds = (Engine.Scene as Level).Session.LevelData.Bounds;
			// 	// plane = MeshData.CreatePlane(bounds.Width, bounds.Height);

			// 	ClearDoubleRenderTarget2D(ref source);
			// 	ClearDoubleRenderTarget2D(ref velocity);
			// 	ClearDoubleRenderTarget2D(ref pressure);
			// 	ClearRenderTarget2D(ref divergence);
				
			// 	Engine.Graphics.GraphicsDevice.SetRenderTarget(source.read);
			// 	Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
			// 	BeginSpriteBatch();
			// 	Engine.Graphics.GraphicsDevice.Textures[0] = GFX.Game["guhcat"].Texture.Texture;
			// 	EndSpriteBatch();
			// 	RenderEffect(displayShader);

			// 	// Engine.Graphics.GraphicsDevice.SetRenderTarget(velocity.read);
			// 	// Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
			// 	// RenderEffect(baseVelocityShader);

			// 	return;
			// }

			// 	return;
			// }

			
			UpdateTextures();
        }

		public void RenderEffect(Effect effect) {
			Viewport viewport = Engine.Graphics.GraphicsDevice.Viewport;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);
			
			effect.Parameters["TransformMatrix"]?.SetValue(projection);
        	effect.Parameters["ViewMatrix"]?.SetValue(Matrix.Identity);

			foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
				pass.Apply();

				Engine.Instance.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, plane.Positions, 0, 4, plane.Indices, 0, 2);
			}
		}
		
		public Color getDyeColor() {
			Color cur = this.dyeColors[(int)Math.Floor(this.dyeCycleTime) % this.dyeColors.Count];
			Color next = this.dyeColors[(int)Math.Ceiling(this.dyeCycleTime) % this.dyeColors.Count];

			return Color.Lerp(cur, next, dyeCycleTime % 1);
		}

		public void Splat(DoubleRenderTarget2D target, Vector3 color, Vector2 position, float size, bool shockwave = false) {
			Engine.Graphics.GraphicsDevice.SetRenderTarget(target.write);
			Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
			Engine.Graphics.GraphicsDevice.Textures[0] = target.read;
			baseVelocityShader.Parameters["splatPosition"].SetValue(position - new Vector2(bounds.X, bounds.Y));
			baseVelocityShader.Parameters["splatColor"].SetValue(color);
			baseVelocityShader.Parameters["screenSize"].SetValue(new Vector2(bounds.Width, bounds.Height));
			baseVelocityShader.Parameters["splatSize"].SetValue(size);
			baseVelocityShader.Parameters["shockwave"].SetValue(shockwave);
			RenderEffect(baseVelocityShader);
			target.swap();
		}

		public void Shockwave(Vector2 position) {
			if(!this.doExplosionShockwave) return;

			this.Splat(
				velocity,
				new Vector3(shockwaveForce,0,0),
				position,
				this.shockwaveSize,
				true
			);
		}

		public void UpdateTextures() {
			Engine.Graphics.GraphicsDevice.SetRenderTarget(velocity.write);
			Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
			diffuseShader.Parameters["amount"].SetValue(this.velocityDiffusion);
			Engine.Graphics.GraphicsDevice.Textures[0] = velocity.read;
			RenderEffect(diffuseShader);
			velocity.swap();

			Engine.Graphics.GraphicsDevice.SetRenderTarget(source.write);
			Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
			diffuseShader.Parameters["amount"].SetValue(this.colorDiffusion);
			Engine.Graphics.GraphicsDevice.Textures[0] = source.read;
			RenderEffect(diffuseShader);
			source.swap();

			Player player = Engine.Scene.Tracker.GetEntity<Player>();

			if(player != null) {
				if(!onlyInfluenceWhileDashing || player.StateMachine.State == Player.StDash) {
					this.Splat(
						velocity,
						new Vector3(player.Speed * Engine.DeltaTime * this.playerVelocityInfluence, 0),
						player.Center,
						this.playerSizeInfluence
					);
				}

				if(!onlyDyeWhileDashing || player.StateMachine.State == Player.StDash) {
					this.Splat(
						source,
						(player.Hair.GetHairColor(0).ToVector3() * this.playerHairDyeFactor + this.getDyeColor().ToVector3() * this.dyeBrightness) * Math.Min(1, player.Speed.Length()/this.playerSpeedForFullBrightness),
						player.Center,
						this.playerSizeInfluence
					);
				}

				if(Input.Talk.Pressed) {
					this.Splat(
						velocity,
						new Vector3(0,0,0),
						player.Center + new Vector2(100, 100),
						this.playerSizeInfluence,
						true
					);

					Input.Talk.ConsumeBuffer();
				}
			}


			Engine.Graphics.GraphicsDevice.SetRenderTarget(source.write);
			Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
			Engine.Graphics.GraphicsDevice.Textures[0] = velocity.read;
			Engine.Graphics.GraphicsDevice.Textures[1] = source.read;
			advectionShader.Parameters["timestep"].SetValue(1000 * Engine.DeltaTime);
			advectionShader.Parameters["pixelSize"].SetValue(new Vector2(1f/bounds.Width, 1f/bounds.Height));
			RenderEffect(advectionShader);
			source.swap(); 

			Engine.Graphics.GraphicsDevice.SetRenderTarget(velocity.write);
			Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
			Engine.Graphics.GraphicsDevice.Textures[0] = velocity.read;
			Engine.Graphics.GraphicsDevice.Textures[1] = velocity.read;
			advectionShader.Parameters["timestep"].SetValue(1000 * Engine.DeltaTime);
			advectionShader.Parameters["pixelSize"].SetValue(new Vector2(1f/bounds.Width, 1f/bounds.Height));
			RenderEffect(advectionShader);
			velocity.swap(); 

			Engine.Graphics.GraphicsDevice.SetRenderTarget(divergenceCurl);
			Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
			Engine.Graphics.GraphicsDevice.Textures[0] = velocity.read;
			divergenceCurlShader.Parameters["textureSize"].SetValue(new Vector2(bounds.Width, bounds.Height));
			RenderEffect(divergenceCurlShader);

			Engine.Graphics.GraphicsDevice.SetRenderTarget(velocity.write);
			Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
			vorticityShader.Parameters["textureSize"].SetValue(new Vector2(bounds.Width, bounds.Height));
			Engine.Graphics.GraphicsDevice.Textures[0] = velocity.read;
			Engine.Graphics.GraphicsDevice.Textures[1] = divergenceCurl;
			vorticityShader.Parameters["timestep"].SetValue(1000 * Engine.DeltaTime);
			vorticityShader.Parameters["curl"].SetValue(vorticity);
			RenderEffect(vorticityShader);
			velocity.swap();

			for(int i = 0; i < this.pressureIterations; i++) {
				Engine.Graphics.GraphicsDevice.SetRenderTarget(pressure.write);
				Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
				jacobiShader.Parameters["textureSize"].SetValue(new Vector2(bounds.Width, bounds.Height));
				Engine.Graphics.GraphicsDevice.Textures[0] = pressure.read;
				Engine.Graphics.GraphicsDevice.Textures[1] = divergenceCurl;
				RenderEffect(jacobiShader);
				pressure.swap();
			}

			Engine.Graphics.GraphicsDevice.SetRenderTarget(velocity.write);
			Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
			gradientShader.Parameters["textureSize"].SetValue(new Vector2(bounds.Width, bounds.Height));
			Engine.Graphics.GraphicsDevice.Textures[0] = pressure.read;
			Engine.Graphics.GraphicsDevice.Textures[1] = velocity.read;
			RenderEffect(gradientShader);
			velocity.swap();

			Engine.Graphics.GraphicsDevice.SetRenderTarget(display);
			Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
			Engine.Graphics.GraphicsDevice.Textures[0] = source.read;
			Engine.Graphics.GraphicsDevice.Textures[1] = velocity.read;
			RenderEffect(displayShader);
		}

        public override void Render()
		{
			if(display != null) {
				Draw.SpriteBatch.Draw(display, Position, Color.White);
			}
		}

		public class DoubleRenderTarget2D {
			public RenderTarget2D read;
			public RenderTarget2D write;

			public DoubleRenderTarget2D(int width, int height) {
				read  = new RenderTarget2D(Engine.Instance.GraphicsDevice, width, height, false, SurfaceFormat.Vector4, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
				write = new RenderTarget2D(Engine.Instance.GraphicsDevice, width, height, false, SurfaceFormat.Vector4, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
			}

			public void swap() {
				var temp = read;

				read = write;
				write = temp;
			}
		}

		public class MeshData {
			public VertexPositionTexture[] Positions;
			public short[] Indices;

			public MeshData(VertexPositionTexture[] Positions, short[] Indices) {
				this.Positions = Positions;
				this.Indices = Indices;
			}

			public static MeshData CreatePlane(float width, float height) {
				VertexPositionTexture[] vertices = new VertexPositionTexture[4];

				vertices[0].Position = new Vector3(0,     0,      0);
				vertices[1].Position = new Vector3(width, 0,      0);
				vertices[2].Position = new Vector3(0,     height, 0);
				vertices[3].Position = new Vector3(width, height, 0);

				vertices[0].TextureCoordinate = new Vector2(0, 0);
				vertices[1].TextureCoordinate = new Vector2(1, 0);
				vertices[2].TextureCoordinate = new Vector2(0, 1);
				vertices[3].TextureCoordinate = new Vector2(1, 1);

				return new MeshData(vertices, [0, 1, 2, 3, 2, 1]);
			}
		}
    }
}