local gooberGodrays = {}

gooberGodrays.name = "GooberHelper/GooberGodrays"
gooberGodrays.canBackground = true
gooberGodrays.canForeground = true

gooberGodrays.fieldInformation = {
    texture = {
        editable = true,
        options = {}
    },
    playerInfluence = {
        minimumValue = -100.0,
        maximumValue = 100.0
    }
}

gooberGodrays.defaultData = {
    texture = "guhcat",
    playerInfluence = 1.0
}

return gooberGodrays