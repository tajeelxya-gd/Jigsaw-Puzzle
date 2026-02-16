using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardDataHolder : MonoBehaviour
{
    [SerializeField] private int _rewardAmount;
    [SerializeField] private int _totalAmount = 1100;
    [SerializeField] private TextMeshProUGUI _rewardAmountText;
    private BulkPopUpEffect _extraRewardGeneratorEffect;
    public int RewardAmount => _rewardAmount;

    private void Start()
    {
        _rewardAmount = PlayerPrefs.GetInt("RewardAmount");
        _rewardAmountText.text = _rewardAmount.ToString();
    }

    private void OnEnable()
    {
        SignalBus.Subscribe<OnRewardAdded>(AddRewardAmount);
        SignalBus.Subscribe<OnGetRewardAmount>(SendRewardAmount);
    }

    private void OnDisable()
    {
        SignalBus.Unsubscribe<OnRewardAdded>(AddRewardAmount);
        SignalBus.Unsubscribe<OnGetRewardAmount>(SendRewardAmount);
    }

    private void SendRewardAmount(OnGetRewardAmount signal)
    {
        signal.RewardAmount = _rewardAmount;
    }

    public void AddRewardAmount(OnRewardAdded signal)
    {
        IBulkPopService popBulkService = GlobalService.IBulkPopService;
        if (signal.ExtraRewardType != null)
        {
            switch (signal.ExtraRewardType.RewardType)
            {
                case WeeklyRewardType.InfiniteHealth:
                    popBulkService.PlayEffect(signal.RewardAmount, PopBulkService.BulkPopUpServiceType.Health,
                        signal.RewardPoint.position, 10);
                    break;
                case WeeklyRewardType.Coin:
                    popBulkService.PlayEffect(signal.RewardAmount, PopBulkService.BulkPopUpServiceType.Coins,
                        signal.RewardPoint.position, 10);
                    break;
                case WeeklyRewardType.Hammer:
                    popBulkService.PlayEffect(signal.RewardAmount, PopBulkService.BulkPopUpServiceType.Hammer,
                        signal.RewardPoint.position, 4);
                    break;
                case WeeklyRewardType.Magnet:
                    popBulkService.PlayEffect(signal.RewardAmount, PopBulkService.BulkPopUpServiceType.Magnets,
                        signal.RewardPoint.position, 4);
                    break;
                case WeeklyRewardType.PopTreasureBox:
                    popBulkService.PlayEffect(signal.RewardAmount, PopBulkService.BulkPopUpServiceType.SlotPopper,
                        signal.RewardPoint.position, 4);
                    break;
                case WeeklyRewardType.MagicWand:
                    popBulkService.PlayEffect(signal.RewardAmount, PopBulkService.BulkPopUpServiceType.Wand,
                        signal.RewardPoint.position, 4);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        _rewardAmount += signal.RewardAmount;
        _rewardAmountText.text = _rewardAmount.ToString();
        popBulkService.PlayEffect(signal.RewardAmount, PopBulkService.BulkPopUpServiceType.Trophy,
            signal.RewardPoint.position, 10);


        SignalBus.Publish(new ProgressBarReward() { _rewardChestAmount = _rewardAmount });
        if (_rewardAmount > _totalAmount)
        {
            _rewardAmount = 0;
        }

        PlayerPrefs.SetInt("RewardAmount", _rewardAmount);
        PlayerPrefs.Save();
    }

    public class WeeklyRewardTypeBlock
    {
        public WeeklyRewardType RewardType;
        public int RewardAmount;
    }

    void ClearProgress()
    {
        PlayerPrefs.SetInt("RewardAmount", 0);
    }
}

public class OnRewardAdded : ISignal
{
    public int RewardAmount;
    public RectTransform RewardPoint;
    public RewardDataHolder.WeeklyRewardTypeBlock ExtraRewardType = null;
}

public class OnGetRewardAmount : ISignal
{
    public int RewardAmount;
}