using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

public class AnimationPoolManager : MonoBehaviour
{
    [SerializeField] private ObjectMovementWithDelay _prefab;
    [SerializeField] private int _initialPoolSize = 10;
    [SerializeField] private Transform _poolParent;
    [SerializeField] private protected Transform _defaultStartPosition;
    [SerializeField] private protected Transform _defaultEndPosition;
    [SerializeField] private int _amountToAnimate = 10;
    private protected List<ObjectMovementWithDelay> _pool = new();

    private protected virtual void Start()
    {
        for (int i = 0; i < _initialPoolSize; i++)
            CreateNewItem();
    }

    [Button]
    public virtual void PlayAll()
    {
        StartCoroutine(PlayWithDelay(_defaultStartPosition.position, _defaultEndPosition, _amountToAnimate));
    }

    private WaitForSeconds _wait = new WaitForSeconds(0.05f);
    private protected virtual IEnumerator PlayWithDelay(Vector3 startPosition, Transform endPosition, int quantity = 5)
    {
        for (int i = 0; i < quantity; i++)
        {
            ObjectMovementWithDelay obj = GetAvailableItem();

            obj.Play(startPosition, endPosition);

            yield return _wait;
        }
    }

    private ObjectMovementWithDelay CreateNewItem()
    {
        ObjectMovementWithDelay obj = Instantiate(_prefab, _poolParent);
        obj.Inject(this);
        obj.gameObject.SetActive(false);
        _pool.Add(obj);
        return obj;
    }

    private ObjectMovementWithDelay GetAvailableItem()
    {
        foreach (var item in _pool)
        {
            if (!item.gameObject.activeInHierarchy)
                return item;
        }

        return CreateNewItem();
    }

    public void ReturnToPool(ObjectMovementWithDelay obj)
    {
        obj.gameObject.SetActive(false);
    }

    public virtual void SetPositions(Transform a, Transform b)
    {
        _defaultStartPosition = a;
        _defaultEndPosition = b;
    }
    public virtual void SetAnimtionAmount(int n)
    {
        _amountToAnimate = n;
    }
}

public class PlayAnimationSignal : ISignal
{
    public Sprite sprite;
    public int Quantity;
    public Transform _startTransform;
    public Transform _endTransform;
}
