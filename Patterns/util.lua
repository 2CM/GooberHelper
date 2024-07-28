local Celeste = require("#celeste")
local Monocle = require("#monocle")
local System = require("#system")
local Vector2 = require("#microsoft.xna.framework.vector2")
local Color = require("#microsoft.xna.framework.color")


_G.shoot = function (position, direction, color, cullDist, size, groupId, bounceAmplitude)
    local bullet = Celeste.Mod.GooberHelper.Entities.Bullet(Parent, position, direction, color, cullDist, size, groupId, bounceAmplitude);

    Monocle.Engine.Scene:Add(bullet);

    return bullet;
end

_G.shootSpecial = function (position, direction, color, cullDist, size, groupId, bounceAmplitude, friction, acceleration)
    local bullet = Celeste.Mod.GooberHelper.Entities.Bullet(Parent, position, direction, color, cullDist, size, groupId, bounceAmplitude, friction, acceleration)

    Monocle.Engine.Scene:Add(bullet)

    return bullet;
end

_G.random = function()
    return Monocle.Calc.NextFloat(System.Random.Shared)
end

_G.randomInt = function()
    return System.Random.Shared.Next(System.Random.Shared)
end
 
_G.randomDirection = function()
    return Monocle.Calc.SafeNormalize(Vector2(random() * 2 - 1, random() * 2 - 1));
end

_G.deltaTime = function()
    return Monocle.Engine.DeltaTime;
end

_G.addCoroutine = function (co)
    return Parent:AddLuaCoroutine(require("#celeste").mod.LuaCoroutine({value = coroutine.create(co), resume = ThreadProxyResume}))
end

_G.removeCoroutine = function (co)
    Parent:Remove(co)
end

_G.getGroup = function (groupId)
    return Celeste.Mod.GooberHelper.Entities.Bullet.GetGroup(groupId)
end

-- taken from here https://stackoverflow.com/questions/68317097/how-to-properly-convert-hsl-colors-to-rgb-colors-in-lua
    local function hue2rgb(p, q, t)
        if t < 0 then t = t + 1 end
        if t > 1 then t = t - 1 end
        if t < 1 / 6 then return p + (q - p) * 6 * t end
        if t < 1 / 2 then return q end
        if t < 2 / 3 then return p + (q - p) * (2 / 3 - t) * 6 end
        return p;
    end

    _G.HSLColor = function (h, s, l)
        local r, g, b;

        if s == 0 then
            r, g, b = l, l, l; -- achromatic
        else
            local q = l < 0.5 and l * (1 + s) or l + s - l * s;
            local p = 2 * l - q;
            r = hue2rgb(p, q, h + 1 / 3);
            g = hue2rgb(p, q, h);
            b = hue2rgb(p, q, h - 1 / 3);
        end

        return Color(r,g,b);
    end