function PrimaryPattern()
    local offset = 0

    while true do
        for angle = 0, 360, 120 do
            Shoot{
                position = Vector2(0, 0),
                velocity = Angle(angle + offset) * 100,
                texture = "bullets/GooberHelper/arrow",
                scale = 0.5,
                color = Hsv(offset * 1.44 - angle, 1, 1)
            }
        end

        offset = offset + 11.11

        coroutine.yield(0.01)
    end
end

function SecondaryPattern()
    local offset = 0

    while true do
        for angle = 0, 360, 60 do
            Shoot{
                position = Vector2(0, 0),
                velocity = Angle(angle + offset) * 50,
                texture = "bullets/GooberHelper/halo",
                scale = 0.5,
                color = Hsv(angle, 0.5, 1),
                additive = true
            }
        end

        offset = offset + 30

        coroutine.yield(1.0)
    end
end

function SmitePattern()
    local sources = { Vector2(100, -100), Vector2(-100, -100), Vector2(50, -100), Vector2(-50, -100) }

    while true do
        for index, source in ipairs(sources) do
            local offsetSource = source + RandomDirection() * 50

            Shoot{
                position = offsetSource,
                velocity = TowardsPlayer(offsetSource) * 1000,
                texture = "bullets/GooberHelper/knife",
                scale = 1,
                color = Hsv(RandomRange(100, 260), RandomRange(0.4, 0.8), 1)
            }
        end


        coroutine.yield(0.01)
    end
end

_G.bps = 60/90

function Run()
    -- coroutine.yield(PlaySyncedMusic("event:/music/lvl1/main"))

    -- require("#Celeste.Audio").CurrentMusicEventInstance:setParameterValue("layer2", 0)
    -- require("#Celeste.Audio").CurrentMusicEventInstance:setParameterValue("layer3", 0)

    -- while true do
    --     Shoot{
    --         position = Vector2(0, 0),
    --         velocity = Vector2(200, 0),
    --         texture = "bullets/GooberHelper/lightning",
    --         scale = 1,
    --         color = Hsv(RandomRange(100, 260), RandomRange(0.4, 0.8), 1)
    --     }

    --     require("#Celeste.Audio").Play("event:/game/06_reflection/fallblock_boss_impact")

    --     coroutine.yield(bps)
    -- end

    -- require("#Celeste.Audio").SetMusic("event:/music/remix/03_resort")

    local primary = AddCoroutine(PrimaryPattern)
    local secondary = AddCoroutine(SecondaryPattern)

    coroutine.yield(10)

    primary:RemoveSelf()
    secondary:RemoveSelf()

    coroutine.yield(1)

    local smite = AddCoroutine(SmitePattern)
end