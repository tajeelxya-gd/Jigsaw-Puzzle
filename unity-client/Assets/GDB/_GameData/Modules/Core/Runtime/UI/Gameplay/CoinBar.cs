using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoinBar : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _amountTxt;
    [SerializeField] private BulkPopUpEffect bulkPopUpEffect;

    [SerializeField] private float _baseAnimDuration = 0.5f;

    private int _displayedCoins;
    private int _targetCoins;

    private Tween _countTween;
    [SerializeField] private Button _button;
    public void Initialize()
    {
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(OnShowCoinPanel);
        _displayedCoins = GlobalService.GameData.Data.Coins;
        _targetCoins = _displayedCoins;

        _amountTxt.text = _displayedCoins.ToString();
        SignalBus.Subscribe<AddCoinsSignal>(OnAddCoinsSignal);
        SignalBus.Subscribe<OnCoinsUpdateSignal>(OnCoinsUpdateSignal);
    }

    void OnCoinsUpdateSignal(OnCoinsUpdateSignal signal)
    {
        _targetCoins += signal.Amount;
        GlobalService.GameData.Data.Coins = _targetCoins;
        StartOrUpdateAnimation();
        ReceiveSpendCoins(signal.Amount);
    }

    void ReceiveSpendCoins(int rewardAmount)
    {
        if (rewardAmount < 0)
            SignalBus.Publish(new OnMissionObjectiveCompleteSignal { MissionType = MissionType.SpendCoins, Amount = Mathf.Abs(rewardAmount) });
    }
    private void OnShowCoinPanel()
    {
        SignalBus.Publish(new OnCoinBundleCalledSignal());
    }
    private void OnAddCoinsSignal(AddCoinsSignal signal)
    {
        _targetCoins += signal.Amount;
        GlobalService.GameData.Data.Coins = _targetCoins;
        if (!signal.IsAdd)
        {
            OnAnimationComplete();
            return;
        }
        StartOrUpdateAnimation();

        bulkPopUpEffect.Generate(null, 15);
    }
    private void StartOrUpdateAnimation()
    {
        _countTween?.Kill();

        int startValue = _displayedCoins;
        int endValue = _targetCoins;

        float duration = CalculateDuration(startValue, endValue);

        _countTween = DOTween.To(
            () => startValue,
            value =>
            {
                startValue = value;
                _displayedCoins = value;
                _amountTxt.text = value.ToString();
            },
            endValue,
            duration
        )
        .SetEase(Ease.OutCubic)
        .OnComplete(OnAnimationComplete).SetDelay(1);
    }
    private float CalculateDuration(int from, int to)
    {
        int delta = Mathf.Abs(to - from);
        return Mathf.Clamp(delta * 0.015f, 0.2f, 1.2f);
    }
    private void OnAnimationComplete()
    {
        _displayedCoins = _targetCoins;
        _amountTxt.text = _targetCoins.ToString();

        PunchEffect();
    }
    private void PunchEffect()
    {
        _amountTxt.transform
            .DOPunchScale(Vector3.one * 0.15f, 0.2f, 6, 0.8f);
    }
    private void OnDestroy()
    {
        SignalBus.Unsubscribe<AddCoinsSignal>(OnAddCoinsSignal);
        SignalBus.Unsubscribe<OnCoinsUpdateSignal>(OnCoinsUpdateSignal);

        _countTween?.Kill();
    }
}
public class AddCoinsSignal : ISignal
{
    public int Amount;
    public bool IsAdd = true;
}