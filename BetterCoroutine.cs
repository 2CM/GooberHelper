using System.Collections;
using System.Collections.Generic;
using Monocle;
using MonoMod.Utils;

class BetterCoroutine : Coroutine {
    public BetterCoroutine (IEnumerator functionCall, bool removeOnComplete = true) : base(functionCall, removeOnComplete) {

    }

    public override void Update()
    {
        float oldWaitTimer = DynamicData.For(this).Get<float>("waitTimer");

        base.Update();

        float waitTimer = DynamicData.For(this).Get<float>("waitTimer");

        if(oldWaitTimer <= 0 && oldWaitTimer != waitTimer) {
            DynamicData.For(this).Set("waitTimer", DynamicData.For(this).Get<float>("waitTimer") + oldWaitTimer);
        }
    }
}