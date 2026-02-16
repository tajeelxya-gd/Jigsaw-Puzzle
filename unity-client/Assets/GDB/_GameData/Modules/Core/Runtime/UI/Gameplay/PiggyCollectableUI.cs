using DG.Tweening;
using UnityEngine;

public class PiggyCollectableUI : CollectableVisual
{
    [SerializeField] private RectTransform _icon;
    [SerializeField] private RectTransform _tick;

    private int _remainingPiggyCount;
    public void Initialize(int piggyCount)
    {
        _tick.gameObject.SetActive(false);
        _icon.anchoredPosition = new Vector2(-15.5f, 1.4f);
        _amountText.gameObject.SetActive(true);
        _remainingPiggyCount = piggyCount;
        UpdateText(_remainingPiggyCount);

        SignalBus.Subscribe<OnPiggyKilledSignal>(OnPiggyKill);
    }
    private Tween _punchScale;
    private void OnPiggyKill(ISignal signal)
    {
        _remainingPiggyCount--;
        UpdateText(_remainingPiggyCount);
        if (_remainingPiggyCount <= 0)
        {
            _amountText.gameObject.SetActive(false);
            _tick.gameObject.SetActive(true);
            _icon.anchoredPosition = new Vector2(0, 1.4f);
        }
        _punchScale?.Kill();
        _punchScale = _icon.DOPunchScale(Vector3.one * 0.2f, 0.25f, 3, 1);
    }
    private void OnDestroy()
    {
        _punchScale?.Kill();
        SignalBus.Unsubscribe<OnPiggyKilledSignal>(OnPiggyKill);
    }
}
public class OnPiggyKilledSignal : ISignal { }