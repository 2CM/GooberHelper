local trigger = {}

trigger.name = "GooberHelper/RetentionFrames"
trigger.placements = {
    name = "retentionFrames",
    data = {
        revertOnLeave = false,
        revertOnDeath = false,

        flag = "",
        notFlag = "",

        retentionFrames = 4
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

-- return trigger