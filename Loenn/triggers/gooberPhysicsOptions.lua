local trigger = {}

trigger.name = "GooberHelper/GooberPhysicsOptions"
trigger.placements = {
    name = "gooberPhysicsOptions",
    data = {
        revertOnLeave = false,
        revertOnDeath = false,

        flag = "",
        notFlag = "",

        cobwobSpeedInversion = false,
        allowRetentionReverse = false,
        jumpInversion = false,
        allowClimbJumpInversion = false,
        keepDashAttackOnCollision = false,
        reboundInversion = false,
        wallbounceSpeedPreservation = false,
        dreamBlockSpeedPreservation = false,
        springSpeedPreservation = false,
        wallJumpSpeedPreservation = false,
        getClimbJumpSpeedInRetainedFrames = false,
        customFeathers = false,
        explodeLaunchSpeedPreservation = false,
        badelineBossSpeedReversing = false,
        alwaysActivateCoreBlocks = false,
        customSwimming = false,
        verticalDashSpeedPreservation = false,
        dashesDontResetSpeed = false,
        hyperAndSuperSpeedPreservation = false,
        removeNormalEnd = false,
        pickupSpeedReversal = false,
        allowHoldableClimbjumping = false,
        wallBoostDirectionBasedOnOppositeSpeed = false,
        wallBoostSpeedIsAlwaysOppositeSpeed = false,
        reverseDashSpeedPreservation = false,
        keepSpeedThroughVerticalTransitions = false,
        bubbleSpeedPreservation = false,
    }
}

trigger.fieldOrder = {
    "x",
    "y",
    "width",
    "height",
    "revertOnLeave",
    "revertOnDeath"
}

return trigger