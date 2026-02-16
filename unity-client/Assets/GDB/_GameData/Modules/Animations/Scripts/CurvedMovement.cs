using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

public class CurvedMovement : MonoBehaviour, IMovable
{
    [SerializeField] private Transform _endPosition;
    [SerializeField] private float _duration = 1f;
    [SerializeField] private float _curveHeight = 2f;

    private Vector3 _startPosition;
    private Tween _tween;

    private void Start()
    {
        _startPosition = transform.position;
    }

    [Button]
    public void Move()
    {
        _tween?.Kill();

        Vector3 midPoint = (_startPosition + _endPosition.position) * 0.5f;
        midPoint.y += _curveHeight;

        Vector3[] path = { _startPosition, midPoint, _endPosition.position };

        _tween = transform.DOPath(path, _duration, PathType.CatmullRom)
            .SetEase(Ease.InOutSine);
    }

    public void Reset()
    {
        transform.position = _startPosition;
    }

    public void ResetMove()
    {
        transform.DOMove(_startPosition, _duration);
    }
}