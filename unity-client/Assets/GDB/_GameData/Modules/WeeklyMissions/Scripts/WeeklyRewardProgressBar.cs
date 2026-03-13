using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class WeeklyRewardProgressBar : MonoBehaviour
{
    [SerializeField] private Image _progressFillImage;
    [SerializeField] private float _maxProgress = 1100f;
    private DataBaseService<WeeklyRewardProgressData> _progressDatabase = new();

    private float _currentProgress;
    WeeklyRewardProgressData _progressData;

    private void Start()
    {
        _progressData = _progressDatabase.Load_Get();
        _currentProgress = _progressData.CurrentProgress;
        //Debug.LogError(_currentProgress);
        var progress = _currentProgress / _maxProgress;
        var fillBarRect = _progressFillImage.rectTransform;
        float fillWidth = fillBarRect.rect.width;
        fillBarRect.DOAnchorPosX((progress - 1) * fillWidth, 0.5f).SetEase(Ease.OutCubic);
    }

    private void OnEnable()
    {
        SignalBus.Subscribe<OnWeeklyProgressUpdatedSignal>(SetProgress);
    }

    private void OnDisable()
    {
        SignalBus.Unsubscribe<OnWeeklyProgressUpdatedSignal>(SetProgress);
    }

    public void SetProgress(OnWeeklyProgressUpdatedSignal signal)
    {
        _currentProgress = Mathf.Clamp(signal.Progress, 0, _maxProgress);
        var progress = _currentProgress / _maxProgress;
        var fillBarRect = _progressFillImage.rectTransform;
        float fillWidth = fillBarRect.rect.width;
        fillBarRect.DOAnchorPosX((progress - 1) * fillWidth, 0.5f).SetEase(Ease.OutCubic);

        _progressData.CurrentProgress = _currentProgress;
        _progressDatabase.Save(_progressData);
    }

}

public class OnWeeklyProgressUpdatedSignal : ISignal
{
    public float Progress;
}

[Serializable]
public class WeeklyRewardProgressData
{
    public float CurrentProgress;
}