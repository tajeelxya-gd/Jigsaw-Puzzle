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
        // 1. Stop any existing animation
        StopAnim();

        // 2. Reset rotations to 00:00
        if (_minutesNeedle != null) _minutesNeedle.localRotation = Quaternion.identity;
        if (_hoursNeedle != null) _hoursNeedle.localRotation = Quaternion.identity;
        if (_rootContainer != null)
        {
            _rootContainer.localScale = Vector3.one;
            _rootContainer.localRotation = Quaternion.identity;
        }

        // 3. Begin the ticking cycle
        PlayNextTick();
    }

    private void PlayNextTick()
    {
        _mainSequence = DOTween.Sequence();
        var targetToBump = _rootContainer != null ? _rootContainer : _minutesNeedle;

        // A. Rotate Minute hand 90 degrees (15 minutes)
        _mainSequence.Append(
            _minutesNeedle.DOLocalRotate(new Vector3(0, 0, -90), _jumpDuration, RotateMode.LocalAxisAdd)
                .SetEase(_jumpEase)
        );

        // B. Rotate Hour hand 7.5 degrees (1/4 of a 30-degree hour mark)
        if (_hoursNeedle != null)
        {
            _mainSequence.Join(
                _hoursNeedle.DOLocalRotate(new Vector3(0, 0, -7.5f), _jumpDuration, RotateMode.LocalAxisAdd)
                    .SetEase(_jumpEase)
            );
        }

        // C. Trigger Visual Bump
        _mainSequence.Append(targetToBump.DOPunchScale(Vector3.one * _bumpScaleAmount, _bumpDuration, 5, 0.5f));

        // D. Wait the pause duration
        _mainSequence.AppendInterval(_pauseDuration);

        // E. Recursively call the next tick to ensure perfect continuity without resetting
        _mainSequence.OnComplete(PlayNextTick);
    }

    public void StopAnim()
    {
        if (_mainSequence != null)
        {
            _mainSequence.Kill();
            _mainSequence = null;
        }

        if (_rootContainer != null) _rootContainer.localScale = Vector3.one;
        else if (_minutesNeedle != null) _minutesNeedle.localScale = Vector3.one;
    }
}
