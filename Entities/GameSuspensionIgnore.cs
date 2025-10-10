using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.GooberHelper.Entities {

    [CustomEntity("GooberHelper/GameSuspensionIgnore")]
    [Tracked(false)]
    public class GameSuspensionIgnore : Entity {
        public List<Entity> Entities = [];
        public Dictionary<Type, bool> WhitelistedEntities = [];

        public GameSuspensionIgnore(EntityData data, Vector2 offset) : base(data.Position + offset) {
            foreach(string typeName in data.Attr("types", "").Split(",")) {
                if(typeName == "") continue;

                Type type = ModIntegration.FrostHelperAPI.EntityNameToType(typeName);

                if(type == null) continue;

                WhitelistedEntities[type] = true;
            }

            this.Collider = new Hitbox(data.Width, data.Height);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            foreach(Entity entity in Scene.Entities) {
                if(Collider.Collide(entity.Position) && (WhitelistedEntities.Count == 0 || WhitelistedEntities.ContainsKey(entity.GetType()))) {
                    Entities.Add(entity);
                }
            }
        }

        public static void UpdateEntities() {
            foreach(GameSuspensionIgnore trigger in Engine.Scene.Tracker.GetEntities<GameSuspensionIgnore>()) {
                foreach(Entity entity in trigger.Entities) {
                    entity.Update();
                }
            }
        }
    }
}