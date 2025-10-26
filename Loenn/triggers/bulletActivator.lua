local trigger = {}

trigger.name = "GooberHelper/BulletActivator"
trigger.placements = {
    name = "bulletActivator",
    data = {
        pattern = "",

        flag = "",
        notFlag = "",

        flagActivated = false
    }
}
trigger.nodeLimits = {1, 1}

return trigger