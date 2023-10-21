namespace Celeste.Mod.GooberHelper {
    public class GooberHelperModuleSettings : EverestModuleSettings {
        [SettingName("GooberHelper_CobwobSpeedInversion")]
        [SettingSubText("GooberHelper_CobwobSpeedInversion_description")]
        public bool CobwobSpeedInversion { get; set; } = false;

        [SettingName("GooberHelper_AllowRetentionReverse")]
        [SettingSubText("GooberHelper_AllowRetentionReverse_description")]
        public bool AllowRetentionReverse { get; set; } = false;

        [SettingName("GooberHelper_JumpInversion")]
        [SettingSubText("GooberHelper_JumpInversion_description")]
        public bool JumpInversion { get; set; } = false;

        [SettingName("GooberHelper_AllowClimbJumpInversion")]
        [SettingSubText("GooberHelper_AllowClimbJumpInversion_description")]
        public bool AllowClimbJumpInversion { get; set; } = false;

        [SettingName("GooberHelper_KeepDashAttackOnCollision")]
        [SettingSubText("GooberHelper_KeepDashAttackOnCollision_description")]
        public bool KeepDashAttackOnCollision { get; set; } = false;

        [SettingName("GooberHelper_ReboundInversion")]
        [SettingSubText("GooberHelper_ReboundInversion_description")]
        public bool ReboundInversion { get; set; } = false;

        [SettingName("GooberHelper_RefillFreezeLength")]
        [SettingSubText("GooberHelper_RefillFreezeLength_description")]
        [SettingRange(-1, 100, true)]
        public int RefillFreezeLength { get; set; } = -1;

        [SettingName("GooberHelper_WallbounceSpeedPreservation")]
        [SettingSubText("GooberHelper_WallbounceSpeedPreservation_description")]
        public bool WallbounceSpeedPreservation { get; set; } = false;
    }
}
