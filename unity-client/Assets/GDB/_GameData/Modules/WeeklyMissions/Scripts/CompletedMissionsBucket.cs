using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
[CreateAssetMenu(fileName = "WeeklyMissionsRuntimeBucket", menuName = "Scriptable Objects/WeeklyMissionsBucket")]
public class CompletedMissionsBucket : ScriptableObject
{
    [SerializeField]
    private WeeklyMissionsRuntimeContainer _weeklyMissionsContainer;
    [HideInInspector]
    public WeeklyMissionsRuntimeContainer CurrentWeeklyMissionsContainer => _weeklyMissionsContainer;
    private IDataBase<WeeklyMissionsRuntimeContainer> _weeklyMissionBucketData;

    public void LoadBucket()
    {
        _weeklyMissionBucketData = new DataBaseService<WeeklyMissionsRuntimeContainer>();
        _weeklyMissionsContainer = _weeklyMissionBucketData.Load_Get();
    }

    void AddItemToBucket(MissionType missionType)
    {
        if (!_weeklyMissionsContainer.HasItem(missionType))
        {
            Debug.LogError("No item found for mission type: " + missionType);
            _weeklyMissionsContainer._data.Add(new WeeklyMissionBucketData { CompletedMissionType = missionType, CollectedAmount = 0 });
        }
    }
    public void AddToBucket(MissionType missionType, int amount = 1)
    {
         Debug.LogError("Added to bucket " + missionType);
        AddItemToBucket(missionType);//create new item if no new item available
        foreach (var weeklyMissionBucketData in _weeklyMissionsContainer._data)
        {
            if (weeklyMissionBucketData.CompletedMissionType == missionType)
                weeklyMissionBucketData.CollectedAmount += amount;
        }

        SaveBucket();
    }

    public void SaveBucket()
    {
        _weeklyMissionBucketData.Save(_weeklyMissionsContainer);
    }
}

[System.Serializable]
public class WeeklyMissionsRuntimeContainer
{
    public List<WeeklyMissionBucketData> _data = new List<WeeklyMissionBucketData>();

    public bool HasItem(MissionType missionType)
    {
        foreach (var weeklyMissionBucketData in _data)
        {
            if (weeklyMissionBucketData.CompletedMissionType == missionType)
            {
                return true;
            }
        }
        return false;
    }
}

[System.Serializable]
public class WeeklyMissionBucketData
{
    public MissionType CompletedMissionType;
    public int CollectedAmount = 0;
}
