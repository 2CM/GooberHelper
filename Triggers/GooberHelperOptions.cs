using System;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using static Celeste.Mod.GooberHelper.OptionsManager;

namespace Celeste.Mod.GooberHelper.Entities {

    [CustomEntity("GooberHelper/GooberHelperOptions")]
    [Tracked(false)]
    public class GooberHelperOptions : Trigger {
        public OptionChanges Changes;
        private bool revertOnLeave;
        private bool revertOnDeath;

        public GooberHelperOptions(EntityData data, Vector2 offset) : base(data, offset) {
            this.Changes = new OptionChanges(data);

            this.revertOnLeave = data.Bool("revertOnLeave");
            this.revertOnDeath = data.Bool("revertOnDeath");
        }

        public static void Load() {
            On.Celeste.Player.Die += modPlayerDie;
        }

        public static void Unload() {
            On.Celeste.Player.Die -= modPlayerDie;
        }

        public bool RemoveFromStack(bool update = true) {
            int countBefore = GooberHelperModule.Session.Stack.Count; 

            GooberHelperModule.Session.Stack.RemoveAll(item => item.ID.Key == this.Changes.ID.Key);

            bool changed = countBefore != GooberHelperModule.Session.Stack.Count;

            if(update && changed) OptionChanges.UpdateStack();

            return changed;
        }

        public void AddToStack() {
            if(!revertOnLeave && !revertOnDeath) GooberHelperModule.Session.Stack.Clear();

            GooberHelperModule.Session.Stack.Add(this.Changes);
            Changes.Apply();
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);
            

            if(this.revertOnDeath) {
                RemoveFromStack();
            }
        }

        public override void SceneEnd(Scene scene) {
            base.SceneEnd(scene);

            if(this.revertOnDeath) {
                RemoveFromStack();
            }
        }

        public static PlayerDeadBody modPlayerDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats) {
            bool didAnything = false;

            foreach(GooberHelperOptions item in Engine.Scene.Tracker.GetEntities<GooberHelperOptions>()) {
                if(item.revertOnDeath && item.RemoveFromStack(false)) didAnything = true;
            }

            if(didAnything) OptionChanges.UpdateStack();

            return orig(self, direction, evenIfInvincible, registerDeathInStats);
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            if(GooberHelperModule.Session.Stack.Count > 0 && GooberHelperModule.Session.Stack.Last() == this.Changes) return;

            AddToStack();
        }

        public override void OnLeave(Player player) {
            base.OnLeave(player);

            if(!this.revertOnLeave || player.Dead) return;

            RemoveFromStack();
        }
    }
}

/*
b,d
- c

b,c,d
+ d
- a

a,b,c
+ a
+ b
+ c
*/