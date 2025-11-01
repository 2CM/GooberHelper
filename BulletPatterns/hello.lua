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

_G.spb = 60/98

local function starAt(center, tuh)
    for index, value in ipairs(require("#Celeste.Mod.GooberHelper.BulletPatternHelper").CreateStar(Vector2(0,0), 0.5, 1, RandomRange(0, 360), 5, 5)) do
        Shoot{
            template = tuh,
            position = center,
            velocity = 20 * value / (value:Length() ^ 0.3) * 8.0,
            texture = "bullets/GooberHelper/arrow",
            scale = 0.5,
            color = Hsv(index / 50 * 360, RandomRange(0.4, 0.8), 1),
        }
    end
end

function PanCamera()
    Player.CameraAnchorLerp = Vector2(0, 1)
    
    while true do
        Player.CameraAnchor = Player.CameraTarget + Vector2(0, -Engine.DeltaTime * 10)

        coroutine.yield(0.016)
    end
end

function Run()
    
    -- coroutine.yield(PlaySyncedMusic("event:/Unowen_Music/bubble"))
    coroutine.yield(PlaySyncedMusic("event:/music/lvl3/oshiro_chase"))
    
    AddCoroutine(PanCamera)

    EnterTouhouState()

    for k = 0, 7, 1 do
        for i = 0, 7, 1 do
            local y = i * 20

            for j = -1, 1, 2 do
                Shoot{
                    position = Vector2(150 * j, -y - 20 - k * 20),
                    velocity = Vector2(-200 * j, RandomRange(-10, 40)),
                    texture = "bullets/GooberHelper/arrow",
                    scale = 0.5,
                    color = Hsv(i / 8 * -180, 1, 1)
                }
            end

            coroutine.yield(spb / 4)
        end

        if k % 2 == 1 and k ~= 7 then
            local angleOffset = RandomRange(0, 100)

            for i = 1, 360, 45 do
                for j = -1, 1, 2 do
                    for l = -1, 1, 2 do
                        local pos = Vector2(j * 100, -100 - k * 20)
                        local vel = Angle(i + math.atan(Calc.Angle(TowardsPlayer(pos))) + angleOffset) * Ternary(l == -1, 100, 80)

                        Shoot{
                            position = pos,
                            velocity = vel,
                            acceleration = Vector2(-vel.Y, vel.X) * l,
                            texture = "bullets/GooberHelper/halo",
                            scale = 0.5,
                            additive = true,
                            color = Hsv(i / 360 * 30 + 120, 1, 1)
                        }
                    end
                end
            end
        end
    end

    for i = 0, 15, 1 do
        for j = -1, 1, 2 do
            local center = Vector2(j * 120, -i * 10 + -200)

            starAt(center, nil)

            coroutine.yield(spb)
        end
    end

    -- coroutine.yield(0)

    -- while true do
    --     Shoot{
    --         position = Vector2(0, 0),
    --         velocity = Vector2(200, 0),
    --         texture = "bullets/GooberHelper/lightning",
    --         scale = 1,
    --         color = Hsv(RandomRange(100, 260), RandomRange(0.4, 0.8), 1)
    --     }

    --     require("#Celeste.Audio").Play("event:/game/05_mirror_temple/redbooster_end")

    --     local a = require("#Celeste.Mod.GooberHelper.SyncedMusicHelper").GetTimelinePosition() % 612.24489

    --     if a > 300 then
    --         a = a - 612.24489
    --     end

    --     print(a)

    --     coroutine.yield(spb)
    -- end

    for i = 1, 15, 1 do
        local center = Vector2(RandomRange(-120, 120), RandomRange(-50, 0))

        starAt(center, nil)

        coroutine.yield(spb)
    end

    local bulletTemplate = CreateBulletTemplate{
        texture = "bullets/GooberHelper/fish",
        scale = 0.5,
        color = Hsv(RandomRange(0, 360), RandomRange(0.4, 0.8), 1),
    }

    local smallerBulletTemplate = CreateBulletTemplate{
        effect = "coolBullet",
        additive = true
    }

    for i = 1, 16, 1 do
        starAt(Vector2(0,0), bulletTemplate + smallerBulletTemplate)

        coroutine.yield(spb * 3)
        
        starAt(Vector2(-100, 0), nil)
        
        coroutine.yield(spb)

        starAt(Vector2(100, 0), nil)

        coroutine.yield(spb * 4)
    end

    -- local buhhlet = Shoot{
    --     position = Vector2(0, 0),
    --     velocity = Vector2(0, 0),
    --     texture = "bullets/GooberHelper/lightning",
    --     scale = 1,
    --     color = Hsv(RandomRange(100, 260), RandomRange(0.4, 0.8), 1),
    -- }

    -- buhhlet.RotationMode = BulletRotationMode.None
    
    -- coroutine.yield(0.2)

    -- for i = 1, 2, 1 do
    --     buhhlet:InterpolateValue("Position", RandomDirection() * 50, 0.5, Ease.SineInOut);
    --     buhhlet:InterpolateValue("Velocity", RandomDirection() * 50, 0.5, Ease.SineInOut);
    --     buhhlet:InterpolateValue("Acceleration", RandomDirection() * 50, 0.5, Ease.SineInOut);
    --     buhhlet:InterpolateValue("Scale", RandomRange(0.5, 4.0), 0.5, Ease.BounceOut);

    --     coroutine.yield(0.6)
    -- end
    
    -- local bulletTemplate = CreateBulletTemplate{
    --     texture = "bullets/GooberHelper/fish",
    --     scale = 0.5,
    --     color = Hsv(RandomRange(0, 360), RandomRange(0.4, 0.8), 1),
    -- }

    -- local smallerBulletTemplate = CreateBulletTemplate{
    --     effect = "coolBullet",
    --     additive = true
    -- }
    
    -- while true do
    --     for i = 1, 360, 10 do
    --         Shoot{
    --             template = bulletTemplate + smallerBulletTemplate,
    --             position = buhhlet.Position,
    --             velocity = Angle(i + RandomRange(-25, 25)) * RandomRange(400, 600),
    --         }
    --     end

    --     coroutine.yield(0.05)
    -- end

    -- print(buhhlet.position)

    -- while true do
    --     -- buhhlet.Position = buhhlet.Position - 

    --     coroutine.yield(0.2)
    -- end

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

    -- local primary = AddCoroutine(PrimaryPattern)
    -- local secondary = AddCoroutine(SecondaryPattern)

    -- coroutine.yield(10)

    -- primary:RemoveSelf()
    -- secondary:RemoveSelf()

    -- coroutine.yield(1)

    -- local smite = AddCoroutine(SmitePattern)
end