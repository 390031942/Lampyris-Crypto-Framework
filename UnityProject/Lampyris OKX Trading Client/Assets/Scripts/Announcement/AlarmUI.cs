using UnityEngine;
using UnityEngine.UI;

public class AlarmUI : MonoBehaviour
{
    public InputField timeInputField;
    public Button setAlarmButton;
    public AlarmManager alarmManager;

    void Start()
    {
        setAlarmButton.onClick.AddListener(OnSetAlarmButtonClicked);
    }

    void OnSetAlarmButtonClicked()
    {
        if (int.TryParse(timeInputField.text, out int delayInSeconds))
        {
            alarmManager.SetAlarm(delayInSeconds);
            Debug.Log($"Alarm set for {delayInSeconds} seconds from now.");
        }
        else
        {
            Debug.LogError("Invalid time input.");
        }
    }
}
