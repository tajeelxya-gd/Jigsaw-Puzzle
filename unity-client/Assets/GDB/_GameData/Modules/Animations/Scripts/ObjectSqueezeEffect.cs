using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class ObjectSqueezeEffect : MonoBehaviour, IMovable
{
    [SerializeField] private Vector3 _decreaseScale;
    [SerializeField] private Vector3 _increaseScale;
    [SerializeField] private float _decreaseTimer = 0.25f;
    [SerializeField] private float _increaseTimer = 0.25f;
    [SerializeField] private float _delay = 0.25f;
    private Vector3 _originalScale;

    private void Start()
    {
        _originalScale = transform.localScale;
    }

    [Button]
    public void SqueezeEffect()
    {
        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(_delay);
        seq.Append(transform.DOScale(_decreaseScale, _decreaseTimer));
        seq.AppendInterval(_delay);
        seq.Append(transform.DOScale(_increaseScale, _increaseTimer));
        seq.AppendInterval(_delay);
        seq.Append(transform.DOScale(_originalScale, _increaseTimer)).SetUpdate(true);
    }

    public void Move()
    {
        SqueezeEffect();
    }
}