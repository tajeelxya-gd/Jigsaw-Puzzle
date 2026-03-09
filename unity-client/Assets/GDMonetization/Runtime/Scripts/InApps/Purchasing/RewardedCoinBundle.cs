using System;
using DG.Tweening;
using Monetization.Runtime.Ads;
using UnityEngine;
using UnityEngine.UI;

public class RewardedCoinBundle : MonoBehaviour
{
    [SerializeField] private int _rewardAmount = 100;
    [SerializeField] private Button _rewardedAdButton;
    private IBulkPopService _popBulkService;
    private void Start()
    {
        _rewardedAdButton.onClick.AddListener(ShowRewardedAd);
        _popBulkService = GlobalService.IBulkPopService;
    }

    private void ShowRewardedAd()
    {
        AdsManager.ShowRewarded("CoinRewardedAd ", () =>
        {
            _popBulkService.PlayEffect(_rewardAmount, PopBulkService.BulkPopUpServiceType.Coins,
                transform.position, 10);
            SignalBus.Publish(new OnMissionObjectiveCompleteSignal { MissionType = MissionType.Get2XCoins, Amount = 1 });
            DOVirtual.DelayedCall(Time.deltaTime,()=>
            {
                SignalBus.Publish(new OnMissionObjectiveCompleteSignal
                    { MissionType = MissionType.EarnCoins, Amount = _rewardAmount });
                if(GameStateGlobal.IsGamePlay)
                    SignalBus.Publish(new AddCoinsSignal { Amount = _rewardAmount });
            });
            AudioController.PlaySFX(AudioType.ButtonClick);
            HapticController.Vibrate(HapticType.Btn);
        });
    }
}
