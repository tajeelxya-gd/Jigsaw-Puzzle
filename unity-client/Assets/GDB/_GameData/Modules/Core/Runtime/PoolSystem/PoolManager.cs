using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
public static class PoolManager
{
    private static readonly Dictionary<Type, IClearable> _pools = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Init()
    {
        _pools.Clear();
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private static void OnSceneUnloaded(Scene scene)
    {
        ResetAllPools();
    }

    public static void CreatePool<T>(T prefab, int defaultCapacity = 10, int maxSize = 50, Transform parent = null)
        where T : Component, IPoolable
    {
        var type = typeof(T);
        if (_pools.ContainsKey(type))
            return;

        _pools[type] = new PoolSystem<T>(prefab, defaultCapacity, maxSize, parent);
    }

    public static PoolSystem<T> GetPool<T>() where T : Component, IPoolable
    {
        if (_pools.TryGetValue(typeof(T), out var pool))
            return pool as PoolSystem<T>;
        return null;
    }

    public static void ResetAllPools()
    {
        foreach (var pool in _pools.Values)
            pool.Clear();

        _pools.Clear();
    }

    public static void ReturnAllItems()
    {
        foreach (var pool in _pools.Values)
        {
            if (pool is IReturnable returnable)
                returnable.ReturnAll();
        }
    }
}