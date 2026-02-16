using System;
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
    [SerializeField] private GameObject[] _lock;
    [SerializeField] private bool _defaultUnlockAll = false;
    private Image[] _dayButtonsImg;

    private void Start()
    {
        int size = _dayButtons.Length;
        _dayButtonsImg=new Image[size];
        for(int i=0;i<_dayButtons.Length;i++)
        {
            if(!_defaultUnlockAll)
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
        _currentDay= signal.NewDay;
        int index = _currentDay - 1;
        for (int x = 0; x < _dayButtons.Length; x++)
        {
            if (x == index)
            {
                _dayButtonsImg[x].sprite = _selectedButton;
            }
            else
            {
                _dayButtonsImg[x].sprite = _originalButton;
            }
        }
       
        for(int i=0;i<_dayButtons.Length;i++)
        {
            if(!_defaultUnlockAll)
                _dayButtons[i].interactable = (i+1)<=_currentDay;
            _lock[i].SetActive((i + 1) > _currentDay && i != 0);
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
                _dayButtonsImg[i].sprite = _originalButton;
            }
        }
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<OnDayChangedSignal>(SetCurrentDay);
    }
}
