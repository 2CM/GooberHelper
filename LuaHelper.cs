using NLua;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.GooberHelper {
    
    //STOLEN FROM LUA CUTSCENES
    public static class LuaHelper {
        public static IEnumerator LuaCoroutineToIEnumerator(LuaCoroutine routine)
        {
            while (routine != null && routine.MoveNext())
            {
                if (routine.Current is double || routine.Current is long)
                {
                    yield return Convert.ToSingle(routine.Current);
                }
                else
                {
                    yield return routine.Current;
                }
            }

            yield return null;
        }

        public static LuaTable ListToLuaTable(IList list)
        {
            Lua lua = Everest.LuaLoader.Context;
            LuaTable table = lua.DoString("return {}").ElementAtOrDefault(0) as LuaTable;

            int ptr = 1;

            foreach (var value in list)
            {
                table[ptr++] = value;
            }

            return table;
        }
    }
}