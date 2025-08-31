using System;
using System.Reflection;
using Celeste.Editor;
using ExtendedVariants.Variants.Vanilla;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.GooberHelper.Entities {

    public class BufferOffsetIndicator : Entity {
        private Image image;
        private Color color;
        private float timer = 0f;

        public BufferOffsetIndicator(VirtualButton button) : base() {
            Player player = Engine.Scene.Tracker.GetEntity<Player>();

            this.Position = player.Center;
            this.timer = 1f;
            this.Depth = Depths.Above;

            float counter = button.bufferCounter;

            if(button == Input.Dash) {
                counter = Math.Max(counter, Input.CrouchDash.bufferCounter);
            }

            this.color =
                counter == button.BufferTime ? Color.Turquoise :
                Color.Lerp(Color.Red, Color.Lime, counter / button.BufferTime);

            this.image = new Image(button == Input.Jump ? GFX.Game["GooberHelper/jumpBufferIndicator"] : GFX.Game["GooberHelper/dashBufferIndicator"])
                .SetColor(color)
                .CenterOrigin();
            
            image.FlipX = player.Facing == Facings.Left;

            Add(this.image);
        }
        
        public static void Load() {
            IL.Celeste.Player.Jump += hookConsumeBuffer;
            IL.Celeste.Player.SuperJump += hookConsumeBuffer;
            IL.Celeste.Player.SuperWallJump += hookConsumeBuffer;
            IL.Celeste.Player.StartDash += hookConsumeBuffer;
            IL.Celeste.Player.SwimUpdate += hookConsumeBuffer;
            IL.Celeste.Player.BoostUpdate += hookConsumeBuffer;
            IL.Celeste.Player.HitSquashUpdate += hookConsumeBuffer;
            IL.Celeste.Player.WallJump += hookConsumeBuffer;
        }

        public static void Unload() {
            IL.Celeste.Player.Jump -= hookConsumeBuffer;
            IL.Celeste.Player.SuperJump -= hookConsumeBuffer;
            IL.Celeste.Player.SuperWallJump -= hookConsumeBuffer;
            IL.Celeste.Player.StartDash -= hookConsumeBuffer;
            IL.Celeste.Player.SwimUpdate -= hookConsumeBuffer;
            IL.Celeste.Player.BoostUpdate -= hookConsumeBuffer;
            IL.Celeste.Player.HitSquashUpdate -= hookConsumeBuffer;
            IL.Celeste.Player.WallJump -= hookConsumeBuffer;
        }

        private static void hookConsumeBuffer(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            MethodBase method = typeof(VirtualButton).GetMethod("ConsumeBuffer");

            if(cursor.TryGotoNext(MoveType.AfterLabel, instr => instr.MatchCallOrCallvirt(method))) {
                cursor.EmitDup();
                cursor.EmitDelegate((VirtualButton button) => {
                    if(!OptionsManager.GetOptionBool(OptionsManager.Option.BufferDelayVisualization)) return;

                    if(Engine.Scene is Level) {
                        Engine.Scene.Add(new BufferOffsetIndicator(button));
                    }
                });
            }
        }

        public override void Update() {
            base.Update();

            this.timer -= Engine.DeltaTime;
            this.image.Color = this.color * Math.Max(0.5f, this.timer / 1f);

            if(this.timer < 0f) {
                this.image.Scale = Vector2.One * Calc.LerpClamp(1f, 0f, Ease.BackIn(-this.timer * 4f));

                if(this.image.Scale.X == 0f) {
                    RemoveSelf();
                }
            }
        }
    }
}