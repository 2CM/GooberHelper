local Celeste = require("#celeste")
local Monocle = require("#monocle")
local systemException = require("#system.exception")
local Vector2 = require("#microsoft.xna.framework.vector2")
local Color = require("#microsoft.xna.framework.color")
local Calc = require("#monocle").calc
local System = require("#system");

function Run()
    -- while true do
    --     local dir = (randomInt() % 2) * 2 - 1;

    --     local bullet = shoot(
    --         Bounds.Center + Vector2(dir * Bounds.Width / 2, (random() * 2 - 1) * Bounds.Height / 2),
    --         Vector2(-dir * 50, 0),
    --         Color.Red,
    --         100,
    --         3,
    --         0,
    --         0
    --     )

    --     addCoroutine(
    --         function()
    --             coroutine.yield(1)

    --             bullet:RemoveSelf();

    --             local rand = random();

    --             for i = 0, 11, 1 do
    --                 shoot(
    --                     bullet:GetPosition(),
    --                     Calc.Rotate(Vector2.UnitY, i / 6 * math.pi) * 50,
    --                     HSLColor(i / 12 + rand, 1, 0.5),
    --                     -1,
    --                     1,
    --                     0,
    --                     0
    --                 )
    --             end
    --         end
    --     )

    --     coroutine.yield(0.2)
    -- end

    local counter = 0
    local dir = 1;
    local count = 12;
    
    while true do
        for i = 0, count - 1, 1 do
            shootSpecialPolar(
                Bounds.Center,
                Calc.Rotate(Vector2.UnitY * 16, 2 * math.pi * i / count + counter),
                HSLColor(i / count, 1, 0.5),
                100,
                1,
                0,
                0,
                0,
                Vector2(0, 0),
                Bounds.Center + Calc.Rotate(Vector2.UnitY * 16, 2 * math.pi * i / count + counter),
                Vector2(5 * dir, 0),
                Vector2(0, 0),
                Vector2(0,0),
                true
            )
        end

        counter = counter + 0.1;
        dir = dir * 1;

        coroutine.yield(0.2)
    end
end

-- local counter = 0;

-- local colors = {
--     Color.Red,
--     Color.Yellow,
--     Color.Green,
--     Color.Blue,
-- }

-- while true do
--     local rand = random()

--     for i = 0, 3, 1 do
--         for j = 0, 11, 1 do
--             shootSpecial(
--                 Center + Calc.Rotate(Vector2.UnitY, j / 6 * math.pi) * 10,
--                 Calc.Rotate(Vector2.UnitY, i / 2 * math.pi) * 50,
--                 colors[i + 1],
--                 0,
--                 1,
--                 i + rand,
--                 0,
--                 3,
--                 Vector2.Zero
--             )
--         end

--         coroutine.yield(0.1)
--     end

--     coroutine.yield(0.2)

--     for i = 0, 3, 1 do
--         for index, value in ipairs(getGroup(rand + i)) do
--             value:SetSpeed(randomDirection() * 50 + Vector2(0, 10))
--             value:SetFriction(0)
--             -- value:SetAcceleration(Vector2(-50 * ((i % 2) * 2 - 1), 0))
--             -- value:SetBounceAmplitude(1)
--         end

--         coroutine.yield(0.1)
--     end

--     coroutine.yield(0.2)
-- end
