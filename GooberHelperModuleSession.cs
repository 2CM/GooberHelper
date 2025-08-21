using System;
using System.Collections.Generic;
using Celeste.Mod.GooberHelper.Entities;

namespace Celeste.Mod.GooberHelper {
    public class GooberHelperModuleSession : EverestModuleSession {
        
        public Dictionary<string, List<StackItem>> Stacks { get; set; } = [];
        public List<OptionsManager.OptionChanges> Stack { get; set; } = new List<OptionsManager.OptionChanges>();
        public Dictionary<OptionsManager.Option, float> MapDefinedOptions { get; set; } = [];
    }
}
