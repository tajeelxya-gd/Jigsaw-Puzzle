using UnityEngine;
using UnityEngine.UI;

public sealed class BannerAdUIBuffer : MonoBehaviour
{
    private const float _bannerHeight = 150f;
    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

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
        UpdateBufferHeight(signal.IsAdsRemoved);
    }

    private void RefreshState()
    {
        var isAdsRemoved = MonetizationPreferences.AdsRemoved.Get();
        UpdateBufferHeight(isAdsRemoved);
    }

    private void UpdateBufferHeight(bool isAdsRemoved)
    {
        if (_rectTransform == null) return;

        float targetBottom = isAdsRemoved ? 0f : _bannerHeight;

        Vector2 offsetMin = _rectTransform.offsetMin;
        offsetMin.y = targetBottom;
        _rectTransform.offsetMin = offsetMin;

        LayoutRebuilder.MarkLayoutForRebuild(_rectTransform);
    }
}
