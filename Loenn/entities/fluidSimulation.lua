local entity = {}

entity.name = "GooberHelper/FluidSimulation"
entity.depth = 9000
entity.color = {0.0, 0.0, 1.0, 0.5}
entity.placements = {
    name = "FluidSimulation",
    data = {
        width = 8,
        height = 8,
        playerVelocityInfluence = 0.1,
        playerSizeInfluence = 8.0,
        texture = "guhcat",
        velocityDiffusion = 0.95,
        colorDiffusion = 1.0,
        playerHairDyeFactor = 0.0,
        dyeColor = "000000",
        onlyDyeWhileDashing = false,
        onlyInfluenceWhileDashing = false,
    }
}

return entity