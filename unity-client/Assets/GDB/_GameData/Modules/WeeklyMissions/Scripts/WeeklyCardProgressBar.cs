using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class WeeklyCardProgressBar : MonoBehaviour
{
    [SerializeField] private Image _filledBar;
    [SerializeField] private float _timer = 0.5f;

    public void FillBar(float progress)
    {
        _filledBar.DOFillAmount(progress, _timer).SetEase(Ease.OutCubic);
    }
}
