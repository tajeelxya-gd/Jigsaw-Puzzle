using DG.Tweening;
using UnityEngine;

public class ShineParticle : MonoBehaviour
{
    [SerializeField] private float _movementDistance = 5f;
    [SerializeField] private float _movementSpeed = 10f;
    private Vector3 _startPosition;
    private void Awake()
    {
        _startPosition = transform.position;
    }

    private void Start()
    {
        MoveParticle();
    }

    private void MoveParticle()
    {
        ResetPosition();
        transform.DOMove(transform.position+new Vector3(_movementDistance,0,0),_movementSpeed).OnComplete(MoveParticle);
    }

    private void ResetPosition()
    {
        transform.position = _startPosition;
    }
}
