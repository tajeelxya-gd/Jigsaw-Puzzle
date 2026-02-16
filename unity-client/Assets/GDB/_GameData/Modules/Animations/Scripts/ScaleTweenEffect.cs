using UnityEngine;
using DG.Tweening;
public class ScaleTweenEffect : MonoBehaviour
{
    [SerializeField] private float _scaleDuration;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void DOScale(float unit)
    {
        transform.DOScale(unit, _scaleDuration);
    }
}
