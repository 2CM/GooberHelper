using System.Runtime.CompilerServices;

namespace Celeste.Mod.GooberHelper {

    //i am so sorry
    //i really dont want to use reflection
    //this seems like the best way to clean up the main mod code
    //homer simpson with fat tied behind his back image
    //man i wish c# supported macros
    //i know i could use a dictionary for session stuff, but i would still have to use raw reflection for the mod options
    //and i really dont want to code my own freaky mod options thing or have a pause menu gui like extvars does
    //i would like to apologize again
    //this is better than manually writing ((Settings.thing && !Settings.DisableSettings) || Session.thing) constantly in the main mod code
    //i know that using reflection wouldnt add Too much extra computation cost, but the game is already laggy enough for some people
    public static class OptionsManager {
        private static GooberHelperModuleSettings st => GooberHelperModule.Settings;
        private static GooberHelperModuleSettings.PhysicsSubMenu ph => GooberHelperModule.Settings.Physics;
        private static GooberHelperModuleSettings.VisualsSubMenu vi => GooberHelperModule.Settings.Visuals;
        private static GooberHelperModuleSettings.MiscellaneousSubMenu mi => GooberHelperModule.Settings.Miscellaneous;
        private static GooberHelperModuleSession se => GooberHelperModule.Session;

        public static bool CobwobSpeedInversion => (ph.CobwobSpeedInversion && !st.DisableSettings) || se.CobwobSpeedInversion;
        public static bool AllowRetentionReverse => (ph.AllowRetentionReverse && !st.DisableSettings) || se.AllowRetentionReverse;
        public static bool JumpInversion => (ph.JumpInversion && !st.DisableSettings) || se.JumpInversion;
        public static bool AllowClimbJumpInversion => (ph.AllowClimbJumpInversion && !st.DisableSettings) || se.AllowClimbJumpInversion;
        public static bool KeepDashAttackOnCollision => (ph.KeepDashAttackOnCollision && !st.DisableSettings) || se.KeepDashAttackOnCollision;
        public static bool ReboundInversion => (ph.ReboundInversion && !st.DisableSettings) || se.ReboundInversion;
        public static bool WallbounceSpeedPreservation => (ph.WallbounceSpeedPreservation && !st.DisableSettings) || se.WallbounceSpeedPreservation;
        public static bool DreamBlockSpeedPreservation => (ph.DreamBlockSpeedPreservation && !st.DisableSettings) || se.DreamBlockSpeedPreservation;
        public static bool SpringSpeedPreservation => (ph.SpringSpeedPreservation && !st.DisableSettings) || se.SpringSpeedPreservation;
        public static bool WallJumpSpeedPreservation => (ph.WallJumpSpeedPreservation && !st.DisableSettings) || se.WallJumpSpeedPreservation;
        public static bool GetClimbJumpSpeedInRetainedFrames => (ph.GetClimbJumpSpeedInRetainedFrames && !st.DisableSettings) || se.GetClimbJumpSpeedInRetainedFrames;
        public static bool CustomFeathers => (ph.CustomFeathers && !st.DisableSettings) || se.CustomFeathers;
        public static bool FeatherEndSpeedPreservation => (ph.FeatherEndSpeedPreservation && !st.DisableSettings) || se.FeatherEndSpeedPreservation;
        public static bool ExplodeLaunchSpeedPreservation => (ph.ExplodeLaunchSpeedPreservation && !st.DisableSettings) || se.ExplodeLaunchSpeedPreservation;
        public static bool BadelineBossSpeedReversing => (ph.BadelineBossSpeedReversing && !st.DisableSettings) || se.BadelineBossSpeedReversing;
        public static bool AlwaysActivateCoreBlocks => (ph.AlwaysActivateCoreBlocks && !st.DisableSettings) || se.AlwaysActivateCoreBlocks;
        public static bool CustomSwimming => (ph.CustomSwimming && !st.DisableSettings) || se.CustomSwimming;
        public static bool VerticalDashSpeedPreservation => (ph.VerticalDashSpeedPreservation && !st.DisableSettings) || se.VerticalDashSpeedPreservation;
        public static bool DashesDontResetSpeed => (ph.DashesDontResetSpeed && !st.DisableSettings) || se.DashesDontResetSpeed;
        public static bool RemoveNormalEnd => (ph.RemoveNormalEnd && !st.DisableSettings) || se.RemoveNormalEnd;
        public static bool HyperAndSuperSpeedPreservation => (ph.HyperAndSuperSpeedPreservation && !st.DisableSettings) || se.HyperAndSuperSpeedPreservation;
        public static bool PickupSpeedReversal => (ph.PickupSpeedReversal && !st.DisableSettings) || se.PickupSpeedReversal;
        public static bool AllowHoldableClimbjumping => (ph.AllowHoldableClimbjumping && !st.DisableSettings) || se.AllowHoldableClimbjumping;
        public static bool WallBoostDirectionBasedOnOppositeSpeed => (ph.WallBoostDirectionBasedOnOppositeSpeed && !st.DisableSettings) || se.WallBoostDirectionBasedOnOppositeSpeed;
        public static bool WallBoostSpeedIsAlwaysOppositeSpeed => (ph.WallBoostSpeedIsAlwaysOppositeSpeed && !st.DisableSettings) || se.WallBoostSpeedIsAlwaysOppositeSpeed;
        public static bool ReverseDashSpeedPreservation => (ph.ReverseDashSpeedPreservation && !st.DisableSettings) || se.ReverseDashSpeedPreservation;
        public static bool KeepSpeedThroughVerticalTransitions => (ph.KeepSpeedThroughVerticalTransitions && !st.DisableSettings) || se.KeepSpeedThroughVerticalTransitions;
        public static bool BubbleSpeedPreservation => (ph.BubbleSpeedPreservation && !st.DisableSettings) || se.BubbleSpeedPreservation;
        public static bool AdditiveVerticalJumpSpeed => (ph.AdditiveVerticalJumpSpeed && !st.DisableSettings) || se.AdditiveVerticalJumpSpeed;
        public static bool WallJumpSpeedInversion => (ph.WallJumpSpeedInversion && !st.DisableSettings) || se.WallJumpSpeedInversion;
        public static bool AllDirectionHypersAndSupers => (ph.AllDirectionHypersAndSupers && !st.DisableSettings) || se.AllDirectionHypersAndSupers;
        public static bool AllDirectionHypersAndSupersWorkWithCoyoteTime => (ph.AllDirectionHypersAndSupersWorkWithCoyoteTime && !st.DisableSettings) || se.AllDirectionHypersAndSupersWorkWithCoyoteTime;
        public static bool PlayerMask => (vi.PlayerMask && !st.DisableSettings) || se.PlayerMask;
        public static bool AlwaysExplodeSpinners => (mi.AlwaysExplodeSpinners && !st.DisableSettings) || se.AlwaysExplodeSpinners;
        public static bool GoldenBlocksAlwaysLoad => (mi.GoldenBlocksAlwaysLoad && !st.DisableSettings) || se.GoldenBlocksAlwaysLoad;
        public static bool Ant => mi.Ant && !st.DisableSettings;
        

        public static int RefillFreezeLength => (ph.RefillFreezeLength != -1 && !st.DisableSettings) ?
            ph.RefillFreezeLength :
            se.RefillFreezeLength;

        public static int RetentionFrames => (ph.RetentionFrames != -1 && !st.DisableSettings) ?
            ph.RetentionFrames :
            se.RetentionFrames;
    }
}