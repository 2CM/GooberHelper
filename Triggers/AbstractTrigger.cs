using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static Celeste.Mod.GooberHelper.OptionsManager;


namespace Celeste.Mod.GooberHelper.Entities {
    public class StackItem {
        public Dictionary<Option, float> SettingValues;
        public string Type;
        public int ID;

        public StackItem() {}

        public StackItem(Dictionary<Option, float> SettingValues, string Type, int ID) {
            this.SettingValues = SettingValues;
            this.Type = Type;
            this.ID = ID;
        }
    }

    public abstract class AbstractTrigger<T> : Trigger where T : Trigger {
        public Dictionary<Option, float> SettingValues = new Dictionary<Option, float>();
        private bool revertOnLeave = false;
        private bool revertOnDeath = false;
        private StackItem stackItem;
        private int id;
        private string flag;
        private string notFlag;

        public static List<StackItem> Stack {
            get {
                if(!GooberHelperModule.Session.Stacks.ContainsKey(typeof(T).Name)) GooberHelperModule.Session.Stacks.Add(typeof(T).Name, new List<StackItem>());

                return GooberHelperModule.Session.Stacks[typeof(T).Name];
            }
            set {}
        }

        public AbstractTrigger(EntityData data, Vector2 offset, OptionType type, List<string> optionNames, Dictionary<string, string> optionNameOverrides) : base(data, offset) {
            if(this.revertOnDeath) {
                Stack.RemoveAll(a => a == this.stackItem);

                this.UpdateStack();
            }

            id = data.ID;

            foreach(StackItem item in Stack.Where(a => a.ID == this.id)) {
                this.stackItem = item;
            }

            // Console.WriteLine("-----");

            // foreach(var i in stack) {
            //     Console.WriteLine(string.Join(Environment.NewLine, i.SettingValues));
            //     Console.WriteLine("");
            // }

            // foreach(var i in Stack) {
            //     foreach(var key in i.SettingValues.Keys) {
            //         object result = i.SettingValues[key];

            //         bool isInt = int.TryParse(i.SettingValues[key].ToString(), out int intResult);
            //         bool isBool = bool.TryParse(i.SettingValues[key].ToString(), out bool booleanResult);
                    
            //         if(isInt) result = intResult;
            //         if(isBool) result = booleanResult;

            //         i.SettingValues[key] = result;
            //     }
            // }

            foreach(string optionName in optionNames) {
                if(Enum.TryParse(optionNameOverrides.TryGetValue(optionName, out string actualOptionName) ? actualOptionName : optionName, out Option option)) {
                    var id = optionName[..1].ToLower() + optionName[1..];

                    SettingValues[option] = type == OptionType.Float ? data.Int(id, (int)Options[option].DefaultValue) : (data.Bool(id, false) ? 1 : 0);
                } else {
                    HandleWeirdOption(optionName);
                }
            }

            this.revertOnDeath = data.Bool("revertOnDeath", false);
            this.revertOnLeave = data.Bool("revertOnLeave", false);

            this.flag = data.Attr("flag", "");
            this.notFlag = data.Attr("notFlag", "");
        }

        public virtual void HandleWeirdOption(string optionName) {}

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
                Stack.RemoveAll(a => a == this.stackItem);

                this.UpdateStack();
            }
        }

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
                foreach(var item in this.SettingValues) {
                    ResetOptionValue(item.Key, OptionSetter.Map);
                }
            } else {
                foreach(var item in Stack.Last().SettingValues) {
                    SetOptionValue(item.Key, item.Value, OptionSetter.Map);
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
                (this.flag    != "" &&  (Engine.Scene as Level).Session.GetFlag(this.flag))   || (this.flag == "") &&
                (this.notFlag != "" && !(Engine.Scene as Level).Session.GetFlag(this.notFlag) || this.notFlag == "")
            )) return;

            if(this.stackItem == null) this.stackItem = new StackItem(this.SettingValues, typeof(T).Name, this.id);

            if(!this.revertOnLeave && !this.revertOnDeath) {
                Stack.Clear();

                // Console.WriteLine("able to clear stack");
            }

            Stack.Add(this.stackItem);

            this.UpdateStack();
        }
    }
}