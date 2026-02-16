using System;
using UnityEngine;

public class DailyRewardReferences : MonoBehaviour
{
    [SerializeField] private DailyRewardManager _dailyRewardManager;
    [SerializeField] private DailyRewardTimer _dailyRewardTimer;
    [SerializeField] private DailyRewardMiss _dailyRewardMiss;

    private void Awake()
    {
       
    }

    private void Start()
    {
        _dailyRewardTimer.Inject(_dailyRewardManager);
        _dailyRewardMiss.Inject(_dailyRewardManager);
        _dailyRewardManager.Inject(_dailyRewardTimer);
    }
}
