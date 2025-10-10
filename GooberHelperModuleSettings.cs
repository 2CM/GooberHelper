using System.Collections.Generic;

namespace Celeste.Mod.GooberHelper {
    public class GooberHelperModuleSettings : EverestModuleSettings {
        [SettingIgnore]
        public Dictionary<OptionsManager.Option, float> UserDefinedOptions { get; set; } = new Dictionary<OptionsManager.Option, float>();

        [SettingIgnore]
        public Dictionary<string, OptionsManager.OptionsProfile> OptionsProfiles { get; set; } = new Dictionary<string, OptionsManager.OptionsProfile>();

        [SettingIgnore]
        public List<string> OptionsProfileOrder { get; set; } = new List<string>();

        [SettingName("GooberHelper_ShowOptionsInGame")]
        public bool ShowOptionsInGame { get; set; } = false;


        [SettingName("DebugMapPhysics")]
        public bool DebugMapPhysics { get; set; } = false;
    }
}
