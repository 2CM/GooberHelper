local entity = {}

entity.name = "GooberHelper/GooberFlingBird"
entity.depth = 0
entity.nodeLineRenderType = "line"
entity.texture = "characters/bird/Hover04"
entity.nodeLimits = {0, -1}
entity.placements = {
    name = "GooberFlingBird",
    data = {
        waiting = false,
        reverse = false,
        index = 0
    }
}

return entity