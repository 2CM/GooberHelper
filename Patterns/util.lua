local Celeste = require("#celeste")
local Monocle = require("#monocle")
local System = require("#system")

_G.shoot = function (position, direction, color, cullDist, groupId)
    Monocle.Engine.Scene:Add(Celeste.Mod.GooberHelper.Entities.Bullet(Parent, position, direction, color, cullDist, groupId))
end

_G.random = function()
    return Monocle.Calc.NextFloat(System.Random.Shared)
end

_G.AddCoroutine = function (co)
    Parent:AddLuaCoroutine(require("#celeste").mod.LuaCoroutine({value = coroutine.create(co), resume = ThreadProxyResume}))
end