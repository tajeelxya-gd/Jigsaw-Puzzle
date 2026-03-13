using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class WeeklyCardProgressBar : MonoBehaviour
{
    [SerializeField] private Image _filledBar;
    [SerializeField] private float _timer = 0.5f;

    public void FillBar(float progress)
    {
        var fillBarRect = _filledBar.rectTransform;
        float fillWidth = fillBarRect.rect.width;
        fillBarRect.DOAnchorPosX((progress - 1) * fillWidth, _timer).SetEase(Ease.OutCubic);
    }
}
