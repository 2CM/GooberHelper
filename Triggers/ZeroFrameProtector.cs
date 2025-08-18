using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;

namespace Celeste.Mod.GooberHelper.Entities {

    [CustomEntity("GooberHelper/ZeroFrameProtector")]
    [Tracked(false)]
    public class ZeroFrameProtector : Trigger {
        private enum Mode {
            Left,
            Right,
            Up,
            Down
        }

        private Mode mode;
        private string flag;
        private string notFlag;
        
        private Vector2 lastPlayerPosition;
        private bool disabled = false;
        private Hitbox actualHitbox;

        public ZeroFrameProtector(EntityData data, Vector2 offset) : base(data, offset) {
            mode = Enum.Parse<Mode>(data.Attr("mode", "Left"));
            flag = data.Attr("flag", "");
            notFlag = data.Attr("notFlag", "");

            actualHitbox = new Hitbox(this.Width, this.Height);

            Collider.Position -= Vector2.One;
            Collider.Height += 2;
            Collider.Width += 2;
        }

        public override void Update() {
            base.Update();

            lastPlayerPosition = Engine.Scene.Tracker.GetEntity<Player>()?.TopLeft ?? Vector2.Zero;
        }

        public override void DebugRender(Camera camera) {            
            base.DebugRender(camera);

            Collider collider = this.Collider;
            this.Collider = this.actualHitbox;
            Draw.HollowRect(this.Collider, Color.Red * (disabled ? 0.5f : 1f));
            this.Collider = collider;
        }

        public override void OnStay(Player player) {
            base.OnStay(player);

            if(disabled) return;

            if(flag != "" && !player.level.Session.GetFlag(flag)) return;
            if(notFlag != "" && player.level.Session.GetFlag(flag)) return;
            Collider playerCollider = player.Collider;
            player.Collider = player.hurtbox;

            float left = Left - player.Right + 1;
            float right = Right - player.Left - 1;
            float top = Top - player.Bottom + 1;
            float bottom = Bottom - player.Top - 1;

            bool leftEntry = lastPlayerPosition.X + player.Collider.Width <= this.Left;
            bool rightEntry = lastPlayerPosition.X >= this.Right;
            bool topEntry = lastPlayerPosition.Y - player.Collider.Height <= this.Top;
            bool bottomEntry = lastPlayerPosition.Y >= this.Bottom;

            bool collides = player.CollideCheck(this);

            player.Collider = playerCollider;

            if(!collides) return;


            //dont worry about it
            if     (mode == Mode.Left  && leftEntry)   player.MoveH(left);
            else if(mode == Mode.Right && rightEntry)  player.MoveH(right);
            else if(mode == Mode.Up    && topEntry)    player.MoveV(top);
            else if(mode == Mode.Down  && bottomEntry) player.MoveV(bottom);

            disabled = true;
        }

        public override void OnLeave(Player player) {
            base.OnLeave(player);

            disabled = false;
        }
    }
}