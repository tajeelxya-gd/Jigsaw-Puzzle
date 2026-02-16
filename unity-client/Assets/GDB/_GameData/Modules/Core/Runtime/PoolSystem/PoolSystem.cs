using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
public class PoolSystem<T> : IClearable, IReturnable where T : Component, IPoolable
{
    private readonly ObjectPool<T> _pool;
    private readonly T _prefab;
    private readonly Transform _parent;

    private readonly HashSet<T> _activeObjects = new();

    public PoolSystem(T prefab, int defaultCapacity = 0, int maxSize = 50, Transform parent = null)
    {
        _prefab = prefab;
        _parent = parent;

        _pool = new ObjectPool<T>(
            CreateFunc,
            OnGet,
            OnRelease,
            OnDestroy,
            collectionCheck: false,
            defaultCapacity,
            maxSize
        );
    }

    private T CreateFunc()
    {
        var obj = Object.Instantiate(_prefab, _parent);
        obj.gameObject.SetActive(false);
        return obj;
    }

    private void OnGet(T obj)
    {
        _activeObjects.Add(obj);
        obj.gameObject.SetActive(true);
        obj.OnSpawned();
    }

    private void OnRelease(T obj)
    {
        _activeObjects.Remove(obj);
        obj.OnDespawned();
        obj.gameObject.SetActive(false);
    }

    private void OnDestroy(T obj)
    {
        if (!obj) return;
        obj.gameObject.SetActive(false);
    }

    public T Get() => _pool.Get();
    public void Release(T obj) => _pool.Release(obj);
    public void Clear() => _pool.Clear();

    public void ReturnAll()
    {
        var tempList = ListPool<T>.Get();
        tempList.AddRange(_activeObjects);

        foreach (var obj in tempList)
            _pool.Release(obj);

        ListPool<T>.Release(tempList);
    }

    public void Prewarm(int count)
    {
        List<T> temp = new(count);
        for (int i = 0; i < count; i++)
            temp.Add(_pool.Get());
        foreach (var t in temp)
            _pool.Release(t);
    }
}