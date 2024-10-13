local Celeste = require("#celeste")
local Monocle = require("#monocle")
local systemException = require("#system.exception")
local Vector2 = require("#microsoft.xna.framework.vector2")
local Color = require("#microsoft.xna.framework.color")
local Calc = require("#monocle").calc
local System = require("#system");

local function zigzag(x)
    return math.abs((x % 2)-1)
end


function Run()
    coroutine.yield(1)

    Celeste.Audio.SetMusic("event:/music/lvl3/clean", true, true)

    local d = 60/98

    local colors = {
        Color.Red,
        Color.Blue,
        Color.Yellow,
    }

    local counter = 0;

    while true do
        for j = 1, 4, 1 do
            local v = 16 - 2 * j;

            for i = 0, v, 1 do
                shoot(
                    Bounds.Center + Vector2(0,0),
                    (60 - j * 15) * math.cos(i/v * math.pi) * Calc.Rotate(Vector2.UnitX, i / v * math.pi + counter),
                    colors[(counter % 3) + 1],
                    200,
                    1,
                    0,
                    0
                )
            end
        end

        counter = counter + 1;

        coroutine.yield(d)
    end

    -- addCoroutine(function()
    --     while true do
    --         shoot(
    --             Bounds.Center + Vector2(0,0),
    --             Vector2(40,0),
    --             Color.Blue,
    --             200,
    --             1,
    --             0,
    --             0
    --         )

    --         coroutine.yield(d * 2)
    --     end
    -- end)

    -- addCoroutine(function()
    --     while true do
    --         shoot(
    --             Bounds.Center + Vector2(0,-4),
    --             Vector2(40,0),
    --             Color.Red,
    --             200,
    --             1,
    --             0,
    --             0
    --         )

    --         coroutine.yield(d / 4)
    --     end
    -- end)

    -- while true do
    --     for i = 0, 6, 1 do
    --         -- if i < 4 then
    --             shoot(
    --                 Bounds.Center + Vector2(0,4),
    --                 Vector2(40,0),
    --                 Color.Green,
    --                 200,
    --                 1,
    --                 0,
    --                 0
    --             )
    --         -- end

    --         coroutine.yield(d / 4)
    --     end

    --     -- coroutine.yield(d / 4)
    -- end

    

    

    -- while true do
    --     local offset = Vector2((random() * 2 - 1) * 200, -50 + random() * 50);

    --     for i = 0, 39, 1 do
    --         local target = Bounds.Center + Calc.Rotate(Vector2.UnitX, i / 20 * math.pi) * (zigzag(i / 4) + 1) * 20
    --         local actual = Bounds.Center + randomDirection() * Vector2(100, 100) + offset
    
    --         shoot(
    --             actual,
    --             0.5 * (target - actual),
    --             HSLColor(i / 40, 1.0, 0.5),
    --             200,
    --             1,
    --             0,
    --             0
    --         )
    --     end

    --     coroutine.yield(2)
    -- end

    -- for i = 0, 39, 1 do
    --     local target = Bounds.Center + Calc.Rotate(Vector2.UnitX, i / 20 * math.pi) * (zigzag(i / 4) + 1) * 20
    --     local actual = Bounds.Center + randomDirection() * Vector2(100, 100) + offset

    --     shoot(
    --         actual,
    --         0.5 * (target - actual),
    --         HSLColor(i / 40, 1.0, 0.5),
    --         200,
    --         1,
    --         0,
    --         0
    --     )
    -- end


end