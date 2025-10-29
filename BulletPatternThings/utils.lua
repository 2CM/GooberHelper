_G.Celeste = require("#Celeste")
_G.Vector2 = require("#Microsoft.Xna.Framework.Vector2")
_G.Color = require("#Microsoft.Xna.Framework.Color")
_G.Calc = require("#Monocle.Calc")
_G.Random = require("#System.Random")
_G.SyncedMusicHelper = require("#Celeste.Mod.GooberHelper.SyncedMusicHelper")
_G.Ease = require("#Monocle.Ease")

-- local Bullet = Celeste.Mod.GooberHelper.Entities.Bullet

_G.PlaySyncedMusic = function(path)
    return SyncedMusicHelper.PlaySyncedMusic(path)
end

_G.RandomDirection = function()
    return Angle(Calc.NextFloat(Random.Shared) * 360)
end

_G.RandomRange = function(a, b)
    return Calc.NextFloat(Random.Shared) * (b - a) + a
end

_G.CreateBulletTemplate = function(props)
    return Celeste.Mod.GooberHelper.BulletTemplate(
        props.velocity,
        props.acceleration,
        props.color,
        props.texture,
        props.scale,
        props.effect,
        props.additive,
        props.lowResolution,
        props.rotation,
        props.colliderRadius
    )
end

_G.Shoot = function(props)
    return Celeste.Mod.GooberHelper.Entities.Bullet(
        Parent,
        props.template,
        props.position,
        props.velocity,
        props.acceleration,
        props.color,
        props.texture,
        props.scale,
        props.effect,
        props.additive,
        props.lowResolution,
        props.rotation,
        props.colliderRadius
    )
end

_G.Angle = function(angle)
    return Calc.AngleToVector(angle / 180 * math.pi, 1)
end

_G.Hsv = function(h, s, v)
    return Calc.HsvToColor((((h / 360) % 1) + 1) % 1, s, v)
end

_G.GetPlayerPosition = function()
    return Player.Position - Parent.BulletFieldCenter
end

_G.TowardsPlayer = function(position)
    return Calc.SafeNormalize(GetPlayerPosition() - position)
end

_G.AddCoroutine = function(co)
    return Parent:AddLuaCoroutine(Celeste.Mod.LuaCoroutine({value = coroutine.create(co), resume = ThreadProxyResume}))
end