using Celeste.Mod.GooberHelper.Attributes;
using Celeste.Mod.GooberHelper.Attributes.Hooks;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

//todo: make this bgettert ioaejiojaeogaze si mtired
//^ im writing this message like a month later; what was i saying here???
//i need an entire team of archaeologists and cryptographers to help me decipher this
//truly fascinating
//our best people think it might be something akin to "im tired"
//surely

namespace Celeste.Mod.GooberHelper.Options.Miscellaneous {
    [GooberHelperOption]
    public class GoldenBlocksAlwaysLoad : AbstractOption {
        [ILHook(typeof(GoldenBlock), "Awake")]
        [ILHook("PlatinumStrawberry", "Celeste.Mod.PlatinumStrawberry.Entities.PlatinumBlock", "Awake")]
        [ILHook("CollabUtils2", "Celeste.Mod.CollabUtils2.Entities.SilverBlock", "Awake")]
        [ILHook("KoseiHelper", "Celeste.Mod.KoseiHelper.Entities.CustomGoldenBlock", "Awake")]
        [ILHook("DSidesHelper", "Celeste.Mod.DSidesPlatinum.Entities.PlatinumBlock", "Awake")] //WHY AM I HOOKING A MAP FROM MY CODEMOD 😭😭😭😭😭
        private static void makeGoldenBlocksOrSimilarEntitiesAlwaysLoad(ILContext il) {
            var cursor = new ILCursor(il);

            //for the local boolean "flag" on golden blocks, silver blocks, and koseihelper custom golden blocks
            patchLocalFalseBool(cursor, 0);

            if(il.Method.DeclaringType.Name != "PlatinumBlock")
                return;
            
            //for the local boolean "flag2" on platinum blocks
            //idk why its coded like this but platinum block use two flags:
            //one of them is for if theres a golden strawberry;
            //one of them is for if theres a platinum berry.
            //adding this second patch makes it work
            patchLocalFalseBool(cursor, 1);
        }

        private static void patchLocalFalseBool(ILCursor cursor, int index) {
            if(cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchLdcI4(0),
                instr => instr.MatchStloc(index)
            )) {
                cursor.Index--;
                cursor.EmitDelegate(overrideFalseBoolean);
            }
        }

        private static float overrideFalseBoolean(int orig)
            => GetOptionBool(Option.GoldenBlocksAlwaysLoad)
                ? 1
                : orig;
    }
}