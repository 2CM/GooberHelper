using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;

namespace Celeste.Mod.GooberHelper.Entities {

    [CustomEntity("GooberHelper/Rotator")]
    public class Rotator : Entity {
        private MTexture sprite = GFX.Game["objects/door/lockdoor12"];

        private float grabCooldown = 0.0f;
        private bool isHeld = false;
        private Vector2 playerSpeed;

        private int rotationDir = 0;
        private float rotationSpeed;
        private float rotation = (float)Math.PI / 2;
        private float previousRotation = 0;
        private float timeRate;
        private float radius;
        private float cooldown;

        public Rotator(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Depth = 2000;

            timeRate = data.Float("timeRate", 1f);
            radius = data.Float("radius", 15f);
            cooldown = data.Float("cooldown", 0.1f);

            Collider = new Hitbox(16f, 16f, -8f, -8f);
            Add(new PlayerCollider(onPlayer, null, Collider));
        }

        private float playerSpeedToRadiansPerSecond(float playerSpeed, float radius) {
            // (acos(-(((playerSpeed * Engine.DeltaTime) ^ 2) / (2 * radius ^ 2)) + 1)) / Engine.DeltaTime

            return (float)Math.Acos(-(Math.Pow(playerSpeed * Engine.DeltaTime, 2) / (2 * Math.Pow(radius, 2))) + 1) / Engine.DeltaTime;
        }

        private void playerCatch(Player player) {
            isHeld = true;

            playerSpeed = player.Speed;

            player.StateMachine.State = 11;
            player.Speed = Vector2.Zero;

            rotationDir = Math.Sign(player.Position.X - Position.X);
            if(rotationDir == 0) rotationDir = 1;
            rotationSpeed = playerSpeedToRadiansPerSecond(playerSpeed.Length(), radius);

            if(timeRate != 1) {
                Engine.TimeRate = timeRate;
            }
        }

        private void playerUpdate(Player player) {
            player.Speed = Vector2.Zero;

            //Logger.Log(LogLevel.Info, "GooberHelper", $"{(new Vector2(1,0).Rotate(rotation) * 10f) - (new Vector2(1,0).Rotate(previousRotation) * 10f)}, {((new Vector2(1,0).Rotate(rotation) * 10f) - (new Vector2(1,0).Rotate(previousRotation) * 10f)).Length()}");

            previousRotation = rotation;
            rotation += rotationSpeed * rotationDir * Engine.DeltaTime;
            player.Position = (Position + new Vector2(1,0).Rotate(rotation) * radius - new Vector2(0, -6)).Round();
        }

        private void playerThrow(Player player) {
            grabCooldown = cooldown;
            isHeld = false;

            if(timeRate != 1) {
                Engine.TimeRate = 1;
            }

            player.StateMachine.State = 0;
            player.Speed = Vector2.Normalize(new Vector2(1,0).Rotate(previousRotation + rotationDir * (float)Math.PI / 2)) * playerSpeed.Length();

            rotationDir = 0;
            rotation = (float)Math.PI / 2;
        }

        private void onPlayer(Player player) {
            if(Input.GrabCheck && grabCooldown <= 0.0f && !isHeld) {
                playerCatch(player);
            }
        }

        public override void Update() {
            if(isHeld && !Input.GrabCheck) {
                playerThrow(Engine.Scene.Tracker.GetEntity<Player>());
            } else if(isHeld) {
                playerUpdate(Engine.Scene.Tracker.GetEntity<Player>());
            }
            

            if(grabCooldown > 0.0f) {
                grabCooldown -= Engine.DeltaTime;
            }
        }

        public override void Render() {
            sprite.DrawCentered(Position);
            base.Render();
        }
    }
}