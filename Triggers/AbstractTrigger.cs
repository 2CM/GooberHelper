using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.GooberHelper.Entities {
    // public abstract class AbstractTrigger<T> : Trigger where T : Trigger {
    public class AbstractTrigger : Trigger {
        public Dictionary<string, object> settingValues;
        public Dictionary<string, object> settingValuesBefore;
        public bool Activated = false;
        // public AbstractTrigger reversionEntity;
        // public bool hasReversionEntity = false;

        public class StackThing {
            public Dictionary<string, object> settingValues;
            public AbstractTrigger entity;

            public StackThing(Dictionary<string, object> settingValues, AbstractTrigger entity) {
                this.settingValues = settingValues;
                this.entity = entity;
            }
        }

        public static List<StackThing> stack = new();

        public AbstractTrigger(EntityData data, Vector2 offset, List<string> optionNames) : base(data, offset) {
            this.settingValues = optionNames.ToDictionary(prop => prop, prop => (object)data.Bool(prop[..1].ToLower() + prop[1..]));
            this.settingValuesBefore = optionNames.ToDictionary(prop => prop, prop => (object)false);

            if(!this.PlayerIsInside) {
                // this.Revert();
            }
        }

        public void updateStack() {
            if(stack.Count == 0) {
                foreach(var item in this.settingValues) {
                    typeof(GooberHelperModuleSession).GetProperty(item.Key).SetValue(GooberHelperModule.Session, false);
                }
            } else {
                foreach(var item in stack.Last().settingValues) {
                    typeof(GooberHelperModuleSession).GetProperty(item.Key).SetValue(GooberHelperModule.Session, item.Value);
                }
            }
        }

        // public override void OnEnter(Player player)
        // {
        //     // this.Activated = true;

        //     // this.settingValuesBefore = typeof(GooberHelperModuleSession).GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(GooberHelperModule.Session));

        //     base.OnEnter(player);

        //     // foreach(var item in this.settingValues) {
        //     //     typeof(GooberHelperModuleSession).GetProperty(item.Key).SetValue(GooberHelperModule.Session, item.Value);
        //     // }
        // }

        // public void Revert() {
        //     this.Activated = false;

        //     Console.WriteLine("REVERTING" + this.Position.ToString());

        //     // if(this.hasReversionEntity) {
        //     //     // if(!this.reversionEntity.Activated) {
        //     //         this.reversionEntity.Revert();
        //     //     // }

        //     //     this.hasReversionEntity = false;
        //     // } {
        //     //     foreach(var item in this.settingValuesBefore) {
        //     //         typeof(GooberHelperModuleSession).GetProperty(item.Key).SetValue(GooberHelperModule.Session, item.Value);
        //     //     }
        //     // }


        // }

        // public override void OnLeave(Player player)
        // {
        //     Console.WriteLine("LEAVING" + this.Position.ToString());

        //     if(this.Activated) this.Revert();

        //     base.OnLeave(player);
        // }
    }
}