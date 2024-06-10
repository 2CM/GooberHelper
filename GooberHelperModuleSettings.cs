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
        
        [SettingName("GooberHelper_DreamBlockSpeedPreservation")]
        [SettingSubText("GooberHelper_WallbounceSpDreamBlockation_description")]
        public bool DreamBlockSpeedPreservation { get; set; } = false;

        [SettingName("GooberHelper_SpringSpeedPreservation")]
        [SettingSubText("GooberHelper_SpringSpeedPreservation_description")]
        public bool SpringSpeedPreservation { get; set; } = false;

        [SettingName("GooberHelper_WallJumpSpeedPreservation")]
        [SettingSubText("GooberHelper_WallJumpSpeedPreservation_description")]
        public bool WallJumpSpeedPreservation { get; set; } = false;

        [SettingName("GooberHelper_GetClimbJumpSpeedInRetainedFrames")]
        [SettingSubText("GooberHelper_GetClimbJumpSpeedInRetainedFrames_description")]
        public bool GetClimbJumpSpeedInRetainedFrames { get; set; } = false;

        [SettingName("GooberHelper_CustomFeathers")]
        [SettingSubText("GooberHelper_CustomFeathers_description")]
        public bool CustomFeathers { get; set; } = false;

        [SettingName("GooberHelper_ExplodeLaunchSpeedPreservation")]
        [SettingSubText("GooberHelper_ExplodeLaunchSpeedPreservation_description")]
        public bool ExplodeLaunchSpeedPreservation { get; set; } = false;

        [SettingName("GooberHelper_BadelineBossSpeedReversing")]
        [SettingSubText("GooberHelper_BadelineBossSpeedReversing_description")]
        public bool BadelineBossSpeedReversing { get; set; } = false;

        [SettingName("GooberHelper_AlwaysActivateCoreBlocks")]
        [SettingSubText("GooberHelper_AlwaysActivateCoreBlocks_description")]
        public bool AlwaysActivateCoreBlocks { get; set; } = false;

        [SettingName("GooberHelper_AlwaysExplodeSpinners")]
        [SettingSubText("GooberHelper_AlwaysExplodeSpinners_description")]
        public bool AlwaysExplodeSpinners { get; set; } = false;

        [SettingName("GooberHelper_CustomSwimming")]
        [SettingSubText("GooberHelper_CustomSwimming_description")]
        public bool CustomSwimming { get; set; } = false;

        [SettingName("GooberHelper_VerticalDashSpeedPreservation")]
        [SettingSubText("GooberHelper_VerticalDashSpeedPreservation_description")]
        public bool VerticalDashSpeedPreservation { get; set; } = false;

        [SettingName("GooberHelper_DashesDontResetSpeed")]
        [SettingSubText("GooberHelper_DashesDontResetSpeed_description")]
        public bool DashesDontResetSpeed { get; set; } = false;

        [SettingName("GooberHelper_RetentionFrames")]
        [SettingSubText("GooberHelper_RetentionFrames_description")]
        [SettingRange(-1, 10000, true)]
        public int RetentionFrames { get; set; } = -1;
        
        [SettingName("GooberHelper_RemoveNormalEnd")]
        [SettingSubText("GooberHelper_RemoveNormalEnd_description")]
        public bool RemoveNormalEnd { get; set; } = false;

        [SettingName("GooberHelper_HyperAndSuperSpeedPreservation")]
        [SettingSubText("GooberHelper_HyperAndSuperSpeedPreservation_description")]
        public bool HyperAndSuperSpeedPreservation { get; set; } = false;

        [SettingName("GooberHelper_GoldenBlocksAlwaysLoad")]
        [SettingSubText("GooberHelper_GoldenBlocksAlwaysLoad_description")]
        public bool GoldenBlocksAlwaysLoad { get; set; } = false;

        [SettingName("GooberHelper_PickupSpeedReversal")]
        [SettingSubText("GooberHelper_PickupSpeedReversal_description")]
        public bool PickupSpeedReversal { get; set; } = false;

        [SettingName("AllowHoldableClimbjumping")]
        [SettingSubText("AllowHoldableClimbjumping_description")]
        public bool AllowHoldableClimbjumping { get; set; } = false;
    }
}
