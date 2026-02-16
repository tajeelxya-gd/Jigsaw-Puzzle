using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class StreakProgressBar : MonoBehaviour
{
    [SerializeField] private Image _filledBar;
    [SerializeField] private float _fillTimer;
    public void UpdateProgressBar(float fill)
    {
        _filledBar.DOFillAmount(fill, _fillTimer);
    }

    public void ResetBar()
    {
        _filledBar.fillAmount = 0;
    }
}
