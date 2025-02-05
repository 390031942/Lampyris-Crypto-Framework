namespace Lampyris.CSharp.Common;

using System.Collections;
using System.Collections.Generic;

[Component]
public class CoroutineManager:ILifecycle
{
    private readonly List<IEnumerator> m_Coroutines = new List<IEnumerator>();

    public void StartCoroutine(IEnumerator coroutine)
    {
        if (m_Coroutines.Contains(coroutine))
            return;
        
        m_Coroutines.Add(coroutine);
    }

    public void RemoveCoroutine(IEnumerator coroutine)
    {
        if (!m_Coroutines.Contains(coroutine))
            return;
        
        m_Coroutines.Remove(coroutine);
    }

    public override void OnUpdate()
    {
        for (int i = m_Coroutines.Count - 1; i >= 0; i--)
        {
            bool needMoveNext = false;
            IEnumerator coroutine = m_Coroutines[i];
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
                    m_Coroutines.Remove(coroutine);
                }
            }
        }
    }
}
