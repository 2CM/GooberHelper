using System;
using Celeste.Mod.GooberHelper.Attributes;

namespace Celeste.Mod.GooberHelper.Options.Physics.Entities {
    [GooberHelperOption]
    public class CustomSwimmingWalljumpSpeed : AbstractOption {
        public override OptionType Type { get; set; } = OptionType.Float;
        public override Type EnumType { get; set; } = typeof(Value);
        public override float DefaultValue { get; set; } = 20f;
        public override float? RightMin { get; set; } = 0f;
        public override float Step { get; set; } = 5f;
        public override string Suffix { get; set; } = " px/s";

        public enum Value {
            Off = ReservedHybridEnumConstant
        }
    }
}