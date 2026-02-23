using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeeklyButtonManager : MonoBehaviour
{
    [SerializeField] private int _currentDay = 1;
    [SerializeField] private RectTransform _weeklGoalPanel;
    [SerializeField] private Button[] _dayButtons;
    [SerializeField] private GameObject[] _days;
    [SerializeField] private Sprite _selectedButton;
    [SerializeField] private Sprite _originalButton;
    [SerializeField] private Sprite _lockedButtonSprite;
    [SerializeField] private GameObject[] _lock;
    [SerializeField] private TMP_Text[] _dayTexts;
    [SerializeField] private bool _defaultUnlockAll = false;
    private Image[] _dayButtonsImg;

    private void Start()
    {
        int size = _dayButtons.Length;
        _dayButtonsImg = new Image[size];
        for (int i = 0; i < _dayButtons.Length; i++)
        {
            if (!_defaultUnlockAll)
                _dayButtons[i].interactable = false;
            _dayButtonsImg[i] = _dayButtons[i].gameObject.GetComponent<Image>();
        }
        SignalBus.Subscribe<OnDayChangedSignal>(SetCurrentDay);
        SetCurrentDay(new OnDayChangedSignal(_currentDay));
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
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<OnDayChangedSignal>(SetCurrentDay);
    }
}
