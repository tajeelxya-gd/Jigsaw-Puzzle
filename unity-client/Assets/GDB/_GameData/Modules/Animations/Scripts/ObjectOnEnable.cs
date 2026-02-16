using System;
using UnityEngine;

public class ObjectOnEnable : MonoBehaviour
{
    [SerializeField] private GameObject[] _objectsToEnable;
    private IMovable[] _movable;

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
        for(int i=0;i<_objectsToEnable.Length;i++)
        {
            if(_movable[i] != null)
            {
                _movable[i].Move();
            }
        }
    }
}
