using Celeste.Mod.GooberHelper.Attributes;

namespace Celeste.Mod.GooberHelper.Options.Physics.Entities {
    [GooberHelperOption]
    public class CustomSwimmingLaunchSpeed : AbstractOption {
        public override OptionType Type { get; set; } = OptionType.Float;
        public override float DefaultValue { get; set; } = 80f;
        public override float? RightMin { get; set; } = 0f;
        public override float Step { get; set; } = 5f;
        public override string Suffix { get; set; } = " px/s";
    }
}