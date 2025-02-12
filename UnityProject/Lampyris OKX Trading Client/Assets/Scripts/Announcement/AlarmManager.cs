using UnityEngine;

public class AlarmManager : MonoBehaviour
{
    public void SetAlarm(int delayInSeconds)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            try
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (AndroidJavaClass alarmHelper = new AndroidJavaClass("com.example.alarm.AlarmManagerHelper"))
                {
                    long triggerTimeMillis = System.DateTimeOffset.Now.ToUnixTimeMilliseconds() + delayInSeconds * 1000;
                    alarmHelper.CallStatic("setAlarm", currentActivity, triggerTimeMillis);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to set alarm: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning("This feature is only available on Android.");
        }
    }
}