using System;
using TMPro;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

public class CustomTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] _timerText;
    private const string StartTimeKey = "DailyTimerStartTime";
    private DateTime _startTime;
    public int _currentDay = 1;
    private int _totalDays = 7; // total number of days in your cycle

    private void Start()
    {
        _currentDay = PlayerPrefs.GetInt("CurrentDay", 1);
        RefreshTimer();
        SignalBus.Publish(new OnDayChangedSignal(_currentDay));
    }

    public void RefreshTimer()
    {
        if (PlayerPrefs.HasKey(StartTimeKey))
        {
            string savedTime = PlayerPrefs.GetString(StartTimeKey);
            _startTime = DateTime.Parse(savedTime);
        }
        else
        {
            _startTime = DateTime.Now;
            PlayerPrefs.SetString(StartTimeKey, _startTime.ToString());
            PlayerPrefs.Save();
        }

        UpdateTimer();
    }

    private void UpdateTimer()
    {
        TimeSpan elapsed = DateTime.Now - _startTime;

        if (elapsed.TotalSeconds >= 24 * 60 * 60)
        {
            GoToNextDay(elapsed);
        }
        else
        {
            TimeSpan remaining = TimeSpan.FromHours(24) - elapsed;
            string formatted = FormatRemainingTime(remaining);

            for (int i = 0; i < _timerText.Length; i++)
            {
                _timerText[i].text = formatted;
            }
        }
    }
    private string FormatRemainingTime(TimeSpan remaining)
    {
        if (remaining.TotalDays >= 1)
        {
            int days = Mathf.FloorToInt((float)remaining.TotalDays);
            int hours = remaining.Hours;
            return $"{days}D {hours}H";
        }
        else
        {
            int hours = remaining.Hours;
            int minutes = remaining.Minutes;
            return $"{hours}H {minutes}M";
        }
    }


    private void GoToNextDay(TimeSpan elapsed)
    {
        // Calculate full days passed
        int daysPassed = Mathf.FloorToInt((float)elapsed.TotalDays);

        // Increment current day by number of days passed
        _currentDay += daysPassed;
        if (_currentDay > _totalDays)
        {
            _currentDay = ((_currentDay - 1) % _totalDays) + 1; // wrap around if exceeding total days
        }

        // Publish signals
        SignalBus.Publish(new OnDayChangedSignal(_currentDay));
        SignalBus.Publish(new OnUnlockNextDayPreset { Day = _currentDay });

        // Save data
        PlayerPrefs.SetInt("CurrentDay", _currentDay);
        PlayerPrefs.Save();

        _startTime = DateTime.Now;
        PlayerPrefs.SetString(StartTimeKey, _startTime.ToString());
        PlayerPrefs.Save();

        for (int i = 0; i < _timerText.Length; i++)
        {
            _timerText[i].text = "00:00:00";
        }
    }

#if ODIN_INSPECTOR
    [Button("Force Next Day")]
#endif
    public void ForceNextDayDebug()
    {
        TimeSpan fakeElapsed = TimeSpan.FromHours(24); // simulate 1 full day
        GoToNextDay(fakeElapsed);
    }
}

public class OnUnlockNextDayPreset : ISignal
{
    public int Day;
}

public class OnDayChangedSignal : ISignal
{
    public int NewDay;
    public OnDayChangedSignal(int newDay)
    {
        NewDay = newDay;
    }
}