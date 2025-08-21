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

local options = {
    "====== JUMPING ======",
    "JumpInversion",
    "AllowClimbJumpInversion",
    "WallJumpSpeedPreservation",
    "GetClimbJumpSpeedInRetainedFrames",
    "AllowHoldableClimbjumping",
    "AdditiveVerticalJumpSpeed",
    "AllDirectionHypersAndSupers",
    "AllDirectionHypersAndSupersWorkWithCoyoteTime",
    "AllowUpwardsCoyote",
    "AllDirectionDreamJumps",
    "UpwardsJumpSpeedPreservation",
    "DownwardsJumpSpeedPreservation",
    "HyperAndSuperSpeedPreservation",
    "CornerboostBlocksEverywhere",
    "SwapHorizontalAndVerticalSpeedOnWallJump",
    "VerticalSpeedToHorizontalSpeedOnGroundJump",
    "WallbounceSpeedPreservation",
    "WallJumpSpeedInversion",
    "====== DASHING ======",
    "KeepDashAttackOnCollision",
    "ReverseDashSpeedPreservation",
    "VerticalDashSpeedPreservation",
    "MagnitudeBasedDashSpeed",
    "MagnitudeBasedDashSpeedOnlyCardinal",
    "DashesDontResetSpeed",
    "====== MOVING ======",
    "CobwobSpeedInversion",
    "AllowRetentionReverse",
    "WallBoostDirectionBasedOnOppositeSpeed",
    "WallBoostSpeedIsAlwaysOppositeSpeed",
    "KeepSpeedThroughVerticalTransitions",
    "HorizontalTurningSpeedInversion",
    "VerticalTurningSpeedInversion",
    "DownwardsAirFrictionBehavior",
    "====== OTHER ======",
    "ReboundInversion",
    "RefillFreezeLength: [number]",
    "DreamBlockSpeedPreservation",
    "SpringSpeedPreservation",
    "CustomFeathers",
    "FeatherEndSpeedPreservation",
    "ExplodeLaunchSpeedPreservation",
    "BadelineBossSpeedReversing",
    "AlwaysActivateCoreBlocks",
    "CustomSwimming",
    "RetentionFrames: [number]",
    "RemoveNormalEnd",
    "PickupSpeedReversal",
    "BubbleSpeedPreservation",
    "LenientStunning",
    "AllowCrouchedHoldableGrabbing",
    "HoldablesInheritSpeedWhenThrown",
    "====== VISUALS ======",
    "PlayerMask",
    "PlayerMaskHairOnly",
    "TheoNuclearReactor",
    "====== MISCELLANEOUS ======",
    "AlwaysExplodeSpinners",
    "GoldenBlocksAlwaysLoad",
    "Ant",
    "====== GENERAL ======",
    "ShowActiveSettings",
}

local numberFieldStartIndices, realOptions = (function()
    local starts = {};
    local reals = {};

    for index, value in ipairs(options) do
        if value:sub(-8, -1) == "[number]" then
            starts[value:sub(0, 5)] = value:find(":") + 1 -- sub(0, 5) is an arbitrary identifier
        end

        if value:sub(1, 1) ~= "=" then
            reals[value] = true;
        end
    end

    return starts, reals
end)()

local buhbu = {
    fieldType = "list",
    elementDefault = "JumpInversion",
    elementSeparator = ",",
    elementOptions = {
        width = 500,
        minWidth = 500,
        fieldType = "string",
        options = options,
        validator = function(input)
            local start = numberFieldStartIndices[input:sub(0, 5)];

            if start ~= nil then
                return tonumber(input:sub(start, -1)) ~= nil
            end

            return realOptions[input] == true
        end
    }
}

trigger.fieldInformation = {
    enable = buhbu,
    disable = buhbu,
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