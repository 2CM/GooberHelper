using System.Collections.Generic;
using Celeste.Mod.GooberHelper.DataStructures;
using Celeste.Mod.GooberHelper.Options;
using Celeste.Mod.GooberHelper.Settings.Root.MouseJoystick;
using Celeste.Mod.GooberHelper.Settings.Root.MouseJoystick.RelativeMode;
using Microsoft.Xna.Framework.Input;

namespace Celeste.Mod.GooberHelper {
    public class GooberHelperModuleSettings : EverestModuleSettings {
        [SettingIgnore]
        public Dictionary<Option, float> UserDefinedOptions { get; set; } = [];

        [SettingIgnore]
        public Dictionary<string, OptionsProfile> OptionsProfiles { get; set; } = [];

        [SettingIgnore]
        public List<string> OptionsProfileOrder { get; set; } = [];

        //actual settings
        //formatting menace
        public bool ShowOptionsInGame { get; set; } = false;
        public bool DebugMapPhysics { get; set; } = false;
        public bool FastMenuing { get; set; } = false;
        public bool UnlimitedBinds { get; set; } = false;
        public MouseJoystickSubMenu MouseJoystick { get; set; } = new(); public class MouseJoystickSubMenu() {
            public Mode.ModeValue Mode { get; set; } = Settings.Root.MouseJoystick.Mode.ModeValue.None;
            public AbsoluteModeSubMenu AbsoluteMode { get; set; } = new(); public class AbsoluteModeSubMenu() {
                public DummyCircle Circle { get; set; } = new DummyCircle(150, 0, 0);
                public Color OuterColor { get; set; } = new Color(0, 0, 0, 0f);
                public Color BorderColor { get; set; } = new Color(1f, 1f, 1f, 1f);
                public Color InnerColor { get; set; } = new Color(0.2f, 0.8f, 1f, 0.2f);
                public float BorderThickness { get; set; } = 2f;
            }

            public RelativeModeSubMenu RelativeMode { get; set; } = new(); public class RelativeModeSubMenu() {
                public DummyCircle Circle { get; set; } = new DummyCircle(150, 0, 0);
                public float DeadzoneRadius { get; set; } = 50f;
                public bool UseRegularMouse { get; set; } = false;
                public ClickBehavior.ClickBehaviorValue ClickBehavior { get; set; } = Settings.Root.MouseJoystick.RelativeMode.ClickBehavior.ClickBehaviorValue.None;
                public ClampBehavior.ClampBehaviorValue ClampBehavior { get; set; } = Settings.Root.MouseJoystick.RelativeMode.ClampBehavior.ClampBehaviorValue.Circle;
                public Color OuterColor { get; set; } = new Color(0, 0, 0, 0);
                public Color BorderColor { get; set; } = new Color(0, 1f, 0, 1f);
                public Color InnerColor { get; set; } = new Color(0.2f, 0.8f, 1f, 0.2f);
                public Color DeadzoneBorderColor { get; set; } = new Color(1f, 1f, 1f, 0.5f);
                public float BorderThickness { get; set; } = 2f;
                public float DeadzoneBorderThickness { get; set; } = 2f;
            }
        }

        //buttons
        //theyre not public because i dont want them to be stored in the mod settings
        //theyre not private because my ide will get mad at me for having private members with PascalCase names
        //first ever use of protected
        //awesome
        protected object GooberHelperOptionsButton { get; }
        protected object ResetAllOptionsButton { get; }

        //keys
        [DefaultButtonBinding(Buttons.LeftTrigger, Keys.Tab)]
        public ButtonBinding ExitGameSuspension { get; set; } = new ButtonBinding(Buttons.LeftTrigger, Keys.Tab);
        public ButtonBinding IncreaseSliderStep { get; set; } = new ButtonBinding(Buttons.RightShoulder, Keys.C);
        public ButtonBinding DecreaseSliderStep { get; set; } = new ButtonBinding(Buttons.LeftShoulder, Keys.Z);
    }
}