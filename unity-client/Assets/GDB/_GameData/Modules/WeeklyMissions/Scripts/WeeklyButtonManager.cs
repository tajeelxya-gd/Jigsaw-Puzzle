using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeeklyButtonManager : MonoBehaviour
{
    [SerializeField] private int _currentDay = 1;
    [SerializeField] private RectTransform _weeklGoalPanel;
    [SerializeField] private Button[] _dayButtons;
    [SerializeField] private Image[] _dayAlerts; // active when claimable and day is unlcoked
    [SerializeField] private GameObject[] _days;
    [SerializeField] private Sprite _selectedButton;
    [SerializeField] private Sprite _originalButton;
    [SerializeField] private Sprite _lockedButtonSprite;
    [SerializeField] private GameObject[] _lock;
    [SerializeField] private TMP_Text[] _dayTexts;
    [SerializeField] private bool _defaultUnlockAll = false;
    private Image[] _dayButtonsImg;
    private WeeklyRewardManager _weeklyRewardManager;

    private void Start()
    {
        _weeklyRewardManager = FindFirstObjectByType<WeeklyRewardManager>();
        int size = _dayButtons.Length;
        _dayButtonsImg = new Image[size];
        for (int i = 0; i < _dayButtons.Length; i++)
        {
            if (!_defaultUnlockAll)
                _dayButtons[i].interactable = false;
            _dayButtonsImg[i] = _dayButtons[i].gameObject.GetComponent<Image>();
        }
        SignalBus.Subscribe<OnDayChangedSignal>(SetCurrentDay);
        SignalBus.Subscribe<OnPlayerDidActionSignal>(RefreshAlerts);
        SignalBus.Subscribe<OnWeeklyProgressUpdatedSignal>(RefreshAlerts);

        SetCurrentDay(new OnDayChangedSignal(_currentDay));
        RefreshAlerts();
    }

    private void RefreshAlerts(OnPlayerDidActionSignal signal) => RefreshAlerts();
    private void RefreshAlerts(OnWeeklyProgressUpdatedSignal signal) => RefreshAlerts();

    private void RefreshAlerts()
    {
        if (_weeklyRewardManager == null) return;

        for (int i = 0; i < _dayAlerts.Length; i++)
        {
            int day = i + 1;
            bool isUnlocked = day <= _currentDay;
            bool hasClaimable = _weeklyRewardManager.HasClaimableMissions(day);

            if (_dayAlerts[i] != null)
                _dayAlerts[i].gameObject.SetActive(isUnlocked && hasClaimable);
        }
    }

    public void UpdateCurrentMissions()
    {
        OpenDay(_currentDay);
    }

    private void SetCurrentDay(OnDayChangedSignal signal)
    {
        _currentDay = signal.NewDay;
        int index = _currentDay - 1;
        for (int x = 0; x < _dayButtons.Length; x++)
        {
            if (x == index)
            {
                _dayButtonsImg[x].sprite = _selectedButton;
            }
            else
            {
                var isLocked = (x + 1) > _currentDay && x != 0;

                _dayButtonsImg[x].sprite = isLocked ? _lockedButtonSprite : _originalButton;
            }
        }

        for (int i = 0; i < _dayButtons.Length; i++)
        {
            if (!_defaultUnlockAll)
                _dayButtons[i].interactable = (i + 1) <= _currentDay;
            var isLocked = (i + 1) > _currentDay && i != 0;
            _lock[i].SetActive(isLocked);
            var text = isLocked ? $"\n{i + 1}" : $"DAY\n{i + 1}";
            _dayTexts[i].SetText(text);
        }

        RefreshAlerts();
    }

    public void OpenDay(int day)
    {
        for (int i = 0; i < _days.Length; i++)
        {
            int temp = i + 1;
            if (temp == day)
            {
                _days[i].SetActive(true);
                _dayButtonsImg[i].sprite = _selectedButton;
            }
            else
            {
                _days[i].SetActive(false);
                var isLocked = (i + 1) > _currentDay && i != 0;
                _dayButtonsImg[i].sprite = isLocked ? _lockedButtonSprite : _originalButton;
            }
        }

        RefreshAlerts();
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<OnDayChangedSignal>(SetCurrentDay);
        SignalBus.Unsubscribe<OnPlayerDidActionSignal>(RefreshAlerts);
        SignalBus.Unsubscribe<OnWeeklyProgressUpdatedSignal>(RefreshAlerts);
    }
}
