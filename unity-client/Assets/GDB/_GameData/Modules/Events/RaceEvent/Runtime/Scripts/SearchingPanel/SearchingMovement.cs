using System.Collections;
using UnityEngine;
using DG.Tweening;

public class SearchingMovement : MonoBehaviour
{
    private IMovable _scaleEffect;
    private void Awake()
    {
        _scaleEffect = GetComponent<IMovable>();
    }

    private void OnEnable()
    {
        _scaleEffect.Move();

    }
}