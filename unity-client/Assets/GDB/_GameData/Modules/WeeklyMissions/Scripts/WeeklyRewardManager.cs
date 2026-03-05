using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

public class WeeklyRewardManager : MonoBehaviour
{
    [SerializeField] private WeeklyMissionsPresets _weeklyMissionsPresets;
    [SerializeField] private WeeklyRewardCard[] _weeklyRewardCards;
    [SerializeField] private MissionManager _missionManager;
    [SerializeField] private CustomTimer _customTimer;
    [SerializeField] private GameObject _weeklyRewardPanel;
    public int _currentDay = 1;

    private DataBaseService<WeeklySaveData> _database = new();
    private WeeklySaveData _saveData;

    private Dictionary<string, MissionSaveData> _missionSaveLookup = new();
    public Dictionary<int, MissionProgress[]> _dailyMissionProgress = new();

    [SerializeField] private GameObject _lockedRaceEvent;
    [SerializeField] private GameObject _unlockedRaceEvent;
    [SerializeField] private Button _unlockWeeklyRewardButton;
    [SerializeField] private Button _infoButton;

    [Title("OnBoarding")][SerializeField] private OnBoardingConfig.OnBoardingType _onBoardingType;
    GameData _gameData;
    public int MinLevelToUnlock => (int)_onBoardingType;
    private void Start()
    {
        _gameData = GlobalService.GameData;
        UpdateUI();
        DOVirtual.DelayedCall(Time.deltaTime * 10, LookForOnBoardingPanel);

    }

    public void ShowPanel(bool status)
    {
        if (status)
        {
            _weeklyRewardPanel.transform.localScale = Vector3.one;
            _weeklyRewardPanel.SetActive(status);
            _weeklyRewardPanel.GetComponent<CanvasGroup>().alpha = 1;
            DOVirtual.DelayedCall(0.15f, () => { AudioController.PlaySFX(AudioType.PanelPop); });

        }

        if (status == false)
        {
            Sequence _closeSequence = DOTween.Sequence();
            _closeSequence.Join(_weeklyRewardPanel.GetComponent<CanvasGroup>().DOFade(0, 0.3f).From(1))
                .Join(_weeklyRewardPanel.transform.DOScale(1.2f, 0.3f).From(1).OnComplete(() =>
                {

                    CloseOnBoardingCommandIfYes();
                }))
                .AppendCallback(() => _weeklyRewardPanel.gameObject.SetActive(false));
        }

        AudioController.PlaySFX(status ? AudioType.RewardPopUp : AudioType.PanelClose);
    }

    void CloseOnBoardingCommandIfYes()
    {
        Debug.Log("CloseOnBoardingCommandIfYes :: " + PopCommandExecutionResponder.HasCommand<OnBoardingMenuCommand>());
        //  if (PopCommandExecutionResponder.HasCommand<OnBoardingMenuCommand>())
        //  {
        PopCommandExecutionResponder.RemoveCommand<OnBoardingMenuCommand>();
        // }
    }

    public void Init()
    {
        for (int i = 0; i < _weeklyRewardCards.Length; i++)
            _weeklyRewardCards[i].Inject(this);

        _saveData = _database.Load_Get();
        _currentDay = _customTimer != null
            ? Mathf.Clamp(_customTimer._currentDay, 1, _weeklyMissionsPresets._weeklyMissionsPresetsData.Length)
            : Mathf.Clamp(_saveData.CurrentDay, 1, _weeklyMissionsPresets._weeklyMissionsPresetsData.Length);

        _missionManager.InjectWeeklyManager(this);
        BuildMissionLookup();
        UnlockCurrentDayMissions();
        LoadWeeklyMissions();

        SignalBus.Subscribe<OnDayChangedSignal>(OnDayChanged);
        SignalBus.Subscribe<OnUnlockNextDayPreset>(UnlockNextDay);



    }

    public void TrackLogin()
    {
        if (_saveData == null) return;
        if (_saveData.CurrentDay > _saveData.LastLogin)
        {
            Debug.LogError("New Login");
            SignalBus.Publish(new OnPlayerDidActionSignal()
            {
                MissionType = MissionType.Login,
                Amount = 1
            });
            _saveData.LastLogin = _currentDay;
            _database.Save(_saveData);
        }
    }


    void UpdateUI()
    {
        _lockedRaceEvent.gameObject.SetActive(_gameData.Data.LevelIndex < (int)_onBoardingType);
        _unlockedRaceEvent.gameObject.SetActive(_gameData.Data.LevelIndex >= (int)_onBoardingType);
    }

    void LookForOnBoardingPanel()
    {
        if (_unlockedRaceEvent.activeInHierarchy) SendMessage("InitOnBoarding");
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<OnDayChangedSignal>(OnDayChanged);
        SignalBus.Unsubscribe<OnUnlockNextDayPreset>(UnlockNextDay);
    }

    private void BuildMissionLookup()
    {
        _missionSaveLookup.Clear();
        foreach (var missionSave in _saveData.Missions)
            _missionSaveLookup[missionSave.MissionID] = missionSave;

        Debug.LogError("mission lookup :: " + _missionSaveLookup.Count);
    }

    private void UnlockCurrentDayMissions()
    {
        int dayIndex = Mathf.Clamp(_currentDay - 1, 0, _weeklyMissionsPresets._weeklyMissionsPresetsData.Length - 1);
        var currentDayData = _weeklyMissionsPresets._weeklyMissionsPresetsData[dayIndex];
        foreach (var mission in currentDayData._missions)
            mission._isInteractable = !_missionSaveLookup.ContainsKey(mission._text + mission.Day) || _missionSaveLookup[mission._text + mission.Day].IsInteractable;
    }

    private void UnlockNextDay(OnUnlockNextDayPreset signal)
    {
        if (signal.Day == 1)
        {
            for (int i = 0; i < _weeklyMissionsPresets._weeklyMissionsPresetsData.Length; i++)
            {
                var dayData = _weeklyMissionsPresets._weeklyMissionsPresetsData[i];
                foreach (var mission in dayData._missions)
                    SaveOrUpdateMission(mission, 0, false);
            }
        }
        BuildMissionLookup();
        LoadWeeklyMissions();
        Debug.LogError("Login");
        SignalBus.Publish(new OnMissionObjectiveCompleteSignal { MissionType = MissionType.Login, Amount = 1 });
    }

    private void LoadWeeklyMissions()
    {
        foreach (var card in _weeklyRewardCards)
            card.gameObject.SetActive(false);

        int cardIndex = 0;

        //for (int day = 1; day <= _currentDay; day++)
        for (int day = 1; day <= _weeklyMissionsPresets._weeklyMissionsPresetsData.Length; day++)
        {
            if (day < 1 || day > _weeklyMissionsPresets._weeklyMissionsPresetsData.Length) continue;

            var dayPreset = _weeklyMissionsPresets._weeklyMissionsPresetsData[day - 1];
            MissionProgress[] dayProgress = new MissionProgress[dayPreset._missions.Length];

            for (int i = 0; i < dayPreset._missions.Length; i++)
            {
                var mission = dayPreset._missions[i];
                int savedAmount = 0;
                bool isInteractable = true;

                if (_missionSaveLookup.TryGetValue(mission._text + mission.Day, out var saved))
                {
                    savedAmount = saved.CurrentAmount;
                    isInteractable = saved.IsInteractable;
                }

                mission._isInteractable = isInteractable;
                mission.Day = dayPreset._missions[i].Day;
                dayProgress[i] = new MissionProgress
                {
                    Mission = mission,
                    CurrentAmount = savedAmount,
                    IsClaimed = saved != null && saved.IsClaimed

                };

                // if (String.CompareOrdinal(dayProgress[i].Mission._text, "LOGIN DAY 1") == 0)  {dayProgress[i].CurrentAmount = 1;}

                if (cardIndex < _weeklyRewardCards.Length)
                {
                    var card = _weeklyRewardCards[cardIndex];
                    card.Setup(dayProgress[i]);
                    card.gameObject.SetActive(true);
                    cardIndex++;
                }
            }

            _dailyMissionProgress[day] = dayProgress;
        }

        _missionManager._missions.Clear();
        foreach (var dayMissions in _dailyMissionProgress.Values)
            _missionManager._missions.AddRange(dayMissions);

        EnableCollectButtonsBasedOnTimer();
    }
    private void SaveOrUpdateMission(MissionData mission, int currentAmount, bool interactable, bool isClaimed = false)
    {
        Debug.LogError("sending claimed progress :: " + isClaimed);
        if (_missionSaveLookup.TryGetValue(mission._text + mission.Day, out var existing))
        {
            Debug.LogError("Getting here!!!");
            existing.CurrentAmount = currentAmount;
            existing.IsInteractable = interactable;
            existing.IsClaimed = isClaimed;
            existing.Day = mission.Day;

        }
        else
        {
            var newSave = new MissionSaveData
            {

                MissionID = mission._text + mission.Day,
                CurrentAmount = currentAmount,
                IsInteractable = interactable,
                IsClaimed = isClaimed
            };
            Debug.LogError("Getting here    sfsds!!!");

            _saveData.Missions.Add(newSave);
            _missionSaveLookup[mission._text + mission.Day] = newSave;
        }

        _database.Save(_saveData);

        // foreach (var data in _saveData.Missions)
        // {
        //  Debug.Log(data.MissionID + "  "+data.IsClaimed);   
        // }
    }
    public void SaveMissionProgress(MissionProgress missionProgress)
    {
        foreach (var dayMissions in _dailyMissionProgress.Values)
        {
            for (int i = 0; i < dayMissions.Length; i++)
            {
                if (dayMissions[i].Mission == missionProgress.Mission)
                {
                    if (dayMissions[i].Mission.Day == missionProgress.Mission.Day)
                        dayMissions[i].IsClaimed = missionProgress.IsClaimed;

                    dayMissions[i].CurrentAmount = missionProgress.CurrentAmount;
                    dayMissions[i].Mission = missionProgress.Mission;
                }
                //updating value on all days for specific key
            }
        }

        SaveOrUpdateMission(missionProgress.Mission, missionProgress.CurrentAmount, missionProgress.Mission._isInteractable, missionProgress.IsClaimed);
    }

    public void SaveMissionProgressBulk(MissionProgress[] missions)
    {
        foreach (var mission in missions)
            SaveOrUpdateMission(mission.Mission, mission.CurrentAmount, mission.Mission._isInteractable, mission.IsClaimed);
    }


    [Button("clear")]
    public void ClearDaysProgress()
    {
        PlayerPrefs.DeleteKey("CurrentDay");
        PlayerPrefs.Save();
    }

    private void OnDayChanged(OnDayChangedSignal signal)
    {
        _currentDay = signal.NewDay;
        PlayerPrefs.SetInt("CurrentDay", _currentDay);
        PlayerPrefs.Save();
        LoadWeeklyMissions();
    }

    public void EnableCollectButtonsBasedOnTimer()
    {
        if (_customTimer == null) return;
        int timerDay = _customTimer._currentDay;

        //foreach (var card in _weeklyRewardCards)
        //    card._collectButton.interactable = false;

        int cardIndex = 0;

        for (int day = 1; day <= _currentDay; day++)
        {
            if (!_dailyMissionProgress.ContainsKey(day)) continue;

            var missionsForDay = _dailyMissionProgress[day];

            for (int i = 0; i < missionsForDay.Length && cardIndex < _weeklyRewardCards.Length; i++, cardIndex++)
            {
                var missionProgress = missionsForDay[i];
                var card = _weeklyRewardCards[cardIndex];

                card._collectButton.interactable = timerDay >= day && missionProgress.IsCompleted;
            }
        }
    }

    public bool HasClaimableMissions(int day)
    {
        if (!_dailyMissionProgress.TryGetValue(day, out var missions)) return false;

        foreach (var mission in missions)
        {
            if (mission.IsCompleted && !mission.IsClaimed)
                return true;
        }

        return false;
    }
}


[Serializable]
public class WeeklySaveData
{
    public int CurrentDay = 1;
    public List<MissionSaveData> Missions = new();
    public int LastLogin = 0;
}

[Serializable]
public class MissionSaveData
{
    public int Day = 0;
    public string MissionID;
    public int CurrentAmount;
    public bool IsInteractable;
    public bool IsClaimed = false;
}
