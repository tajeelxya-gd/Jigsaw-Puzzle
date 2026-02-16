using System;
using System.Collections;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class RaceModuleTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private string _dayKey = "RaceModuleCurrentDay";
    [SerializeField] private string _startTimeKey = "RaceStartTime";
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
            GoToNextDay();
        }
        else
        {
            TimeSpan remaining = TimeSpan.FromHours(24) - elapsed;
            _timerText.text = $"{remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
        }
    }

    private void GoToNextDay()
    {
        CurrentDay++;

        if (CurrentDay > _totalDays)
            CurrentDay = 1;

        _startTime = DateTime.Now;
        SaveData();
        //RESETTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTT
        //SignalBus.Publish(new PuzzleManiaReset());

        if (_timerText != null)
            _timerText.text = "00:00:00";
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
        GoToNextDay();
    }

    public float ReturnTimeElapsed()
    {
        return DateTime.Now.Subtract(_startTime).Seconds;
    }
}