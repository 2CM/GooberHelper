using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;

namespace Celeste.Mod.GooberHelper.Entities {

    [CustomEntity("GooberHelper/ReboundInversion")]
    public class ReboundInversion : Trigger {
        private bool ReboundInversionValue;

        public ReboundInversion(EntityData data, Vector2 offset) : base(data, offset) {
            ReboundInversionValue = data.Bool("reboundInversion", true);
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            GooberHelperModule.Session.ReboundInversion = ReboundInversionValue;
        }
    }
}