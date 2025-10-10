local trigger = {}

trigger.name = "GooberHelper/ZeroFrameProtector"
trigger.placements = {
    name = "zeroFrameProtector",
    data = {
        flag = "",
        notFlag = "",

        mode = "Left"
    }
}

trigger.fieldInformation = {
    mode = {
        options = {
            ["Left"] = "Left",
            ["Right"] = "Right",
            ["Up"] = "Up",
            ["Down"] = "Down"
        },
        editable = false
    }
}

trigger.fieldOrder = {
    "x",
    "y",
    "width",
    "height",
}

return trigger