using System;
using UnityEngine;

public class WeeklyMissionRuntimeBucketController : MonoBehaviour
{
    [SerializeField] private CompletedMissionsBucket _dataAsset;

    private void Awake()
    {
    }

    private void Start()
    {
      
        if (_dataAsset == null)
            return;

        SignalBus.Subscribe<OnMissionObjectiveCompleteSignal>(OnMissionCompletedSignal);
        _dataAsset.LoadBucket();
    }

    private void OnDisable()
    {
        SignalBus.Unsubscribe<OnMissionObjectiveCompleteSignal>(OnMissionCompletedSignal);
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<OnMissionObjectiveCompleteSignal>(OnMissionCompletedSignal);

    }

    private void OnMissionCompletedSignal(OnMissionObjectiveCompleteSignal signal)
    {
            Debug.LogError("Recieving objective To Add");
        if(CanCollectData())
            _dataAsset.AddToBucket(signal.MissionType, signal.Amount);
        else
            Debug.Log("Can't collect data !! weekly mission runtime bucket");
    }

    bool CanCollectData() => GlobalService.GameData.Data.LevelNumber >= (int)OnBoardingConfig.OnBoardingType.WeeklyRewards;
}