using System;
using System.Linq;
using Celeste.Mod.Backdrops;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.GooberHelper.Backdrops
{

	[CustomBackdrop("GooberHelper/GooberGodRays")]
	public class GooberGodrays : Backdrop
	{
		VirtualRenderTarget target = null;

		Effect shader = null;

		MeshData plane;
		
		public GooberGodrays()
		{
			plane = MeshData.CreatePlane();
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
		public static void ApplyGlitch(VirtualRenderTarget source, float timer, float seed, float amplitude, float value)
		{
			Effect fxGlitch = GFX.FxGlitch;
			Vector2 vector = new Vector2((float)Engine.Graphics.GraphicsDevice.Viewport.Width, (float)Engine.Graphics.GraphicsDevice.Viewport.Height);
			fxGlitch.Parameters["dimensions"].SetValue(vector);
			fxGlitch.Parameters["amplitude"].SetValue(amplitude);
			fxGlitch.Parameters["minimum"].SetValue(-1f);
			fxGlitch.Parameters["glitch"].SetValue(value);
			fxGlitch.Parameters["timer"].SetValue(timer);
			fxGlitch.Parameters["seed"].SetValue(seed);
			VirtualRenderTarget tempA = GameplayBuffers.TempA;
			Engine.Instance.GraphicsDevice.SetRenderTarget(tempA);
			Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, fxGlitch);
			Draw.SpriteBatch.Draw(source, Vector2.Zero, Color.White);
			Draw.SpriteBatch.End();
			Engine.Instance.GraphicsDevice.SetRenderTarget(source);
			Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, fxGlitch);
			Draw.SpriteBatch.Draw(tempA, Vector2.Zero, Color.White);
			Draw.SpriteBatch.End();
		}
		public void ApplyOther(VirtualRenderTarget source) {
			Effect eff = shader;

			Viewport viewport = Engine.Graphics.GraphicsDevice.Viewport;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);

			eff.Parameters["TransformMatrix"]?.SetValue(projection);
        	eff.Parameters["ViewMatrix"]?.SetValue(Matrix.Identity);
			
			// Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.TempB);
			// Engine.Instance.GraphicsDevice.Clear(Color.Transparent);
			// Draw.SpriteBatch.Begin();
			// GFX.Game["objects/door/lockdoor12"].DrawCentered(new Vector2(60, 90));
			// Draw.SpriteBatch.End();

			// eff.Parameters["other"]?.SetValue(GameplayBuffers.TempB);

			// Draw.SpriteBatch.Begin();

			// eff.Parameters["other"]?.SetValue(target);

			// Draw.SpriteBatch.End();

			foreach (EffectParameter p in eff.Parameters.ToList()) {
				Logger.Log(LogLevel.Info, "b", p.Name + ", " + p.GetType().ToString());
			}

			Logger.Log(LogLevel.Info, "b", "--");
			// Logger.Log(LogLevel.Info, "b", Engine.Graphics.GraphicsDevice.Textures[0].ToString());


			VirtualRenderTarget tempA = GameplayBuffers.TempA;

			Engine.Instance.GraphicsDevice.SetRenderTarget(tempA);
			Engine.Instance.GraphicsDevice.Clear(Color.Transparent);

			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, eff);

			Texture2D t = GFX.Game["objs/waterfall/noiseOverlay"].Texture.Texture;
        	eff.Parameters["FreakyTexture"].SetValue(t);

			Draw.SpriteBatch.Draw(source, Vector2.Zero, Color.White);
			Draw.SpriteBatch.End();

			Engine.Instance.GraphicsDevice.SetRenderTarget(source);
			Engine.Instance.GraphicsDevice.Clear(Color.Transparent);

			Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null);
			Draw.SpriteBatch.Draw(tempA, Vector2.Zero, Color.White);
			Draw.SpriteBatch.End();
		}

		public override void BeforeRender(Scene scene)
		{
			if(shader == null) {
				shader = TryGetEffect("testShader");

				return;	
			}

			if(target == null) {
				target = VirtualContent.CreateRenderTarget("GooberGodrays", 320, 180, false, true, 0);
			}

			Effect eff = shader;

			Engine.Graphics.GraphicsDevice.SetRenderTarget(target);
			Engine.Instance.GraphicsDevice.Clear(Color.Transparent);

			Viewport viewport = Engine.Graphics.GraphicsDevice.Viewport;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, viewport.Width, viewport.Height, 0, 0, 1);

			eff.Parameters["TransformMatrix"]?.SetValue(projection);
        	eff.Parameters["ViewMatrix"]?.SetValue(Matrix.Identity);

			Texture2D first = GFX.Game["objs/waterfall/noiseOverlay"].Texture.Texture;
        	eff.Parameters["FirstTexture"].SetValue(first);

			Texture2D second = GFX.Game["guhcat"].Texture.Texture;
        	eff.Parameters["SecondTexture"].SetValue(second);

			foreach (EffectPass pass in eff.CurrentTechnique.Passes)
			{
				pass.Apply();

				Engine.Instance.GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList, plane.Positions, 0, 4, plane.Indices, 0, 2);
			}

			// Draw.SpriteBatch.Begin();

			// GFX.Game["guhcat"].DrawCentered(new Vector2(160, 90));

			// Draw.SpriteBatch.End();

			// ApplyGlitch(target, scene.TimeActive, 0.4241f, MathF.Tau, 1);
			// ApplyOther(target);
		}

		public override void Render(Scene scene)
		{
			if(target != null) {
				Draw.SpriteBatch.Draw(this.target, Vector2.Zero, new Rectangle?(target.Bounds), Color.White);
			}
		}

		public class MeshData {
			public VertexPositionTexture[] Positions;
			public short[] Indices;

			public MeshData(VertexPositionTexture[] Positions, short[] Indices) {
				this.Positions = Positions;
				this.Indices = Indices;
			}

			public static MeshData CreatePlane() {
				VertexPositionTexture[] vertices = new VertexPositionTexture[4];

				vertices[0].Position = new Vector3(0,   0,   0);
				vertices[1].Position = new Vector3(320, 0,   0);
				vertices[2].Position = new Vector3(0,   180, 0);
				vertices[3].Position = new Vector3(320, 180, 0);

				vertices[0].TextureCoordinate = new Vector2(0, 0);
				vertices[1].TextureCoordinate = new Vector2(1, 0);
				vertices[2].TextureCoordinate = new Vector2(0, 1);
				vertices[3].TextureCoordinate = new Vector2(1, 1);

				return new MeshData(vertices, [0,1,2,2,3,1]);
			}
		}


		// // Token: 0x0600111C RID: 4380 RVA: 0x00052860 File Offset: 0x00050A60
		// public GooberGodrays()
		// {
		// 	this.UseSpritebatch = false;
		// 	for (int i = 0; i < this.rays.Length; i++)
		// 	{
		// 		this.rays[i].Reset();
		// 		this.rays[i].Percent = Calc.Random.NextFloat();
		// 	}
		// }

		// // Token: 0x0600111D RID: 4381 RVA: 0x000528EC File Offset: 0x00050AEC
		// public override void Update(Scene scene)
		// {
		// 	Level level = scene as Level;
		// 	bool flag = base.IsVisible(level);
		// 	this.fade = Calc.Approach(this.fade, (float)(flag ? 1 : 0), Engine.DeltaTime);
		// 	this.Visible = this.fade > 0f;
		// 	if (!this.Visible)
		// 	{
		// 		return;
		// 	}
		// 	Player entity = level.Tracker.GetEntity<Player>();
		// 	Vector2 vector = Calc.AngleToVector(-1.6707964f, 1f);
		// 	Vector2 vector2 = new Vector2(-vector.Y, vector.X);
		// 	int num = 0;
		// 	for (int i = 0; i < this.rays.Length; i++)
		// 	{
		// 		if (this.rays[i].Percent >= 1f)
		// 		{
		// 			this.rays[i].Reset();
		// 		}
		// 		GooberGodrays.Ray[] array = this.rays;
		// 		int num2 = i;
		// 		array[num2].Percent = array[num2].Percent + Engine.DeltaTime / this.rays[i].Duration;
		// 		GooberGodrays.Ray[] array2 = this.rays;
		// 		int num3 = i;
		// 		array2[num3].Y = array2[num3].Y + 8f * Engine.DeltaTime;
		// 		float percent = this.rays[i].Percent;
		// 		float num4 = -32f + this.Mod(this.rays[i].X - level.Camera.X * 0.9f, 384f);
		// 		float num5 = -32f + this.Mod(this.rays[i].Y - level.Camera.Y * 0.9f, 244f);
		// 		float width = this.rays[i].Width;
		// 		float length = this.rays[i].Length;
		// 		Vector2 vector3 = new Vector2((float)((int)num4), (float)((int)num5));
		// 		Color color = this.rayColor * Ease.CubeInOut(Calc.Clamp(((percent < 0.5f) ? percent : (1f - percent)) * 2f, 0f, 1f)) * this.fade;
		// 		if (entity != null)
		// 		{
		// 			float num6 = (vector3 + level.Camera.Position - entity.Position).Length();
		// 			if (num6 < 64f)
		// 			{
		// 				color *= 0.25f + 0.75f * (num6 / 64f);
		// 			}
		// 		}
		// 		VertexPositionColor vertexPositionColor = new VertexPositionColor(new Vector3(vector3 + vector2 * width + vector * length, 0f), color);
		// 		VertexPositionColor vertexPositionColor2 = new VertexPositionColor(new Vector3(vector3 - vector2 * width, 0f), color);
		// 		VertexPositionColor vertexPositionColor3 = new VertexPositionColor(new Vector3(vector3 + vector2 * width, 0f), color);
		// 		VertexPositionColor vertexPositionColor4 = new VertexPositionColor(new Vector3(vector3 - vector2 * width - vector * length, 0f), color);
		// 		this.vertices[num++] = vertexPositionColor;
		// 		this.vertices[num++] = vertexPositionColor2;
		// 		this.vertices[num++] = vertexPositionColor3;
		// 		this.vertices[num++] = vertexPositionColor2;
		// 		this.vertices[num++] = vertexPositionColor3;
		// 		this.vertices[num++] = vertexPositionColor4;
		// 	}
		// 	this.vertexCount = num;
		// }

		// // Token: 0x0600111E RID: 4382 RVA: 0x00026894 File Offset: 0x00024A94
		// private float Mod(float x, float m)
		// {
		// 	return (x % m + m) % m;
		// }

		// // Token: 0x0600111F RID: 4383 RVA: 0x00052C88 File Offset: 0x00050E88
		// public override void Render(Scene scene)
		// {
		// 	if (this.vertexCount > 0 && this.fade > 0f)
		// 	{
		// 		GFX.DrawVertices<VertexPositionColor>(Matrix.Identity, this.vertices, this.vertexCount, null, null);
		// 	}
		// }

		// // Token: 0x04000CC2 RID: 3266
		// private const int RayCount = 6;

		// // Token: 0x04000CC3 RID: 3267
		// private VertexPositionColor[] vertices = new VertexPositionColor[36];

		// // Token: 0x04000CC4 RID: 3268
		// private int vertexCount;

		// // Token: 0x04000CC5 RID: 3269
		// private Color rayColor = Calc.HexToColor("f52b63") * 0.5f;

		// // Token: 0x04000CC6 RID: 3270
		// private GooberGodrays.Ray[] rays = new GooberGodrays.Ray[6];

		// // Token: 0x04000CC7 RID: 3271
		// private float fade;

		// // Token: 0x02000519 RID: 1305
		// private struct Ray
		// {
		// 	// Token: 0x06002534 RID: 9524 RVA: 0x000F7B34 File Offset: 0x000F5D34
		// 	public void Reset()
		// 	{
		// 		this.Percent = 0f;
		// 		this.X = Calc.Random.NextFloat(384f);
		// 		this.Y = Calc.Random.NextFloat(244f);
		// 		this.Duration = 4f + Calc.Random.NextFloat() * 8f;
		// 		this.Width = (float)Calc.Random.Next(8, 16);
		// 		this.Length = (float)Calc.Random.Next(20, 40);
		// 	}

		// 	// Token: 0x040024FC RID: 9468
		// 	public float X;

		// 	// Token: 0x040024FD RID: 9469
		// 	public float Y;

		// 	// Token: 0x040024FE RID: 9470
		// 	public float Percent;

		// 	// Token: 0x040024FF RID: 9471
		// 	public float Duration;

		// 	// Token: 0x04002500 RID: 9472
		// 	public float Width;

		// 	// Token: 0x04002501 RID: 9473
		// 	public float Length;
		// }
	}
}
