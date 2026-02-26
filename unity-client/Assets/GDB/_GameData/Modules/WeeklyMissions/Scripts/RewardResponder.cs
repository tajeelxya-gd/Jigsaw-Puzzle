using System;
using UnityEngine;

public class RewardResponder : MonoBehaviour
{
    void OnClaimReward(RewardProgressHolder _currentRewardProgressData)
    {
        if (_currentRewardProgressData == null) return;
        GameData _gameData = GlobalService.GameData;
        IBulkPopService _bulkPopService = GlobalService.IBulkPopService;
        foreach (var _reward in _currentRewardProgressData._rewards)
        {
            switch (_reward.rewardType)
            {
                case WeeklyRewardType.None:
                    break;
                case WeeklyRewardType.InfiniteHealth:
                    //  _bulkPopService.PlayEffect(PopBulkService.BulkPopUpServiceType.Health,_rewardItemContainer.transform.position);
                    SignalBus.Publish(new OnHealthUpdateSignal { TimeToAdd = _reward.rewardChestAmount });
                    break;
                case WeeklyRewardType.Coin:
                    // _bulkPopService.PlayEffect(PopBulkService.BulkPopUpServiceType.Coins,_rewardItemContainer.transform.position);
                    SignalBus.Publish(new OnCoinsUpdateSignal { Amount = _reward.rewardChestAmount });
                    break;
                case WeeklyRewardType.Eye:
                    _gameData.Data.Eye += _reward.rewardChestAmount;
                    break;
                case WeeklyRewardType.Magnet:
                    _gameData.Data.Magnets += _reward.rewardChestAmount;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
