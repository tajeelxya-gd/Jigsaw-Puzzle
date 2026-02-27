using System;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "WeeklyMissionsPresets", menuName = "Scriptable Objects/WeeklyMissionsPresets")]
public class WeeklyMissionsPresets : ScriptableObject
{
    public WeeklyMissionsPresetsData[] _weeklyMissionsPresetsData;

    private void OnValidate()
    {
        for (int i = 0; i < _weeklyMissionsPresetsData.Length; i++)
        {
            for (int j = 0; j < _weeklyMissionsPresetsData[i]._missions.Length; j++)
            {
                _weeklyMissionsPresetsData[i]._missions[j].Day = i + 1;
            }
        }
    }
}
[Serializable]
public class WeeklyMissionsPresetsData
{
    public MissionData[] _missions;
}
[Serializable]


public class MissionData
{
    [Title("Day")]
    public int Day = 1;
    public bool _isInteractable = false;
    public string _text;
    public Sprite[] _rewardIcon;
    public bool _multipleReward;
    public int _rewardAmount;
    public WeeklyRewardType _extraReward;
    public MissionType _missionType;
    public int _targetAmount;
    public Sprite _taskImage;
}
[Serializable]
public class MissionProgress
{
    public MissionData Mission;
    public int CurrentAmount;
    public bool IsCompleted => CurrentAmount >= Mission._targetAmount;

    public bool IsClaimed = false;
    public void IncrementProgress(int amount = 1)
    {
        if (amount < 0) { CurrentAmount = 0; return; }
        CurrentAmount += amount;
        if (CurrentAmount > Mission._targetAmount)
            CurrentAmount = Mission._targetAmount;
    }
}

public enum WeeklyRewardType
{
    None,
    InfiniteHealth,
    Coin,
    Eye,
    Magnet
}

public enum MissionType
{
    None,
    Login,
    UseBooster,
    WinLevel,
    Get2XCoins,
    WinStreak,
    WinHardLevel,
    SpendCoins,
    EarnCoins,
    JoinTinyRacer,
    WinTinyRace,
    WinSuperHardLevel,
    Top3inLeague,
    UseMagnet,
    UseEye,
    JoinPieces
}
