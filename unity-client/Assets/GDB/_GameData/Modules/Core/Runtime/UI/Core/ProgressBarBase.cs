using System;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarBase : MonoBehaviour, IProgress
{
    public float Progress => _fillImage.fillAmount;
    public Action OnBarCompleted { get; set; }

    [SerializeField] private protected Image _fillImage;

    public virtual void Initialize()
    {
        _fillImage.fillAmount = 0;
        OnBarCompleted = null;
    }
    public virtual void UpdateProgress(float val)
    {
        if (_fillImage == null) return;

        val = Mathf.Clamp01(val);

        _fillImage.fillAmount = val;

        if (val >= 1f)
            OnBarCompleted?.Invoke();
    }
}
public interface IProgress
{
    public float Progress { get; }
    public Action OnBarCompleted { get; set; }
    public void UpdateProgress(float val);
}