local trigger = {}

trigger.name = "GooberHelper/RetentionFrames"
trigger.placements = {
    name = "retentionFrames",
    data = {
        revertOnLeave = false,
        revertOnDeath = false,

        retentionFrames = 4
    }
}

trigger.fieldOrder = {
    "revertOnLeave",
    "revertOnDeath"
}

return trigger