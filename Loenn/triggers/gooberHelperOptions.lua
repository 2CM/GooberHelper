local trigger = {}

trigger.name = "GooberHelper/GooberHelperOptions"
trigger.placements = {
    name = "gooberHelperOptions",
    data = {
        enable = "",
        disable = "",
        flag = "",
        notFlag = "",
        revertOnLeave = false,
        revertOnDeath = false,
        resetAll = false,
    }
}

local function categoryHeader(name)
    return "====== " .. name:upper() .. " ======"
end

local options = {
    categoryHeader("Jumping"),
    "JumpInversion: [None|GroundJumps|All]",
    "WalljumpSpeedPreservation: [None|FakeRCB|Preserve|Invert]",
    "WallbounceSpeedPreservation",
    "HyperAndSuperSpeedPreservation: [DashSpeed|None|number]",
    "UpwardsJumpSpeedPreservationThreshold: [DashSpeed|None|number]",
    "DownwardsJumpSpeedPreservationThreshold",

    "GetClimbjumpSpeedInRetention",
    "AdditiveVerticalJumpSpeed",
    "SwapHorizontalAndVerticalSpeedOnWalljump",
    "VerticalToHorizontalSpeedOnGroundJump: [None|Vertical|Magnitude]",
    "CornerboostBlocksEverywhere",

    "AllDirectionHypersAndSupers: [None|RequireGround|WorkWithCoyoteTime]",
    "AllowUpwardsCoyote",
    "AllDirectionDreamJumps",
    "AllowHoldableClimbjumping",

    categoryHeader("Dashing"),
    "VerticalDashSpeedPreservation",
    "ReverseDashSpeedPreservation",

    "MagnitudeBasedDashSpeed: [None|OnlyCardinal|All]",

    "DashesDontResetSpeed: [None|Legacy|On]",
    "KeepDashAttackOnCollision",

    categoryHeader("Moving"),
    "CobwobSpeedInversion: [None|RequireSpeed|WorkWithRetention]",

    "WallboostDirectionIsOppositeSpeed",
    "WallboostSpeedIsOppositeSpeed",
    "HorizontalTurningSpeedInversion",
    "VerticalTurningSpeedInversion",
    "DownwardsAirFrictionBehavior",

    "UpwardsTransitionSpeedPreservation",

    categoryHeader("Other"),
    "RefillFreezeLength: [number]",
    "RetentionLength: [number]",

    "DreamBlockSpeedPreservation: [None|Horizontal|Vertical|Both|Magnitude]",
    "SpringSpeedPreservation: [None|Preserve|Invert]",
    "ReboundSpeedPreservation",
    "ExplodeLaunchSpeedPreservation",
    "PickupSpeedInversion",
    "BubbleSpeedPreservation",
    "FeatherEndSpeedPreservation",
    "BadelineBossSpeedPreservation",

    "CustomFeathers: [None|KeepIntro|SkipIntro]",
    "CustomSwimming",
    "RemoveNormalEnd",
    "LenientStunning",
    "HoldablesInheritSpeedWhenThrown",

    "AllowCrouchedHoldableGrabbing",
    "AllowUpwardsClimbGrabbing",
    "AllowCrouchedClimbGrabbing",
    "ClimbingSpeedPreservation",
    "AllowClimbingInDashState",
    "CoreBlockAllDirectionActivation",
    "LiftBoostAdditionHorizontal: [number]",
    "LiftBoostAdditionVertical: [number]",

    categoryHeader("Visual"),
    "PlayerShaderMask: [None|Cover|HairOnly]",
    "TheoNuclearReactor",
    
    categoryHeader("Miscellaneous"),
    "AlwaysExplodeSpinners",
    "GoldenBlocksAlwaysLoad",
    "RefillFreezeGameSuspension",
    "BufferDelayVisualization",
    "Ant",
    
    categoryHeader("General"),
    "ShowActiveOptions",
}

local disableOptions = {};
local numberFieldStartIndices = {};
local enumFields = {};
local isOption = {};
local isDisableOption = {};

local specialInputFields = {};

for index, value in ipairs(options) do
    local disableOptionName = value;

    if value:sub(-1, -1) == "]" then
        local field = {
            options = {},
            canBeNumber = false
        }

        local splitter = value:find(":");
        local optionKey = value:sub(1, splitter - 1);
        local optionEnumContent = value:sub(splitter + 3, -2);

        for str in string.gmatch(optionEnumContent, "([^|]+)") do
            local id = str:lower()
            
            if id == "number" then
                field.canBeNumber = true;
            else
                field.options[id] = true;
            end
        end

        specialInputFields[optionKey] = field

        disableOptionName = optionKey
    else
        if value:sub(1, 1) ~= "=" then
            isOption[value] = true;
        end
    end

    table.insert(disableOptions, disableOptionName);
    isDisableOption[disableOptionName] = true;
end

-- function dump(obj, indent)
--     indent = indent or ""
--     if type(obj) == "table" then
--         for k, v in pairs(obj) do
--             if type(v) == "table" and v ~= obj then -- Avoid infinite recursion for self-referencing tables
--                 print(indent .. tostring(k) .. ":")
--                 dump(v, indent .. "  ")
--             else
--                 print(indent .. tostring(k) .. ": " .. tostring(v))
--             end
--         end
--     else
--         print(indent .. tostring(obj))
--     end
-- end

-- dump(isOption);
-- dump(specialInputFields);

local function createOptionsField(fieldOptions, validator)
    return {
        fieldType = "list",
        elementDefault = "",
        elementSeparator = ",",
        elementOptions = {
            width = 500,
            minWidth = 500,
            fieldType = "string",
            options = fieldOptions,
            validator = validator,
            searchable = true
        }
    }
end

trigger.fieldInformation = {
    enable = createOptionsField(
        options,
        function(input)
            if #input == 0 then return true end

            local splitter = input:find(":") or 1;
            local valueData = specialInputFields[input:sub(1, splitter - 1)];

            if valueData == nil then
                return isOption[input] == true;
            end

            if input:sub(splitter + 1, splitter + 1) == " " then
                splitter = splitter + 1
            end

            if valueData.canBeNumber and tonumber(input:sub(splitter + 1, -1)) ~= nil then
                return true;
            end
            
            return valueData.options[input:sub(splitter + 1, -1):lower()] ~= nil;
        end
    ),
    disable = createOptionsField(
        disableOptions,
        function(input)
            if #input == 0 then return true end

            return isDisableOption[input] == true;
        end
    ),
    flag = {
        fieldType = "string"
    },
    notFlag = {
        fieldType = "string"
    },
    revertOnLeave = {
        fieldType = "boolean"
    },
    revertOnDeath = {
        fieldType = "boolean"
    },
    resetAll = {
        fieldType = "boolean"
    },
}

-- trigger.triggerText = function(room, tuh) 
--     return string.gsub(string.gsub(tuh.enable, "[ %-] ", ""), ",", "\n");
-- end

trigger.fieldOrder = {
    "x",
    "y",
    "width",
    "height",
    "enable",
    "disable",
    "flag",
    "notFlag",
    "revertOnLeave",
    "revertOnDeath",
    "resetAll",
}

return trigger

-- local displayOptionsList = {};
-- local realOptionsList = {};
-- local isRealOption = {};
-- local numberFieldStartIndices = {};

-- local function generateOptions(data)
--     local function recur(obj, prefix)
--         if type(obj[1]) == "table" then
--             for index, value in ipairs(obj) do
--                 local display = prefix .. value.name .. ":";

--                 table.insert(displayOptionsList, #displayOptionsList + 1, display);

--                 recur(value.content, prefix .. "  ");
--             end
--         else
--             table.sort(obj)
            
--             for index, value in ipairs(obj) do
--                 local display = prefix .. "- " .. value;

--                 table.insert(displayOptionsList, #displayOptionsList + 1, display);
--                 table.insert(realOptionsList, #realOptionsList + 1, display);
--                 isRealOption[display] = true;

--                 if string.sub(value, -8, -1) == "[number]" then
--                     numberFieldStartIndices[string.sub(display, 2, 10)] = string.len(display) - 8;
--                 end
--             end
--         end
--     end

--     recur(data, "");
-- end

-- generateOptions(options);

-- local function print(data) for key, value in pairs(data) do print(tostring(key) .. ": ".. tostring(value)) end end

            -- valueTransformer = function(input)
            --     local buh = require("ui.windows.selection_context_window");

            --     if buh == nil then
            --         print("nillll");

            --         return;
            --     end

            --     Buh = buh;

            --     for index, value in pairs(buh) do
            --        print(tostring(index) .. ": ".. tostring(value)); 
            --     end
                
            --     return input;
            -- end,

            -- enable = {
    --     fieldType = "list",
    --     elementDefault = "zingle",
    --     elementSeparator = " ",
    --     elementOptions = {
    --         fieldType = "list",
    --         elementDefault = "-",
    --         elementSeparator = ":",
    --         minimumElements = 1,
    --         maximumElements = 2,
    --         elementOptions = {
    --             fieldType = "string",
    --             options = options
    --         }
    --     }
    -- },
    -- enable = {
    --     fieldType = "list",
    --     elementDefault = realOptionsList[1],
    --     elementSeparator = ",",
    --     elementOptions = {
    --         fieldType = "string",
    --         options = displayOptionsList,
    --         -- valueTransformer = function(input) return GooberHelperGeneratedOptions.optionValueMap[input] or "nil" end,
    --         -- displayTransformer = function(input) return GooberHelperGeneratedOptions.optionDisplayMap[input] or "nil" end,
    --         displayTransformer = function(input) print(input); return input; end,
    --         validator = function(input)
    --             local start = numberFieldStartIndices[string.sub(input, 2, 10)];

    --             if start ~= nil then
    --                 return tonumber(string.sub(input, start, -1)) ~= nil
    --             end

    --             return isRealOption[input] == true
    --         end
    --     }
    -- },