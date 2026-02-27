using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class StreakBarManager : MonoBehaviour
{
    [SerializeField] private float _totalDays;
    [SerializeField] private StreakProgressBar _progressBar;
    [SerializeField] private StreakBox[] _box;
    [SerializeField] RewardProgressHolder[] _rewardProgressHolder;
    [SerializeField] private TextMeshProUGUI _streakCountText;
    [SerializeField] private TextMeshProUGUI _streakCount_EffectText;
    [SerializeField] private GameObject _streakCountEffect;
    [SerializeField] private RectTransform _streakEffect;
    [SerializeField] private RectTransform _targetMovePoint;
    private bool[] _claimed;
    [ReadOnly, SerializeField]
    private int _streakCount = 0;
    private StreakData _streakData;
    private DataBaseService<StreakData> _dataBaseService;

    private void Awake()
    {
        _dataBaseService = new DataBaseService<StreakData>();
        _claimed = new bool[_rewardProgressHolder.Length];
        _streakData = _dataBaseService.Load_Get();
        if (_streakData._rewardClaimed == null)
        {
            _streakData._rewardClaimed = new bool[_rewardProgressHolder.Length];
            _dataBaseService.Save(_streakData);
        }
        LoadClaimedRewards();
    }

    private void Start()
    {
        if (_streakData == null)
        {
            _streakData = new StreakData();
            _streakData._streakCount = 0;
            _dataBaseService.Save(_streakData);
        }
        _streakCount = _streakData._streakCount;
        Debug.LogError(_streakCount);
        SignalBus.Subscribe<OnDailyRewardClaim>(IncreaseStreakCount);
        SignalBus.Subscribe<StreakBreakSignal>(ResetStreak);
        UpdateBar();
        _streakCountText.text = _streakCount.ToString();
        _streakCount_EffectText.text = (_streakCount - 1).ToString();
    }

    private Sequence sequence;
    private void IncreaseStreakCount(OnDailyRewardClaim signal)
    {
        DOVirtual.DelayedCall(0.5f, () => { OnStreakIcrease(); });
    }

    void OnStreakIcrease()
    {
        AudioController.PlaySFX(AudioType.ChestOpen);
        _streakCount++;
        _streakData._streakCount = _streakCount;
        _dataBaseService.Save(_streakData);
        _streakCountText.text = _streakCount.ToString();
        _streakEffect.gameObject.SetActive(true);
        _streakCountEffect.gameObject.SetActive(true);
        sequence = DOTween.Sequence();
        float animationDuration = 1f;
        sequence.Join(
                _streakEffect
                    .DOJump(
                        _targetMovePoint.transform.position, // end position
                        -1.5f,                                 // jump power (height of jump arc)
                        1,                                    // number of jumps
                        animationDuration                     // total duration
                    )
                    .SetEase(Ease.OutSine)
            )
            .Join(_streakEffect.transform.DOScale(Vector3.zero, animationDuration))
            .Join(_streakEffect.transform.DOLocalRotate(new Vector3(0, 0, 180), animationDuration).From(Vector3.zero))
            .OnComplete(() =>
            {
                _streakEffect.gameObject.SetActive(false);
                _streakCountEffect.gameObject.SetActive(false);
                sequence.Rewind();
            });

        DOVirtual.DelayedCall(1, () => { SignalBus.Publish(new OnCelebrationAchievementSignal()); });
        UpdateBar();
        bool rewardAvailable = false;
        for (int i = 0; i < _box.Length; i++)
        {
            int day = _box[i].GetDay();
            if (!_claimed[i] && day == _streakCount || _streakCount == _rewardProgressHolder[i]._threshold)
            {
                _box[i].OpenVisual();
                _claimed[i] = true;
                SaveClaimedReward(i);
                GiveReward(i);
                rewardAvailable = true;
            }
        }

        if (!rewardAvailable)
            SignalBus.Publish(new OnShowDailyRewardPopUpEffect());
    }

    private void UpdateBar() => _progressBar.UpdateProgressBar((float)(_streakCount + 1) / _totalDays);

    private void SaveClaimedReward(int index)
    {
        _streakData._rewardClaimed[index] = true;
        _dataBaseService.Save(_streakData);
    }

    private void LoadClaimedRewards()
    {
        for (int i = 0; i < _rewardProgressHolder.Length; i++)
        {
            _claimed[i] = _streakData._rewardClaimed[i];
        }
    }
    [Button]
    private void ResetStreak(StreakBreakSignal signal)
    {
        _streakCount = 0;
        _streakData._streakCount = 0;
        _progressBar.ResetBar();
        _streakCountText.text = _streakCount.ToString();
        for (int i = 0; i < _claimed.Length; i++)
        {
            _claimed[i] = false;
            _streakData._rewardClaimed[i] = false;
        }

        foreach (var box in _box)
        {
            box.ResetBox();
        }
        _dataBaseService.Save(_streakData);
    }

    private void GiveReward(int index)
    {
        DOVirtual.DelayedCall(1.25f, () =>
        {
            SignalBus.Publish(new OnShowRewardProgressSignal
            {
                RewardsData = _rewardProgressHolder[index],
                OnRewardComplete = () => { PopCommandExecutionResponder.RemoveCommand<DailyRewardShowCommand>(); }
            });
        });
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<OnDailyRewardClaim>(IncreaseStreakCount);
        SignalBus.Unsubscribe<StreakBreakSignal>(ResetStreak);
    }
}

public class StreakBreakSignal : ISignal
{
}

public class StreakData
{
    public int _streakCount;
    public bool[] _rewardClaimed;
}

