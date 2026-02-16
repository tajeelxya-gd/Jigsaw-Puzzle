using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManiaReward : MonoBehaviour
{
    private Queue<RewardProgressModelView> _rewardQueue = new();
    private bool _isShowing;

    private void Awake()
    {
        SignalBus.Subscribe<OnRewardClaimedSignal>(OnRewardClosed);
    }

    private void OnDisable()
    {
        SignalBus.Unsubscribe<OnRewardClaimedSignal>(OnRewardClosed);
    }

    public void GiveReward(RewardProgressModelView reward)
    {
        StartCoroutine(ShowRewardWithDelay(reward));
    }

    private IEnumerator ShowRewardWithDelay(RewardProgressModelView reward)
    {
        yield return new WaitForSeconds(0.75f);
        _rewardQueue.Enqueue(reward);
        if (!_isShowing)
            ShowNext();
    }

    private void ShowNext()
    {
        if (_rewardQueue.Count == 0)
        {
            _isShowing = false;
            return;
        }
        _isShowing = true;
        RewardProgressHolder holder = new RewardProgressHolder();
        holder._rewardCrateType = RewardProgressModelView.RewardCrateType.Yellow;
        holder._rewards.Add(_rewardQueue.Dequeue());
        SignalBus.Publish(new OnShowRewardProgressSignal
        {
            RewardsData = holder
        });
    }

    private void OnRewardClosed(OnRewardClaimedSignal signal)
    {
        ShowNext();
    }
}