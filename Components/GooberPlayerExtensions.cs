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
        public Vector2 AwesomeRetentionSpeed = Vector2.Zero;
        public float AwesomeRetentionTimer = 0f;
        public Vector2 AwesomeRetentionDirection = Vector2.Zero;
        public bool AwesomeRetentionWasInWater = false;
        public Platform AwesomeRetentionPlatform;
        
        public Vector2 BoostSpeedPreserved = Vector2.Zero;
        public Vector2 StarFlySpeedPreserved = Vector2.Zero;
        public Vector2 AttractSpeedPreserved = Vector2.Zero;
        
        public Vector2 PreservedDreamBlockSpeedMagnitude = Vector2.Zero;

        public int Counter = 0;
        public int LastPauseCounterValue = 0;
        public float StunningWatchTimer = 0;
        public float StunningOffset = 0f;
        public int StunningGroup = 0;

        public bool FreezeFrameFrozen = false;
        public Utils.InputState FreezeFrameFrozenInputs;

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