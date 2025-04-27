namespace Celeste.Mod.GooberHelper {
    public class GooberHelperModuleSettings : EverestModuleSettings {
        [SettingSubMenu]
        public class PhysicsSubMenu {
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
            public float RefillFreezeLength { get; set; } = -1f;

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
            public float RetentionFrames { get; set; } = -1f;
            
            [SettingName("GooberHelper_RemoveNormalEnd")]
            [SettingSubText("GooberHelper_RemoveNormalEnd_description")]
            public bool RemoveNormalEnd { get; set; } = false;

            [SettingName("GooberHelper_HyperAndSuperSpeedPreservation")]
            [SettingSubText("GooberHelper_HyperAndSuperSpeedPreservation_description")]
            public bool HyperAndSuperSpeedPreservation { get; set; } = false;

            [SettingName("GooberHelper_PickupSpeedReversal")]
            [SettingSubText("GooberHelper_PickupSpeedReversal_description")]
            public bool PickupSpeedReversal { get; set; } = false;

            [SettingName("GooberHelper_AllowHoldableClimbjumping")]
            [SettingSubText("GooberHelper_AllowHoldableClimbjumping_description")]
            public bool AllowHoldableClimbjumping { get; set; } = false;

            [SettingName("GooberHelper_WallBoostDirectionBasedOnOppositeSpeed")]
            [SettingSubText("GooberHelper_WallBoostDirectionBasedOnOppositeSpeed_description")]
            public bool WallBoostDirectionBasedOnOppositeSpeed { get; set; } = false;

            [SettingName("GooberHelper_WallBoostSpeedIsAlwaysOpposite")]
            [SettingSubText("GooberHelper_WallBoostSpeedIsAlwaysOpposite_description")]
            public bool WallBoostSpeedIsAlwaysOppositeSpeed { get; set; } = false;

            [SettingName("GooberHelper_ReverseDashSpeedPreservation")]
            [SettingSubText("GooberHelper_ReverseDashSpeedPreservation_description")]
            public bool ReverseDashSpeedPreservation { get; set; } = false;

            [SettingName("GooberHelper_KeepSpeedThroughVerticalTransitions")]
            [SettingSubText("GooberHelper_KeepSpeedThroughVerticalTransitions_description")]
            public bool KeepSpeedThroughVerticalTransitions { get; set; } = false;
            
            [SettingName("GooberHelper_BubbleSpeedPreservation")]
            [SettingSubText("GooberHelper_BubbleSpeedPreservation_description")]
            public bool BubbleSpeedPreservation { get; set; } = false;
        }

        [SettingSubMenu]
        public class VisualsSubMenu {
            [SettingName("GooberHelper_PlayerMask")]
            [SettingSubText("GooberHelper_PlayerMask_description")]
            public bool PlayerMask { get; set; } = false;
        }
        
        [SettingSubMenu]
        public class MiscellaneousSubMenu {
            [SettingName("GooberHelper_AlwaysExplodeSpinners")]
            [SettingSubText("GooberHelper_AlwaysExplodeSpinners_description")]
            public bool AlwaysExplodeSpinners { get; set; } = false;

            [SettingName("GooberHelper_GoldenBlocksAlwaysLoad")]
            [SettingSubText("GooberHelper_GoldenBlocksAlwaysLoad_description")]
            public bool GoldenBlocksAlwaysLoad { get; set; } = false;

            [SettingName("GooberHelper_Ant")]
            [SettingSubText("GooberHelper_Ant_description")]
            public bool Ant { get; set; } = false;
        }

        public PhysicsSubMenu Physics { get; set; } = new(); 
        public VisualsSubMenu Visuals { get; set; } = new(); 
        public MiscellaneousSubMenu Miscellaneous { get; set; } = new(); 

        [SettingName("GooberHelper_ShowActiveSettings")]
        [SettingSubText("GooberHelper_ShowActiveSettings_description")]
        public bool ShowActiveSettings { get; set; } = false;


        [SettingName("GooberHelper_DisableSettings")]
        [SettingSubText("GooberHelper_DisableSettings_description")]
        public bool DisableSettings { get; set; } = false;
    }
}
