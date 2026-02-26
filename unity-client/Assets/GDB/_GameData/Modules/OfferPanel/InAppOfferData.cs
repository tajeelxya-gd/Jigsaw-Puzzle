using System;
using DG.Tweening;
using Monetization.Runtime.Ads;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "InAppOffer", menuName = "Scriptable Objects/InAppOffer")]
public class InAppOfferData : ScriptableObject
{
    private const string KEY = "IAP-OFFERS";
    public string OfferName => offerName;
    [SerializeField] private string offerName;
    [Title("InApp-Key")]
    [SerializeField] private protected string _id = "";
    public string Id => _id;
    [Title("Consumeable")]
    [SerializeField] private bool _isConsumeable = false;
    public bool IsConsumeable => _isConsumeable;

    [Title("Remove Ad")]
    [SerializeField] private bool _isRemoveAd = false;
    public bool IsRemoveAd => _isRemoveAd;
    [Title("Other Offers")]
    [SerializeField] private RewardProgressModelView[] _allOffersRewards;

    public void BuyOffer()
    {
        // If there are no rewards, only process RemoveAds (if applicable)
        if (_allOffersRewards == null || _allOffersRewards.Length == 0)
        {
            if (!_isRemoveAd) return;
            CompletePurchase();
            return;
        }

        var gameData = GlobalService.GameData;
        var bulkPopService = GlobalService.IBulkPopService;
        var screenCentre = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        foreach (var reward in _allOffersRewards)
        {
            var capturedReward = reward; // avoid closure issue

            DOVirtual.DelayedCall(0f, () =>
            {
                ProcessReward(capturedReward, bulkPopService, gameData, screenCentre);
            });
        }

        CompletePurchase();
    }

    private void CompletePurchase()
    {
        if (_isRemoveAd)
            BuyRemoveAd();

        SetBuyState();
        SignalBus.Publish(new OnInAppBuySignal { Id = offerName });
    }


    private void ProcessReward(RewardProgressModelView reward, IBulkPopService bulkPop, GameData data, Vector2 center)
    {
        int amount = reward.rewardChestAmount;
        Debug.LogError("IAP Purchase Executing");
        switch (reward.rewardType)
        {
            case WeeklyRewardType.Eye:
                HandlePowerup(PowerupType.MagicWand, PopBulkService.BulkPopUpServiceType.Wand, amount, center, 4);
                break;
            case WeeklyRewardType.Magnet:
                HandlePowerup(PowerupType.Magnet, PopBulkService.BulkPopUpServiceType.Magnets, amount, center, 4);
                break;
            case WeeklyRewardType.InfiniteHealth:
                if (IsGameScene()) { SignalBus.Publish(new OnHealthUpdateSignal() { TimeToAdd = amount }); break; }
                bulkPop.PlayEffect(amount, PopBulkService.BulkPopUpServiceType.Health, center, 10);
                break;
            case WeeklyRewardType.Coin:
                Debug.LogError("adding coins effect");
                if (IsGameScene()) { SignalBus.Publish(new AddCoinsSignal { IsAdd = true, Amount = amount }); break; }
                bulkPop.PlayEffect(amount, PopBulkService.BulkPopUpServiceType.Coins, center, 10);
                break;
            case WeeklyRewardType.None:
                break;
            default:
                Debug.LogWarning($"Unhandled reward type: {reward.rewardType}");
                break;
        }

    }

    private void HandlePowerup(PowerupType pType, PopBulkService.BulkPopUpServiceType vfxType, int amount, Vector2 center, int effectCount)
    {
        if (IsGameScene())
        {
            SignalBus.Publish(new PowerUpAddSignal { powerupType = pType, Amount = amount });
        }
        else
        {
            GlobalService.IBulkPopService.PlayEffect(amount, vfxType, center, effectCount);
        }
    }

    void BuyRemoveAd()
    {
        MonetizationPreferences.AdsRemoved.Set(true);
        AdsManager.HideBanner();
        AdsManager.HideMRec();
    }

    public bool IsOfferAvailable()
    {
        return PlayerPrefs.GetInt(KEY + Id, 0) == 0;
    }

    void SetBuyState()
    {
        PlayerPrefs.SetInt(KEY + Id, 1);
    }

    bool IsGameScene()
    {
        return SceneManager.GetActiveScene().name == "GamePlay" || SceneManager.GetActiveScene().name == "Game";
    }

}
