using System;
using System.Collections;
using UnityEngine;

public class ProceduralAnimation : MonoBehaviour
{
    [SerializeField] private GameObject[] _objectsToAnimate;
    [SerializeField] private float _animationDuration = 1f;
    [SerializeField] private float _delay=0.25f;
    private IMovable[] _movableObjects;
    private ISetDuration[] _setDurations;

    private void Awake()
    {
        _movableObjects=new IMovable[_objectsToAnimate.Length];
        _setDurations = new ISetDuration[_objectsToAnimate.Length];
        for (int i = 0; i < _objectsToAnimate.Length; i++)
        {
            _movableObjects[i] = _objectsToAnimate[i].GetComponent<IMovable>();
        }
        for(int i = 0; i < _objectsToAnimate.Length; i++)
        {
            _setDurations[i] = _objectsToAnimate[i].GetComponent<ISetDuration>();
            if (_setDurations[i] != null)
            {
                _setDurations[i].SetDuration(_animationDuration);
            }
        }
    }

    private void OnEnable()
    {
        StartCoroutine(PlayAnimationWithDelay());
    }

    private IEnumerator PlayAnimationWithDelay()
    {
        foreach (var movable in _movableObjects)
        {
            yield return new WaitForSeconds(_delay);
            movable.Move();
        }
    }
}
