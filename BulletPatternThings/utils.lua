_G.Celeste = require("#Celeste")
_G.Calc = require("#Monocle.Calc")
_G.Random = require("#System.Random")
_G.SyncedMusicHelper = require("#Celeste.Mod.GooberHelper.SyncedMusicHelper")

---@class BulletData
    ---@field velocity Vector2?
    ---@field acceleration Vector2?
    ---@field color Color?
    ---@field texture string?
    ---@field scale number?
    ---@field effect string?
    ---@field additive boolean?
    ---@field lowResolution boolean?
    ---@field rotation number?
    ---@field colliderRadius number?
    ---@field rotationMode BulletRotationMode?

---@class BulletCreationData : BulletData
    ---@field position Vector2?
    ---@field template BulletTemplate?


---@return table
---@param path string
_G.PlaySyncedMusic = function(path)
    return SyncedMusicHelper.PlaySyncedMusic(path)
end

---@return Vector2
_G.RandomDirection = function()
    return Angle(Calc.NextFloat(Random.Shared) * 360)
end

---@return number
---@param a number
---@param b number
_G.RandomRange = function(a, b)
    return Calc.NextFloat(Random.Shared) * (b - a) + a
end

---@param props BulletData
---@return BulletTemplate
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

---@return Bullet
---@param props BulletCreationData
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

---@return Vector2
---@param angle number
_G.Angle = function(angle)
    return Calc.AngleToVector(angle / 180 * math.pi, 1)
end

---@return Color
---@param h number
---@param s number
---@param v number
_G.Hsv = function(h, s, v)
    return Calc.HsvToColor((((h / 360) % 1) + 1) % 1, s, v)
end

---@return Vector2
_G.GetPlayerPosition = function()
    return Player.Position - Parent.BulletFieldCenter
end

---@return Vector2
---@param position Vector2
_G.TowardsPlayer = function(position)
    return Calc.SafeNormalize(GetPlayerPosition() - position)
end

---@param co function
_G.AddCoroutine = function(co)
    return Parent:AddLuaCoroutine(Celeste.Mod.LuaCoroutine({value = coroutine.create(co), resume = ThreadProxyResume}))
end