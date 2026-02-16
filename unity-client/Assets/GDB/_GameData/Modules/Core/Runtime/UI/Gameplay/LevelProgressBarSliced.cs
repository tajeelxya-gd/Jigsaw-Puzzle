using Sirenix.OdinInspector;
using UnityEngine;

public class LevelProgressBarSliced : ProgressBarBase
{
    RectTransform _rectTransform;
    [ReadOnly,SerializeField]
    private float _maxBarWidth = 0;
    private bool inisialized = false;
    void SetUp()
    {
        inisialized = true;
        _rectTransform = _fillImage.GetComponent<RectTransform>();
        _maxBarWidth = _rectTransform.sizeDelta.x;
    }
    
    public override void Initialize()
    {
        if(!inisialized) SetUp();
        _rectTransform.sizeDelta = new Vector2(0, _rectTransform.sizeDelta.y);
        OnBarCompleted = null;
    }
    public override void UpdateProgress(float val)
    {
        
        if (_fillImage == null) return;
        //Debug.LogError("LevelProgressBarSliced::Initialize :: "+val);
        var currentProgressX = _maxBarWidth * val;
        Vector3 targetSizeDelta = _rectTransform.sizeDelta;
        targetSizeDelta.x = currentProgressX;
        _rectTransform.sizeDelta = targetSizeDelta;
        if (val >= 1f)
            OnBarCompleted?.Invoke();
    }
}
