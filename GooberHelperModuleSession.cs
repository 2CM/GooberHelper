namespace Celeste.Mod.GooberHelper {
    public class GooberHelperModuleSession : EverestModuleSession {
        public bool CobwobSpeedInversion { get; set; } = false;
        public bool AllowRetentionReverse { get; set; } = false;
        public bool JumpInversion { get; set; } = false;
        public bool AllowClimbJumpInversion { get; set; } = false;
        public bool KeepDashAttackOnCollision { get; set; } = false;
        public bool ReboundInversion { get; set; } = false;
        public float RefillFreezeLength { get; set; } = -1;
        public bool WallbounceSpeedPreservation { get; set; } = false;
        public bool DreamBlockSpeedPreservation { get; set; } = false;
        public bool SpringSpeedPreservation { get; set; } = false;
        public bool WallJumpSpeedPreservation { get; set; } = false;
        public bool GetClimbJumpSpeedInRetainedFrames { get; set; } = false;
        public bool CustomFeathers { get; set; } = false;
        public bool ExplodeLaunchSpeedPreservation { get; set; } = false;
        public bool BadelineBossSpeedReversing { get; set; } = false;
        public bool AlwaysActivateCoreBlocks { get; set; } = false;
        public bool AlwaysExplodeSpinners { get; set; } = false;
        public bool CustomSwimming { get; set; } = false;
        public bool VerticalDashSpeedPreservation { get; set; } = false;
        public bool DashesDontResetSpeed { get; set; } = false;
        public float RetentionFrames { get; set; } = -1;
    }
}
