using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeekllyRewardReferences : MonoBehaviour
{
    [SerializeField] private WeeklyRewardManager WeeklyRewardManager;
    [SerializeField] private CompletedMissionsBucket _completedMissionsBucket;

    private void Start()
    {
        Initialize();
        StartCoroutine(EnqueAllCompletedMissions());
    }

    private void Initialize()
    {
        if(GlobalService.GameData.Data.LevelNumber >= (int)OnBoardingConfig.OnBoardingType.WeeklyRewards)
            WeeklyRewardManager.Init();
    }

    void TrackPlayerLogin()
    {
        WeeklyRewardManager.TrackLogin();
        
    }
    
    IEnumerator EnqueAllCompletedMissions()
    {
        WaitForSeconds wait = new WaitForSeconds(0.1f);
        yield return new WaitForSeconds(0.15f);

        TrackPlayerLogin();

        while (true)
        {
            var data = _completedMissionsBucket.CurrentWeeklyMissionsContainer._data;

            if (data.Count > 0)
            {
                // Create a snapshot copy
                var snapshot = new List<WeeklyMissionBucketData>(data);

                foreach (var missionObjective in snapshot)
                {
                    SignalBus.Publish(new OnPlayerDidActionSignal
                    {
                        MissionType = missionObjective.CompletedMissionType,
                        Amount = missionObjective.CollectedAmount
                    });

                    yield return null;
                }

                data.Clear();
                _completedMissionsBucket.SaveBucket();
            }

            yield return wait;
        }
    }

}
