using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DailyRewardPresets", menuName = "Scriptable Objects/DailyRewardPresets")]
public class DailyRewardPresets : ScriptableObject
{
    public DailyRewardPresetsData[] _dailyRewardPresetsData;
}
[Serializable]
public class DailyRewardPresetsData
{
    
    public DailyRewardData[] _dailyRewards;
}

[Serializable]
public class DailyRewardData
{
    public string _dayText; 
    public int _currentDay;
    public RewardProgressModelView []_progressModels;
    
}