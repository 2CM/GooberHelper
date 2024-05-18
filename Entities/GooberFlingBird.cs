using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;
using System.Collections.Generic;
using MonoMod.Utils;
using System.Collections;

namespace Celeste.Mod.GooberHelper.Entities {

    [CustomEntity("GooberHelper/GooberFlingBird")]
    public class GooberFlingBird : Entity {
		private MTexture indicatorTexture = GFX.Game["birdIndicator"];

        public GooberFlingBird(Vector2[] nodes, bool skippable, int dir, int index, bool indicator = true)
			: base(nodes[0])
		{
			this.index = index;
			this.dir = dir;
			base.Depth = -1;
			base.Add(this.sprite = GFX.SpriteBank.Create("bird"));
			this.sprite.Play("hover", false, false);
			this.sprite.Scale.X = -1f;
			this.sprite.Position = this.spriteOffset;
			this.sprite.OnFrameChange = delegate(string spr)
			{
				BirdNPC.FlapSfxCheck(this.sprite);
			};
			base.Collider = new Circle(16f, 0f, 0f);
			base.Add(new PlayerCollider(new Action<Player>(this.OnPlayer), null, null));
			base.Add(this.moveSfx = new SoundSource());
			this.NodeSegments = new List<Vector2[]>();
			this.NodeSegments.Add(nodes);
			this.SegmentsWaiting = new List<bool>();
			this.SegmentsWaiting.Add(skippable);
			this.SegmentDirections = new List<int>();
			this.SegmentDirections.Add(dir);
			this.SegmentIndicators = new List<bool>();
			this.SegmentIndicators.Add(indicator);
			base.Add(new TransitionListener
			{
				OnOut = delegate(float t)
				{
					this.sprite.Color = Color.White * (1f - Calc.Map(t, 0f, 0.4f, 0f, 1f));
				}
			});
		}

		// Token: 0x06000EBD RID: 3773 RVA: 0x00037B73 File Offset: 0x00035D73
		public GooberFlingBird(EntityData data, Vector2 levelOffset)
			: this(data.NodesWithPosition(levelOffset), data.Bool("waiting", false), data.Bool("reverse", false) == true ? -1 : 1, data.Int("index", 0), data.Bool("indicator", true))
		{
			this.entityData = data;
		}

		// Token: 0x06000EBE RID: 3774 RVA: 0x00037B98 File Offset: 0x00035D98
		public override void Awake(Scene scene)
		{
			base.Awake(scene);
			List<GooberFlingBird> list = base.Scene.Entities.FindAll<GooberFlingBird>();
			for (int i = list.Count - 1; i >= 0; i--)
			{
				if (list[i].entityData.Level.Name != this.entityData.Level.Name)
				{
					list.RemoveAt(i);
				}
			}
			list.Sort((GooberFlingBird a, GooberFlingBird b) => Math.Sign(a.index - b.index));
			if (list[0] == this)
			{
				for (int j = 1; j < list.Count; j++)
				{
					this.NodeSegments.Add(list[j].NodeSegments[0]);
					this.SegmentsWaiting.Add(list[j].SegmentsWaiting[0]);
					this.SegmentDirections.Add(list[j].SegmentDirections[0]);
					this.SegmentIndicators.Add(list[j].SegmentIndicators[0]);
					list[j].RemoveSelf();
				}
			}
			if (this.SegmentsWaiting[0])
			{
				this.sprite.Play("hoverStressed", false, false);
				this.sprite.Scale.X = 1f;
			}
			// Player entity = scene.Tracker.GetEntity<Player>();
			// if (entity != null && entity.X > base.X)
			// {
			// 	base.RemoveSelf();
			// }
		}

		// Token: 0x06000EBF RID: 3775 RVA: 0x00037CE2 File Offset: 0x00035EE2
		private void Skip()
		{
			this.state = GooberFlingBird.States.Move;
			base.Add(new Coroutine(this.MoveRoutine(), true));
		}

		// Token: 0x06000EC0 RID: 3776 RVA: 0x00037D00 File Offset: 0x00035F00
		private void OnPlayer(Player player)
		{
			if (this.state == GooberFlingBird.States.Wait && DoPlayerStuff())
			{
				this.flingSpeed = player.Speed * 0.4f;
				this.flingSpeed.Y = 120f;
				this.flingTargetSpeed = new Vector2(Math.Max(Math.Abs(player.Speed.X) + 60.0f, Math.Abs(GooberFlingBird.FlingSpeed.X)) * this.SegmentDirections[this.segmentIndex], GooberFlingBird.FlingSpeed.Y);
				this.flingAccel = 4000f;
				player.Speed = Vector2.Zero;

				this.state = GooberFlingBird.States.Fling;
				base.Add(new Coroutine(this.DoFlingRoutine(player), true));
				Audio.Play("event:/new_content/game/10_farewell/bird_throw", base.Center);
				player.MoveV(Math.Sign(player.Y - this.Center.Y) * -3);
				player.MoveH(Math.Sign(player.X - this.Center.X) * -3);
			}
		}

		// Token: 0x06000EC1 RID: 3777 RVA: 0x00037D90 File Offset: 0x00035F90
		public override void Update()
		{
			base.Update();
			if (this.state != GooberFlingBird.States.Wait)
			{
				this.sprite.Position = Calc.Approach(this.sprite.Position, this.spriteOffset, 32f * Engine.DeltaTime);
			}
			switch (this.state)
			{
			case GooberFlingBird.States.Wait:
			{
				Player entity = base.Scene.Tracker.GetEntity<Player>();
				// if (entity != null && entity.X - base.X >= 100f)
				// {
				// 	this.Skip();
				// 	return;
				// }
				if (this.SegmentsWaiting[this.segmentIndex] && this.LightningRemoved)
				{
					this.Skip();
					return;
				}
				if (entity != null)
				{
					float num = Calc.ClampedMap((entity.Center - this.Position).Length(), 16f, 64f, 12f, 0f);
					Vector2 vector = (entity.Center - this.Position).SafeNormalize();
					this.sprite.Position = Calc.Approach(this.sprite.Position, this.spriteOffset + vector * num, 32f * Engine.DeltaTime);
					return;
				}
				break;
			}
			case GooberFlingBird.States.Fling:
				Logger.Log(LogLevel.Info, "b", this.flingSpeed.ToString());

				if (this.flingAccel > 0f)
				{
					this.flingSpeed = Calc.Approach(this.flingSpeed, this.flingTargetSpeed, this.flingAccel * Engine.DeltaTime);
				}
				this.Position += this.flingSpeed * Engine.DeltaTime;

				Logger.Log(LogLevel.Info, "a", this.flingSpeed.ToString());
				return;
			case GooberFlingBird.States.Move:
				break;
			case GooberFlingBird.States.WaitForLightningClear:
				if (base.Scene.Entities.FindFirst<Lightning>() != null || base.X > (float)(base.Scene as Level).Bounds.Right)
				{
					this.sprite.Scale.X = 1f;
					this.state = GooberFlingBird.States.Leaving;
					base.Add(new Coroutine(this.LeaveRoutine(), true));
				}
				break;
			default:
				return;
			}
		}

		private void LaunchPlayer(Player player) {
			player.StateMachine.State = 0;
			player.AutoJump = true;
			player.Speed = this.flingSpeed;// * new Vector2(this.SegmentDirections[this.segmentIndex], 1);

			DynamicData data = DynamicData.For(player);

			data.Set("varJumpTimer", 0.2f);
			data.Set("varJumpSpeed", player.Speed.Y);
			data.Set("launched", true);
		}

		private bool DoPlayerStuff() {
			Player player = Scene.Tracker.GetEntity<Player>();

			if(player.StateMachine.State != GooberFlingBird.CustomStateId) {
				player.RefillDash();
				player.RefillStamina();
				player.StateMachine.State = GooberFlingBird.CustomStateId;
				player.DummyGravity = false;
				player.DummyAutoAnimate = false;
				DynamicData.For(player).Set("varJumpTimer", 0.0f);

				GooberFlingBird.currentBird = this;

				return true;
			}

			return false;
		}

		// Token: 0x06000EC2 RID: 3778 RVA: 0x00037F84 File Offset: 0x00036184
		private IEnumerator DoFlingRoutine(Player player)
		{
			// Level level = base.Scene as Level;
			// Vector2 position = level.Camera.Position;
			// Vector2 vector = player.Position - position;
			// vector.X = Calc.Clamp(vector.X, 145f, 215f);
			// vector.Y = Calc.Clamp(vector.Y, 85f, 95f);
			// base.Add(new Coroutine(level.ZoomTo(vector, 1.1f, 0.2f), true));
			// Engine.TimeRate = 0.8f;
			// Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
			// while (this.flingSpeed != Vector2.Zero)
			// {
			// 	yield return null;
			// }
			// this.sprite.Play("throw", false, false);
			// this.sprite.Scale.X = 1f;
			// this.flingSpeed = new Vector2(-140f * this.SegmentDirections[this.segmentIndex], 140f);
			// this.flingTargetSpeed = Vector2.Zero;
			// this.flingAccel = 1400f;
			// yield return 0.1f;
			// Celeste.Freeze(0.05f);
			// this.flingTargetSpeed = GooberFlingBird.FlingSpeed;
			// this.flingAccel = 6000f;
			// yield return 0.1f;
			// Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
			// Engine.TimeRate = 1f;
			// level.Shake(0.3f);
			// base.Add(new Coroutine(level.ZoomBack(0.1f), true));
			// LaunchPlayer(player);
			// this.flingTargetSpeed = Vector2.Zero;
			// this.flingAccel = 4000f;
			// yield return 0.3f;
			// base.Add(new Coroutine(this.MoveRoutine(), true));
			// yield break;

			// while (this.flingSpeed != Vector2.Zero)
			// {
			// 	yield return null;
			// }

			// this.sprite.Play("throw", false, false);
			// this.flingSpeed = new Vector2(-140f * this.SegmentDirections[this.segmentIndex], 140f);
			// this.flingTargetSpeed = Vector2.Zero;
			// this.flingAccel = 1400f;
			// yield return 0.1f;

			while(Math.Abs(this.flingSpeed.X) < Math.Abs(this.flingTargetSpeed.X)) {
				yield return null;
			}



			LaunchPlayer(player);
			this.flingTargetSpeed = Vector2.Zero;
			this.flingAccel = 4000f;

			base.Add(new Coroutine(this.MoveRoutine(), true));
			yield break;
		}

		// Token: 0x06000EC3 RID: 3779 RVA: 0x00037F9A File Offset: 0x0003619A
		private IEnumerator MoveRoutine()
		{
			this.state = GooberFlingBird.States.Move;
			this.sprite.Play("fly", false, false);
			this.sprite.Scale.X = 1f;
			this.moveSfx.Play("event:/new_content/game/10_farewell/bird_relocate", null, 0f);
			for (int nodeIndex = 1; nodeIndex < this.NodeSegments[this.segmentIndex].Length - 1; nodeIndex += 2)
			{
				Vector2 position = this.Position;
				Vector2 vector = this.NodeSegments[this.segmentIndex][nodeIndex];
				Vector2 vector2 = this.NodeSegments[this.segmentIndex][nodeIndex + 1];
				yield return this.MoveOnCurve(position, vector, vector2);
			}
			this.segmentIndex++;
			bool atEnding = this.segmentIndex >= this.NodeSegments.Count;
			if (!atEnding)
			{
				Vector2 position2 = this.Position;
				Vector2 vector3 = this.NodeSegments[this.segmentIndex - 1][this.NodeSegments[this.segmentIndex - 1].Length - 1];
				Vector2 vector4 = this.NodeSegments[this.segmentIndex][0];
				yield return this.MoveOnCurve(position2, vector3, vector4);
			}
			this.sprite.Rotation = 0f;
			this.sprite.Scale = Vector2.One;
			if (atEnding)
			{
				this.sprite.Play("hoverStressed", false, false);
				this.sprite.Scale.X = 1f;
				this.state = GooberFlingBird.States.WaitForLightningClear;
			}
			else
			{
				if (this.SegmentsWaiting[this.segmentIndex])
				{
					this.sprite.Play("hoverStressed", false, false);
				}
				else
				{
					this.sprite.Play("hover", false, false);
				}
				this.sprite.Scale.X = -1f;
				this.state = GooberFlingBird.States.Wait;
			}
			yield break;
		}

		// Token: 0x06000EC4 RID: 3780 RVA: 0x00037FA9 File Offset: 0x000361A9
		private IEnumerator LeaveRoutine()
		{
			this.sprite.Scale.X = 1f;
			this.sprite.Play("fly", false, false);
			Vector2 vector = new Vector2((float)((base.Scene as Level).Bounds.Right + 32), base.Y);
			yield return this.MoveOnCurve(this.Position, (this.Position + vector) * 0.5f - Vector2.UnitY * 12f, vector);
			base.RemoveSelf();
			yield break;
		}

		// Token: 0x06000EC5 RID: 3781 RVA: 0x00037FB8 File Offset: 0x000361B8
		private IEnumerator MoveOnCurve(Vector2 from, Vector2 anchor, Vector2 to)
		{
			SimpleCurve curve = new SimpleCurve(from, to, anchor);
			float duration = curve.GetLengthParametric(32) / 2000f;
			Vector2 was = from;
			for (float t = 0.016f; t <= 1f; t += Engine.DeltaTime / duration)
			{
				this.Position = curve.GetPoint(t).Floor();
				this.sprite.Rotation = Calc.Angle(curve.GetPoint(Math.Max(0f, t - 0.05f)), curve.GetPoint(Math.Min(1f, t + 0.05f)));
				this.sprite.Scale.X = 1.25f;
				this.sprite.Scale.Y = 0.7f;
				if ((was - this.Position).Length() > 32f)
				{
					TrailManager.Add(this, this.trailColor, 1f, false, false);
					was = this.Position;
				}
				yield return null;
			}
			this.Position = to;
			yield break;
		}

		// Token: 0x06000EC6 RID: 3782 RVA: 0x0002FD8C File Offset: 0x0002DF8C
		public override void Render()
		{
			base.Render();
			
			for(int i = this.segmentIndex + 1; i < this.NodeSegments.Count; i++) {
				Vector2 segment = this.NodeSegments[i][0];

				if(!this.SegmentIndicators[i]) continue;

				indicatorTexture.DrawCentered(segment, Color.White, new Vector2(this.SegmentDirections[i], 1));
			}
		}

		// Token: 0x06000EC7 RID: 3783 RVA: 0x00037FDC File Offset: 0x000361DC
		private void DrawLine(Vector2 a, Vector2 anchor, Vector2 b)
		{
			SimpleCurve simpleCurve = new SimpleCurve(a, b, anchor);
			simpleCurve.Render(Color.Red, 32);
		}

		// Token: 0x040009F0 RID: 2544
		public static ParticleType P_Feather;

		// Token: 0x040009F1 RID: 2545
		public const float SkipDist = 100f;

		// Token: 0x040009F2 RID: 2546
		public static readonly Vector2 FlingSpeed = new Vector2(380f, -100f);

		// Token: 0x040009F3 RID: 2547
		private Vector2 spriteOffset = new Vector2(0f, 8f);

		// Token: 0x040009F4 RID: 2548
		private Sprite sprite;

		// Token: 0x040009F5 RID: 2549
		private GooberFlingBird.States state;

		// Token: 0x040009F6 RID: 2550
		private Vector2 flingSpeed;

		// Token: 0x040009F7 RID: 2551
		private Vector2 flingTargetSpeed;

		// Token: 0x040009F8 RID: 2552
		private float flingAccel;

		// Token: 0x040009F9 RID: 2553
		private Color trailColor = Calc.HexToColor("639bff");

		// Token: 0x040009FA RID: 2554
		private EntityData entityData;

		// Token: 0x040009FB RID: 2555
		private SoundSource moveSfx;

		// Token: 0x040009FC RID: 2556
		private int segmentIndex;

		// Token: 0x040009FD RID: 2557
		public List<Vector2[]> NodeSegments;

		// Token: 0x040009FE RID: 2558
		public List<bool> SegmentsWaiting;

		public List<int> SegmentDirections;

		public List<bool> SegmentIndicators;

		// Token: 0x040009FF RID: 2559
		public bool LightningRemoved;

		// Token: 0x020004B6 RID: 1206
		private enum States
		{
			// Token: 0x0400233F RID: 9023
			Wait,
			// Token: 0x04002340 RID: 9024
			Fling,
			// Token: 0x04002341 RID: 9025
			Move,
			// Token: 0x04002342 RID: 9026
			WaitForLightningClear,
			// Token: 0x04002343 RID: 9027
			Leaving
		}

		public int dir = 1;

		public int index = 0;

		public static int CustomStateId = 3874783;

		public static GooberFlingBird currentBird = null;

		public static int CustomStateUpdate() {
			Player player = Engine.Scene.Tracker.GetEntity<Player>();

			player.MoveTowardsX(currentBird.X, 400f * Engine.DeltaTime, null);
			player.MoveTowardsY(currentBird.Y + 8f + player.Collider.Height, 400f * Engine.DeltaTime, null);

			return CustomStateId;
		}
	}
}