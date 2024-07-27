using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System;
using NLua;
using System.Text;
using System.Linq;
using System.Collections;

namespace Celeste.Mod.GooberHelper.Entities {

    [CustomEntity("GooberHelper/BulletSource")]
    [Tracked(false)]
    public class BulletSource : Actor {
        private bool going = false;
        IEnumerator luaRoutine = null;
        // STOLEN FROM LUACUTSCENES
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

        public BulletSource(EntityData data, Vector2 offset) : base(data.Position + offset) {
            base.Add(new PlayerCollider(CollidePlayer, new Hitbox(10, 10, 0, 0)));

            // Everest.Content.TryGet("Patterns/Simple", out var pattern, true);

            // try {
            //     text = Encoding.UTF8.GetString(pattern.Data);

            //     Logger.Log(LogLevel.Info, "GooberHelper", "success");
            //     Logger.Log(LogLevel.Info, "GooberHelper", text);
            // } catch(Exception e) {
            //     Logger.Log(LogLevel.Info, "GooberHelper", "failed");
            //     Logger.Log(LogLevel.Info, "GooberHelper", e.ToString());
            // }
        }


        public override void Update()
        {
            base.Update();
        }

        public IEnumerator luaWrapper() {
            yield return luaRoutine;
        }

        public void CollidePlayer(Player player) {
            if(!going) {
                going = true;

                LuaTable t = Everest.LuaLoader.Require("Patterns/Simple") as LuaTable;

                Logger.Log(LogLevel.Info, "GooberHelper", "a");
                Logger.Log(LogLevel.Info, "GooberHelper", t.ToString());

                object[] res = (t["init"] as LuaFunction).Call(new object[] {this});

                object raw = res.ElementAtOrDefault(0);

                luaRoutine = LuaCoroutineToIEnumerator(raw as LuaCoroutine);
                
                Logger.Log(LogLevel.Info, "GooberHelper", raw.ToString());

                Add(new Coroutine(luaWrapper()));
            }
        }
    }
}

// using Monocle;
// using Microsoft.Xna.Framework;
// using Celeste.Mod.Entities;
// using System;
// using System.Collections;
// using Mono.CSharp;
// using System.Text;
// using System.IO;
// using System.Collections.Generic;
// using System.Reflection;

// namespace Celeste.Mod.GooberHelper.Entities {

//     [CustomEntity("GooberHelper/BulletSource")]
//     public class BulletSource : Actor {
//         private StringBuilder errors = new StringBuilder();
//         private Evaluator eval;
//         private bool going = false;

//         private string text = null;

//         public BulletSource(EntityData data, Vector2 offset) : base(data.Position + offset) {
//             base.Add(new PlayerCollider(CollidePlayer, new Hitbox(10, 10, 0, 0)));

            
//             Everest.Content.TryGet("Patterns/Simple", out var pattern, true);

//             try {
//                 text = Encoding.UTF8.GetString(pattern.Data);

//                 Logger.Log(LogLevel.Info, "GooberHelper", "success");
//                 Logger.Log(LogLevel.Info, "GooberHelper", text);
//             } catch(Exception e) {
//                 Logger.Log(LogLevel.Info, "GooberHelper", "failed");
//                 Logger.Log(LogLevel.Info, "GooberHelper", e.ToString());
//             }

//             var ctx = new CompilerContext(new CompilerSettings(), new StreamReportPrinter(new StringWriter(errors)));

//             eval = new Evaluator(ctx);

//             foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
//             {
//                 string name = asm.GetName().Name;
//                 string text = name;
//                 string text2 = text;
//                 if (!(text2 == "System") && !(text2 == "System.Core") && !(text2 == "mscorlib"))
//                 {
//                     eval.ReferenceAssembly(asm);
//                 }
//             }

//             eval.Run("using Celeste;");
//             eval.Run("using Monocle;");
//             eval.Run("using Celeste.Mod.DebugConsole;");
//             eval.Run("using Microsoft.Xna.Framework;");
//             eval.Run("using Microsoft.Xna.Framework.Input;");
//             eval.Run("using System;");
//             eval.Run("using System.Collections;");
//             eval.Run("using System.Collections.Generic;");
//             eval.Run("using System.Linq;");
        
//             // 
//         }

//         public IEnumerator MainCoroutine() {
//             yield return 1;
//             float a = 2 + 4;
//             yield return 5;

//             int counter = 0;

//             while(true) {
//                 counter++;

//                 if(counter > 10) break;

//                 yield return a;
//             }

//             yield return 2;

//             // Logger.Log(LogLevel.Info, "----------", "----------");

//             // try {
//             //     eval.Evaluate(text);
//             // } catch(Exception e) {
//             //     Logger.Log(LogLevel.Info, "f", "failed");
//             //     Logger.Log(LogLevel.Info, "f", e.ToString());
//             // }

//             // Logger.Log(LogLevel.Info, "f", errors.ToString());
//         }

//         public override void Update()
//         {
//             base.Update();

//             // Logger.Log(LogLevel.Info, "GooberHelper", Scene.Tracker.GetEntities<Bullet>().Count.ToString());
//         }

//         public void CollidePlayer(Player player) {
//             if(!going) {
//                 // Add(new Coroutine(MainCoroutine()));

//                 try {
//                     eval.Evaluate(text);
//                 } catch(Exception e) {
//                     Logger.Log(LogLevel.Info, "f", "failed");
//                     Logger.Log(LogLevel.Info, "f", e.ToString());
//                 }

//                 Logger.Log(LogLevel.Info, "f", errors.ToString());
                
//                 // Func<IEnumerator> zong = delegate() {yield return 10;};

//                 // try {
//                 //     Logger.Log(LogLevel.Info, "f", "about to evaluate 2");
//                 //     evaluator.Evaluate(text);
//                 // } catch {
//                 //     Logger.Log(LogLevel.Info, "f", "failed");
//                 // }
                
                
                

//                 // // Logger.Log(LogLevel.Info, "f", result.ToString());
//                 // // Logger.Log(LogLevel.Info, "f", results.ToString());

//                 // Logger.Log(LogLevel.Info, "f", errors.ToString());

//                 going = true;
//             }   
//         }
//     }
// }