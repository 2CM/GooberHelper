using System;
using System.Reflection;
using Celeste.Editor;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.GooberHelper.Entities {

    public class BufferOffsetIndicator : Entity {
        private Image image;
        private Color color;
        private float timer = 0f;

        private static ILHook playerWallJumpHook;

        public BufferOffsetIndicator(VirtualButton button, Player player) : base() {
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
            playerWallJumpHook = new ILHook(typeof(Player).GetMethod("orig_WallJump", BindingFlags.NonPublic | BindingFlags.Instance), hookConsumeBuffer);

            IL.Celeste.Player.Jump += hookConsumeBuffer;
            IL.Celeste.Player.SuperJump += hookConsumeBuffer;
            IL.Celeste.Player.SuperWallJump += hookConsumeBuffer;
            IL.Celeste.Player.StartDash += hookConsumeBuffer;
            IL.Celeste.Player.SwimUpdate += hookConsumeBuffer;
            IL.Celeste.Player.BoostUpdate += hookConsumeBuffer;
            IL.Celeste.Player.HitSquashUpdate += hookConsumeBuffer;
        }

        public static void Unload() {
            playerWallJumpHook.Dispose();

            IL.Celeste.Player.Jump -= hookConsumeBuffer;
            IL.Celeste.Player.SuperJump -= hookConsumeBuffer;
            IL.Celeste.Player.SuperWallJump -= hookConsumeBuffer;
            IL.Celeste.Player.StartDash -= hookConsumeBuffer;
            IL.Celeste.Player.SwimUpdate -= hookConsumeBuffer;
            IL.Celeste.Player.BoostUpdate -= hookConsumeBuffer;
            IL.Celeste.Player.HitSquashUpdate -= hookConsumeBuffer;
        }

        private static void hookConsumeBuffer(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            MethodBase consumeBufferMethod = typeof(VirtualButton).GetMethod("ConsumeBuffer");
            MethodBase consumePressMethod = typeof(VirtualButton).GetMethod("ConsumePress"); //this is only used in boostupdate 😭

            if(
                cursor.TryGotoNext(MoveType.AfterLabel, instr => instr.MatchCallOrCallvirt(consumePressMethod)) ||
                cursor.TryGotoNext(MoveType.AfterLabel, instr => instr.MatchCallOrCallvirt(consumeBufferMethod))
            ) {
                cursor.EmitDup();
                cursor.EmitLdarg0();
                cursor.EmitDelegate((VirtualButton button, Player player) => {
                    if(!OptionsManager.GetOptionBool(OptionsManager.Option.BufferDelayVisualization)) return;

                    if(Engine.Scene is Level) {
                        Engine.Scene.Add(new BufferOffsetIndicator(button, player));
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