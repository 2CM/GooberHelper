local loader = {}

-- STOLEN FROM LUA CUTSCENES
function ThreadProxyResume(self, ...)
    local thread = self.value

    if coroutine.status(thread) == "dead" then
        return false, nil
    end

    local success, message = coroutine.resume(thread)

    -- The error message should be returned as an exception and not a string
    if not success then
        return success, require("#system.exception")(message)
    end

    return success, message
end

function loader.readFile(name)
    return require("#celeste.mod").GooberHelper.LuaHelper.GetFileContent(name)
end

function loader.loadUtil()
    local utilFile = loader.readFile("Patterns/util")
    local func = load(utilFile)

    local res = pcall(func);
end

function loader.load(name, parent)
    local patternFile = loader.readFile(name)
    local func = load(patternFile)

    _G.Parent = parent;

    loader.loadUtil();

    local res = pcall(func);

    if res then
        -- local corou = require("#celeste").mod.LuaCoroutine({value = coroutine.create(_G.Run), resume = ThreadProxyResume})

        -- Parent:AddLuaCoroutine(corou);
        AddCoroutine(_G.Run)
    end
end

return loader