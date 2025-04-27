local trigger = {}

trigger.name = "GooberHelper/GooberVisualOptions"
trigger.placements = {
    name = "gooberVisualOptions",
    data = {
        revertOnLeave = false,
        revertOnDeath = false,

        flag = "",
        notFlag = "",

        playerMask = false,
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