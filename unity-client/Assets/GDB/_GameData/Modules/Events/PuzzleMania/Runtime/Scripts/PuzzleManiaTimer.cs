using System;
using System.Collections;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class PuzzleManiaTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] _timerText;
    [SerializeField] private string _dayKey = "PuzzleManiaCurrentDay";
    [SerializeField] private string _startTimeKey = "PuzzleManiaStartTime";
    [SerializeField] private int _totalDays = 7;
    [ShowInInspector] public int CurrentDay { get; private set; } = 1;
    private DateTime _startTime;

    private void OnEnable()
    {
        LoadDayAndStartTime();
        StartCoroutine(UpdateTimerCoroutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator UpdateTimerCoroutine()
    {
        while (true)
        {
            UpdateTimer();
            yield return new WaitForSeconds(1f);
        }
    }

    private void LoadDayAndStartTime()
    {
        CurrentDay = PlayerPrefs.GetInt(_dayKey, 1);

        if (PlayerPrefs.HasKey(_startTimeKey))
            _startTime = DateTime.Parse(PlayerPrefs.GetString(_startTimeKey));
        else
        {
            _startTime = DateTime.Now;
            SaveData();
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
        int daysPassed = Mathf.FloorToInt((float)elapsed.TotalDays);
        if (daysPassed < 1)
            return;

        CurrentDay += daysPassed;
        if (daysPassed >= 7 || CurrentDay > _totalDays)
        {
            CurrentDay = 1; 
            _startTime = DateTime.Now;
            SaveData();
            SignalBus.Publish(new PuzzleManiaReset());
            UpdateAllTimerTexts("00:00:00");
            return;
        }
        if (CurrentDay > _totalDays)
            CurrentDay = ((CurrentDay - 1) % _totalDays) + 1;

        _startTime = DateTime.Now;
        SaveData();
        UpdateTimer();
    }
    private void UpdateAllTimerTexts(string value)
    {
        if (_timerText == null) return;

        for (int i = 0; i < _timerText.Length; i++)
            _timerText[i].text = value;
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(_dayKey, CurrentDay);
        PlayerPrefs.SetString(_startTimeKey, _startTime.ToString());
        PlayerPrefs.Save();
    }

    [Button]
    public void GoToNextDayButton()
    {
        TimeSpan fakeElapsed = TimeSpan.FromHours(24);
        GoToNextDay(fakeElapsed);
    }
}

public class PuzzleManiaReset : ISignal
{
}

public class NextPuzzleUnlock:ISignal{}
