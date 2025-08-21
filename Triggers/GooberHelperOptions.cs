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

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            
            if(this.revertOnDeath) {
                GooberHelperModule.Session.Stack.RemoveAll(a => a == Changes);

                UpdateStack();
            }
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);

            if(this.revertOnDeath) {
                GooberHelperModule.Session.Stack.RemoveAll(a => a == Changes);

                UpdateStack();
            }
        }

        public static PlayerDeadBody modPlayerDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats) {
            foreach(GooberHelperOptions item in Engine.Scene.Tracker.GetEntities<GooberHelperOptions>()) {
                if(item.revertOnDeath) {
                    GooberHelperModule.Session.Stack.RemoveAll(a => a == item.Changes);
                }
            }

            UpdateStack();

            return orig(self, direction, evenIfInvincible, registerDeathInStats);
        }

        public static void UpdateStack() {
            GooberHelperModule.Session.MapDefinedOptions.Clear();

            Console.WriteLine("updating stack");

            foreach(var changes in GooberHelperModule.Session.Stack) {
                changes.Apply();
            }
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            if(GooberHelperModule.Session.Stack.Count > 0 && GooberHelperModule.Session.Stack.Last() == this.Changes) return;

            GooberHelperModule.Session.Stack.Add(this.Changes);

            UpdateStack();
        }

        public override void OnLeave(Player player) {
            base.OnLeave(player);

            if(!this.revertOnLeave) return;

            GooberHelperModule.Session.Stack.RemoveAll(a => a == Changes);

            UpdateStack();
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