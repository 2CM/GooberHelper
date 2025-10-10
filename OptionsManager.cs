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
        public static Color DefaultColor = Color.White;
        public static Color MapDefinedColor = Color.DeepSkyBlue;
        public static Color UserDefinedEvilColor = new Color(0.5f,0.5f,1f,0.2f);
        public static Color UserDefinedCoolColor = new Color(1f,0.5f,0f,0.2f);

        public class OptionData {
            public Option Id;
            public string Name;
            public OptionType Type;
            public string Category;
            public float DefaultValue;
            public float Min = 0;
            public float Max = 100;
            public float Step = 1;
            public bool ExponentialIncrease = true;
            public string MaxLabel;
            public string Suffix;
            public Type EnumType;
            public float EnumMax;

            public OptionData(Option option, OptionType type = OptionType.Boolean, float defaultValue = 0) {
                this.Id = option;
                this.Name = Enum.GetName(typeof(Option), option);
                this.Type = type;
                this.DefaultValue = defaultValue;
            }

            public OptionData(Option option, Type enumType, OptionType type, float defaultValue) {
                this.Id = option;
                this.Name = Enum.GetName(typeof(Option), option);
                this.Type = type;
                this.EnumType = enumType;
                this.DefaultValue = defaultValue;
                this.EnumMax = Enum.GetValues(enumType).Length;
            }

            public OptionData(Option option, Type enumType, Enum defaultValue) {
                this.Id = option;
                this.Name = Enum.GetName(typeof(Option), option);
                this.EnumType = enumType;
                this.Type = OptionType.Enum;
                this.DefaultValue = Convert.ToSingle(defaultValue);
                this.EnumMax = Enum.GetValues(enumType).Length - 1;
            }

            public string GetDialogName() {
                return Dialog.Clean($"gooberhelper_option_{this.Name}");
            }

            public string GetDialogDescription() {
                string id = $"gooberhelper_option_description_{this.Name}";

                return Dialog.Has(id) ? Dialog.Clean(id) : "";
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
            public EntityID ID;

            public static Regex ParsingRegex = new Regex(@"(?<key>[A-Z|a-z]+)($|:(\s+)?(?<value>[-\w\.]+))");

            public OptionChanges() {}

            public OptionChanges(EntityData data) {
                this.ID = new EntityID(data.Level.Name, data.ID);

                this.Enable = ParseOptionsString(data.Attr("enable"));
                this.Disable = ParseOptionsString(data.Attr("disable"));
                this.ResetAll = data.Bool("resetAll");
            }

            public static Dictionary<Option, float> ParseOptionsString(string str) {
                Dictionary<Option, float> options = new();

                if(str.Length == 0) return options;

                foreach(string assignment in str.Split(",")) {
                    Match match = ParsingRegex.Match(assignment);

                    if(match.Success) {
                        if(!match.Groups.TryGetValue("key", out Group keyGroup)) {
                            Logger.Warn("GooberHelper", $"Weird assignment \"{assignment}\"");
                        }

                        if(!Enum.TryParse(keyGroup.Value, false, out Option option)) {
                            Logger.Warn("GooberHelper", $"Failed to parse {keyGroup.Value} as an option name!");
                        }

                        float value = 1;

                        if(match.Groups.TryGetValue("value", out Group valueGroup) && valueGroup.Success) {
                            string valueString = valueGroup.Value;

                            if(float.TryParse(valueString, out float floatValue)) {
                                value = floatValue;
                            } else if(Options[option].EnumType != null) {
                                if(Enum.TryParse(Options[option].EnumType, valueString, true, out object enumValue)) {
                                    value = (int)enumValue;
                                } else {
                                    Logger.Warn("GooberHelper", $"Failed to parse {valueString} as an option enum value!");
                                }
                            }
                        }

                        options[option] = value;
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

            public static void UpdateStack() {
                GooberHelperModule.Session.MapDefinedOptions.Clear();

                // Console.WriteLine("updating stack");

                foreach(var changes in GooberHelperModule.Session.Stack) {
                    changes.Apply();
                }
            }

            // public static OptionChanges GetEntityOptionChanges(EntityData data) {
            //     EntityID id = new(data.Level.Name, data.ID);

            //     if(GooberHelperModule.Session.EntityOptionChanges.TryGetValue(id, out OptionChanges value)) {
            //         return value;
            //     } else {
            //         OptionChanges changes = new OptionChanges(data);

            //         GooberHelperModule.Session.EntityOptionChanges[id] = changes;

            //         return changes;
            //     }
            // }
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
            Enum
        }

        public static float GetOptionValue(Option option) {
            return 
                GooberHelperModule.Settings.UserDefinedOptions.TryGetValue(option, out float userValue) ? userValue :
                GooberHelperModule.Session?.MapDefinedOptions.TryGetValue(option, out float mapValue) == true ? mapValue :
                Options[option].DefaultValue;
        }

        //i would just reference GetOptionValue here but that would be a few extra instructions and im really cautious about performance stuff
        //the compiler would probably inline it but whatever
        public static bool GetOptionBool(Option option) {
            return
                GooberHelperModule.Settings.UserDefinedOptions.TryGetValue(option, out float userValue) ? userValue >= 1 :
                GooberHelperModule.Session?.MapDefinedOptions.TryGetValue(option, out float mapValue) == true ? mapValue >= 1 :
                Options[option].DefaultValue == 1;
        }

        //stupid dumb scuffed c# code
        //this method is dumb dont use it
        public static T GetOptionEnum<T>(Option option) where T : Enum {
            return (T)Enum.ToObject(typeof(T),
                GooberHelperModule.Settings.UserDefinedOptions.TryGetValue(option, out float userValue) ? userValue :
                GooberHelperModule.Session?.MapDefinedOptions.TryGetValue(option, out float mapValue) == true ? mapValue :
                Options[option].DefaultValue);
        }

        public static string GetOptionEnumName(Option option) {
            Type type = Options[option].EnumType;

            float value = MathF.Floor(Math.Max(GetOptionValue(option), -Options[option].EnumMax));

            return Dialog.Clean($"gooberhelper_enum_{type.GetEnumName((int)(value > Options[option].Max ? 0 : value))}");
        }

        public static OptionSetter GetOptionSetter(Option option) {
            return
                GooberHelperModule.Settings.UserDefinedOptions.ContainsKey(option) ? OptionSetter.User :
                GooberHelperModule.Session?.MapDefinedOptions.ContainsKey(option) == true ? OptionSetter.Map :
                OptionSetter.None;
        }

        public static Color GetOptionColor(Option option) {
            OptionSetter optionSetter = GetOptionSetter(option);

            return 
                optionSetter == OptionSetter.User ? (option == Option.GoldenBlocksAlwaysLoad ? UserDefinedCoolColor : UserDefinedEvilColor) :
                optionSetter == OptionSetter.Map ? MapDefinedColor :
                DefaultColor;
        }

        public static float GetOptionMapDefinedValueOrDefault(Option option) {
            return GooberHelperModule.Session?.MapDefinedOptions.TryGetValue(option, out float value) == true ? value : Options[option].DefaultValue;
        }

        public static string GetEnabledOptionsString() {
            string str = "";

            foreach(KeyValuePair<Option, OptionData> pair in Options) {
                if(GetOptionSetter(pair.Key) != OptionSetter.None) {
                    str += $"{pair.Value.GetDialogName()}: {(
                        pair.Value.Type == OptionType.Boolean ? GetOptionBool(pair.Key).ToString() :
                        pair.Value.Type == OptionType.Enum || (pair.Value.EnumType != null && GetOptionValue(pair.Key) < 0) ? GetOptionEnumName(pair.Key).ToString() :
                        GetOptionValue(pair.Key).ToString() + pair.Value.Suffix)}\n";
                }
            }

            return str;
        }

        public static bool SetOptionValue(Option option, float value, OptionSetter setter) {
            if(setter == OptionSetter.User) {
                GooberHelperModule.Settings.UserDefinedOptions[option] = value;
                float neutralValue = GooberHelperModule.Session?.MapDefinedOptions.TryGetValue(option, out float v) == true ? v : Options[option].DefaultValue;

                if(value == neutralValue) {
                    GooberHelperModule.Settings.UserDefinedOptions.Remove(option);

                    return true;
                }
            } else if(setter == OptionSetter.Map && GooberHelperModule.Session != null) {
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
                GooberHelperModule.Session?.MapDefinedOptions.Remove(option);
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
                if(GooberHelperModule.Settings.UserDefinedOptions.ContainsKey(optionData.Id)) {
                    if(optionData.Id != Option.GoldenBlocksAlwaysLoad) return UserDefinedEvilColor;

                    color = UserDefinedCoolColor;
                }

                if(GooberHelperModule.Session?.MapDefinedOptions.ContainsKey(optionData.Id) == true && color == DefaultColor) color = MapDefinedColor;
            }

            return color;
        }

        public static void ResetAll(OptionSetter setter) {
            if(setter == OptionSetter.User) {
                GooberHelperModule.Settings.UserDefinedOptions.Clear();
            }
        }

        public static bool GetUserEnabledEvilOption() {
            return GooberHelperModule.Settings.UserDefinedOptions.Any(a =>
                Options[a.Key].Category != "Visuals" &&
                a.Key != Option.GoldenBlocksAlwaysLoad &&
                a.Key != Option.ShowActiveOptions
            );
        }

        public static bool GetUserEnabledCoolOption() {
            return GooberHelperModule.Settings.UserDefinedOptions.TryGetValue(Option.GoldenBlocksAlwaysLoad, out float value) && value == 1;
        }

        public static Color GetGlobalColor() {
            // Vector3 color = new Vector3();
            // float count = 0;

            // if(GetUserEnabledEvilOption()) {
            //     color += UserDefinedEvilColor.ToVector3();

            //     count++;
            // }

            // if(GetUserEnabledCoolOption()) {
            //     color += UserDefinedCoolColor.ToVector3();

            //     count++;
            // }

            // if(GooberHelperModule.Session.MapDefinedOptions.Count > 0) {
            //     color += MapDefinedColor.ToVector3();

            //     count++;
            // }

            // if(count == 0) return DefaultColor;

            // return new Color(color/count);
            
            return 
                GetUserEnabledEvilOption() ? UserDefinedEvilColor :
                GetUserEnabledCoolOption() ? UserDefinedCoolColor :
                GooberHelperModule.Session?.MapDefinedOptions.Count > 0 ? MapDefinedColor :
                DefaultColor;
        }

        // [Command("goob", "")]
        // public static void CmdGoob() {
        //     Engine.Commands.Log("Session.MapDefinedOptions:");
        //     foreach(var pair in GooberHelperModule.Session.MapDefinedOptions) {
        //         Engine.Commands.Log($"- {pair.Key}: {pair.Value}");
        //     }

        //     Engine.Commands.Log("Settings.UserDefinedOptions:");
        //     foreach(var pair in GooberHelperModule.Settings.UserDefinedOptions) {
        //         Engine.Commands.Log($"- {pair.Key}: {pair.Value}");
        //     }
        // }

        //maybe sort these chronologically?
        public enum Option {
            //jumping
            JumpInversion,
            WalljumpSpeedPreservation,
            WallbounceSpeedPreservation,
            HyperAndSuperSpeedPreservation,
            UpwardsJumpSpeedPreservationThreshold,
            DownwardsJumpSpeedPreservationThreshold,

            GetClimbjumpSpeedInRetention,
            AdditiveVerticalJumpSpeed,
            SwapHorizontalAndVerticalSpeedOnWalljump,
            VerticalToHorizontalSpeedOnGroundJump,
            CornerboostBlocksEverywhere,

            AllDirectionHypersAndSupers,
            AllowUpwardsCoyote,
            AllDirectionDreamJumps,
            AllowHoldableClimbjumping,

            //dashing
            VerticalDashSpeedPreservation,
            ReverseDashSpeedPreservation,

            MagnitudeBasedDashSpeed,

            DashesDontResetSpeed,
            KeepDashAttackOnCollision,

            //moving
            CobwobSpeedInversion,

            WallboostDirectionIsOppositeSpeed,
            WallboostSpeedIsOppositeSpeed,
            HorizontalTurningSpeedInversion,
            VerticalTurningSpeedInversion,
            DownwardsAirFrictionBehavior,

            UpwardsTransitionSpeedPreservation,

            //other
            RefillFreezeLength,
            RetentionLength,

            DreamBlockSpeedPreservation,
            SpringSpeedPreservation,
            ReboundSpeedPreservation,
            ExplodeLaunchSpeedPreservation,
            PickupSpeedInversion,
            BubbleSpeedPreservation,
            FeatherEndSpeedPreservation,
            BadelineBossSpeedPreservation,

            CustomFeathers,
            CustomSwimming,
            RemoveNormalEnd,
            LenientStunning,
            HoldablesInheritSpeedWhenThrown,

            AllowCrouchedHoldableGrabbing,
            CoreBlockAllDirectionActivation,

            //visuals
            PlayerShaderMask,
            TheoNuclearReactor,

            //miscellaneous
            AlwaysExplodeSpinners,
            GoldenBlocksAlwaysLoad,
            RefillFreezeGameSuspension,
            BufferDelayVisualization,
            Ant,

            //general
            ShowActiveOptions,
        }

        public enum JumpInversionValue {
            None,
            GroundJumps,
            All
        }

        public enum WalljumpSpeedPreservationValue {
            None,
            FakeRCB,
            Preserve,
            Invert,
        }

        public enum VerticalJumpSpeedPreservationHybridValue {
            None = -1,
            DashSpeed = -2,
        }

        public enum AllDirectionHypersAndSupersValue {
            None,
            RequireGround,
            WorkWithCoyoteTime
        }

        public enum VerticalToHorizontalSpeedOnGroundJumpValue {
            None,
            Vertical,
            Magnitude
        }

        public enum MagnitudeBasedDashSpeedValue {
            None,
            OnlyCardinal,
            All
        }

        public enum CobwobSpeedInversionValue {
            None,
            RequireSpeed,
            WorkWithRetention
        }

        public enum DreamBlockSpeedPreservationValue {
            None,
            Horizontal,
            Vertical,
            Both,
            Magnitude,
        }

        public enum SpringSpeedPreservationValue {
            None,
            Preserve,
            Invert
        }
        
        public enum CustomFeathersValue {
            None,
            KeepIntro,
            SkipIntro
        }

        public enum PlayerShaderMaskValue {
            None,
            HairOnly,
            Cover,
        }

        public enum DashesDontResetSpeedValue {
            None,
            Legacy,
            On,
        }

        //the order within categories is
        //- speed preservation
        //- new thing
        //- allowing things that are prevented in vanilla
        //these subcategories are sorted roughly by creation order or however i want 😭
        //important things can be pinned to the top
        
        //important terminology definitions:
        //preservation = it preserves speed
        //inversion -> it preserves speed AND the player can decide which direction to go 

        public static Dictionary<string, List<OptionData>> Categories = new() {
            { "Jumping", [
                //goodbye buhbu 💗 i will love you forever
                // new OptionData(Option.buhbu, OptionType.Float, 0) { min = 0, max = 10, growthFactor = 10, suffix = " frames" },
                // new OptionData(Option.zonmgle),
                // new OptionData(Option.zingle)
                new OptionData(Option.JumpInversion, typeof(JumpInversionValue), JumpInversionValue.None),
                new OptionData(Option.WalljumpSpeedPreservation, typeof(WalljumpSpeedPreservationValue), WalljumpSpeedPreservationValue.None),
                new OptionData(Option.WallbounceSpeedPreservation),
                new OptionData(Option.HyperAndSuperSpeedPreservation),
                new OptionData(Option.UpwardsJumpSpeedPreservationThreshold, typeof(VerticalJumpSpeedPreservationHybridValue), OptionType.Integer, -1) { Max = 240, Step = 10, ExponentialIncrease = false, Suffix = "px/s" },
                new OptionData(Option.DownwardsJumpSpeedPreservationThreshold, typeof(VerticalJumpSpeedPreservationHybridValue), OptionType.Integer, -1) { Max = 240, Step = 10, ExponentialIncrease = false, Suffix = "px/s" },

                new OptionData(Option.GetClimbjumpSpeedInRetention),
                new OptionData(Option.AdditiveVerticalJumpSpeed),
                new OptionData(Option.SwapHorizontalAndVerticalSpeedOnWalljump),
                new OptionData(Option.VerticalToHorizontalSpeedOnGroundJump, typeof(VerticalToHorizontalSpeedOnGroundJumpValue), VerticalToHorizontalSpeedOnGroundJumpValue.None),
                new OptionData(Option.CornerboostBlocksEverywhere),
                
                new OptionData(Option.AllDirectionHypersAndSupers, typeof(AllDirectionHypersAndSupersValue), AllDirectionHypersAndSupersValue.None),
                new OptionData(Option.AllowUpwardsCoyote),
                new OptionData(Option.AllDirectionDreamJumps),
                new OptionData(Option.AllowHoldableClimbjumping),
            ]},
            { "Dashing", [
                new OptionData(Option.VerticalDashSpeedPreservation),
                new OptionData(Option.ReverseDashSpeedPreservation),

                new OptionData(Option.MagnitudeBasedDashSpeed, typeof(MagnitudeBasedDashSpeedValue), MagnitudeBasedDashSpeedValue.None),
                
                new OptionData(Option.DashesDontResetSpeed, typeof(DashesDontResetSpeedValue), DashesDontResetSpeedValue.None),
                new OptionData(Option.KeepDashAttackOnCollision),
            ]},
            { "Moving", [
                new OptionData(Option.CobwobSpeedInversion, typeof(CobwobSpeedInversionValue), CobwobSpeedInversionValue.None),
                
                new OptionData(Option.WallboostDirectionIsOppositeSpeed),
                new OptionData(Option.WallboostSpeedIsOppositeSpeed),
                new OptionData(Option.HorizontalTurningSpeedInversion),
                new OptionData(Option.VerticalTurningSpeedInversion),
                new OptionData(Option.DownwardsAirFrictionBehavior),

                new OptionData(Option.UpwardsTransitionSpeedPreservation),
            ]},
            { "Other", [
                new OptionData(Option.RefillFreezeLength, OptionType.Float, 3) { Min = 0, Max = 10000, Step = 1, Suffix = "f" },
                new OptionData(Option.RetentionLength, OptionType.Float, 4) { Min = 0, Max = 10000, Step = 1, Suffix = "f" },
                
                new OptionData(Option.DreamBlockSpeedPreservation, typeof(DreamBlockSpeedPreservationValue), DreamBlockSpeedPreservationValue.None),
                new OptionData(Option.SpringSpeedPreservation, typeof(SpringSpeedPreservationValue), SpringSpeedPreservationValue.None),
                new OptionData(Option.ReboundSpeedPreservation),
                new OptionData(Option.ExplodeLaunchSpeedPreservation),
                new OptionData(Option.PickupSpeedInversion),
                new OptionData(Option.BubbleSpeedPreservation),
                new OptionData(Option.FeatherEndSpeedPreservation),
                new OptionData(Option.BadelineBossSpeedPreservation),

                new OptionData(Option.CustomFeathers, typeof(CustomFeathersValue), CustomFeathersValue.None),
                new OptionData(Option.CustomSwimming),
                new OptionData(Option.RemoveNormalEnd),
                new OptionData(Option.LenientStunning),
                new OptionData(Option.HoldablesInheritSpeedWhenThrown),

                new OptionData(Option.AllowCrouchedHoldableGrabbing),
                new OptionData(Option.CoreBlockAllDirectionActivation),
            ]},
            { "Visuals", [
                new OptionData(Option.PlayerShaderMask, typeof(PlayerShaderMaskValue), PlayerShaderMaskValue.None),
                new OptionData(Option.TheoNuclearReactor),
            ]},
            { "Miscellaneous", [
                new OptionData(Option.AlwaysExplodeSpinners),
                new OptionData(Option.GoldenBlocksAlwaysLoad),
                new OptionData(Option.RefillFreezeGameSuspension),
                new OptionData(Option.BufferDelayVisualization),
                new OptionData(Option.Ant),
            ]},
            { "General", [
                new OptionData(Option.ShowActiveOptions),
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