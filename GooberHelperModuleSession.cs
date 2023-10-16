namespace Celeste.Mod.GooberHelper {
    public class GooberHelperModuleSession : EverestModuleSession {
        public bool CobwobSpeedInversion { get; set; } = false;
        public bool AllowRetentionReverse { get; set; } = false;
        public bool JumpInversion { get; set; } = false;
        public bool AllowClimbJumpInversion { get; set; } = false;
        public bool KeepDashAttackOnCollision { get; set; } = false;
        public bool ReboundInversion { get; set; } = false;
    }
}
