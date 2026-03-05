using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeeklyRewardCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textMeshProUGUI;
    [SerializeField] private TextMeshProUGUI _claimTextMeshProUGUI;
    [SerializeField] private RewardContainerUI _rewardText_Main;
    [SerializeField] private RewardContainerUI _rewardText_Extra;
    [SerializeField] private Image[] _rewardIcons;
    [SerializeField] private GameObject _claimedOverlay;
    [SerializeField] private TextMeshProUGUI _progressText;
    [SerializeField] public Button _collectButton;
    [SerializeField] public Button _lockButton;
    [SerializeField] private RectTransform _rewardTransform;
    [SerializeField] private Image _taskImage;
    [SerializeField] private GameObject _lockIcon;
    [SerializeField] private WeeklyCardProgressBar _progressBar;
    [SerializeField] private GameObject _progressRoot;
    [SerializeField] private GameObject _titleRoot;
    [SerializeField] private Image _completionImage;
    [SerializeField] private LayoutGroup _layout;
    private MissionProgress _missionProgress;
    private WeeklyRewardManager _weeklyRewardManager;

    [SerializeField]
    private GameObject _animationEffect;
    [SerializeField] private ObjectScaleEffect _popEffect;
    [SerializeField] private float _delay;
    [ReadOnly, SerializeField] private int _currentday = 1;
    Sequence lockSeq;
    public void Inject(WeeklyRewardManager weeklyRewardManager)
    {
        _weeklyRewardManager = weeklyRewardManager;
    }

    private void OnEnable()
    {
        RefreshProgress();
        //_popEffect.MoveDelayed(_delay);
        _lockButton.onClick.AddListener(OnLockButtonPressed);
        lockSeq = DOTween.Sequence()
            .Append(_lockIcon.transform.DOPunchRotation(new Vector3(0, 0, 20), 1f, 7, 0.96f))
            .Join(_lockIcon.transform.DOPunchPosition(new Vector3(0, 10, 0), 1f, 6, 1f))
            .Join(transform.DOShakePosition(1f, new Vector3(5, 0, 0)))
            .SetAutoKill(false)
            .Pause();
    }

    private void OnDisable()
    {
        lockSeq?.Kill();
    }

    public void Setup(MissionProgress missionProgress)
    {
        _currentday = missionProgress.Mission.Day;
        _missionProgress = missionProgress;
        _textMeshProUGUI.text = _missionProgress.Mission._text;
        _taskImage.sprite = _missionProgress.Mission._taskImage;
        _taskImage.SetNativeSize();
        for (int i = 0; i < _rewardIcons.Length; i++)
        {
            if (i < _missionProgress.Mission._rewardIcon.Length && _missionProgress.Mission._rewardIcon[i] != null)
            {
                _rewardIcons[i].sprite = _missionProgress.Mission._rewardIcon[i];
                _rewardIcons[i].gameObject.SetActive(true);
                _rewardIcons[i].SetNativeSize();
            }
            else
            {
                _rewardIcons[i].gameObject.SetActive(false);
            }
        }

        // _collectButton.interactable = _missionProgress.Mission._isInteractable;
        RefreshProgress();
    }

    void OnLockButtonPressed()
    {
        lockSeq.Restart();
        HapticController.VibrateForcefully(HapticType.Wrong);

    }

    private void RefreshProgress()
    {
        if (_missionProgress == null) return;

        int clampedProgressAmount = Mathf.Clamp(_missionProgress.CurrentAmount, 0, _missionProgress.Mission._targetAmount);
        _progressText.text = $"{clampedProgressAmount}/{_missionProgress.Mission._targetAmount}";
        _collectButton.gameObject.SetActive(_missionProgress.IsCompleted && !_missionProgress.IsClaimed);
        // _textMeshProUGUI.gameObject.SetActive(!(_missionProgress.IsCompleted && !_missionProgress.IsClaimed));
        var claimable = _missionProgress.IsCompleted && !_missionProgress.IsClaimed;
        _claimTextMeshProUGUI.gameObject.SetActive(claimable);
        if (_missionProgress.IsClaimed)
        {
            _progressRoot.gameObject.SetActive(false);
            _layout.childAlignment = TextAnchor.MiddleCenter;
        }
        else
        {
            _progressRoot.gameObject.SetActive(!claimable);
            _layout.childAlignment = TextAnchor.UpperCenter;
        }
        float progress = (float)clampedProgressAmount / _missionProgress.Mission._targetAmount;
        _progressBar.FillBar(progress);
        GetComponent<CanvasGroup>().alpha = _missionProgress.IsClaimed ? 0.7f : 1;
        _claimedOverlay.gameObject.SetActive(_missionProgress.IsClaimed);
        // _taskImage.gameObject.SetActive(!_missionProgress.IsClaimed);
        _completionImage.gameObject.SetActive(_missionProgress.IsClaimed);
        _lockButton.gameObject.SetActive(!(_currentday <= _weeklyRewardManager._currentDay));
        _lockIcon.gameObject.SetActive(!(_currentday <= _weeklyRewardManager._currentDay));
        _rewardText_Extra.gameObject.SetActive(_missionProgress.Mission._extraReward != WeeklyRewardType.None);
        _rewardText_Extra.Init(null, _missionProgress.Mission._rewardAmount);
        _rewardText_Main.Init(null, _missionProgress.Mission._rewardAmount);
    }

    void SendAnalyticEvent()
    {
        GameAnalytics.PublishAnalytic(
            AnalyticEventType.GameData, "Events",
            nameof(AnalyticEventType.AchievementEvent),
            "Day " + _currentday.ToString(),
            _missionProgress.Mission._text);
    }
    public void OnCollectButtonPressed()
    {
        AudioController.PlaySFX(AudioType.ButtonClick);
        AudioController.PlaySFX(AudioType.ItemRattle);
        HapticController.Vibrate(HapticType.Btn);
        RewardDataHolder.WeeklyRewardTypeBlock extraRewardType =
            _missionProgress.Mission._extraReward == WeeklyRewardType.None
                ? null
                : new RewardDataHolder.WeeklyRewardTypeBlock
                {
                    RewardType = _missionProgress.Mission._extraReward,
                    RewardAmount = _missionProgress.Mission._rewardAmount
                };
        SignalBus.Publish(new OnRewardAdded
        {
            RewardAmount = _missionProgress.Mission._rewardAmount,
            RewardPoint = _rewardTransform,
            ExtraRewardType = extraRewardType
        });

        var getRewardSignal = new OnGetRewardAmount();
        SignalBus.Publish(getRewardSignal);

        int currentReward = getRewardSignal.RewardAmount;

        SignalBus.Publish(new OnWeeklyProgressUpdatedSignal
        {
            Progress = currentReward
        });

        if (_missionProgress.IsCompleted)
        {
            _missionProgress.CurrentAmount = 0;
            _missionProgress.Mission._isInteractable = false;
        }

        _missionProgress.IsClaimed = true;
        RefreshProgress();
        _popEffect.Move();
        _weeklyRewardManager.SaveMissionProgress(_missionProgress);
        _animationEffect.gameObject.SetActive(true);
        SendAnalyticEvent();
    }

}