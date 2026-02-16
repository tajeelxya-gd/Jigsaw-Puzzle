using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class MoveAndDisappearObject : MonoBehaviour, IMovable
{
    [SerializeField] private Transform _startPoint;
    [SerializeField] private Transform _endPoint;
    [SerializeField] private Transform _middlePoint;
    [SerializeField] private float _duration = 1f;
    [SerializeField] private float _delay = 1f;

    [Button]
    public void Move()
    {
        transform.position = _startPoint.position;
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOMove(_middlePoint.position, _duration));
        SignalBus.Publish(new PerformAction());
        seq.AppendInterval(_delay);
        seq.Append(transform.DOMove(_endPoint.position, _duration).SetEase(Ease.InOutSine));
    }
}

public class PerformAction:ISignal
{
    
}