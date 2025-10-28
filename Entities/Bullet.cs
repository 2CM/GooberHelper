using System;
using System.Collections;
using System.Reflection;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using NLua;

#nullable enable

namespace Celeste.Mod.GooberHelper.Entities {
    [Tracked(false)]
    public class Bullet : Actor {
        public enum BulletRotationMode {
            None,
            Velocity,
            PositionChange
        }

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
                        DynamicData.For(level).Get<HighResolutionBulletRenderer>("HighResolutionBulletRenderer")?.Render(level);
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
        public float Rotation = 0f;
        public BulletRotationMode RotationMode = BulletRotationMode.PositionChange;

        public Vector2 ActualPosition {
            get => base.Position;
            set => base.Position = value;
        }

        //evil
        public new Vector2 Position {
            get => base.Position - Parent.BulletFieldCenter;
            set => base.Position = value + Parent.BulletFieldCenter;
        }

        public PlayerCollider PlayerCollider;

        public Bullet(BulletActivator parent, LuaTable props) : base(parent.BulletFieldCenter + (Vector2)(props["position"] ?? Vector2.Zero)) {
            parent.Scene.Add(this);
            Parent = parent;
            
            Add(PlayerCollider = new PlayerCollider(onCollidePlayer, new Circle(2)));

            if(props["template"] is LuaTable template) 
                ApplyProps(template);

            ApplyProps(props);
        }

        public void ApplyProps(LuaTable props) {
            if(props["velocity"] is Vector2 velocity) Velocity = velocity;
            if(props["acceleration"] is Vector2 acceleration) Acceleration = acceleration;
            if(props["color"] is Color color) Color = color;
            if(props["texture"] is string texture) Texture = texture;
            if(props["scale"] is double scale) Scale = (float)scale;
            if(props["effect"] is string effect) Effect = effect;
            if(props["additive"] is bool additive) Additive = additive;
            if(props["highResolution"] is bool highResolution) HighResolution = highResolution;
            if(props["colliderRadius"] is double colliderRadius) (PlayerCollider.Collider as Circle)!.Radius = (float)colliderRadius;
        }

        public void InterpolateValue(string key, object to, float time = 1f, Ease.Easer? easer = null) {
            if(typeof(Bullet).GetField(key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance) is not FieldInfo fieldInfo) 
                throw new Exception("failed to find fieldinfo of value to interpolate");

            if(key == "Position" && to is Vector2 toVector)
                to = toVector + Parent.BulletFieldCenter;

            var from = fieldInfo.GetValue(this);

            object difference = (from, to) switch {
                (Vector2 fromValue, Vector2 toValue) => toValue - fromValue,
                (Color fromValue, Color toValue) => new Color(toValue.ToVector4() - fromValue.ToVector4()),
                (float fromValue, float toValue) => toValue - fromValue,
                (float fromValue, double toValue) => (float)(toValue - fromValue),
                (float fromValue, int toValue) => toValue - fromValue,
                _ => throw new Exception("from and to values in interpolation dont match types or they arent valid types to interpolate"),
            };

            Add(new Coroutine(interpolateCoroutine(fieldInfo, difference, time, easer ?? Ease.SineInOut), true));
        }

        private IEnumerator interpolateCoroutine<T>(FieldInfo fieldInfo, T difference, float time, Ease.Easer easer) {
            var dx = 0.001f;
            var progress = 0f;

            while(progress < 1f) {
                var derivative = (easer(progress + dx) - easer(progress - dx))/dx * Engine.DeltaTime;
                var current = fieldInfo.GetValue(this);

                object newValue = (difference, current) switch {
                    (Vector2 diff, Vector2 curr) => curr + diff * derivative,
                    (Color diff, Color curr) => new Color(curr.ToVector4() + (diff * derivative).ToVector4()),
                    (float diff, float curr) => curr + diff * derivative,
                    _ => throw new Exception("interpolation difference doesnt match value type"),
                };

                // if(
                //     RotationMode == BulletRotationMode.PositionChange &&
                //     difference is Vector2 differenceVector &&
                //     fieldInfo.Name == "Position"
                // ) {
                //     Rotation = (this.Velocity + differenceVector * derivative).Angle() + MathF.PI/2;
                // }

                fieldInfo.SetValue(this, newValue);
                
                progress += Engine.DeltaTime / time;
                yield return null;
            }

            yield break;
        }

        public override void Update() {
            Vector2 oldPosition = this.Position;

            base.Update();

            ActualPosition += Velocity * Engine.DeltaTime;
            Velocity += Acceleration * Engine.DeltaTime;

            Rotation = RotationMode switch {
                BulletRotationMode.Velocity => Rotation = this.Velocity.Angle() + MathF.PI / 2,
                BulletRotationMode.PositionChange => Rotation = (Position - oldPosition).Angle() + MathF.PI / 2,
                _ => Rotation,
            };

            if(Position.Length() > 200) this.RemoveSelf();
        }

        private void onCollidePlayer(Player player) {
            // player.Die((player.Position - Position).SafeNormalize());

            player.Play("event:/char/madeline/death");

            RemoveSelf();
        }

        //todo please optimize this
        public override void Render() {
            base.Render();

            if(Engine.Scene is not Level level || HighResolutionBulletRenderer.DontRender) return;

            var bulletEffect = ModIntegration.FrostHelperAPI.GetEffectOrNull.Invoke(Effect);

            //init effect
            if(HighResolution)
                Draw.SpriteBatch.End();
            else
                GameplayRenderer.End();

            Matrix matrix = level.GameplayRenderer.Camera.Matrix;

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
            GFX.Game[Texture].DrawCentered(this.ActualPosition, this.Color, this.Scale, this.Rotation);
            
            //uninit effect
            Draw.SpriteBatch.End();
            
            if(HighResolution)
                HudRenderer.BeginRender();
            else
                GameplayRenderer.Begin();
        }
    }
}

/*
        // public class BulletReference {
        //     private static Dictionary<string, FieldInfo> bulletFieldMap = generateBulletFieldMap();
        //     public Bullet Reference;

        //     public BulletReference(Bullet reference) {
        //         this.Reference = reference;

        //         Console.WriteLine("yo");
        //     }

        //     private static Dictionary<string, FieldInfo> generateBulletFieldMap() {
        //         return typeof(Bullet)
        //             .GetFields(BindingFlags.Instance | BindingFlags.Public)
        //             .ToDictionary(field => field.Name.ToLower(), field => field);
        //     }

        //     public object this[string key] {
        //         get {
        //             Console.WriteLine("ijef");

        //             FieldInfo fieldInfo = bulletFieldMap[key.ToLower()];

        //             if(fieldInfo.GetValue(Reference) is object obj)
        //                 return new ValueContainer<object>(obj);

        //             Logger.Error("GooberHelper", "tried to access invalid bullet field");

        //             return null;
        //         }
        //         set {
        //             FieldInfo fieldInfo = bulletFieldMap[key.ToLower()];

        //             fieldInfo.SetValue(Reference, value);
        //         } 
        //     }
        // }

        // public class ValueContainer<T> {
        //     public T Value;

        //     public ValueContainer(T value) {
        //         Value = value;
        //     }

        //     public static implicit operator T(ValueContainer<T> obj) {
        //         return (T)obj;
        //     }
        // }
*/  