local Celeste = require("#celeste")
local Monocle = require("#monocle")
local systemException = require("#system.exception")
local Vector2 = require("#microsoft.xna.framework.vector2")
local Color = require("#microsoft.xna.framework.color")
local Calc = require("#monocle").calc
local System = require("#system");

function Run()
    local counter = 0;

    while true do
        shoot(
            Vector2.Zero,
            Calc.SafeNormalize(Calc.Rotate(Vector2.UnitY, counter + random() * 0.05)) * 100,
            Color(1, 0, 0),
            0,
            0
        )

        counter = counter + 0.1;

        coroutine.yield(0.01)
    end
end