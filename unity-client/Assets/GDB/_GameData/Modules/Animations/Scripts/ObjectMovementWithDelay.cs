using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class ObjectMovementWithDelay : MonoBehaviour, IMovable
{
    [SerializeField] private int _floatLoops = -1;
    [SerializeField] private float _time = 0.5f;
    [SerializeField] private float _jitterDistance = 0.25f;
    [SerializeField] private float _jitterTime = 0.15f;
    [SerializeField] private float _delayBeforeMovement = 0.3f;
    [SerializeField] private float _floatAmplitudeX = 0.02f;
    [SerializeField] private float _floatAmplitudeY = 0.1f;
    [SerializeField] private float _floatDuration = 0.2f;
    [SerializeField] private float _popScale = 1.2f;
    [SerializeField] private float _popTime = 0.2f;
    [SerializeField] private float _scaleInTime = 0.25f;

    private IScaling _scaling;
    private Tween _mainTween;
    private Tween _floatTween;
    private Vector3 _startPosition;
    private Transform _target;
    private AnimationPoolManager _pool;

    private void Awake()
    {
        _scaling = GetComponent<IScaling>();
    }

    private void OnDisable()
    {
        _mainTween?.Kill();
        _floatTween?.Kill();
    }
    public void SetTime(float time)
    {
        _time = time;
    }

    public void Inject(AnimationPoolManager pool)
    {
        _pool = pool;
    }

    public void Play(Vector3 startPosition, Transform target)
    {
        _startPosition = startPosition;
        _target = target;

        transform.position = startPosition;
        transform.localScale = Vector3.one;
        gameObject.SetActive(true);

        Move();
    }

    [Button]
    public void Move()
    {
        _mainTween?.Kill();
        _floatTween?.Kill();

        Vector3 randomOffset = new Vector3(
            Random.Range(-_jitterDistance, _jitterDistance),
            Random.Range(-_jitterDistance, _jitterDistance),
            0);

        _floatTween = transform.DOBlendableMoveBy(
            new Vector3(
                Random.Range(-_floatAmplitudeX, _floatAmplitudeX),
                Random.Range(-_floatAmplitudeY, _floatAmplitudeY),
                0),
            _floatDuration)
            .SetLoops(_floatLoops, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);

        Sequence seq = DOTween.Sequence();

        seq.Append(transform.DOMove(transform.position + randomOffset, _jitterTime));

        if (_scaling != null)
        {
            seq.AppendInterval(_scaleInTime);
            _scaling.ScaleIn();
        }

        seq.AppendInterval(_delayBeforeMovement);
        seq.Append(transform.DOMove(_target.position, _time).SetEase(Ease.InOutSine));

        seq.Append(transform.DOScale(transform.localScale * _popScale, _popTime)
            .SetEase(Ease.OutBack)
            .OnComplete(OnAnimationComplete));

        _mainTween = seq;
    }

    private void OnAnimationComplete()
    {
        _floatTween?.Kill();
        Reset();

        if (_pool != null)
            _pool.ReturnToPool(this);
        else
            gameObject.SetActive(false);
    }

    private void Reset()
    {
        transform.position = _startPosition;
        transform.localScale = Vector3.one;
    }
}
