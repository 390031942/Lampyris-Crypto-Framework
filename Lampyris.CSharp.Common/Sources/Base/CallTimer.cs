namespace Lampyris.CSharp.Common;

public class CallTimer:BehaviourSingleton<CallTimer>
{
    /* 自增的key */
    private int m_IncreaseKey = 0;

    /* key对应的DelayHandler */
    private readonly Dictionary<int, DelayHandler> m_Id2DelayHandlerDict = new Dictionary<int, DelayHandler>();

    /* 要移除ID的临时列表 */
    private readonly List<int> m_ShouldRemoveIDList = new List<int>();

    private enum DelayHandlerType
    {
        Interval = 0,
        FrameLoop = 1,
    }

    private class DelayHandler
    {
        public DelayHandlerType Type;
        public Action?          Action;
        public float            DelayMs;
        public int              DelayFrame;
        public int              RepeatTime;
        public float            TotalTime;
        public int              TotalFrame;
    }

    public int SetInterval(Action action,float delayMs,int repeatTime = -1)
    {
        lock (m_Id2DelayHandlerDict)
        {
            int id = m_IncreaseKey++;
            m_Id2DelayHandlerDict[id] = new DelayHandler()
            {
                Type = DelayHandlerType.Interval,
                Action = action,
                DelayMs = delayMs,
                RepeatTime = repeatTime,
            };

            return id;
        }
    }

    public int SetFrameLoop(Action action,int delayFrame,int repeatTime = -1)
    {
        lock (m_Id2DelayHandlerDict)
        {
            int id = m_IncreaseKey++;
            m_Id2DelayHandlerDict[id] = new DelayHandler()
            {
                Type = DelayHandlerType.FrameLoop,
                Action = action,
                DelayFrame = delayFrame,
                RepeatTime = repeatTime,
            };

            return id;
        }
    }

    public void ClearTimer(int id)
    {
        lock (m_Id2DelayHandlerDict)
        {
            if (m_Id2DelayHandlerDict.ContainsKey(id))
            {
                m_Id2DelayHandlerDict.Remove(id);
            }
        }
    }

    public override void OnStart()
    {
       
    }

    public override void OnUpdate(float deltaTime)
    {
        lock (m_Id2DelayHandlerDict)
        {
            foreach (var pair in m_Id2DelayHandlerDict)
            {
                bool shouldDoAction = false;
                DelayHandler delayHandler = pair.Value;

                if (delayHandler.Type == DelayHandlerType.Interval)
                {
                    delayHandler.TotalTime += deltaTime;
                    if (delayHandler.TotalTime >= delayHandler.DelayMs)
                    {
                        shouldDoAction = true;
                        delayHandler.TotalTime = 0.0f;
                    }
                }
                else
                {
                    delayHandler.TotalFrame += 1;
                    if (delayHandler.TotalFrame >= delayHandler.DelayFrame)
                    {
                        shouldDoAction = true;
                        delayHandler.TotalFrame = 0;
                    }
                }

                if (shouldDoAction)
                {
                    delayHandler.Action?.Invoke();
                    if (delayHandler.RepeatTime != -1)
                    {
                        if (--delayHandler.RepeatTime <= 0)
                        {
                            m_ShouldRemoveIDList.Add(pair.Key);
                        }
                    }
                }
            }

            foreach (int id in m_ShouldRemoveIDList)
            {
                ClearTimer(id);
            }
            m_ShouldRemoveIDList.Clear();
        }
    }

    public override void OnDestroy()
    {
    
    }
}
