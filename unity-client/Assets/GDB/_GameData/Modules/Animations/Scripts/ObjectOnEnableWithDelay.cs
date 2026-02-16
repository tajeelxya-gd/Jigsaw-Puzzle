using System;
using System.Collections;
using UnityEngine;

public class ObjectOnEnableWithDelay : MonoBehaviour
{
    [SerializeField] private GameObject[] _objectsToEnable;
    private IMovable[] _movable;
    [SerializeField] private float _delayBetweenMoves = 0.2f;

    private void Start()
    {
        for (int i = 0; i < _objectsToEnable.Length; i++)
        {
            _movable[i] = _objectsToEnable[i].GetComponent<IMovable>();
        }
    }

    private void OnEnable()
    {
        _movable = new IMovable[_objectsToEnable.Length];
        StartCoroutine(PlayWithDelay());
    }

    private IEnumerator PlayWithDelay()
    {
        yield return new WaitForSeconds(0.2f);
        for(int i=0;i<_objectsToEnable.Length;i++)
        {
            if(_movable[i] != null)
            {
                _movable[i].Move();
            }
            yield return new WaitForSeconds(_delayBetweenMoves);
        }
    }
}
