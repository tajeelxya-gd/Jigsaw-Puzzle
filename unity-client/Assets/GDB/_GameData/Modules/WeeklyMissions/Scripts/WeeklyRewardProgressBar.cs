using System;
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
        _progressFillImage.fillAmount = _currentProgress / _maxProgress;
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
        _progressFillImage.fillAmount = _currentProgress / _maxProgress;

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