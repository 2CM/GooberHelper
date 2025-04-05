using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;
using System.Collections.Generic;
using MonoMod.Utils;
using System.Collections;
using System.Linq;
using System.Diagnostics;

/*
group propagation process:
- an entity awakes
- if its not assigned to be a leader,
	- it claims the leader position
	- it searches for all neighbors
	- it sets them to be followers and assigns them a pointer to itself
- else
	- it searches for all neighbors
	- it sets them to be followers and assigns them a pointer to its leader
*/

namespace Celeste.Mod.GooberHelper.Entities {

    [CustomEntity("GooberHelper/GooberCrumblePlatform")]
	[Tracked(false)]
    public class GooberCrumblePlatform : Solid {
		// Token: 0x06001A03 RID: 6659 RVA: 0x000A6DA4 File Offset: 0x000A4FA4
		public GooberCrumblePlatform(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, false) {
			this.texture = GFX.Game["objects/crumbleBlock/" + data.Attr("texture")];
			
			//i dont know why im using reflection for this but i reaaallllly dont want to write the same thing 4 times in a row

			Dictionary<string, float> fields = new Dictionary<string, float>() {
				{"respawnTime", 2f},
				{"crumbleTime", 0.4f},
				{"minGrabCrumbleTime", 0.6f},
				{"minTopCrumbleTime", 0.2f},
			};

			//CORRECT SYNTAX FOR THIS: 0.4,0.2x5,you get it

			foreach(var item in fields) {
				List<float> list = new List<float>();

				foreach(string segment in data.Attr(item.Key, item.Value.ToString()).Split(",")) {
					float value = float.Parse(segment.Split("x")[0]);
					int amount = segment.Split("x").Length > 1 ? int.Parse(segment.Split("x")[1]) : 1;

					for(int i = 0; i < amount; i++) {
						list.Add(value);

						Console.WriteLine(value);
					}
				}

				typeof(GooberCrumblePlatform).GetProperty(item.Key).SetValue(this, list);
			}
		}

		// Token: 0x06001A04 RID: 6660 RVA: 0x000A6DC0 File Offset: 0x000A4FC0
		public override void Added(Scene scene) {
			base.Added(scene);
			this.isVertical = this.Height > this.Width;
			MTexture mtexture = GFX.Game["objects/crumbleBlock/outline"];
			this.outline = new List<Image>();
			
			if(this.isVertical) {
				int num = 0;
				while ((float)num < base.Height) {
					Image image2 = new Image(mtexture.GetSubtexture(24, 0, 8, 8, null));
					image2.Position = new Vector2(0f, (float)num);
					image2.Color = Color.White * 0f;
					base.Add(image2);
					this.outline.Add(image2);
					num += 8;
				}
			} else {
				if (base.Width <= 8f) {
					Image image = new Image(mtexture.GetSubtexture(24, 0, 8, 8, null));
					image.Color = Color.White * 0f;
					base.Add(image);
					this.outline.Add(image);
				}
				else {
					int num = 0;
					while ((float)num < base.Width) {
						int num2;
						if (num == 0) {
							num2 = 0;
						}
						else if (num > 0 && (float)num < base.Width - 8f) {
							num2 = 1;
						}
						else {
							num2 = 2;
						}
						Image image2 = new Image(mtexture.GetSubtexture(num2 * 8, 0, 8, 8, null));
						image2.Position = new Vector2((float)num, 0f);
						image2.Color = Color.White * 0f;
						base.Add(image2);
						this.outline.Add(image2);
						num += 8;
					}
				}
			}
			base.Add(this.outlineFader = new Coroutine(true));
			this.outlineFader.RemoveOnComplete = false;
			this.images = new List<Image>();
			this.falls = new List<Coroutine>();
			this.fallOrder = new List<int>();
			int num3 = 0;
			float dimension = this.isVertical ? this.Height : this.Width;
			while ((float)num3 < dimension) {
				int num4 = (int)((Math.Abs(this.isVertical ? base.Y : base.X) + (float)num3) / 8f) % 4;
				Image image3 = new Image(texture.GetSubtexture(num4 * 8, 0, 8, 8, null));
				image3.Position = this.isVertical ? new Vector2(4f, (float)(4 + num3)) : new Vector2((float)(4 + num3), 4f);
				image3.CenterOrigin();
				base.Add(image3);
				this.images.Add(image3);
				Coroutine coroutine = new Coroutine(true);
				coroutine.RemoveOnComplete = false;
				this.falls.Add(coroutine);
				base.Add(coroutine);
				this.fallOrder.Add(num3 / 8);
				num3 += 8;
			}
			this.fallOrder.Shuffle<int>();
			base.Add(this.shaker = new ShakerList(this.images.Count, false, delegate(Vector2[] v) {
				for (int i = 0; i < this.images.Count; i++){
					this.images[i].Position = (isVertical ? new Vector2(4f, (float)(4 + i * 8)) : new Vector2((float)(4 + i * 8), 4f)) + v[i];
				}
			}));
			base.Add(this.occluder = new LightOcclude(0.2f));
		}

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

			this.followers = new HashSet<GooberCrumblePlatform>();

			if(this.isFollower == false) {
				this.isLeader = true;

				base.Add(new Coroutine(this.Sequence(), true));
			}

			//this code is so bad 😭 theres probably some field that i just Dont know exists that would make this way easier
			//its fine though

			this.moverShakers = new List<Shaker>();
			this.moverImageOriginalPositions = new Dictionary<StaticMover, Dictionary<Image, Vector2>>();

			foreach(StaticMover mover in this.staticMovers) {
				moverImageOriginalPositions[mover] = new Dictionary<Image, Vector2>();

				foreach(Image image in mover.Entity.Components.Where(component => component is Image)) {
					moverImageOriginalPositions[mover][image] = image.Position;
				}

				Shaker moverShaker = new Shaker(false, delegate(Vector2 v) {
					foreach(Image image in mover.Entity.Components.Where(component => component is Image)) {
						image.Position = this.moverImageOriginalPositions[mover][image] + v;
					}
				});

				this.moverShakers.Add(moverShaker);

				mover.Entity.Add(moverShaker);
			}

			var neighbors = this.CollideAll<GooberCrumblePlatform>(this.Position + new Vector2(1, 1)).Concat(this.CollideAll<GooberCrumblePlatform>(this.Position + new Vector2(-1, -1))).ToHashSet();

			foreach(GooberCrumblePlatform platform in neighbors) {
				if(!platform.isLeader) platform.isFollower = true;

				if(this.isFollower) {
					platform.leader = this.leader;

					this.leader.followers.Add(this);
				} else {
					platform.leader = this;

					this.followers.Add(platform);
				}
			}
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);

			if(this.isFollower == true) {
				Draw.Circle(this.Center, 5f, Color.Green, 16);
			}

			foreach(GooberCrumblePlatform platform in this.followers) {
				Draw.Line(this.Center, platform.Center, Color.Red);
			}
        }

        public IEnumerator StartCrumbling(bool fromTop, bool playSound = true) {
            if(playSound) Audio.Play("event:/game/general/platform_disintegrate", base.Center);
            this.shaker.ShakeFor(fromTop ? 0.6f : 1f, false);
            foreach (Image image in this.images) {
                base.SceneAs<Level>().Particles.Emit(CrumblePlatform.P_Crumble, 2, this.Position + image.Position + new Vector2(0f, 2f), Vector2.One * 3f);
            }
            int num;
            for (int i = 0; i < (fromTop ? 1 : 3); i = num + 1) {
                yield return 0.2f;
                foreach (Image image2 in this.images) {
                    base.SceneAs<Level>().Particles.Emit(CrumblePlatform.P_Crumble, 2, this.Position + image2.Position + new Vector2(0f, 2f), Vector2.One * 3f);
                }
                num = i;
            }
        }

        public void EndCrumbling() {
			foreach(StaticMover mover in this.staticMovers) {
				mover.Entity.Visible = false;
				mover.Entity.Collidable = false;
			}

            this.outlineFader.Replace(this.OutlineFade(1f));
            this.occluder.Visible = false;
            this.Collidable = false;
            float num2 = 0.05f;

            for (int j = 0; j < 4; j++) {
                for (int k = 0; k < this.images.Count; k++) {
                    if (k % 4 - j == 0) {
                        this.falls[k].Replace(this.TileOut(this.images[this.fallOrder[k]], num2 * (float)j));
                    }
                }
            }
        }

        public void Respawn(bool playSound = true) {
            this.outlineFader.Replace(this.OutlineFade(0f));
            this.occluder.Visible = true;
            this.Collidable = true;

			foreach(StaticMover mover in this.staticMovers) {
				mover.Entity.Visible = true;
				mover.Entity.Collidable = true;
			}

            for (int l = 0; l < 4; l++) {
                for (int m = 0; m < this.images.Count; m++) {
                    if (m % 4 - l == 0) {
                        this.falls[m].Replace(this.TileIn(m, this.images[this.fallOrder[m]], 0.05f * (float)l, playSound));
                    }
                }
            }
        }

		// Token: 0x06001A05 RID: 6661 RVA: 0x000A7041 File Offset: 0x000A5241
		private IEnumerator Sequence() {
			while(true) {
				bool onTop = GetPlayerOnTop() != null || this.followers.Any(a => a.GetPlayerOnTop() != null);
				bool climbing = GetPlayerClimbing() != null || this.followers.Any(a => a.GetPlayerClimbing() != null);

                if(onTop || climbing) {
					bool initiatedByClimbing = climbing;

					base.Add(new Coroutine(this.StartCrumbling(!initiatedByClimbing)));
						foreach(GooberCrumblePlatform platform in this.followers) {
							platform.Add(new Coroutine(platform.StartCrumbling(!initiatedByClimbing, false)));
						}

						foreach(Shaker shaker in this.moverShakers) {
							shaker.ShakeFor(initiatedByClimbing ? 1f : 0.6f, false);
						}

					yield return initiatedByClimbing ?
						(this.iteration < this.minGrabCrumbleTime.Count ? this.minGrabCrumbleTime.ElementAt(this.iteration) : this.minGrabCrumbleTime.Last()) :
						(this.iteration < this.minTopCrumbleTime.Count ? this.minTopCrumbleTime.ElementAt(this.iteration) : this.minTopCrumbleTime.Last());

                    float timer = this.iteration < this.crumbleTime.Count ? this.crumbleTime.ElementAt(this.iteration) : this.crumbleTime.Last();

                    while(timer > 0) {
                        timer -= Engine.DeltaTime;

                        if(!initiatedByClimbing && !(GetPlayerOnTop() != null || this.followers.Any(a => a.GetPlayerOnTop() != null))) break;

                        yield return null;
                    }

                    EndCrumbling();
						foreach(GooberCrumblePlatform platform in this.followers) {
							platform.EndCrumbling();
						}

                    yield return this.iteration < this.respawnTime.Count ? this.respawnTime.ElementAt(this.iteration) : this.respawnTime.Last();
                    while (base.CollideCheck<Actor>() || base.CollideCheck<Solid>() || this.followers.Any(a => a.CollideCheck<Actor>() || a.CollideCheck<Solid>())) {
                        yield return null;
                    }

                    Respawn();
						foreach(GooberCrumblePlatform platform in this.followers) {
							platform.Respawn(false);
						}
					
					this.iteration++;
                }

                yield return null;
            }
		}

		// Token: 0x06001A06 RID: 6662 RVA: 0x000A7050 File Offset: 0x000A5250
		private IEnumerator OutlineFade(float to) {
			float from = 1f - to;
			for (float t = 0f; t < 1f; t += Engine.DeltaTime * 2f) {
				Color color = Color.White * (from + (to - from) * Ease.CubeInOut(t));
				foreach (Image image in this.outline) {
					image.Color = color;
				}
				yield return null;
			}
			yield break;
		}

		// Token: 0x06001A07 RID: 6663 RVA: 0x000A7066 File Offset: 0x000A5266
		private IEnumerator TileOut(Image img, float delay)
		{
			img.Color = Color.Gray;
			yield return delay;
			float distance = (img.X * 7f % 3f + 1f) * 12f;
			Vector2 from = img.Position;
			for (float time = 0f; time < 1f; time += Engine.DeltaTime / 0.4f) {
				yield return null;
				img.Position = from + Vector2.UnitY * Ease.CubeIn(time) * distance;
				img.Color = Color.Gray * (1f - time);
				img.Scale = Vector2.One * (1f - time * 0.5f);
			}
			img.Visible = false;
			yield break;
		}

		// Token: 0x06001A08 RID: 6664 RVA: 0x000A707C File Offset: 0x000A527C
		private IEnumerator TileIn(int index, Image img, float delay, bool playSound = true)
		{
			yield return delay;
			if(playSound) Audio.Play("event:/game/general/platform_return", base.Center);
			img.Visible = true;
			img.Color = Color.White;
			img.Position = this.isVertical ? new Vector2(4f, (float)(index * 8 + 4)) : new Vector2((float)(index * 8 + 4), 4f);
			for (float time = 0f; time < 1f; time += Engine.DeltaTime / 0.25f) {
				yield return null;
				img.Scale = Vector2.One * (1f + Ease.BounceOut(1f - time) * 0.2f);
			}
			img.Scale = Vector2.One;
			yield break;
		}

		public int iteration = 0;

		public List<float> respawnTime { get; set; }

		public List<float> crumbleTime { get; set; }

		public List<float> minGrabCrumbleTime { get; set; }

		public List<float> minTopCrumbleTime { get; set; }

		public MTexture texture;

		public bool isVertical;

		public bool isLeader;	

		public bool isFollower;

		public GooberCrumblePlatform leader;

		public HashSet<GooberCrumblePlatform> followers;

		public List<Shaker> moverShakers;

		public Dictionary<StaticMover, Dictionary<Image, Vector2>> moverImageOriginalPositions;

		// Token: 0x040016A8 RID: 5800
		public static ParticleType P_Crumble;

		// Token: 0x040016A9 RID: 5801
		private List<Image> images;

		// Token: 0x040016AA RID: 5802
		private List<Image> outline;

		// Token: 0x040016AB RID: 5803
		private List<Coroutine> falls;

		// Token: 0x040016AC RID: 5804
		private List<int> fallOrder;

		// Token: 0x040016AD RID: 5805
		private ShakerList shaker;

		// Token: 0x040016AE RID: 5806
		private LightOcclude occluder;

		// Token: 0x040016AF RID: 5807
		private Coroutine outlineFader;
    }
}