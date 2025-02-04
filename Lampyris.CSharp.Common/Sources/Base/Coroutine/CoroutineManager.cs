namespace Lampyris.CSharp.Common;

using System.Collections;
using System.Collections.Generic;

public class CoroutineManager:BehaviourSingleton<CoroutineManager>
{
    private readonly List<IEnumerator> m_coroutines = new List<IEnumerator>();

    public void StartCoroutine(IEnumerator coroutine)
    {
        if (m_coroutines.Contains(coroutine))
            return;
        
        m_coroutines.Add(coroutine);
    }

    public void RemoveCoroutine(IEnumerator coroutine)
    {
        if (!m_coroutines.Contains(coroutine))
            return;
        
        m_coroutines.Remove(coroutine);
    }

    public override void OnStart()
    {
    }

    public override void OnUpdate(float dTime)
    {
        for (int i = m_coroutines.Count - 1; i >= 0; i--)
        {
            bool needMoveNext = false;
            IEnumerator coroutine = m_coroutines[i];
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
                    m_coroutines.Remove(coroutine);
                }
            }
        }
    }

    public override void OnDestroy()
    {
    }
}
