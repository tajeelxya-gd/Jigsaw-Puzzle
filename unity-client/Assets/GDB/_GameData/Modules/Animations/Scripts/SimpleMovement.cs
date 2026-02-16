using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.Events;
using UniTx.Runtime;

public class SimpleMovement : MonoBehaviour, IMenuScreen
{
    [SerializeField] private GameObject _screenRoot;
    [SerializeField] private Transform _endPosition;
    [SerializeField] private float _duration = 1f;
    [Header("Random generation before moving")]
    [SerializeField] private bool _hasRandomGeneration = false;
    [SerializeField] private float _minRange;
    [SerializeField] private float _maxRange;
    [SerializeField] private float _delay = 1f;
    [SerializeField] UnityEvent _onComplete;
    [SerializeField] UnityEvent _onStart;
    private Tween _tween;
    private Vector3 _startPosition;

    private void Awake()
    {
        _startPosition = transform.position;
    }

    [Button]
    public void Move(UnityAction callBack, Transform overrideTransform = null)
    {
        ShowScreen(true);
        Sequence seq = DOTween.Sequence();
        _tween.Kill();
        _onStart?.Invoke();
        if (overrideTransform != null)
        {
            seq.Append(transform.DOMove(overrideTransform.position, _duration).OnComplete(() =>
            {
                if (callBack != null)
                    callBack?.Invoke();
            }));
            return;
        }//for override transforms only do this
        if (_hasRandomGeneration)
        {
            Vector3 randomOffset =
                new Vector3(Random.Range(-_minRange, _maxRange), Random.Range(-_minRange, _maxRange), 0);
            seq.Append(transform.DOMove(transform.position + randomOffset, _duration));
            seq.AppendInterval(_delay);
        }

        seq.Append(_tween = transform.DOMove(_endPosition.position, _duration));
        _onComplete?.Invoke();
        UniStatics.LogInfo(gameObject.name + " moving to default Position");


    }

    public bool IsActive() => _screenRoot.activeInHierarchy;
    public void Reset()
    {
        transform.position = _startPosition;
        _onComplete?.Invoke();
    }

    public void ResetMove()
    {
        transform.DOMove(_startPosition, _duration).OnComplete(() => ShowScreen(false));
        _onComplete?.Invoke();

        UniStatics.LogInfo(gameObject.name + " moving to default Position");

    }

    public void ShowScreen(bool status)
    {
        if (_screenRoot == null) return;
        _screenRoot.gameObject.SetActive(status);

        if (status)
            DOVirtual.DelayedCall(0.15f, () => { AudioController.PlaySFX(AudioType.PanelPop); });
    }
}