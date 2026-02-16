using System.Collections;
using DG.Tweening;
using UnityEngine;

public class CannonUIAnimation : MonoBehaviour
{
    [SerializeField] private RectTransform[] _points;
    [SerializeField] private float _moveDuration = 0.5f;
    [SerializeField] private Ease _ease = Ease.InOutSine;

    [Header("Jump Feel")]
    [SerializeField] private float _stretchAmount = 0.15f;
    [SerializeField] private float _squashAmount = 0.2f;
    [SerializeField] private float _jumpScaleDuration = 0.15f;

    [Header("Jump Feel")] [SerializeField] private Vector3 _firstTarget;
    [SerializeField] private float _cannonDelay = 0.5f;
    
    private Sequence _sequence;
    private RectTransform _rectTransform;
    private Vector3 _initialScale;
    private Vector2 _initialAnchoredPosition;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _initialAnchoredPosition = _rectTransform.anchoredPosition;
        _initialScale = _rectTransform.localScale;
    }

    private void OnEnable()
    {
        DOVirtual.DelayedCall(0.25f, Play).SetUpdate(true);
    }

    private void Play()
    {
        if (_points == null || _points.Length == 0) return;
        StartCoroutine(WaitAndPlay(_cannonDelay));
    }
    
    private IEnumerator WaitAndPlay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        _sequence?.Kill();
        _sequence = DOTween.Sequence().SetUpdate(true);

        foreach (var point in _points)
        {
            Vector2 targetPos = _rectTransform.parent.InverseTransformPoint(point.position);
            _sequence.Append(_rectTransform.DOAnchorPos(_firstTarget, _moveDuration).SetEase(_ease)).SetUpdate(true);
            _sequence.AppendInterval(0.2f);
            _sequence.AppendCallback(() =>
            {
                SignalBus.Publish(new ChangeCannonSlotSignal { _isRed = true });
            });
            _sequence.AppendInterval(0.8f);

            _sequence.Append(_rectTransform.DOAnchorPos(targetPos, _moveDuration).SetEase(_ease)).SetUpdate(true);
            _sequence.Join(
                _rectTransform.DOScale(
                    new Vector3(_initialScale.x + _stretchAmount, _initialScale.y + _stretchAmount, 1f),
                    _moveDuration * 0.5f
                ).SetEase(Ease.OutQuad)
            ).SetUpdate(true);
            _sequence.Append(_rectTransform.DOScale(_initialScale, _jumpScaleDuration)).SetUpdate(true);
            _sequence.AppendCallback(() =>
            {
                SignalBus.Publish(new ChangeCannonSlotSignal { _isRed = false });
            });

        }
        
        _sequence.AppendInterval(1f).SetLoops(-1, LoopType.Restart).Play();
    }

    private void OnDisable()
    {
        _sequence?.Kill();
        _rectTransform.anchoredPosition = _initialAnchoredPosition;
        _rectTransform.localScale = _initialScale;
    }
}

public class ChangeCannonSlotSignal:ISignal
{
    public bool _isRed;
}
