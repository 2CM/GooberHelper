using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Celeste.Mod.Entities;
using FMOD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using NLua;

namespace Celeste.Mod.GooberHelper.Entities {
    [CustomEntity("GooberHelper/BulletActivator")]
    public class BulletActivator : Trigger {
        private string pattern;
        private string flag;
        private string notFlag;
        private string activationFlag;

        public Vector2 BulletFieldCenter;

        public bool Activated = false;
        public string ShaderPath = "coloredBullet";

        public BulletActivator(EntityData data, Vector2 offset) : base(data, offset) {
            pattern = data.Attr("pattern");
            flag = data.Attr("flag");
            notFlag = data.Attr("notFlag");
            activationFlag = data.Attr("activationFlag");

            BulletFieldCenter = data.Nodes.Last() + offset;
        }

        public Utils.BetterCoroutine AddLuaCoroutine(LuaCoroutine coroutine) {
            var component = new Utils.BetterCoroutine(LuaHelper.LuaCoroutineToIEnumerator(coroutine));

            Add(component);

            return component;
        }
        
        public bool CheckFlags() {
            return
                ((Scene as Level).Session.GetFlag(flag) || flag == "") &&
                (!(Scene as Level).Session.GetFlag(notFlag) || notFlag == "");
        }

        public override void Update() {
            base.Update();

            if(
                !Activated &&
                CheckFlags() &&
                (Scene as Level).Session.GetFlag(activationFlag)
            ) {
                Activate();
            }
        }

        public void Activate() {
            Activated = true;

            var patternLoaderCode = LuaHelper.GetFileContent("BulletPatternThings/patternLoader");
            var patternLoaderTable = Everest.LuaLoader.Run(patternLoaderCode, "BulletPatternThings/patternLoader")[0] as LuaTable;
            
            var path = $"BulletPatterns/{pattern}";

            var propsDict = new Dictionary<object, object>() {
                { "Parent", this },
                { "Player", Engine.Scene.Tracker.GetEntity<Player>() },
                { "Level", Engine.Scene as Level }
            };

            var propsTable = LuaHelper.DictionaryToLuaTable(propsDict);

            (patternLoaderTable["load"] as LuaFunction).Call(path, propsTable);
        }

        public override void OnEnter(Player player) {
            if(
                !Activated &&
                CheckFlags()
            ) {
                Activate();
            }
        }

        //stupid dumb hack
        [Command("reloadlua", "reloadlua")]
        public static void CmdReloadLua() {
            Everest.LuaLoader.AllNamespaces.Clear();
            Everest.LuaLoader.AllTypes.Clear();
            Everest.LuaLoader.Global.TypeMap.Clear();
            Everest.LuaLoader.Global.NamespaceMap.Clear();

            (typeof(Everest.LuaLoader).GetField("_Preloaded", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as HashSet<string>).Clear();
            typeof(Everest.LuaLoader).GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, []);
        }
    }
}

/*
hell

// foreach(var pair in Everest.LuaLoader.AllTypes) {
//     Console.WriteLine($"{pair.Key}: {pair.Value}");
// }

// Everest.LuaLoader.AllTypes[""].Type

// typeof(Everest.LuaLoader.CachedType).GetField("Type").SetValue(Everest.LuaLoader.AllTypes["Celeste.Mod.GooberHelper.Entities.BulletActivator"], typeof(BulletActivator));

// var loader = Everest.LuaLoader.Run("""
//     local activator = require("#Celeste.Mod.GooberHelper.Entities").BulletActivator
//     local loader = {}

//     function loader.load(thing)
//         activator.AddLuaCoroutine()
//     end

//     return loader
// """, Random.Shared.Next().ToString())[0] as LuaTable;

// (loader["load"] as LuaFunction).Call(this);

// var objectTranslator = typeof(NLua.Lua).GetField("_translator", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Everest.LuaLoader.Context) as NLua.ObjectTranslator;
// var assemblies = typeof(NLua.ObjectTranslator).GetField("assemblies", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(objectTranslator) as List<Assembly>;

// foreach(Assembly assembly in assemblies) {
//     Console.WriteLine($"{assembly.FullName}, {assembly == typeof(BulletActivator).Assembly}");
// }

// typeof(Everest.LuaLoader.CachedType).GetField("Type").SetValue(Everest.LuaLoader.AllTypes["Celeste.Mod.GooberHelper.Entities.BulletActivator"], typeof(BulletActivator));
// typeof(Everest.LuaLoader.CachedNamespace).GetField("Type").SetValue(Everest.LuaLoader.AllTypes["Celeste.Mod.GooberHelper.Entities.BulletActivator"], typeof(BulletActivator));

// foreach(var pair in Everest.LuaLoader.AllTypes) {
//     if(pair.Key.Contains("GooberHelper")) {
//         Everest.LuaLoader.AllTypes.Remove(pair.Key);
//     }
// }

// foreach(var pair in Everest.LuaLoader.AllNamespaces) {
//     if(pair.Key.Contains("GooberHelper")) {
//         Everest.LuaLoader.AllNamespaces.Remove(pair.Key);
//     }
// }

// (typeof(Everest.LuaLoader).GetField("_LoadAssembly", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as Action<string>).Invoke(typeof(BulletActivator).Assembly.FullName);

Everest.LuaLoader.Precache(typeof(BulletActivator).Assembly);


var loader = Everest.LuaLoader.Run("""
    local loader = {}

    function loader.load(thing)
        thing:AddLuaCoroutine()
    end

    return loader
""", Random.Shared.Next().ToString())[0] as LuaTable;

(loader["load"] as LuaFunction).Call(this);
*/