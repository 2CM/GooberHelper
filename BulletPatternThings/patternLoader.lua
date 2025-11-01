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
        return success, error(message)
    end

    return success, message
end

function loader.readFile(name)
    return require("#Celeste.Mod.GooberHelper").LuaHelper.GetFileContent(name)
end

function loader.loadFile(path)
    local utilsFile = loader.readFile(path)
    
    local chunk = load(utilsFile)

    if chunk then
        pcall(chunk)
    end
end

function loader.load(name, props)
    -- for key, value in pairs(package.loaded) do
    --     if key:sub(1, 1) == "#" then
    --         package.loaded[key] = nil;

    --         print("cleared " .. key)
    --     end
    -- end

    local patternFile = loader.readFile(name)

    local chunk = load(patternFile)

    _G.Parent = props.Parent
    _G.Player = props.Player
    _G.Level = props.Level

    loader.loadFile("BulletPatternThings/utils")
    loader.loadFile("BulletPatternThings/types")

    print("iuiuii")
    
    if chunk then
        local res = pcall(chunk)
        
        if res then
            AddCoroutine(_G.Run)
        end
    end
end

return loader