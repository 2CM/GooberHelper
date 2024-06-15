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
        🐸READ_THIS_TEXTBOX🐸 = "this entity uses a lot of rendertargets to be able to run. the larger the entity is, the harder it will be to run. 320x184 sized simulations seem to work fine but PLEASE be careful. please dm me any ideas for more customization options, id be happy to implement them. have fun!!!!",
    }
}

entity.fieldOrder = {
    "x",
    "y",
    "width",
    "height",
    "🐸READ_THIS_TEXTBOX🐸",
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