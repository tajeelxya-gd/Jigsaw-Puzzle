using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class ObjectSettleEffect : MonoBehaviour, ISettleEffect, IMovable
{
    [SerializeField] private float _upDistance = 30f;
    [SerializeField] private float _downDistance = 15f;
    [SerializeField] private float _upDuration = 0.25f;
    [SerializeField] private float _downDuration = 0.2f;
    [SerializeField] private float _reverseUpDistance = 20f;
    [SerializeField] private float _reverseUpDuration = 0.2f;
    [SerializeField] private float _reverseDownDuration = 0.2f;
    [SerializeField] private bool _runAtStart = false;

    private Vector3 _startPosition;
    private Sequence _seq;

    private void Start()
    {
        _startPosition = transform.localPosition;
        if (!_runAtStart) return;
        PlayEffect();
    }

    private void OnDisable()
    {
        _seq?.Kill();
    }

    [Button("Play Effect")]
    public void PlayEffect()
    {
        if (!gameObject.activeInHierarchy) return;
        _seq?.Kill();
        _seq = DOTween.Sequence().SetUpdate(true);

        Vector3 upPos = _startPosition + Vector3.up * _upDistance;
        Vector3 downPos = _startPosition + Vector3.up * _downDistance;

        _seq.Append(transform.DOLocalMove(upPos, _upDuration).SetEase(Ease.OutCubic));
        _seq.Append(transform.DOLocalMove(downPos, _downDuration).SetEase(Ease.OutCubic));
    }

    [Button("Play Reverse Effect")]
    public void PlayReverseEffect()
    {
        _seq?.Kill();
        _seq = DOTween.Sequence().SetUpdate(true);

        Vector3 upPos = transform.localPosition + Vector3.up * _reverseUpDistance;

        _seq.Append(transform.DOLocalMove(upPos, _reverseUpDuration).SetEase(Ease.OutCubic));
        _seq.Append(transform.DOLocalMove(_startPosition, _reverseDownDuration).SetEase(Ease.OutCubic));
    }

    public void ResetPosition()
    {
        _seq?.Kill();
        transform.localPosition = _startPosition;
    }

    public void Move() => PlayEffect();
}


public interface ISettleEffect
{
    void PlayEffect();
    void PlayReverseEffect();
}