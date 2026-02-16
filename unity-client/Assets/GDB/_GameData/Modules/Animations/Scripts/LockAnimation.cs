using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class LockAnimation : MonoBehaviour, IMovable
{
    [SerializeField] private Vector3 _rotation = new Vector3(0, 0, 45f);
    [SerializeField] private float _duration = 0.5f;
    [SerializeField] private int _swingCount = 5;
    private Quaternion _initialRotation;
    private void Start()
    {
        _initialRotation = transform.rotation;
    }

    [Button]
    public void Move()
    {
        transform.DOKill();
        transform.DORotate(_rotation, _duration).SetLoops(_swingCount, LoopType.Yoyo).SetEase(Ease.InOutSine).OnComplete(Reset);
    }

    private void Reset()
    {
        transform.rotation = _initialRotation;
    }
}