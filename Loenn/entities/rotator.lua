local rotator = {}

rotator.name = "GooberHelper/Rotator"
rotator.depth = 9000
rotator.texture = "objects/door/lockdoor12"
rotator.justification = {0.5, 0.5}
rotator.placements = {
    name = "Rotator",
    data = {
        timeRate = 1,
        radius = 15,
        cooldown = 0.1
    }
}

return rotator