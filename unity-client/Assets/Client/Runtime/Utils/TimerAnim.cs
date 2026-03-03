using DG.Tweening;
using UnityEngine;

public class TimerAnim : MonoBehaviour
{
    [SerializeField] private RectTransform _hoursNeedle;
    [SerializeField] private RectTransform _minutesNeedle;
    [SerializeField] private RectTransform _rootContainer;

    [Header("Ticking Settings")]
    [Tooltip("Time it takes to rotate 90 degrees (to the next 15-min mark)")]
    [SerializeField] private float _jumpDuration = 0.3f;
    [Tooltip("Time to stay at the mark before the next jump")]
    [SerializeField] private float _pauseDuration = 0.6f;
    [SerializeField] private Ease _jumpEase = Ease.OutBack;

    [Header("Bump Settings")]
    [SerializeField] private float _bumpScaleAmount = 0.12f;
    [SerializeField] private float _bumpDuration = 0.2f;

    private Sequence _mainSequence;
    private Vector3 _originalScale;
    private RectTransform _bumpTarget;

    private void Awake()
    {
        // Cache the target and its original scale once to avoid guesswork later
        _bumpTarget = _rootContainer != null ? _rootContainer : _minutesNeedle;
        if (_bumpTarget != null)
        {
            _originalScale = _bumpTarget.localScale;
        }
    }

    private void OnEnable()
    {
        StartAnim();
    }

    private void OnDisable()
    {
        StopAnim();
    }

    public void StartAnim()
    {
        StopAnim();

        // 1. Reset rotations to 00:00
        if (_minutesNeedle != null) _minutesNeedle.localRotation = Quaternion.identity;
        if (_hoursNeedle != null) _hoursNeedle.localRotation = Quaternion.identity;

        if (_rootContainer != null) _rootContainer.localRotation = Quaternion.identity;

        // 2. Reset scale to cached original
        if (_bumpTarget != null)
        {
            _bumpTarget.localScale = _originalScale;
        }

        // 3. Begin the ticking cycle
        PlayNextTick();
    }

    private void PlayNextTick()
    {
        _mainSequence = DOTween.Sequence();

        // A. Rotate Minute hand 90 degrees (15 minutes)
        if (_minutesNeedle != null)
        {
            _mainSequence.Append(
                _minutesNeedle.DOLocalRotate(new Vector3(0, 0, -90), _jumpDuration, RotateMode.LocalAxisAdd)
                    .SetEase(_jumpEase)
            );
        }

        // B. Rotate Hour hand 7.5 degrees (1/4 of a 30-degree hour mark)
        if (_hoursNeedle != null)
        {
            _mainSequence.Join(
                _hoursNeedle.DOLocalRotate(new Vector3(0, 0, -7.5f), _jumpDuration, RotateMode.LocalAxisAdd)
                    .SetEase(_jumpEase)
            );
        }

        // C. Trigger Visual Bump (using the stored original scale to determine punch magnitude)
        if (_bumpTarget != null)
        {
            _mainSequence.Append(_bumpTarget.DOPunchScale(_originalScale * _bumpScaleAmount, _bumpDuration, 5, 0.5f));
        }

        // D. Wait the pause duration
        _mainSequence.AppendInterval(_pauseDuration);

        // E. Recursively call the next tick
        _mainSequence.OnComplete(PlayNextTick);
    }

    public void StopAnim()
    {
        if (_mainSequence != null)
        {
            _mainSequence.Kill();
            _mainSequence = null;
        }

        // Reset scale to the cached original value
        if (_bumpTarget != null)
        {
            _bumpTarget.localScale = _originalScale;
        }
    }
}
