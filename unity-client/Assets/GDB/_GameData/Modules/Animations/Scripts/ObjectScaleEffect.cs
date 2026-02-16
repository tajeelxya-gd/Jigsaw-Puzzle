using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class ObjectScaleEffect : MonoBehaviour, IMovable, ISetDuration
{
    [SerializeField] private Vector3 _targetScale = Vector3.one * 1.2f;
    [SerializeField] private float _duration = 0.75f;
    [SerializeField] private bool _hasLoop = false;
    private Vector3 _originalScale;
    private Tween _tween;

    private void Awake()
    {
        _originalScale = transform.localScale;
    }

    private void OnDisable()
    {
        _tween?.Kill();
        transform.localScale = _originalScale;
    }

    public void SetDuration(float duration)
    {
        _duration = duration;
    }

    public void SetLooping(bool hasLoop)
    {
        _hasLoop = hasLoop;
    }

    [Button]
    public void Move()
    {
        _tween?.Kill();
        if (!_hasLoop)
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DOScale(_targetScale, _duration).SetEase(Ease.OutSine));
            seq.Append(transform.DOScale(_originalScale, _duration).SetEase(Ease.InSine)).SetUpdate(true);
            _tween = seq;
        }
        else
        {
            _tween = transform.DOScale(_targetScale, _duration).SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
        }
    }

    [Button]
    public void AdjustScaleInstantly()
    {
        _tween?.Kill();
        _tween = transform.DOScale(_targetScale, _duration).SetEase(Ease.OutSine).SetUpdate(true);
    }

    public void SetOwned(bool owned)
    {
        _hasLoop = owned;
        Move();
    }

    public void MoveDelayed(float delay)
    {
        _tween?.Kill();
        _originalScale = transform.localScale;
        if (!_hasLoop)
        {
            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(delay);
            seq.Append(transform.DOScale(_targetScale, _duration).SetEase(Ease.OutSine));
            seq.Append(transform.DOScale(_originalScale, _duration).SetEase(Ease.InSine)).SetUpdate(true);
            _tween = seq;
        }
        else
        {
            _tween = transform.DOScale(_targetScale, _duration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
        }
    }

    public void StopAnimation()
    {
        _tween.Kill();
        transform.localScale = _originalScale;
    }
}