using System;
using System.Collections;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class DailyRewardTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private string _dayKey = "DailyRewardCurrentDay";
    [SerializeField] private string _lastClaimDateKey = "LastClaimDate";
    [SerializeField] private int _totalDays = 7;

    [ShowInInspector] private int _currentDay = 1;

    private DailyRewardManager _dailyRewardManager;
    private bool _canClaimToday = false;

    private void Awake()
    {
        CheckDailyStatus();

    }

    private void Start()
    {
        StartCoroutine(UpdateTimerCoroutine());
        SignalBus.Subscribe<OnDailyRewardClaim>(OnClaimReward);
    }

    public void Inject(DailyRewardManager dailyRewardManager)
    {
        _dailyRewardManager = dailyRewardManager;
    }

    public int GetCurrentDay() => PlayerPrefs.GetInt(_dayKey, 1);
    public int GetDaysPassedSinceLastClaims()
    {
        if (!PlayerPrefs.HasKey(_lastClaimDateKey))
            return 1;

        long ticks = long.Parse(PlayerPrefs.GetString(_lastClaimDateKey));
        DateTime lastClaimDate = new DateTime(ticks).Date;
        DateTime today = DateTime.UtcNow.Date;

        int daysPassed = (today - lastClaimDate).Days;

        return Mathf.Max(0, daysPassed);
    }

    public int GetActiveRewardCollectionDay()
    {
        if (!PlayerPrefs.HasKey(_lastClaimDateKey))
            return 1;

        long ticks = long.Parse(PlayerPrefs.GetString(_lastClaimDateKey));
        DateTime lastClaimDate = new DateTime(ticks).Date;
        DateTime today = DateTime.UtcNow.Date;

        int daysPassed = (today - lastClaimDate).Days;

        Debug.LogError("days passed :: "+_currentDay+" "+daysPassed);
        return (_currentDay-1) + Mathf.Max(0, daysPassed);
    }

    // =========================================================
    // MAIN DAILY CHECK LOGIC
    // =========================================================
    [Button]
    public void CheckDailyStatus()
    {
        _currentDay = GetCurrentDay();
        DateTime today = DateTime.UtcNow.Date;

        // FIRST TIME LOGIN
        if (!PlayerPrefs.HasKey(_lastClaimDateKey))
        {
            Debug.Log("First Login - Reward Available");

            _canClaimToday = true;

            SignalBus.Publish(new DailyRewardTimerData() { _currentDay = _currentDay });
            return;
        }

        // Load last claim date safely
        long ticks = long.Parse(PlayerPrefs.GetString(_lastClaimDateKey));
        DateTime lastClaimDate = new DateTime(ticks).Date;

        int daysPassed = (today - lastClaimDate).Days;

        if (daysPassed >= 1)
        {
            _canClaimToday = true;

            if (daysPassed > 1)
            {
                Debug.Log("Player missed one or more days.");
                SignalBus.Publish(new RewardMissSignal());

                // Optional reset:
                // _currentDay = 1;
                // PlayerPrefs.SetInt(_dayKey, _currentDay);
            }

            SignalBus.Publish(new DailyRewardTimerData() { _currentDay = _currentDay });
        }
        else
        {
            _canClaimToday = false;
        }
    }

    // =========================================================
    // CLAIM REWARD
    // =========================================================
    private void OnClaimReward(OnDailyRewardClaim signal)
    {
        if (!_canClaimToday)
            return;

        Debug.Log("Reward Claimed");

        // Progress day
        _dailyRewardManager?.GoToNextDay(_currentDay);
        _currentDay++;
        if (_currentDay > _totalDays)
        {
            _currentDay = 1;
            _dailyRewardManager.ResetEverything();
            ResetData();
        }

        // Save new day
        PlayerPrefs.SetInt(_dayKey, _currentDay);

        // Save today's date safely (Ticks)
        PlayerPrefs.SetString(_lastClaimDateKey, DateTime.UtcNow.Date.Ticks.ToString());

        PlayerPrefs.Save();

        _canClaimToday = false;

    }

    // =========================================================
    // TIMER UI
    // =========================================================
    private IEnumerator UpdateTimerCoroutine()
    {
        while (true)
        {
            UpdateTimerUI();

            // Re-check when midnight hits
            if (DateTime.UtcNow.Hour == 0 &&
                DateTime.UtcNow.Minute == 0 &&
                DateTime.UtcNow.Second == 0)
            {
                CheckDailyStatus();
            }

            yield return new WaitForSecondsRealtime(1f);
        }
    }

    private void UpdateTimerUI()
    {
        if (_canClaimToday)
        {
            _timerText.text = "READY!";
            return;
        }

        DateTime tomorrow = DateTime.UtcNow.Date.AddDays(1);
        TimeSpan timeRemaining = tomorrow - DateTime.UtcNow;

        _timerText.text = string.Format("{0:D2}H:{1:D2}M:{2:D2}S",
            timeRemaining.Hours,
            timeRemaining.Minutes,
            timeRemaining.Seconds);
    }

    // =========================================================
    // DEBUG
    // =========================================================
    [Button]
    public void ResetData()
    {
        PlayerPrefs.DeleteKey(_dayKey);
        PlayerPrefs.DeleteKey(_lastClaimDateKey);
        PlayerPrefs.Save();

        _currentDay = 1;
        CheckDailyStatus();
    }

    private void OnDisable()
    {
        SignalBus.Unsubscribe<OnDailyRewardClaim>(OnClaimReward);
    }
}

public class DailyRewardTimerData : ISignal
{
    public int _currentDay;
}
