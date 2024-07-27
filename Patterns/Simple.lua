local simple = {}

local Celeste = require("#celeste")
local Monocle = require("#monocle")
local systemException = require("#system.exception")
local Vector2 = require("#microsoft.xna.framework.vector2")
local Color = require("#microsoft.xna.framework.color")
local Calc = require("#monocle").calc
local System = require("#system");

Parent = nil;

local function random()
    return Calc.NextFloat(System.Random.Shared)
end

local function randomDirection()
    return Vector2(random() * 2 - 1, random() * 2 - 1)
end

local function getGroup(groupId)
    return Celeste.Mod.GooberHelper.Entities.Bullet.GetGroup(groupId)
end


local function shoot(position, direction, color, groupId)
    -- celeste.Mod.Logger.Log(celeste.Mod.LogLevel.Info, "Lua", "zorg")

    Monocle.Engine.Scene:Add(Celeste.Mod.GooberHelper.Entities.Bullet(simple.Parent, position, direction, color, groupId))
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
    -- local offset = 0;

    -- local Colors = {
    --     Color.Red,
    --     Color.Blue,
    --     Color.Green,
    --     Color.Green,
    --     Color.Green,
    -- }

    -- while true do
    --     for j = 0, 1, 1 do
    --         for i = 0, math.pi, math.pi / 15 do
    --             shoot(Vector2(0, 0), Calc.Rotate(Vector2.UnitY, i) * 50 * math.sin(i + j * math.pi + offset) + randomDirection() * 5 + Calc.Rotate(Vector2.UnitY, i * 2 + j * math.pi + offset) * 20, Colors[j + 1]);
    --         end
    --     end

    --     coroutine.yield(0.5);

    --     offset = offset + math.pi / 4 + 0.02;
    -- end

    while true do
        local centerPos1 = Vector2(random() * 50 - 25, random() * 50 - 25);
        local centerPos2 = Vector2(random() * 50 - 25, random() * 50 - 25);
        local tagBase1 = random();
        local tagBase2 = random();

        for i = 0, 24, 1 do
            shoot(centerPos1 + Calc.Rotate(Vector2.UnitY * 10, i / 12 * math.pi), Vector2.Zero, Color(i / 12, i % 2, 1 - i / 12), tagBase1)
            shoot(centerPos2 + Calc.Rotate(Vector2.UnitY * 10, i / 12 * math.pi), Vector2.Zero, Color(i / 12, i % 2, 1 - i / 12), tagBase2)

            coroutine.yield(0.01)
        end

        coroutine.yield(0.25)

        for i, value in ipairs(getGroup(tagBase1)) do
            value:SetSpeed(Calc.SafeNormalize(value:GetPosition() - centerPos1) * ((i % 2) * 2 - 1) * 100)
        end

        for i, value in ipairs(getGroup(tagBase2)) do
            value:SetSpeed(Calc.SafeNormalize(value:GetPosition() - centerPos2) * ((i % 2) * 2 - 1) * 100)
        end
    end
end

function simple.init(parent)
    simple.Parent = parent;

    local corou = Celeste.mod.LuaCoroutine({value = coroutine.create(Run), resume = threadProxyResume})

    return corou;
end

return simple