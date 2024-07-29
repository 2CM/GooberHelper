using NLua;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Celeste.Mod.GooberHelper {
    
    //STOLEN FROM LUA CUTSCENES
    public static class LuaHelper {
        public static string GetFileContent(string path)
        {
            Stream stream = Everest.Content.Get(path)?.Stream;

            if (stream != null)
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }

            return null;
        }

        private static bool SafeMoveNext(this LuaCoroutine enumerator)
        {
            try
            {
                return enumerator.MoveNext();
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, "Lua Cutscenes", $"Failed to resume coroutine");
                Logger.LogDetailed(e);

                return false;
            }
        }

        public static IEnumerator LuaCoroutineToIEnumerator(LuaCoroutine routine)
        {
            while (routine != null && routine.SafeMoveNext())
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

        public static LuaTable DictionaryToLuaTable(IDictionary<object, object> dict)
        {
            Lua lua = Everest.LuaLoader.Context;
            LuaTable table = lua.DoString("return {}").FirstOrDefault() as LuaTable;

            foreach (KeyValuePair<object, object> pair in dict)
            {
                table[pair.Key] = pair.Value;
            }

            return table;
        }

        public static LuaTable ListToLuaTable(IList list)
        {
            Lua lua = Everest.LuaLoader.Context;
            LuaTable table = lua.DoString("return {}").FirstOrDefault() as LuaTable;

            int ptr = 1;

            foreach (var value in list)
            {
                table[ptr++] = value;
            }

            return table;
        }
    }
}