using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;

namespace Celeste.Mod.GooberHelper.Entities {

    [CustomEntity("GooberHelper/JumpInversion")]
    public class JumpInversion : Trigger {
        private bool JumpInversionValue;
        private bool AllowClimbJumpInversionValue;

        public JumpInversion(EntityData data, Vector2 offset) : base(data, offset) {
            JumpInversionValue = data.Bool("jumpInversion", true);
            AllowClimbJumpInversionValue = data.Bool("allowClimbJumpInversion", true);
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            GooberHelperModule.Session.JumpInversion = JumpInversionValue;
            GooberHelperModule.Session.AllowClimbJumpInversion = AllowClimbJumpInversionValue;
        }
    }
}