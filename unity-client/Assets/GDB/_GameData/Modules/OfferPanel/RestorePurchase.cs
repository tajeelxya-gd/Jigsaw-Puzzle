using System;
using Monetization.Runtime.InAppPurchasing;
using UnityEngine;
using UnityEngine.UI;

public class RestorePurchase : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnRestorePurchase);
    }

    void OnRestorePurchase()
    {
        MonetizationPreferences.AdsRemoved.Set(false);
        SignalBus.Publish(new BannerAdStatusChangedSignal { IsAdsRemoved = false });
        GDInAppPurchaseManager.RestorePurchases();
    }
}
