using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;

namespace Celeste.Mod.GooberHelper.Entities {

    [CustomEntity("GooberHelper/CobwobSpeedInversion")]
    public class CobwobSpeedInversion : Trigger {
        private bool CobwobSpeedInversionValue;
        private bool AllowRetentionReverseValue;

        public CobwobSpeedInversion(EntityData data, Vector2 offset) : base(data, offset) {
            CobwobSpeedInversionValue = data.Bool("cobwobSpeedInversion", true);
            AllowRetentionReverseValue = data.Bool("allowRetentionReverse", true);
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            GooberHelperModule.Session.CobwobSpeedInversion = CobwobSpeedInversionValue;
            GooberHelperModule.Session.AllowRetentionReverse = AllowRetentionReverseValue;
        }
    }
}