using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;

namespace Celeste.Mod.GooberHelper.Entities {

    [CustomEntity("GooberHelper/KeepDashAttackOnCollision")]
    public class KeepDashAttackOnCollision : Trigger {
        private bool KeepDashAttackOnCollisionValue;

        public KeepDashAttackOnCollision(EntityData data, Vector2 offset) : base(data, offset) {
            KeepDashAttackOnCollisionValue = data.Bool("keepDashAttackOnCollision", true);
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            GooberHelperModule.Session.KeepDashAttackOnCollision = KeepDashAttackOnCollisionValue;
        }
    }
}