using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Monocle;
using static Celeste.Mod.GooberHelper.GooberHelperModule;

namespace Celeste.Mod.GooberHelper {

    //who up reworking they helper
    public static class OptionsManager {
        private static Color DefaultColor = Color.White;
        private static Color MapDefinedColor = Color.DeepSkyBlue;
        private static Color UserDefinedColor = new Color(0.5f,0.5f,1f,0.2f);

        public class OptionData {
            public Option Id;
            public string Name;
            public OptionType Type;
            public string Category;
            public float DefaultValue;
            public float Min = 0;
            public float Max = 100;
            public float Step = 1;
            public string Suffix;

            public OptionData(Option option, OptionType type = OptionType.Boolean, float defaultValue = 0) {
                this.Id = option;
                this.Name = Enum.GetName(typeof(Option), option);
                this.Type = type;
                this.DefaultValue = defaultValue;
            }
        }

        public class OptionsProfile {
            public string Name;
            public Dictionary<Option, float> UserDefinedOptions;

            public OptionsProfile() {}

            public OptionsProfile(string name, Dictionary<Option, float> userDefinedOptions) {
                this.Name = name;
                this.UserDefinedOptions = userDefinedOptions;
            }

            public static void Create(string name) {
                GooberHelperModule.Settings.OptionsProfiles[name] = new OptionsProfile(name, GooberHelperModule.Settings.UserDefinedOptions.ToDictionary());
                GooberHelperModule.Settings.OptionsProfileOrder.Add(name);
            }

            public static OptionsProfile CreateFromImport() {
                OptionsProfile deserializedProfile = Deserialize(TextInput.GetClipboardText());

                deserializedProfile.Name = Utils.PreventNameCollision(deserializedProfile.Name, GooberHelperModule.Settings.OptionsProfiles);

                GooberHelperModule.Settings.OptionsProfiles[deserializedProfile.Name] = deserializedProfile;
                GooberHelperModule.Settings.OptionsProfileOrder.Add(deserializedProfile.Name);

                return deserializedProfile;
            }

            public static void Load(string name) {
                GooberHelperModule.Settings.UserDefinedOptions = GooberHelperModule.Settings.OptionsProfiles[name].UserDefinedOptions.ToDictionary();
            }

            public static void Save(string name) {
                GooberHelperModule.Settings.OptionsProfiles[name].UserDefinedOptions = GooberHelperModule.Settings.UserDefinedOptions.ToDictionary();
            }

            public static void Rename(string from, string to) {
                if(from == to) return;

                GooberHelperModule.Settings.OptionsProfiles[to] = GooberHelperModule.Settings.OptionsProfiles[from];
                GooberHelperModule.Settings.OptionsProfiles.Remove(from);

                GooberHelperModule.Settings.OptionsProfiles[to].Name = to;

                GooberHelperModule.Settings.OptionsProfileOrder[GooberHelperModule.Settings.OptionsProfileOrder.IndexOf(from)] = to;
            }

            public static OptionsProfile Duplicate(string name, out int insertionIndex) {
                OptionsProfile duplicate = new OptionsProfile(
                    name: Utils.CreateCopyName(name, GooberHelperModule.Settings.OptionsProfiles, out string lastNameCollision),
                    userDefinedOptions: GooberHelperModule.Settings.OptionsProfiles[name].UserDefinedOptions.ToDictionary()
                );

                GooberHelperModule.Settings.OptionsProfiles[duplicate.Name] = duplicate;

                insertionIndex = GooberHelperModule.Settings.OptionsProfileOrder.IndexOf(lastNameCollision) + 1;
                GooberHelperModule.Settings.OptionsProfileOrder.Insert(insertionIndex, duplicate.Name);

                return duplicate;

                //* barely ever
            }

            public static void Export(string name) {
                TextInput.SetClipboardText(GooberHelperModule.Settings.OptionsProfiles[name].Serialize());
            }

            public static OptionsProfile Import(string name) {
                OptionsProfile deserializedProfile = Deserialize(TextInput.GetClipboardText());

                GooberHelperModule.Settings.OptionsProfiles[name].UserDefinedOptions = deserializedProfile.UserDefinedOptions;

                return deserializedProfile;
            }

            public static void Delete(string name) {
                GooberHelperModule.Settings.OptionsProfiles.Remove(name);
                GooberHelperModule.Settings.OptionsProfileOrder.Remove(name);
            }

            //this oop stuff is getting ridiculous
            public static bool GetExists(string name) {
                return GooberHelperModule.Settings.OptionsProfiles.ContainsKey(name);
            }

            public string Serialize() {
                List<byte> data = [];
                byte[] nameBytes = Encoding.UTF8.GetBytes(Name);

                for(int i = 0; i < nameBytes.Length; i++) {
                    data.Add(nameBytes[i]);
                }

                data.Add(0); //null termination

                //i shouldve just used a binary writer for this but it works and i dont want to redo it
                //i didnt know those existed when writing this lmao
                foreach(var pair in UserDefinedOptions) {
                    byte[] keyBytes = BitConverter.GetBytes((ushort)pair.Key);
                    byte[] valueBytes = BitConverter.GetBytes(pair.Value);
                    
                    data.Add(keyBytes[0]);
                    data.Add(keyBytes[1]);
                    data.Add(valueBytes[0]);
                    data.Add(valueBytes[1]);
                    data.Add(valueBytes[2]);
                    data.Add(valueBytes[3]);

                    Console.WriteLine("b");
                }

                using(var compressedStream = new MemoryStream()) {
                    using(var gzipStream = new GZipStream(compressedStream, CompressionLevel.Fastest)) {
                        gzipStream.Write(data.ToArray());
                        gzipStream.Close();

                        return Convert.ToBase64String(compressedStream.ToArray());
                    }
                }
            }

            public static OptionsProfile Deserialize(string str) {
                OptionsProfile profile = new("", new Dictionary<Option, float>());
                byte[] data;

                using(var compressedStream = new MemoryStream(Convert.FromBase64String(str))) {
                    using(var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress)) {
                        using(var resultsStream = new MemoryStream()) {
                            gzipStream.CopyTo(resultsStream);
                            gzipStream.Close();

                            data = resultsStream.ToArray();
                        }
                    }
                }

                int stringLength = Array.IndexOf(data, (byte)0);

                if(stringLength == -1) {
                    throw new Exception("couldnt find the null termination of the options profile's name");
                }

                profile.Name = Encoding.UTF8.GetString(data, 0, stringLength);

                for(int i = stringLength + 1; i < data.Length; i += 6) {
                    Option key = (Option)BitConverter.ToUInt16(data, i);
                    float value = BitConverter.ToSingle(data, i + 2);

                    profile.UserDefinedOptions[key] = value;
                }

                return profile;
            }
        }

        public class OptionChanges {
            public Dictionary<Option, float> Enable;
            public Dictionary<Option, float> Disable;
            public bool ResetAll;

            public OptionChanges(EntityData data) {
                this.Enable = ParseOptionsString(data.Attr("enable"));
                this.Disable = ParseOptionsString(data.Attr("disable"));
                this.ResetAll = data.Bool("resetAll");
            }

            public static Dictionary<Option, float> ParseOptionsString(string str) {
                Dictionary<Option, float> options = new();

                if(str.Length == 0) return options;

                foreach(string assignment in str.Split(",")) {
                    string[] pair = assignment.Split(":");
                    string key = pair[0];
                    float value = pair.Length > 1 ? float.Parse(pair[1]) : 1;

                    if(Enum.TryParse(typeof(Option), key, false, out object option)) {
                        options[(Option)option] = value;
                    } else {
                        Logger.Log(LogLevel.Warn, "GooberHelper", $"Failed to parse {key} as an option!");
                    }
                }

                return options;
            }

            public void Apply() {
                if(ResetAll) {
                    ResetAll(OptionSetter.Map);
                } else {
                    foreach(var pair in Disable) {
                        ResetOptionValue(pair.Key, OptionSetter.Map);
                    }
                }

                foreach(var pair in Enable) {
                    SetOptionValue(pair.Key, pair.Value, OptionSetter.Map);
                }
            }
        }

        public enum OptionSetter {
            None,
            Map,
            User
        }

        
        public enum OptionType {
            Boolean,
            Integer,
            Float,
        }

        public static float GetOptionValue(Option option) {
            return 
                GooberHelperModule.Settings.UserDefinedOptions.TryGetValue(option, out float userValue) ? userValue :
                GooberHelperModule.Session.MapDefinedOptions.TryGetValue(option, out float mapValue) ? mapValue :
                Options[option].DefaultValue;
        }

        //i would just reference GetOptionValue here but that would be a few extra instructions and im really cautious about performance stuff
        //the compiler would probably inline it but whatever
        public static bool GetOptionBool(Option option) {
            return
                GooberHelperModule.Settings.UserDefinedOptions.TryGetValue(option, out float userValue) ? userValue >= 1 :
                GooberHelperModule.Session.MapDefinedOptions.TryGetValue(option, out float mapValue) ? mapValue >= 1 :
                Options[option].DefaultValue == 1;
        }

        public static OptionSetter GetOptionSetter(Option option) {
            bool isDefinedByUser = GooberHelperModule.Settings.UserDefinedOptions.ContainsKey(option);
            bool isDefinedByMap = GooberHelperModule.Session.MapDefinedOptions.ContainsKey(option);

            return
                isDefinedByUser ? OptionSetter.User :
                isDefinedByMap ? OptionSetter.Map :
                OptionSetter.None;
        }

        public static Color GetOptionColor(Option option) {
            OptionSetter optionSetter = GetOptionSetter(option);

            return 
                optionSetter == OptionSetter.User ? UserDefinedColor :
                optionSetter == OptionSetter.Map ? MapDefinedColor :
                DefaultColor;
        }

        public static float GetOptionMapDefinedValueOrDefault(Option option) {
            return GooberHelperModule.Session.MapDefinedOptions.TryGetValue(option, out float value) ? value : Options[option].DefaultValue;
        }

        public static string GetEnabledOptionsString() {
            string str = "";

            foreach(KeyValuePair<Option, OptionData> pair in Options) {
                if(GetOptionSetter(pair.Key) != OptionSetter.None) {
                    str += $"{pair.Value.Name}: {(pair.Value.Type == OptionType.Boolean ? GetOptionBool(pair.Key).ToString() : GetOptionValue(pair.Key).ToString() + pair.Value.Suffix)}\n";
                }
            }

            return str;
        }

        public static bool SetOptionValue(Option option, float value, OptionSetter setter) {
            if(setter == OptionSetter.User) {
                GooberHelperModule.Settings.UserDefinedOptions[option] = value;
                float sessionValue = GooberHelperModule.Session.MapDefinedOptions.TryGetValue(option, out float v) ? v : Options[option].DefaultValue;

                if(value == sessionValue) {
                    GooberHelperModule.Settings.UserDefinedOptions.Remove(option);

                    return true;
                }
            } else if(setter == OptionSetter.Map) {
                GooberHelperModule.Session.MapDefinedOptions[option] = value;

                if(value == Options[option].DefaultValue) {
                    GooberHelperModule.Session.MapDefinedOptions.Remove(option);

                    return true;
                }
            }

            return false;
        }

        public static void ResetOptionValue(Option option, OptionSetter setter) {
            if(setter == OptionSetter.User) {
                GooberHelperModule.Settings.UserDefinedOptions.Remove(option);
            } else if(setter == OptionSetter.Map) {
                GooberHelperModule.Session.MapDefinedOptions.Remove(option);
            }
        }

        public static void ResetCategory(string category, OptionSetter setter) {
            if(setter == OptionSetter.User) {
                foreach(OptionData optionData in Categories[category]) {
                    GooberHelperModule.Settings.UserDefinedOptions.Remove(optionData.Id);
                }
            }
        }

        public static Color GetCategoryColor(string category) {
            Color color = DefaultColor;

            if(!Categories.ContainsKey(category)) return color;

            foreach(OptionData optionData in Categories[category]) {
                if(GooberHelperModule.Settings.UserDefinedOptions.ContainsKey(optionData.Id)) return UserDefinedColor;
                if(GooberHelperModule.Session.MapDefinedOptions.ContainsKey(optionData.Id)) color = MapDefinedColor;
            }

            return color;
        }

        public static void ResetAll(OptionSetter setter) {
            if(setter == OptionSetter.User) {
                GooberHelperModule.Settings.UserDefinedOptions.Clear();
            }
        }

        public static Color GetGlobalColor() {
            return
                GooberHelperModule.Settings.UserDefinedOptions.Count > 0 ? UserDefinedColor :
                GooberHelperModule.Session.MapDefinedOptions.Count > 0 ? MapDefinedColor :
                DefaultColor;
        }

        [Command("goob", "")]
        public static void CmdGoob() {
            Engine.Commands.Log("Session.MapDefinedOptions:");
            foreach(var pair in GooberHelperModule.Session.MapDefinedOptions) {
                Engine.Commands.Log($"- {pair.Key}: {pair.Value}");
            }

            Engine.Commands.Log("Settings.UserDefinedOptions:");
            foreach(var pair in GooberHelperModule.Settings.UserDefinedOptions) {
                Engine.Commands.Log($"- {pair.Key}: {pair.Value}");
            }
        }

        //maybe sort these chronologically?
        public enum Option {
            //jumping
            JumpInversion,
            AllowClimbJumpInversion,
            WallJumpSpeedPreservation,
            GetClimbJumpSpeedInRetainedFrames,
            AllowHoldableClimbjumping,
            AdditiveVerticalJumpSpeed,
            WallJumpSpeedInversion,
            UpwardsJumpSpeedPreservation,
            DownwardsJumpSpeedPreservation,
            AllDirectionDreamJumps,
            SwapHorizontalAndVerticalSpeedOnWallJump,
            VerticalSpeedToHorizontalSpeedOnGroundJump,
            AllDirectionHypersAndSupers,
            AllDirectionHypersAndSupersWorkWithCoyoteTime,
            AllowUpwardsCoyote,
            CornerboostBlocksEverywhere,
            WallbounceSpeedPreservation,
            HyperAndSuperSpeedPreservation,

            //dashing
            KeepDashAttackOnCollision,
            VerticalDashSpeedPreservation,
            ReverseDashSpeedPreservation,
            MagnitudeBasedDashSpeed,
            MagnitudeBasedDashSpeedOnlyCardinal,
            DashesDontResetSpeed,

            //moving
            CobwobSpeedInversion,
            AllowRetentionReverse,
            WallBoostDirectionBasedOnOppositeSpeed,
            WallBoostSpeedIsAlwaysOppositeSpeed,
            KeepSpeedThroughVerticalTransitions,
            HorizontalTurningSpeedInversion,
            VerticalTurningSpeedInversion,
            DownwardsAirFrictionBehavior,

            //other
            RefillFreezeLength,
            RetentionFrames,
            ReboundInversion,
            DreamBlockSpeedPreservation,
            SpringSpeedPreservation,
            CustomFeathers,
            FeatherEndSpeedPreservation,
            ExplodeLaunchSpeedPreservation,
            BadelineBossSpeedReversing,
            AlwaysActivateCoreBlocks,
            CustomSwimming,
            RemoveNormalEnd,
            PickupSpeedReversal,
            BubbleSpeedPreservation,
            LenientStunning,
            AllowCrouchedHoldableGrabbing,
            HoldablesInheritSpeedWhenThrown,

            //visual
            PlayerMask,
            PlayerMaskHairOnly,
            TheoNuclearReactor,

            //miscellaneous
            AlwaysExplodeSpinners,
            GoldenBlocksAlwaysLoad,
            Ant,

            //general
            ShowActiveSettings
        }

        public static Dictionary<string, List<OptionData>> Categories = new() {
            { "Jumping", [
                //goodbye buhbu 💗 i will love you forever
                // new OptionData(Option.buhbu, OptionType.Float, 0) { min = 0, max = 10, growthFactor = 10, suffix = " frames" },
                // new OptionData(Option.zonmgle),
                // new OptionData(Option.zingle)
                new OptionData(Option.JumpInversion),
                new OptionData(Option.AllowClimbJumpInversion),
                new OptionData(Option.WallJumpSpeedPreservation),
                new OptionData(Option.GetClimbJumpSpeedInRetainedFrames),
                new OptionData(Option.AllowHoldableClimbjumping),
                new OptionData(Option.AdditiveVerticalJumpSpeed),
                new OptionData(Option.WallJumpSpeedInversion),
                new OptionData(Option.UpwardsJumpSpeedPreservation),
                new OptionData(Option.DownwardsJumpSpeedPreservation),
                new OptionData(Option.AllDirectionDreamJumps),
                new OptionData(Option.SwapHorizontalAndVerticalSpeedOnWallJump),
                new OptionData(Option.VerticalSpeedToHorizontalSpeedOnGroundJump),
                new OptionData(Option.AllDirectionHypersAndSupers),
                new OptionData(Option.AllDirectionHypersAndSupersWorkWithCoyoteTime),
                new OptionData(Option.AllowUpwardsCoyote),
                new OptionData(Option.CornerboostBlocksEverywhere),
                new OptionData(Option.WallbounceSpeedPreservation),
                new OptionData(Option.HyperAndSuperSpeedPreservation),
            ]},
            { "Dashing", [
                new OptionData(Option.KeepDashAttackOnCollision),
                new OptionData(Option.VerticalDashSpeedPreservation),
                new OptionData(Option.ReverseDashSpeedPreservation),
                new OptionData(Option.MagnitudeBasedDashSpeed),
                new OptionData(Option.MagnitudeBasedDashSpeedOnlyCardinal),
                new OptionData(Option.DashesDontResetSpeed),
            ]},
            { "Moving", [
                new OptionData(Option.CobwobSpeedInversion),
                new OptionData(Option.AllowRetentionReverse),
                new OptionData(Option.WallBoostDirectionBasedOnOppositeSpeed),
                new OptionData(Option.WallBoostSpeedIsAlwaysOppositeSpeed),
                new OptionData(Option.KeepSpeedThroughVerticalTransitions),
                new OptionData(Option.HorizontalTurningSpeedInversion),
                new OptionData(Option.VerticalTurningSpeedInversion),
                new OptionData(Option.DownwardsAirFrictionBehavior),
            ]},
            { "Other", [
                new OptionData(Option.RefillFreezeLength, OptionType.Float, 3) { Min = 0, Max = 10000, Step = 1, Suffix = " frames" },
                new OptionData(Option.RetentionFrames, OptionType.Float, 4) { Min = 0, Max = 10000, Step = 1, Suffix = " frames" },
                new OptionData(Option.ReboundInversion),
                new OptionData(Option.DreamBlockSpeedPreservation),
                new OptionData(Option.SpringSpeedPreservation),
                new OptionData(Option.CustomFeathers),
                new OptionData(Option.FeatherEndSpeedPreservation),
                new OptionData(Option.ExplodeLaunchSpeedPreservation),
                new OptionData(Option.BadelineBossSpeedReversing),
                new OptionData(Option.AlwaysActivateCoreBlocks),
                new OptionData(Option.CustomSwimming),
                new OptionData(Option.RemoveNormalEnd),
                new OptionData(Option.PickupSpeedReversal),
                new OptionData(Option.BubbleSpeedPreservation),
                new OptionData(Option.LenientStunning),
                new OptionData(Option.AllowCrouchedHoldableGrabbing),
                new OptionData(Option.HoldablesInheritSpeedWhenThrown),
            ]},
            { "Visuals", [
                new OptionData(Option.PlayerMask),
                new OptionData(Option.PlayerMaskHairOnly),
                new OptionData(Option.TheoNuclearReactor),
            ]},
            { "Miscellaneous", [
                new OptionData(Option.AlwaysExplodeSpinners),
                new OptionData(Option.GoldenBlocksAlwaysLoad),
                new OptionData(Option.Ant),
            ]},
            { "General", [
                new OptionData(Option.ShowActiveSettings),
            ]},
        };

        private static Dictionary<Option, OptionData> createOptionsFromCategories() {
            Dictionary<Option, OptionData> dict = [];

            foreach(KeyValuePair<string, List<OptionData>> pair in Categories) {
                foreach(OptionData option in pair.Value) {
                    dict[option.Id] = option;

                    option.Category = pair.Key;
                }
            }

            return dict;
        }

        public static Dictionary<Option, OptionData> Options = createOptionsFromCategories();
    }
}