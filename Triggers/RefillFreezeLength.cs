using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;

namespace Celeste.Mod.GooberHelper.Entities {

    [CustomEntity("GooberHelper/RefillFreezeLength")]
    public class RefillFreezeLength : Trigger {
        private float RefillFreezeLengthValue;

        public RefillFreezeLength(EntityData data, Vector2 offset) : base(data, offset) {
            RefillFreezeLengthValue = data.Float("refillFreezeLength", 3f);
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            GooberHelperModule.Session.RefillFreezeLength = RefillFreezeLengthValue;
        }
    }
}