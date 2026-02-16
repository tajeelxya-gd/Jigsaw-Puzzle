using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class SizeIncreaseEffect : MonoBehaviour, IMovable
{
    [SerializeField] private Vector3 _targetScale = Vector3.one;
    [SerializeField] private float _duration = 0.75f;
    [SerializeField] private Ease _easeType = Ease.OutSine;
    private Tween _tween;

    private void OnDisable()
    {
        _tween?.Kill();
        transform.localScale = Vector3.zero;
    }

    [Button]
    public void Move()
    {
        _tween?.Kill();
        transform.localScale=Vector3.zero;
        _tween = transform.DOScale(_targetScale, _duration).SetEase(_easeType);
    }
}
