local simple = {}

local celeste = require("#celeste")
local monocle = require("#monocle")
local systemException = require("#system.exception")
local vector2 = require("#microsoft.xna.framework.vector2")

Parent = nil;

function Vector2(x, y)
    return vector2(x, y)
end


local function shoot()
    -- celeste.Mod.Logger.Log(celeste.Mod.LogLevel.Info, "Lua", "zorg")

    monocle.Engine.Scene:Add(celeste.Mod.GooberHelper.Entities.Bullet())
end

-- STOLEN FROM LUA CUTSCENES
local function threadProxyResume(self, ...)
    local thread = self.value

    if coroutine.status(thread) == "dead" then
        return false, nil
    end

    local success, message = coroutine.resume(thread)

    -- The error message should be returned as an exception and not a string
    if not success then
        return success, systemException(message)
    end

    return success, message
end

function Run()
    -- celesteMod.logger.log(celesteMod.LogLevel.Info, "Lua", "hi")

    for i = 0, 10, 1 do
        shoot()

        coroutine.yield(0.5);
    end
end

function simple.init(parent)
    simple.Parent = parent;

    local corou = celeste.mod.LuaCoroutine({value = coroutine.create(Run), resume = threadProxyResume})

    return corou;
end

return simple