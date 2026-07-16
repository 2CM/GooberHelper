using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Celeste.Mod.GooberHelper.Attributes.Hooks;
using Celeste.Mod.GooberHelper.Extensions;
using Celeste.Mod.GooberHelper.Options.Visuals;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;

namespace Celeste.Mod.GooberHelper.Options.GeneralHooks {
    public static class PlayerRender {
        private static bool startedRendering = false;

        public static bool ShouldCustomRenderBody(Player player)
            => CustomSwimmingAnimation.ShouldDoAnimation(player) || GetOptionBool(Option.RotatePlayerToSpeed) || GetOptionEnum<PlayerShaderMask.Value>(Option.PlayerShaderMask) == PlayerShaderMask.Value.Cover;
        
        public static bool ShouldCustomRenderHair(Player player)
            => GetOptionEnum<PlayerShaderMask.Value>(Option.PlayerShaderMask) == PlayerShaderMask.Value.HairOnly;

        private static FieldInfo f_RasterizerState_CullNone = typeof(RasterizerState).GetField("CullNone");
        private static FieldInfo f_SpriteBatch_transformMatrix = typeof(SpriteBatch).GetField("transformMatrix", Utils.BindingFlagsAll);
        private static Matrix levelBatchMatrix;

        private static float nextTrailSnapshotRotation;
        private static Dictionary<int, float> trailManagerSnapshotIndexRotations = [];
        
        public enum RenderSource {
            Body,
            DeadBody,
            Hair
        }

        public static Matrix RotateMatrixAroundVector(Matrix matrix, Vector2 vector, float angle) {
            var offset = new Vector3(vector, 0);

            return matrix
                * Matrix.CreateTranslation(offset)
                * Matrix.CreateRotationZ(angle)
                * Matrix.CreateTranslation(-offset);
        }

        private static Matrix getSpriteBatchTransformMatrix(PlayerExtensions.PlayerExtensionFields ext)
            => (Matrix)f_SpriteBatch_transformMatrix.GetValue(Draw.SpriteBatch);

        private static void setSpriteBatchTransformMatrix(Matrix value, PlayerExtensions.PlayerExtensionFields ext) {
            if (ext.PlayerRotation == 0f)
                return;
            
            f_SpriteBatch_transformMatrix.SetValue(Draw.SpriteBatch, value);
        }

        [OnHook]
        private static void patch_Player_UpdateSprite(On.Celeste.Player.orig_UpdateSprite orig, Player self) {            
            var ext = self.GetExtensionFields();

            var shouldCancel = false;
            var somethingActive = false;
            
            shouldCancel = shouldCancel
                || CustomSwimmingAnimation.OnUpdateSprite(self, ext, ref somethingActive)
                || RotatePlayerToSpeed.OnUpdateSprite(self, ext, ref somethingActive);
            
            if(!shouldCancel) {
                orig(self);
            } else {
                //these are important
                self.Sprite.Scale.X = Calc.Approach(self.Sprite.Scale.X, 1f, 1.75f * Engine.DeltaTime);
                self.Sprite.Scale.Y = Calc.Approach(self.Sprite.Scale.Y, 1f, 1.75f * Engine.DeltaTime);
            }

            if(!somethingActive) {
                ext.PlayerRotation = 0;
                ext.PlayerRotationTarget = 0;

                return;
            }

            ext.PlayerRotation = Calc.AngleApproach(ext.PlayerRotation, ext.PlayerRotationTarget, 20f * Engine.DeltaTime);
        }

        [OnHook]
        private static void patch_PlayerHair_Render(On.Celeste.PlayerHair.orig_Render orig, PlayerHair self) {
            if(self.Entity is not Player player) {
                orig(self);

                return;
            }

            var ext = player.GetExtensionFields();

            //i need the custom shader to not execute if the startedRendering boolean is false
            //this took way longer than it shouldve to figure out
            //there was a bug where the player trail would be offset from the player for some odd reason
            //i assumed it was a shader problem or more general thing for a while, but i eventually had the idea that it might be specific to the TrailManager (it was)
            //in TrailManager.BeforeRender(), it interrupts the spritebatch and renders specifically the PlayerHair with this method
            //that causes Something to mess up and somehow shift the player trail
            //the startedRendering boolean is set to true when the actual render method is called
            //that Should prevent this method from executing the custom shader code
            //i should document these things more often

            rotateHairNodes(self, -ext.PlayerRotation);

            //you
            if(!ShouldCustomRenderHair(player) || !startedRendering) {
                orig(self);

                rotateHairNodes(self, ext.PlayerRotation);

                return;
            }
            
            var previousMatrix = getSpriteBatchTransformMatrix(ext);

            beforeRender(player, ext.PlayerRotation, RenderSource.Hair);

            orig(self);
            
            afterRender();

            rotateHairNodes(self, ext.PlayerRotation);

            startedRendering = false;

            setSpriteBatchTransformMatrix(previousMatrix, ext);
        }

        private static void rotateHairNodes(PlayerHair hair, float rotation) {
            if(rotation == 0f)
                return;

            var center = hair.Nodes[0];

            for(var i = 0; i < hair.Nodes.Count; i++)
                hair.Nodes[i] = (hair.Nodes[i] - center).Rotate(rotation) + center;
        }

        [OnHook]
        private static void patch_PlayerDeadBody_Render(On.Celeste.PlayerDeadBody.orig_Render orig, PlayerDeadBody self) {
            if(!ShouldCustomRenderBody(null)) {
                orig(self);

                return;
            }

            beforeRender(self, 0, RenderSource.DeadBody);

            orig(self);

            afterRender();
        }
        
        [OnHook]
        private static void patch_Player_Render(On.Celeste.Player.orig_Render orig, Player self) {
            startedRendering = true;
            
            var ext = self.GetExtensionFields();

            levelBatchMatrix = getSpriteBatchTransformMatrix(ext);
            PlayerShaderMask.SetMaskColor(self);

            if(!ShouldCustomRenderBody(self)) {
                orig(self);

                return;
            }

            beforeRender(self, ext.PlayerRotation, RenderSource.Body);

            orig(self);

            afterRender();

            setSpriteBatchTransformMatrix(levelBatchMatrix, ext);
        }

        [OnHook]
        private static void patch_TrailManager_Add_Entity_Vector2_Color_float(On.Celeste.TrailManager.orig_Add_Entity_Vector2_Color_float orig, Entity entity, Vector2 scale, Color color, float duration) {
            if(entity is Player player) {
                nextTrailSnapshotRotation = player.GetExtensionFields().PlayerRotation;

                // Utils.Log($"ITS A PLAYER!!! SETTING THE NEXT ROTATION TO {nextTrailSnapshotRotation}");
            }

            orig(entity, scale, color, duration);
        }
        
        //oh my god dude
        [OnHook]
        private static TrailManager.Snapshot patch_TrailManager_Add_Vector2_Image_PlayerHair_Vector2_Color_int_float_bool_bool(On.Celeste.TrailManager.orig_Add_Vector2_Image_PlayerHair_Vector2_Color_int_float_bool_bool orig, Vector2 position, Image sprite, PlayerHair hair, Vector2 scale, Color color, int depth, float duration, bool frozenUpdate, bool useRawDeltaTime) {
            var res = orig(position, sprite, hair, scale, color, depth, duration, frozenUpdate, useRawDeltaTime);

            // Utils.Log($"GOT A RESULT!! ITS {result}");

            if(res != null && nextTrailSnapshotRotation != 0) {
                trailManagerSnapshotIndexRotations[res.Index] = nextTrailSnapshotRotation;

                // Utils.Log($"{result.Index} -> {nextTrailSnapshotRotation}");
            }

            nextTrailSnapshotRotation = 0;

            return res;
        }

        [ILHook]
        private static void patch_TrailManager_BeforeRender(ILContext il) {
            var cursor = new ILCursor(il);

            if(cursor.TryGotoNextBestFit(MoveType.After,
                instr => instr.MatchLdsfld(f_RasterizerState_CullNone),
                instr => instr.MatchCallOrCallvirt<SpriteBatch>("Begin")
            )) {
                cursor.EmitLdarg0();
                cursor.EmitDelegate(rotateTrailManagerMatrix);
            }
        }

        [OnHook]
        private static void patch_TrailManager_BeforeRenderPatch(On.Celeste.TrailManager.orig_BeforeRenderPatch orig, TrailManager self) {
            orig(self);

            if(trailManagerSnapshotIndexRotations.Count > 0)
                Utils.Log($"HEY!!! THERE ARE STILL {trailManagerSnapshotIndexRotations.Count} ITEMS IN THE DICTIONARY!!!");

            trailManagerSnapshotIndexRotations.Clear();

            // Utils.Log("CLEARING --------------------");
        }

        private static void beforeRender(Entity playerMaybe, float rotation, RenderSource source) {
            if(Engine.Scene is not Level level)
                return;

            //should be equal to the camera position (but it accounts for translation like in mirror reflections)
            //dw about the negation
            var translation = -levelBatchMatrix.Translation;

            GameplayRenderer.End();

            var playerSpriteCenter = playerMaybe.Center - new Vector2(0, 2);
            
            Effect effect = null;
            var matrix = RotateMatrixAroundVector(levelBatchMatrix, new Vector2(translation.X, translation.Y) - playerSpriteCenter, rotation);

            PlayerShaderMask.BeforeRender(playerMaybe, level, source, ref effect, ref matrix);

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, effect, matrix);
        }

        private static void afterRender() {
            Draw.SpriteBatch.End();
            GameplayRenderer.Begin();
        }

        private static void rotateTrailManagerMatrix(TrailManager trailManager) {
            var snapshot = trailManager.snapshots.ElementAtOrDefault(0);

            if(snapshot is null)
                return;

            // Utils.Log($"HI IM A SNAPSHOT MY INDEX IS {snapshot.Index}!!!!");
            
            if(trailManagerSnapshotIndexRotations.TryGetValue(snapshot.Index, out var rotation)) {
                // Utils.Log($"GOT THE VALUE OF {rotation}");

                var ext = Engine.Scene.Tracker.GetEntity<Player>().GetExtensionFields();
                
                var matrix = getSpriteBatchTransformMatrix(ext);

                //256 = 512/2 = buffer.width/2 = buffer.height/2
                //the 8 is just a magic number to make the rotation around the player sprite work dw about it
                matrix = RotateMatrixAroundVector(matrix, -new Vector2(256, 256) + new Vector2(0, 8), rotation);

                setSpriteBatchTransformMatrix(matrix, ext);

                trailManagerSnapshotIndexRotations.Remove(snapshot.Index);
            }
        }
    }
}