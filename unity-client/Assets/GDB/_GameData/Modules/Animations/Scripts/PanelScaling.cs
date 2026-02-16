using System;
using DG.Tweening;
using UnityEngine;

public class PanelScaling : MonoBehaviour, IScaling, IMovable
{
    [SerializeField] private Vector3 _size = Vector3.one;
    [SerializeField] private Vector3 _size_from = Vector3.zero;
    [SerializeField] private float _duration;
    private Tween _tween;
    [SerializeField] private bool _scaleOnEnable = false;

    private void OnEnable()
    {
        if (_scaleOnEnable)
            ScaleIn();
    }

    public void ScaleIn()
    {
        _tween?.Kill();
        transform.localScale = Vector3.zero;
        _tween = transform.DOScale(_size, _duration).From(_size_from).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public void ScaleOut()
    {
        _tween?.Kill();
        _tween = transform.DOScale(Vector3.zero, _duration).SetEase(Ease.InBack).SetUpdate(true) ;
    }

    public void Move()
    {
        ScaleIn();
    }
}

public interface IScaling
{
    void ScaleIn();
    void ScaleOut();
}
