local trigger = {}

trigger.name = "GooberHelper/RefillFreezeLength"
trigger.placements = {
    name = "refillFreezeLength",
    data = {
        revertOnLeave = false,
        revertOnDeath = false,

        flag = "",
        notFlag = "",

        refillFreezeLength = 3
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