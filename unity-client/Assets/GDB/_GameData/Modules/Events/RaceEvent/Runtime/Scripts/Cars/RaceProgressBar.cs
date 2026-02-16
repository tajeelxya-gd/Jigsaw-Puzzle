using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Sirenix.OdinInspector;

public class RaceProgressBar : MonoBehaviour, IProgressBar
{
    [SerializeField] private Image _fillBar;
    [SerializeField] private float fillDuration = 0.5f;
    [SerializeField] private int _totalWins = 5;

    private float _baseProgress = 0.25f;
    private float _currentBarProgress;

    private float _incrementPerWin => (1f - _baseProgress) / _totalWins;

    public void UpdateFillBar(float totalWinsOfAI)
    {
        float target = _baseProgress + totalWinsOfAI * _incrementPerWin;
        target = Mathf.Clamp01(target);

        _currentBarProgress = target;

        _fillBar.DOFillAmount(_currentBarProgress, fillDuration).SetEase(Ease.OutSine);
    }

    public float GetCurrentProgress() => _currentBarProgress;

    public void SetCurrentProgress(float progress)
    {
        _currentBarProgress = progress;
        _fillBar.fillAmount = progress;
    }

    public void Reset()
    {
        _currentBarProgress = _baseProgress;
        _fillBar.fillAmount = _baseProgress;
    }
}


public interface IProgressBar
{
    public float GetCurrentProgress();
    public void SetCurrentProgress(float progress);
    public void UpdateFillBar(float progress);
    public void Reset();
}