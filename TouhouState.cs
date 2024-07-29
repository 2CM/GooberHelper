using System;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.GooberHelper {
    public static class TouhouState {

        [Command("touhou", "touhou")]
		private static void CmdTouhou()
		{
            Player player = Engine.Scene.Tracker.GetEntity<Player>();
			
            player.StateMachine.State = TouhouStateId;
		}

        public static int TouhouStateId;

        public static void TouhouStateBegin() {
            Player player = Engine.Scene.Tracker.GetEntity<Player>();


            player.Collider = DynamicData.For(player).Get<Hitbox>("starFlyHitbox");
			DynamicData.For(player).Set("hurtbox", touhouHurtbox);
        }

        public static int TouhouStateUpdate() {
            Player player = Engine.Scene.Tracker.GetEntity<Player>();

            player.Speed = Input.Feather.Value.SafeNormalize() * (Input.Jump.Check ? 60 : 120);

            return TouhouStateId;
        }

        public static void modPlayerRender(On.Celeste.Player.orig_Render orig, Player self) {
            orig(self);

            if(self.StateMachine.State != TouhouStateId || !Input.Jump.Check) return;

            Collider collider = self.Collider;
			self.Collider = touhouHurtbox;
			Draw.HollowRect(self.Collider, Color.Lime);
			self.Collider = collider;
        }

        public static void TouhouStateEnd() {
            Player player = Engine.Scene.Tracker.GetEntity<Player>();

            Hitbox normalHitbox = DynamicData.For(player).Get<Hitbox>("normalHitbox");
            Hitbox normalHurtbox = DynamicData.For(player).Get<Hitbox>("normalHurtbox");
            Hitbox duckHitbox = DynamicData.For(player).Get<Hitbox>("duckHitbox");

			player.Collider = normalHitbox;
			DynamicData.For(player).Set("hurtbox", normalHurtbox);
			if (!player.CollideCheck<Solid>())
			{
				return;
			}
			Vector2 position = player.Position;
			player.Y -= normalHitbox.Bottom - touhouHitbox.Bottom;
			if (!player.CollideCheck<Solid>())
			{
				return;
			}
			player.Position = position;
			player.Ducking = true;
			player.Y -= duckHitbox.Bottom - touhouHurtbox.Bottom;
			if (player.CollideCheck<Solid>())
			{
				player.Position = position;
				throw new Exception("Could not get out of solids when exiting Touhou state!");
			}
        }

        static Hitbox touhouHitbox = new Hitbox(6, 6, -3, -3);
        static Hitbox touhouHurtbox = new Hitbox(4, 4, -2, -8);
    }
}