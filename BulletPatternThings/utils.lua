_G.Celeste = require("#Celeste")
_G.Vector2 = require("#Microsoft.Xna.Framework.Vector2")

_G.shoot = function(props)
    local bullet = Celeste.Mod.GooberHelper.Entities.Bullet(Parent, props.position)

    bullet.Velocity = props.velocity

    return bullet
end

_G.addCoroutine = function(co)
    print("iiee")
    print(Parent:GetType())

    local luaCoroutine = require("#Celeste.Mod").LuaCoroutine({value = coroutine.create(co), resume = ThreadProxyResume})

    print(luaCoroutine:GetType())

    return Parent:AddLuaCoroutine(luaCoroutine)
end