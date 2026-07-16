using System.Collections.Generic;
using Celeste.Mod.GooberHelper.Attributes.Hooks;
using Celeste.Mod.GooberHelper.Options.Miscellaneous;
using MonoMod.Utils;

namespace Celeste.Mod.GooberHelper.Extensions {
    public static class SceneExtensions {
        public class SceneExtensionFields {
            public int Counter = 0;
            public int LastPauseCounterValue = 0;
            public float StunningWatchTimer = 0f;
            public float StunningOffset = 0f;
            public int StunningGroup = 0;

            public bool FreezeFrameFrozen = false;
            public RefillFreezeGameSuspension.InputState FreezeFrameFrozenInputs;

            public Stack<TextMenu> MenuStack = [];
            public TextMenu RootMenu;
            public TextMenu CurrentMenu;
        }

        [Tracked]
        private class SceneExtensionEntity : Entity {
            public SceneExtensionFields ExtensionFields { get; private init; } = new();
        }

        private static SceneExtensionFields getExtensionFieldsOrDefault(Scene scene)
            => scene.Tracker.GetEntity<SceneExtensionEntity>()?.ExtensionFields;

        private static SceneExtensionFields initExtensionFields(Scene scene) {
            var entity = new SceneExtensionEntity();
                
            scene.Entities.Add(entity);

            return entity.ExtensionFields;
        }

        public static SceneExtensionFields GetExtensionFields(this Scene scene) {
            if (getExtensionFieldsOrDefault(scene) is { } fields)
                return fields;

            return initExtensionFields(scene);
        }
        
        [OnHook]
        private static void patch_Scene_ctor(On.Monocle.Scene.orig_ctor orig, Scene self) {
            orig(self);

            initExtensionFields(self);
        }
    }
}