using Celeste.Mod.GooberHelper.Attributes.Hooks;
using MonoMod.Utils;

namespace Celeste.Mod.GooberHelper.Extensions {
    public static class PlayerExtensions {
        public class PlayerExtensionFields {
            public Vector2 SwimmingRetentionSpeed = Vector2.Zero;
            public float SwimmingRetentionTimer = 0f;
            public Platform SwimmingRetentionPlatform;

            //i love naming variables
            public bool IsDolphin = false;
            public float SwimLaunchCooldownTimer = 0f;

            public bool DashStickyRetentionExists = false;
            public Vector2 DashStickyRetentionSpeed = Vector2.Zero;

            public float VerticalRetentionSpeed;
            public float VerticalRetentionTimer;
            
            public Vector2 BeforeDashSpeedConserved = Vector2.Zero;
            
            public Vector2 BoostSpeedPreserved = Vector2.Zero;
            public Vector2 StarFlySpeedPreserved = Vector2.Zero;
            public Vector2 AttractSpeedPreserved = Vector2.Zero;
            public Vector2 WallJumpSpeedPreserved = Vector2.Zero;
            
            public Vector2 PreservedDreamBlockSpeedMagnitude = Vector2.Zero;
            public Vector2 DashWindBoost = Vector2.Zero;
            
            public float HitboxCompression = 0f;

            public float PlayerRotationTarget = 0f;
            public float PlayerRotation = 0f;

            public int StackedUltras = 0;

            public PlayerExtensionFields()
                => Utils.Log("CREATING EXTENSION FIELDS!!!");
        }

        private class PlayerExtensionComponent() : Component(false, false) {
            public PlayerExtensionFields ExtensionFields { get; private init; } = new();
        }

        private static PlayerExtensionFields getExtensionFieldsOrDefault(Player player)
            => player.Components.Get<PlayerExtensionComponent>()?.ExtensionFields;

        private static PlayerExtensionFields initExtensionFields(Player player) {
            var component = new PlayerExtensionComponent();
            
            player.Components.Add(component);

            return component.ExtensionFields;
        }
        
        public static PlayerExtensionFields GetExtensionFields(this Player player) {
            if(getExtensionFieldsOrDefault(player) is { } fields)
                return fields;

            return initExtensionFields(player);
        }

        [OnHook]
        private static void patch_Player_ctor(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode) {
            orig(self, position, spriteMode);

            initExtensionFields(self);
        }

        public static Vector2 GetConservedVisualSpeed(this Player self, PlayerExtensionFields ext) {
            ext ??= self.GetExtensionFields();

            return new Vector2(
                Utils.SignedAbsMax(
                    self.Speed.X,
                    self.wallSpeedRetentionTimer > 0f
                        ? self.wallSpeedRetained
                        : 0f,
                    ext.SwimmingRetentionTimer > 0f
                        ? ext.SwimmingRetentionSpeed.X
                        : 0f
                ),
                Utils.SignedAbsMax(
                    self.Speed.Y,
                    ext.SwimmingRetentionTimer > 0f
                        ? ext.SwimmingRetentionSpeed.Y
                        : 0f
                )
            );
        }

        public static Vector2 GetConservedSpeed(this Player self, PlayerExtensionFields ext = null, Vector2 extraThing = default) {
            var conserveBeforeDashSpeed = self.StateMachine.State == Player.StDash && GetOptionBool(Option.ConserveBeforeDashSpeed);
            
            ext ??= self.GetExtensionFields();

            return new Vector2(
                Utils.SignedAbsMax(
                    self.Speed.X,
                    self.wallSpeedRetentionTimer > 0f
                        ? self.wallSpeedRetained
                        : 0f,
                    conserveBeforeDashSpeed
                        ? self.beforeDashSpeed.X
                        : 0f,
                    conserveBeforeDashSpeed && ext.DashStickyRetentionExists
                        ? ext.DashStickyRetentionSpeed.X
                        : 0f,
                    extraThing.X
                ),
                Utils.SignedAbsMax(
                    self.Speed.Y,
                    conserveBeforeDashSpeed
                        ? self.beforeDashSpeed.Y
                        : 0f,
                    conserveBeforeDashSpeed && ext.DashStickyRetentionExists
                        ? ext.DashStickyRetentionSpeed.Y
                        : 0f,
                    extraThing.Y
                )
            );
        }
    }
}