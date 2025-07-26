using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Celeste.Mod.GooberHelper.Components {
    [Tracked(false)]
    public class GooberPlayerExtensions : Component {
        public float WaterRetentionSpeed = 0f;
        public float WaterRetentionTimer = 0f;
        public Vector2 WaterRetentionDirection = new Vector2(0,0);
        
        public Vector2 BoostSpeedPreserved = new Vector2(0,0);
        public Vector2 StarFlySpeedPreserved = new Vector2(0,0);
        public Vector2 AttractSpeedPreserved = new Vector2(0,0);

        public static GooberPlayerExtensions Instance {
            get {
                GooberPlayerExtensions zog = Engine.Scene.Tracker.GetComponent<GooberPlayerExtensions>();

                if(zog == null) {
                    zog = new GooberPlayerExtensions();

                    Engine.Scene.Tracker.GetEntity<Player>().Add(zog);
                }

                return zog;
            }
        }

        public GooberPlayerExtensions() : base(true, true) {}
    }
}