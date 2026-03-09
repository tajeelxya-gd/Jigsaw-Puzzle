using System.Collections;
using System.Collections.Generic;
using Monetization.Runtime.Ads;
using Monetization.Runtime.InAppPurchasing;
using UnityEngine;

[CreateAssetMenu(fileName = "RemoveAds", menuName = "GDMonetization/InApps/RemoveAds")]
public class InAppProduct_Generic : InAppProductSO
{
    public override void OnPurchaseSuccess(bool isRestoring)
    {
        OnBuyEvent?.Invoke();
        SignalBus.Publish(new OnInAppSuccessFullyBought{IsSuccess = true});
    }
}