using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExtendedVariants.UI;
using Microsoft.Xna.Framework;
using Monocle;
using static Celeste.Mod.GooberHelper.OptionsManager;

namespace Celeste.Mod.GooberHelper.UI {
    public class OuiGooberHelperOptions : Oui {
        public class NumericSliderOptions : IEnumerable<KeyValuePair<float, string>> {
            private OptionData optionData;

            public static int DefaultIndex;

            public NumericSliderOptions(OptionData optionData) {
                this.optionData = optionData;

                DefaultIndex = 0;
            }

            public IEnumerator<KeyValuePair<float, string>> GetEnumerator() {
                // int counter = 0;

                // for(float i = min; i < max; i += (int)Math.Max(Math.Floor(i / 10) * growthFactor, 1)) {
                //     yield return new KeyValuePair<float, string>(i, i.ToString() + suffix);

                //     if(i == defaultValue) DefaultIndex = counter;

                //     counter++;
                // }

                float n = 0;
                float mag = optionData.Step;
                float defaultValue = GetOptionMapDefinedValueOrDefault(optionData.Id);

                for(int i = 0; n < optionData.Max; i++) {
                    yield return new KeyValuePair<float, string>(n, n.ToString() + optionData.Suffix);

                    if(i == defaultValue) DefaultIndex = i;

                    if(n == mag * 100) mag *= 10;

                    n += mag * (
                        n < mag * 20 ? 1 :
                        n < mag * 50 ? 2 :
                        5
                    );

                    n = MathF.Round(n / optionData.Step) * optionData.Step;
                }

                yield return new KeyValuePair<float, string>(optionData.Max, optionData.Max.ToString() + optionData.Suffix);

                yield break;
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }
        }

        public TextMenu menu = null;
        public static int pauseMenuReturnIndex;
        public static bool pauseMenuMinimal;
        public static Dictionary<string, TextMenuExt.ButtonExt> categoryButtons = [];
        // public static Dictionary<Option, Tuple<TextMenuExt.EnumerableSlider<float>, int>> optionSliders = [];
        public static Dictionary<TextMenuExt.EnumerableSlider<float>, Option> optionSliders = [];

        public static string queuedOptionsProfileName;
        public static bool wasAllowingHudHide = true;

        public enum OptionsProfileAction {
            Load,
            Save,
            Rename,
            Duplicate,
            Export,
            Import,
            Delete
        }

        private static Color importErrorColor = Color.Red;
        private static Color importSuccessColor = Color.Lime;

        public OuiGooberHelperOptions() : base() {}

        private static TextMenuExt.ButtonExt createCategoryButton(string categoryName, TextMenu menu) {
            TextMenuExt.ButtonExt button = new TextMenuExt.ButtonExt(Dialog.Clean($"menu_gooberhelper_category_{categoryName}"));

            button.TextColor = GetCategoryColor(categoryName);
            
            button.OnPressed = () => {
                int returnIndex = menu.IndexOf(button);
                menu.RemoveSelf();

                TextMenu categoryMenu = CreateCategoryMenu(categoryName);

                categoryMenu.OnESC = categoryMenu.OnCancel = () => {
                    categoryMenu.CloseAndRun(null, () => {
                        categoryMenu.Scene.Add(CreateMenu(returnIndex));
                    });
                };

                menu.Scene.Add(categoryMenu);
            };

            categoryButtons[categoryName] = button;

            menu.Add(button);

            return button;
        }

        private static TextMenuExt.EnumerableSlider<float> addOptionSlider(OptionData optionData, TextMenu menu) {
            float startValue = GetOptionValue(optionData.Id);

            if(optionData.Type == OptionType.Boolean) startValue = startValue >= 1 ? 1 : 0;

            var optionSlider = new TextMenuExt.EnumerableSlider<float>(
                label: Dialog.Clean($"gooberhelper_option_{optionData.Name}"),
                options: optionData.Type == OptionType.Boolean ?
                    [ new KeyValuePair<float, string>(0, Dialog.Clean("options_off")), new KeyValuePair<float, string>(1, Dialog.Clean("options_on")) ] :
                    new NumericSliderOptions(optionData),
                startValue
            );

            menu.Add(optionSlider);
            optionSliders[optionSlider] = optionData.Id;

            string dialogId = $"gooberhelper_option_description_{optionData.Name}";
            if(Dialog.Has(dialogId)) optionSlider.AddDescription(menu, Dialog.Clean(dialogId));
            
            if(optionSlider.Values[optionSlider.Index].Item2 != startValue) {
                updateOptionSlider(optionSlider);
            } else {
                optionSlider.UnselectedColor = GetOptionColor(optionData.Id);
            }

            optionSlider.OnValueChange = value => {
                SetOptionValue(optionData.Id, value, OptionSetter.User);

                optionSlider.UnselectedColor = GetOptionColor(optionData.Id);
            };

            optionSlider.OnAltPressed = () => {
                ResetOptionValue(optionData.Id, OptionSetter.User);

                updateOptionSlider(optionSlider);
            };

            return optionSlider;
        }

        private static void updateOptionSlider(TextMenuExt.EnumerableSlider<float> optionSlider) {
            Option option = optionSliders[optionSlider];
            float newValue = GetOptionValue(option);

            optionSlider.UnselectedColor = GetOptionColor(option);

            if(Options[option].Type == OptionType.Boolean) {
                if(newValue < 0 || newValue > 1) { //qhat the fuck this isnt a boolean
                    // if(optionSlider.Values.First().Item1 != "INVALID") optionSlider.Values.Insert(0, new Tuple<string, float>("INVALID", int.MinValue));

                    optionSlider.Index = optionSlider.PreviousIndex = newValue >= 1 ? 1 : 0;

                    return;
                }
            }

            if(optionSlider.Values.Last().Item2 < newValue) {
                optionSlider.Add(newValue.ToString() + Options[option].Suffix, newValue, true);

                return;
            }

            int min = 0;
            int max = optionSlider.Values.Count - 1;

            while(min <= max) {
                int mid = (int)Math.Floor((min + max)/2f);

                if(optionSlider.Values[mid].Item2 > newValue) {
                    max = mid - 1;
                } else if(optionSlider.Values[mid].Item2 < newValue) {
                    min = mid + 1;
                } else {
                    optionSlider.Index = optionSlider.PreviousIndex = mid;

                    return;
                }
            }

            optionSlider.Values.Insert(min, new Tuple<string, float>(newValue.ToString() + Options[option].Suffix, newValue));

            optionSlider.Index = optionSlider.PreviousIndex = min;

            // for(int i = 0; i < optionSlider.Values.Count; i++) {
            //     var sliderOption = optionSlider.Values[i];

            //     if(sliderOption.Item2 == newValue) {
            //         optionSlider.Index = i;
            //     }
            // }
        }

        public static TextMenu CreateCategoryMenu(string categoryName) {
            TextMenu menu = new();

            optionSliders.Clear();
            categoryButtons.Clear();

            menu.Add(new TextMenu.Header(Dialog.Clean($"menu_gooberhelper_category_{categoryName}")));

            foreach(OptionData optionData in Categories[categoryName]) {
                TextMenuExt.EnumerableSlider<float> menuItem = addOptionSlider(optionData, menu);
            }

            menu.Add(new TextMenu.SubHeader(""));
            menu.Add(new TextMenu.Button(Dialog.Clean("menu_gooberhelper_reset_all_options")).Pressed(() => {
                foreach(var pair in optionSliders) {
                    ResetOptionValue(pair.Value, OptionSetter.User);
                    updateOptionSlider(pair.Key);
                    // SetOptionValue(pair.Key, GetOptionMapDefinedValueOrDefault(pair.Key), OptionSetter.User);

                    // pair.Value.Item1.Index = pair.Value.Item2;
                    // pair.Value.Item1.UnselectedColor = GetOptionColor(pair.Key);
                }
            }));

            return menu;
        }

        private static void createOptionsProfileButton(string name, TextMenu menu, int insertionIndex = -1) {
            var profileItem = new TextMenuExt.EnumSlider<OptionsProfileAction>(name);
            TextMenuExt.EaseInSubHeaderExt description;

            if(insertionIndex == -1) {
                menu.Add(profileItem);

                profileItem.AddDescription(menu, "");
                description = menu.Items.Last() as TextMenuExt.EaseInSubHeaderExt;
            } else {
                menu.Insert(insertionIndex, profileItem);

                profileItem.AddDescription(menu, "");
                description = menu.Items[insertionIndex + 1] as TextMenuExt.EaseInSubHeaderExt;
            }


            profileItem.OnPressed = () => {
                switch(profileItem.Index) {
                    case (int)OptionsProfileAction.Load:
                        OptionsProfile.Load(name);

                        UpdateMenu();
                    break;
                    case (int)OptionsProfileAction.Save:
                        OptionsProfile.Save(name);

                        UpdateMenu();
                    break;
                    case (int)OptionsProfileAction.Rename:
                        Action<string> finish = (newName) => {
                            OptionsProfile.Rename(name, newName);

                            profileItem.Label = newName;

                            name = newName;
                        };

                        Utils.OpenTextInputField(finish, null, "Rename the options profile");
                    break;
                    case (int)OptionsProfileAction.Duplicate:
                        OptionsProfile duplicate = OptionsProfile.Duplicate(name);

                        createOptionsProfileButton(duplicate.Name, menu, menu.items.IndexOf(profileItem) + 1);
                    break;
                    case (int)OptionsProfileAction.Export:
                        OptionsProfile.Export(name);
                    break;
                    case (int)OptionsProfileAction.Import:
                        try {
                            OptionsProfile.Import(name);

                            description.Title = Dialog.Clean("menu_gooberhelper_import_profile_success");
                            description.TextColor = importSuccessColor;
                        } catch {
                            description.Title = Dialog.Clean("menu_gooberhelper_import_profile_error");
                            description.TextColor = importErrorColor;
                        }
                    break;
                    case (int)OptionsProfileAction.Delete:
                        OptionsProfile.Delete(name);

                        menu.items.Remove(profileItem);
                        menu.items.Remove(description);

                        if(menu.Selection >= menu.items.Count) {
                            menu.Selection -= 2; //idk why its 2 im not gonna worry about it its 1:46 am please i need to go to sleep i have done anything other than code this stuff all day
                        }
                    break;
                }
            };

            profileItem.OnValueChange = (value) => {
                description.TextColor = Color.Gray;

                switch(profileItem.Index) {
                    case (int)OptionsProfileAction.Export:
                        description.Title = Dialog.Clean("menu_gooberhelper_export_profile_description");
                    break;
                    case (int)OptionsProfileAction.Import:
                        description.Title = Dialog.Clean("menu_gooberhelper_import_profile_description");
                    break;
                    case (int)OptionsProfileAction.Delete:
                        description.Title = Dialog.Clean("menu_gooberhelper_delete_profile_description");
                        description.TextColor = Color.Red;
                    break;
                    default:
                        description.Title = "";
                    break;
                }
            };
        }

        public static void UpdateMenu() {
            foreach(var pair in categoryButtons) {
                pair.Value.TextColor = GetCategoryColor(pair.Key);
            }

            foreach(var pair in optionSliders) {
                updateOptionSlider(pair.Key);
            }
        }

        public static TextMenu CreateMenu(int startIndex = 2) { //2 because title and input field modal thing
            TextMenu menu = new();

            wasAllowingHudHide = (Engine.Scene as Level).AllowHudHide;
            (Engine.Scene as Level).AllowHudHide = false;

            Utils.CreateTextInputField(menu);

            menu.OnUpdate = () => {
                if(queuedOptionsProfileName != null) {
                    createOptionsProfileButton(queuedOptionsProfileName, menu);

                    queuedOptionsProfileName = null;
                }
            };

            menu.OnESC = menu.OnCancel = () => {
                menu.CloseAndRun(null, () => {
                    GooberHelperModule.Instance.SaveSettings();

                    (menu.Scene as Level).AllowHudHide = wasAllowingHudHide;
                    (menu.Scene as Level).Pause(pauseMenuReturnIndex, pauseMenuMinimal, false);
                });
            };

            optionSliders.Clear();
            categoryButtons.Clear();

            menu.Add(new TextMenu.Header(Dialog.Clean("menu_gooberhelper_title")));

            menu.Add(new TextMenu.Button(Dialog.Clean("menu_gooberhelper_reset_all_options")).Pressed(() => {
                ResetAll(OptionSetter.User);

                UpdateMenu();
            }));

            menu.Add(new TextMenuExt.SubHeaderExt(Dialog.Clean("menu_gooberhelper_category_physics")));
                createCategoryButton("Jumping",  menu);
                createCategoryButton("Dashing", menu);
                createCategoryButton("Moving", menu);
                createCategoryButton("Other", menu);


            menu.Add(new TextMenuExt.SubHeaderExt(Dialog.Clean("menu_gooberhelper_category_visuals")));
                createCategoryButton("Visuals", menu);


            menu.Add(new TextMenuExt.SubHeaderExt(Dialog.Clean("menu_gooberhelper_category_miscellaneous")));
                createCategoryButton("Miscellaneous", menu);


            menu.Add(new TextMenuExt.SubHeaderExt(Dialog.Clean("menu_gooberhelper_category_general")));
                addOptionSlider(Options[Option.ShowActiveSettings], menu); //should this not be reset along with the others??


            menu.Add(new TextMenuExt.SubHeaderExt(Dialog.Clean("menu_gooberhelper_category_profiles")));
            
            menu.Add(new TextMenu.Button(Dialog.Clean("menu_gooberhelper_create_profile")).Pressed(() => {
                Action<string> finish = (name) => {
                    if(OptionsProfile.GetExists(name)) {
                        OptionsProfile.Save(name);

                        return;
                    }

                    OptionsProfile.Create(name);

                    //this code is running while the textmenu is enumerating all of its items
                    //trying to add a new one causes a crash
                    queuedOptionsProfileName = name;
                };

                Utils.OpenTextInputField(finish, null, "Name the profile");
            }));

            TextMenu.Button importButton = new TextMenu.Button(Dialog.Clean("menu_gooberhelper_import_create_profile"));
                menu.Add(importButton);
                importButton.AddDescription(menu, Dialog.Clean("menu_gooberhelper_import_create_profile_description"));

                var description = menu.items.Last() as TextMenuExt.EaseInSubHeaderExt;

                importButton.OnPressed = () => {
                    try {
                        queuedOptionsProfileName = OptionsProfile.CreateFromImport().Name;

                        description.Title = Dialog.Clean("menu_gooberhelper_import_create_profile_success").Replace("name", queuedOptionsProfileName);
                        description.TextColor = importSuccessColor;
                    } catch {
                        description.Title = Dialog.Clean("menu_gooberhelper_import_profile_error");
                        description.TextColor = importErrorColor;
                    }
                };

            if(GooberHelperModule.Settings.OptionsProfileOrder.Count == 0) {
                foreach(var pair in GooberHelperModule.Settings.OptionsProfiles) {
                    GooberHelperModule.Settings.OptionsProfileOrder.Add(pair.Key);
                }
            }

            foreach(var optionProfileName in GooberHelperModule.Settings.OptionsProfileOrder) {
                createOptionsProfileButton(optionProfileName, menu);
            }

            menu.Selection = startIndex;

            return menu;
        }

        public override IEnumerator Enter(Oui from) {
            this.menu = CreateMenu();
            this.Visible = true;
            this.menu.Focused = true;

            yield break;
        }

        public override IEnumerator Leave(Oui from) {
            this.Visible = false;
            this.menu.RemoveSelf();
            this.menu = null;

            yield break;
        }
    } 
}

/*
0 - 10: 1

10 - 50: 2
50 - 100: 5

100 - 500: 10
500 - 1000: 20

1000 - 5000: 50
5000 - 10000: 100



0 - 10: 1

10 - 20: 1
20 - 50: 2
50 - 100: 5

100 - 200: 10



*/