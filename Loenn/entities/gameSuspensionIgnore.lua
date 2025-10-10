
-- https://github.com/JaThePlayer/FrostHelper/blob/9b11b96e5c94326ee743db592de2b15888867ea1/Loenn/libraries/jautils.lua#L233
-- this entire file refrenced/copied from a lot in this file
-- thank you ja

local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")

local entities = require("entities") -- the jautils version of this uses a tryRequire but i dont have that and its probably fine

local function getAllSids() end

if entities and entities.registeredEntities then
    getAllSids = function ()
        local ret = {}
        local amt = 0
        for k,v in pairs(entities.registeredEntities) do
            table.insert(ret, k)
            amt = amt + 1
        end

        table.sort(ret)

        return ret
    end
end

local entity = {}

entity.name = "GooberHelper/GameSuspensionIgnore"
entity.fillColor = { 0.5, 1, 1, 0.1 }
entity.borderColor = { 0.8, 0.8, 1, 1 }
entity.placements = {
    name = "gameSuspensionIgnore",
    data = {
        width = 8,
        height = 8,
        types = ""
    }
}

entity.fieldInformation = {
    types = {
        fieldType = "list",
        elementSeparator = ",",
        elementDefault = "",
        elementOptions = {
            options = getAllSids(),
            searchable = true
        }
    }
}

entity.fieldOrder = {
    "x",
    "y",
    "width",
    "height",
}

return entity