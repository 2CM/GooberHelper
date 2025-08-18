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
        public float AwesomeRetentionSpeed = 0f;
        public float AwesomeRetentionTimer = 0f;
        public Vector2 AwesomeRetentionDirection = new Vector2(0,0);
        public bool AwesomeRetentionWasInWater = false;
        
        public Vector2 BoostSpeedPreserved = new Vector2(0,0);
        public Vector2 StarFlySpeedPreserved = new Vector2(0,0);
        public Vector2 AttractSpeedPreserved = new Vector2(0,0);

        public int Counter = 0;
        public int LastPauseCounterValue = 0;
        public float StunningWatchTimer = 0;
        public float StunningOffset = 0f;
        public int StunningGroup = 0;

        public static GooberPlayerExtensions Instance {
            get {
                GooberPlayerExtensions zog = Engine.Scene.Tracker.GetComponent<GooberPlayerExtensions>();

                // //i dont think this should happen anymore but whatever
                // if(zog == null) {
                //     zog = new GooberPlayerExtensions();

                //     Player player = Engine.Scene.Tracker.GetEntity<Player>();
                    
                //     if(player == null) return null;

                //     player.Add(zog);
                // }

                return zog;
            }
        }

        public GooberPlayerExtensions() : base(false, false) {}
    }
}