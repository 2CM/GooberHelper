local trigger = {}

trigger.name = "GooberHelper/RefillFreezeLength"
trigger.placements = {
    name = "refillFreezeLength",
    data = {
        revertOnLeave = false,
        revertOnDeath = false,

        refillFreezeLength = 3
    }
}

trigger.fieldOrder = {
    "revertOnLeave",
    "revertOnDeath"
}

return trigger