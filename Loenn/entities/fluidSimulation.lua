local entity = {}

entity.name = "GooberHelper/FluidSimulation"
entity.depth = 9000
entity.color = {0.0, 0.0, 1.0, 0.5}
entity.placements = {
    name = "fluidSimulation",
    data = {
        width = 8,
        height = 8,
        playerVelocityInfluence = 0.1,
        playerSizeInfluence = 15.0,
        texture = "",
        velocityDiffusion = 0.95,
        colorDiffusion = 0.95,
        playerHairDyeFactor = 0.0,
        dyeColor = "00ffff,ffffff,ff44ff|0.5",
        dyeCycleSpeed = 4.0,
        onlyDyeWhileDashing = false,
        onlyInfluenceWhileDashing = false,
        depth = 10001.0,
        playerSpeedForFullBrightness = 90,
        pressureIterations = 50,
        vorticity = 0.00,
        doExplosionShockwave = false,
        shockwaveSize = 20,
        shockwaveForce = 10,
        HOVER_OVER_THIS_TEXTBOX = "",
    }
}

entity.fieldOrder = {
    "x",
    "y",
    "width",
    "height",
    "HOVER_OVER_THIS_TEXTBOX",
    "playerVelocityInfluence",
    "playerSizeInfluence",
    "texture",
    "velocityDiffusion",
    "colorDiffusion",
    "playerHairDyeFactor",
    "dyeColor",
    "dyeCycleSpeed",
    "depth",
    "playerSpeedForFullBrightness",
    "pressureIterations",
    "vorticity",
    "shockwaveSize",
    "shockwaveForce",
    "doExplosionShockwave",
    "onlyDyeWhileDashing",
    "onlyInfluenceWhileDashing",
}

return entity