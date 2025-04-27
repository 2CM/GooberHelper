using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//probably just use a typed controller entity

namespace Celeste.Mod.GooberHelper.Entities {
    public class StackItem {
        public Dictionary<string, object> SettingValues;
        public string Type;
        public int ID;

        public StackItem() {}

        public StackItem(Dictionary<string, object> SettingValues, string Type, int ID) {
            this.SettingValues = SettingValues;
            this.Type = Type;
            this.ID = ID;
        }
    }

    public class AbstractTrigger<T> : Trigger where T : Trigger {
        public Dictionary<string, object> settingValues;
        public bool revertOnLeave = false;
        public bool revertOnDeath = false;
        StackItem stackItem;
        public object baseValue;
        public int ID;
        public string Flag;
        public string NotFlag;

        public static List<StackItem> Stack {
            get {
                if(!GooberHelperModule.Session.Stacks.ContainsKey(typeof(T).Name)) GooberHelperModule.Session.Stacks.Add(typeof(T).Name, new List<StackItem>());

                return GooberHelperModule.Session.Stacks[typeof(T).Name];
            }
            set {}
        }

        public AbstractTrigger(EntityData data, Vector2 offset, object baseValue, List<string> optionNames) : base(data, offset) {
            if(this.revertOnDeath) {
                Stack.RemoveAll(a => a == this.stackItem);

                this.UpdateStack();
            }

            ID = data.ID;

            foreach(StackItem item in Stack.Where(a => a.ID == this.ID)) {
                this.stackItem = item;
            }

            // Console.WriteLine("-----");

            // foreach(var i in stack) {
            //     Console.WriteLine(string.Join(Environment.NewLine, i.SettingValues));
            //     Console.WriteLine("");
            // }

            foreach(var i in Stack) {
                foreach(var key in i.SettingValues.Keys) {
                    object result = i.SettingValues[key];

                    bool isFloat = float.TryParse(i.SettingValues[key].ToString(), out float floatResult);
                    bool isBool = bool.TryParse(i.SettingValues[key].ToString(), out bool booleanResult);
                    
                    if(isFloat) result = floatResult;
                    if(isBool) result = booleanResult;

                    i.SettingValues[key] = result;
                }
            }

            //assign to the correct stackitem thing based on entity id

            this.settingValues = optionNames.ToDictionary(prop => prop, prop => {
                var id = prop[..1].ToLower() + prop[1..];

                return baseValue.GetType() == typeof(float) ? data.Float(id, (float)baseValue) : (object)data.Bool(id, false);
            });

            this.revertOnDeath = data.Bool("revertOnDeath", false);
            this.revertOnLeave = data.Bool("revertOnLeave", false);

            this.Flag = data.Attr("flag", "");
            this.NotFlag = data.Attr("notFlag", "");

            this.baseValue = baseValue;
        }

        public static void Load() {
            On.Celeste.Player.Die += modPlayerDie;
        }

        public static void Unload() {
            On.Celeste.Player.Die -= modPlayerDie;
        }

        // public override void Removed(Scene scene)
        // {
        //     base.Removed(scene);
            
        //     if(this.PlayerIsInside && this.revertOnDeath) {
        //         Stack.RemoveAll(a => a == this.stackItem);

        //         this.UpdateStack();
        //     }
        // }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);

            if(this.revertOnDeath) {
                Stack.RemoveAll(a => a == this.stackItem);

                this.UpdateStack();
            }
        }

        public static PlayerDeadBody modPlayerDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats) {
            foreach(AbstractTrigger<T> item in Engine.Scene.Tracker.GetEntities<T>()) {
                // Console.WriteLine(typeof(T).Name);
                
                if(item.revertOnDeath) {
                    Stack.RemoveAll(a => a == item.stackItem);
                }

                item.UpdateStack();
            }

            return orig(self, direction, evenIfInvincible, registerDeathInStats);
        }

        public void UpdateStack() {
            if(Stack.Count == 0) {
                foreach(var item in this.settingValues) {
                    typeof(GooberHelperModuleSession).GetProperty(item.Key).SetValue(GooberHelperModule.Session, baseValue);
                }
            } else {
                foreach(var item in Stack.Last().SettingValues) {
                    typeof(GooberHelperModuleSession).GetProperty(item.Key).SetValue(GooberHelperModule.Session, item.Value);
                }
            }
        }

        public override void OnLeave(Player player) {
            base.OnLeave(player);

            // if((this.revertOnLeave && !player.Dead) || (this.revertOnDeath && player.Dead)) {
            if(this.revertOnLeave && !player.Dead) {
                Stack.RemoveAll(a => a == this.stackItem);

                this.UpdateStack();
            }
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            if(!(
                (this.Flag    != "" &&  (Engine.Scene as Level).Session.GetFlag(this.Flag))   || (this.Flag == "") &&
                (this.NotFlag != "" && !(Engine.Scene as Level).Session.GetFlag(this.NotFlag) || this.NotFlag == "")
            )) return;

            if(this.stackItem == null) this.stackItem = new StackItem(this.settingValues, typeof(T).Name, this.ID);

            if(!this.revertOnLeave && !this.revertOnDeath) {
                Stack.Clear();

                // Console.WriteLine("able to clear stack");
            }

            Stack.Add(this.stackItem);

            this.UpdateStack();
        }
    }
}