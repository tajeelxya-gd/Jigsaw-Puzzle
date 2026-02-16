using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ShowPrize : MonoBehaviour
{
    [SerializeField] private Image[] _prizeImages;
    [SerializeField] private float _popDuration = 0.3f;  
    [SerializeField] private float _displayTime = 2f;    
    [SerializeField] private float _popScale = 1.2f;   

    public void DisplayPrizes()
    {
        AnimatePrizes();
    }

    private void AnimatePrizes()
    {
        foreach (var img in _prizeImages)
        {
            img.gameObject.SetActive(true);
            img.transform.localScale = Vector3.zero;

     
            img.transform.DOScale(_popScale, _popDuration).SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    img.transform.DOScale(Vector3.zero, _popDuration).SetEase(Ease.InBack)
                        .SetDelay(_displayTime)
                        .OnComplete(() => img.gameObject.SetActive(false));
                });
        }
    }
}