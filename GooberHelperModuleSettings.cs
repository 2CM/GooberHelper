namespace Celeste.Mod.GooberHelper {
    public class GooberHelperModuleSettings : EverestModuleSettings {
        [SettingSubMenu]
        public class PhysicsSubMenu
        {
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
            [SettingSubText("GooberHelper_DreamBlockSpeedPreservation_description")]
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

            [SettingName("GooberHelper_FeatherEndSpeedPreservation")]
            [SettingSubText("GooberHelper_FeatherEndSpeedPreservation_description")]
            public bool FeatherEndSpeedPreservation { get; set; } = false;

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

            [SettingName("GooberHelper_ReverseDashSpeedPreservation")]
            [SettingSubText("GooberHelper_ReverseDashSpeedPreservation_description")]
            public bool ReverseDashSpeedPreservation { get; set; } = false;

            [SettingName("GooberHelper_MagnitudeBasedDashSpeed")]
            [SettingSubText("GooberHelper_MagnitudeBasedDashSpeed_description")]
            public bool MagnitudeBasedDashSpeed { get; set; } = false;

            [SettingName("GooberHelper_MagnitudeBasedDashSpeedOnlyCardinal")]
            [SettingSubText("GooberHelper_MagnitudeBasedDashSpeedOnlyCardinal_description")]
            public bool MagnitudeBasedDashSpeedOnlyCardinal { get; set; } = false;

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

            [SettingName("GooberHelper_PickupSpeedReversal")]
            [SettingSubText("GooberHelper_PickupSpeedReversal_description")]
            public bool PickupSpeedReversal { get; set; } = false;

            [SettingName("GooberHelper_AllowHoldableClimbjumping")]
            [SettingSubText("GooberHelper_AllowHoldableClimbjumping_description")]
            public bool AllowHoldableClimbjumping { get; set; } = false;

            [SettingName("GooberHelper_WallBoostDirectionBasedOnOppositeSpeed")]
            [SettingSubText("GooberHelper_WallBoostDirectionBasedOnOppositeSpeed_description")]
            public bool WallBoostDirectionBasedOnOppositeSpeed { get; set; } = false;

            [SettingName("GooberHelper_WallBoostSpeedIsAlwaysOppositeSpeed")]
            [SettingSubText("GooberHelper_WallBoostSpeedIsAlwaysOppositeSpeed_description")]
            public bool WallBoostSpeedIsAlwaysOppositeSpeed { get; set; } = false;

            [SettingName("GooberHelper_KeepSpeedThroughVerticalTransitions")]
            [SettingSubText("GooberHelper_KeepSpeedThroughVerticalTransitions_description")]
            public bool KeepSpeedThroughVerticalTransitions { get; set; } = false;

            [SettingName("GooberHelper_BubbleSpeedPreservation")]
            [SettingSubText("GooberHelper_BubbleSpeedPreservation_description")]
            public bool BubbleSpeedPreservation { get; set; } = false;
            
            [SettingName("GooberHelper_AdditiveVerticalJumpSpeed")]
            [SettingSubText("GooberHelper_AdditiveVerticalJumpSpeed_description")]
            public bool AdditiveVerticalJumpSpeed { get; set; } = false;

            [SettingName("GooberHelper_WallJumpSpeedInversion")]
            [SettingSubText("GooberHelper_WallJumpSpeedInversion_description")]
            public bool WallJumpSpeedInversion { get; set; } = false;

            [SettingName("GooberHelper_AllDirectionHypersAndSupers")]
            [SettingSubText("GooberHelper_AllDirectionHypersAndSupers_description")]
            public bool AllDirectionHypersAndSupers { get; set; } = false;

            [SettingName("AllDirectionHypersAndSupersWorkWithCoyoteTime")]
            [SettingSubText("GooberHelper_AllDirectionHypersAndSupersWorkWithCoyoteTime_description")]
            public bool AllDirectionHypersAndSupersWorkWithCoyoteTime { get; set; } = false;

            [SettingName("AllowUpwardsCoyote")]
            [SettingSubText("GooberHelper_AllowUpwardsCoyote_description")]
            public bool AllowUpwardsCoyote { get; set; } = false;
            
            [SettingName("AllDirectionDreamJumps")]
            [SettingSubText("GooberHelper_AllDirectionDreamJumps_description")]
            public bool AllDirectionDreamJumps { get; set; } = false;

            [SettingName("LenientStunning")]
            [SettingSubText("GooberHelper_LenientStunning_description")]
            public bool LenientStunning { get; set; } = false;

            [SettingName("HorizontalTurningSpeedInversion")]
            [SettingSubText("GooberHelper_HorizontalTurningSpeedInversion_description")]
            public bool HorizontalTurningSpeedInversion { get; set; } = false;

            [SettingName("VerticalTurningSpeedInversion")]
            [SettingSubText("GooberHelper_VerticalTurningSpeedInversion_description")]
            public bool VerticalTurningSpeedInversion { get; set; } = false;

            [SettingName("AllowCrouchedHoldableGrabbing")]
            [SettingSubText("GooberHelper_AllowCrouchedHoldableGrabbing_description")]
            public bool AllowCrouchedHoldableGrabbing { get; set; } = false;

            [SettingName("HoldablesInheritSpeedWhenThrown")]
            [SettingSubText("GooberHelper_HoldablesInheritSpeedWhenThrown_description")]
            public bool HoldablesInheritSpeedWhenThrown { get; set; } = false;

            [SettingName("UpwardsJumpSpeedPreservation")]
            [SettingSubText("GooberHelper_UpwardsJumpSpeedPreservation_description")]
            public bool UpwardsJumpSpeedPreservation { get; set; } = false;

            [SettingName("DownwardsJumpSpeedPreservation")]
            [SettingSubText("GooberHelper_DownwardsJumpSpeedPreservation_description")]
            public bool DownwardsJumpSpeedPreservation { get; set; } = false;

            [SettingName("DownwardsAirFrictionBehavior")]
            [SettingSubText("GooberHelper_DownwardsAirFrictionBehavior_description")]
            public bool DownwardsAirFrictionBehavior { get; set; } = false;

            [SettingName("CornerboostBlocksEverywhere")]
            [SettingSubText("GooberHelper_CornerboostBlocksEverywhere_description")]
            public bool CornerboostBlocksEverywhere { get; set; } = false;

            [SettingName("SwapHorizontalAndVerticalSpeedOnWallJump")]
            [SettingSubText("GooberHelper_SwapHorizontalAndVerticalSpeedOnWallJump_description")]
            public bool SwapHorizontalAndVerticalSpeedOnWallJump { get; set; } = false;

            [SettingName("VerticalSpeedToHorizontalSpeedOnGroundJump")]
            [SettingSubText("GooberHelper_VerticalSpeedToHorizontalSpeedOnGroundJump_description")]
            public bool VerticalSpeedToHorizontalSpeedOnGroundJump { get; set; } = false;
        }

        [SettingSubMenu]
        public class VisualsSubMenu {
            [SettingName("GooberHelper_PlayerMask")]
            [SettingSubText("GooberHelper_PlayerMask_description")]
            public bool PlayerMask { get; set; } = false;
            
            [SettingName("GooberHelper_PlayerMaskHairOnly")]
            [SettingSubText("GooberHelper_PlayerMaskHairOnly_description")]
            public bool PlayerMaskHairOnly { get; set; } = false;

            [SettingName("GooberHelper_TheoNuclearReactor")]
            [SettingSubText("GooberHelper_TheoNuclearReactor_description")]
            public bool TheoNuclearReactor { get; set; } = false;
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
