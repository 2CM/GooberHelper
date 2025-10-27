using System;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.GooberHelper.States {
    public static class TouhouState {
        public static int TouhouStateId = -1;

        public static void Load() {
            On.Celeste.Player.Render += modPlayerRender;
            On.Celeste.Player.ctor += modPlayerCtor;
        }

        public static void Unload() {
            On.Celeste.Player.Render -= modPlayerRender;
            On.Celeste.Player.ctor -= modPlayerCtor;
        }

        public static void TouhouStateBegin() {
            Player player = Engine.Scene.Tracker.GetEntity<Player>();

            player.Collider = player.starFlyHitbox;
            player.hurtbox = touhouHurtbox;
        }

        public static int TouhouStateUpdate() {
            Player player = Engine.Scene.Tracker.GetEntity<Player>();

            player.Speed = Input.Feather.Value.SafeNormalize() * (Input.Jump.Check ? 60 : 120);

            return TouhouStateId;
        }

        public static void TouhouStateEnd() {
            Player player = Engine.Scene.Tracker.GetEntity<Player>();

            if(player == null) return;

			player.Collider = player.normalHitbox;
			player.hurtbox = player.normalHurtbox;
        }

        private static void modPlayerRender(On.Celeste.Player.orig_Render orig, Player self) {
            orig(self);

            if(self.StateMachine.State != TouhouStateId || !Input.Jump.Check) return;

            Collider collider = self.Collider;
			self.Collider = touhouHurtbox;
			
            Draw.HollowRect(self.Collider, Color.Lime);
            
			self.Collider = collider;
        }

        private static void modPlayerCtor(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode) {
            orig(self, position, spriteMode);

            TouhouStateId = self.StateMachine.AddState(
                "Touhou",
                new Func<int>(TouhouStateUpdate),
                null,
                new Action(TouhouStateBegin),
                new Action(TouhouStateEnd)
            );
        }

        [Command("touhou", "touhou")]
		public static void CmdTouhou() {
            Player player = Engine.Scene.Tracker.GetEntity<Player>();
			
            player.StateMachine.State = TouhouStateId;
		}

        static Hitbox touhouHitbox = new Hitbox(6, 6, -3, -3);
        static Hitbox touhouHurtbox = new Hitbox(4, 4, -2, -8);
    }
}