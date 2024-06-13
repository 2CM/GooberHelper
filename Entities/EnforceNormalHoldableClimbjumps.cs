using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Celeste.Mod.GooberHelper.Entities {

    [CustomEntity("GooberHelper/EnforceNormalHoldableClimbjumps")]
    [Tracked(false)]
    public class EnforceNormalHoldableClimbjumps : Entity {

        public EnforceNormalHoldableClimbjumps(Vector2 position, int width, int height) : base(position) {
            this.Collider = new Hitbox(width, height, 0, 0);
        }

        public EnforceNormalHoldableClimbjumps(EntityData data, Vector2 offset) : this(data.Position + offset, data.Width, data.Height) {
		}
    }
}