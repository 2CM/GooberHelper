local drawableNinePatch = require("structs.drawable_nine_patch")
local utils = require("utils")

local gooberCrumbleBlocks = {}

local textures = {
    "default", "cliffside"
}

gooberCrumbleBlocks.name = "GooberHelper/GooberCrumblePlatform"
gooberCrumbleBlocks.depth = 0
gooberCrumbleBlocks.fieldInformation = {
    texture = {
        options = textures,
    }
}
gooberCrumbleBlocks.placements = {}

for _, texture in ipairs(textures) do
    table.insert(gooberCrumbleBlocks.placements, {
        name = "GooberCrumblePlatform (" .. texture .. ")",
        data = {
            width = 8,
            height = 8,
            texture = texture,
            respawnTime = "2",
            crumbleTime = "0.4",
            minGrabCrumbleTime = "0.6",
            minTopCrumbleTime = "0.2",
        }
    })
end

local ninePatchOptions = {
    mode = "fill",
    fillMode = "repeat",
    border = 0
}

function gooberCrumbleBlocks.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width = math.max(entity.width or 0, 8)
    local height = math.max(entity.height or 0, 8)

    local variant = entity.texture or "default"
    local texture = "objects/crumbleBlock/" .. variant
    local ninePatch = drawableNinePatch.fromTexture(texture, ninePatchOptions, x, y, width, height)

    return ninePatch
end

function gooberCrumbleBlocks.selection(room, entity)
    return utils.rectangle(entity.x or 0, entity.y or 0, math.max(entity.width or 0, 8), math.max(entity.height or 0, 8))
end

return gooberCrumbleBlocks