local trigger = {}

trigger.name = "GooberHelper/GooberMiscellaneousOptions"
trigger.placements = {
    name = "gooberMiscellaneousOptions",
    data = {
        revertOnLeave = false,
        revertOnDeath = false,

        flag = "",
        notFlag = "",

        alwaysExplodeSpinners = false,
        goldenBlocksAlwaysLoad = false,
        showActiveSettings = false,
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