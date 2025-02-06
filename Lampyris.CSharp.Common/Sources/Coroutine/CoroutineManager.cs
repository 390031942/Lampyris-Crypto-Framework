namespace Lampyris.CSharp.Common;

using System.Collections;
using System.Collections.Generic;

public static class CoroutineManager
{
    private static readonly List<IEnumerator> ms_Coroutines = new List<IEnumerator>();

    public static void StartCoroutine(IEnumerator coroutine)
    {
        if (ms_Coroutines.Contains(coroutine))
            return;
        
        ms_Coroutines.Add(coroutine);
    }

    public static void RemoveCoroutine(IEnumerator coroutine)
    {
        if (!ms_Coroutines.Contains(coroutine))
            return;
        
        ms_Coroutines.Remove(coroutine);
    }

    public static void Tick()
    {
        for (int i = ms_Coroutines.Count - 1; i >= 0; i--)
        {
            bool needMoveNext = false;
            IEnumerator coroutine = ms_Coroutines[i];
            if (coroutine.Current is IEnumerator nestedCoroutine)
            {
                if (nestedCoroutine.MoveNext())
                {
                    needMoveNext = true;
                }
            }
            else {
                needMoveNext = true;
            }

            if(needMoveNext)
            {
                if (!coroutine.MoveNext())
                {
                    ms_Coroutines.Remove(coroutine);
                }
            }
        }
    }
}
