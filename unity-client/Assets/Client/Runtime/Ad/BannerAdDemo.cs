using Sirenix.OdinInspector;
using UnityEngine;

public sealed class BannerAdDemo : MonoBehaviour
{
    private void OnEnable()
    {
        RefreshState();
        SignalBus.Subscribe<BannerAdStatusChangedSignal>(OnBannerStatusChanged);
    }

    private void OnDisable()
    {
        SignalBus.Unsubscribe<BannerAdStatusChangedSignal>(OnBannerStatusChanged);
    }

    private void OnBannerStatusChanged(BannerAdStatusChangedSignal signal)
    {
        RefreshState();
    }

    private void RefreshState()
    {
        var isAdsRemoved = MonetizationPreferences.AdsRemoved.Get();
        gameObject.SetActive(!isAdsRemoved);
    }

    [Button]
    private void ChangeAdStatus(bool isAdsRemoved)
    {
        MonetizationPreferences.AdsRemoved.Set(isAdsRemoved);
        SignalBus.Publish(new BannerAdStatusChangedSignal { IsAdsRemoved = isAdsRemoved });
        RefreshState();
    }
}
