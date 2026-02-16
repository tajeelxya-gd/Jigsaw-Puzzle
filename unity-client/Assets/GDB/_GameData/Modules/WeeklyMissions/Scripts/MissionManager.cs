using System.Collections.Generic;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public List<MissionProgress> _missions = new();
    private WeeklyRewardManager _weeklyRewardManager;

    public void InjectWeeklyManager(WeeklyRewardManager manager)
    {
        _weeklyRewardManager = manager;
        SignalBus.Subscribe<OnPlayerDidActionSignal>(PlayerDidAction);
        LoadMissions();
        _weeklyRewardManager.EnableCollectButtonsBasedOnTimer();
    }

    private void OnDisable()
    {
        SignalBus.Unsubscribe<OnPlayerDidActionSignal>(PlayerDidAction);
    }

    private void PlayerDidAction(OnPlayerDidActionSignal signal)
    {
        if (GlobalService.GameData.Data.LevelNumber < _weeklyRewardManager.MinLevelToUnlock) return;
        bool updated = false;
        foreach (var mission in _missions)
        {
            if (mission.Mission._missionType == signal.MissionType)
            {
                mission.IncrementProgress(signal.Amount);
                updated = true;
            }
        }

        if (updated)
        {
            SaveMissions();
            foreach (var dayMissions in _weeklyRewardManager._dailyMissionProgress.Values)
                _weeklyRewardManager.SaveMissionProgressBulk(dayMissions);

            //_weeklyRewardManager.RefreshAllCards();
            _weeklyRewardManager.EnableCollectButtonsBasedOnTimer();
        }
    }

    private void SaveMissions()
    {
        foreach (var dayMissions in _weeklyRewardManager._dailyMissionProgress)
        {
            int day = dayMissions.Key;
            foreach (var mission in dayMissions.Value)
            {
                string key = $"MissionDay{day}_{mission.Mission._text}";
                PlayerPrefs.SetInt(key, mission.CurrentAmount);
            }
        }
        PlayerPrefs.Save();
    }

    public void LoadMissions()
    {
        foreach (var dayMissions in _weeklyRewardManager._dailyMissionProgress)
        {
            int day = dayMissions.Key;
            for (int i = 0; i < dayMissions.Value.Length; i++)
            {
                string key = $"MissionDay{day}_{dayMissions.Value[i].Mission._text}";
                dayMissions.Value[i].CurrentAmount = PlayerPrefs.GetInt(key, 0);
            }
        }
    }
}


public class OnPlayerDidActionSignal : ISignal
{
    public MissionType MissionType;
    public int Amount;
}

public class OnMissionObjectiveCompleteSignal : ISignal
{
    public MissionType MissionType;
    public int Amount;
}
