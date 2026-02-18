using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class ProgressBarRewards : MonoBehaviour
{
    [SerializeField] RewardProgressHolder  []_rewardProgressHolder;
    private bool[] _claimed;

    private void Awake()
    {
        _claimed = new bool[_rewardProgressHolder.Length];
        LoadClaimedRewards();
    }

    private void OnEnable()
    {
        SignalBus.Subscribe<ProgressBarReward>(CheckProgressBarReward);
    }

    private void OnDisable()
    {
        SignalBus.Unsubscribe<ProgressBarReward>(CheckProgressBarReward);
    }

    private void CheckProgressBarReward(ProgressBarReward signal)
    {
        int amount = signal._rewardChestAmount;

        for (int i = 0; i < _rewardProgressHolder.Length; i++)
        {
            if (!_claimed[i] && amount >= _rewardProgressHolder[i]._threshold)
            {
                _claimed[i] = true; 
                SaveClaimedReward(i);
                GiveReward(i);
            }
        }
    }

   
    [Button("Test Reward")]
    private void GiveReward(int index)
    {
        Debug.LogError($"Reward {index + 1} claimed!");
        SignalBus.Publish(new OnShowRewardProgressSignal{RewardsData = _rewardProgressHolder[index]});
        // Add reward logic here
    }

    private void SaveClaimedReward(int index)
    {
        PlayerPrefs.SetInt($"RewardClaimed_{index}", 1);
        PlayerPrefs.Save();
    }

    private void LoadClaimedRewards()
    {
        for (int i = 0; i < _rewardProgressHolder.Length; i++)
        {
            _claimed[i] = PlayerPrefs.GetInt($"RewardClaimed_{i}", 0) == 1;
        }
    }
}

[Serializable]
public class ProgressBarReward : ISignal
{
    public int _rewardChestAmount;
}

public class OnShowRewardProgressSignal : ISignal
{
    public RewardProgressHolder RewardsData;
    public UnityAction OnRewardComplete;
}


[System.Serializable]


public class RewardProgressHolder
{
    [Title("Progress")]
    [SerializeField] public RewardProgressModelView.RewardCrateType _rewardCrateType;
    [SerializeField] public int _threshold;
    [SerializeField] public List<RewardProgressModelView>  _rewards = new List<RewardProgressModelView>();
}

[System.Serializable]
public class RewardProgressModelView
{
    [HorizontalGroup("Row", 64)]
    [PreviewField(Alignment = ObjectFieldAlignment.Left, Height = 64)]
    [HideLabel]
    [SerializeField] public Sprite _rewardIcon;
    [VerticalGroup("Row/Right")]
    [LabelText("Reward Type")]
    public WeeklyRewardType rewardType;
    [VerticalGroup("Row/Right")]
    [LabelText("Chest Amount")]
    public int rewardChestAmount;
   
    
   
    public enum  RewardCrateType{Blue,Purple,Yellow,Green}
   
}
