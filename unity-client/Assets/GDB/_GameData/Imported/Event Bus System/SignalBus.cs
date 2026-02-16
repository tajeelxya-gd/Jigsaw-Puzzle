using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SignalBus
{
    private static readonly Dictionary<Type, List<Delegate>> listeners = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        Reset();
    }

    public static void Subscribe<TSignal>(Action<TSignal> action) where TSignal : ISignal
    {
        var type = typeof(TSignal);

        if (!listeners.ContainsKey(type))
            listeners[type] = new List<Delegate>();

        // Avoid adding duplicate listeners
        if (!listeners[type].Contains(action))
        {
            listeners[type].Add(action);
        }
    }

    public static void Unsubscribe<TSignal>(Action<TSignal> action) where TSignal : ISignal
    {
        var type = typeof(TSignal);

        if (listeners.ContainsKey(type))
        {
            listeners[type].Remove(action);

            // Clean up the list if its empty
            if (listeners[type].Count == 0)
            {
                listeners.Remove(type);
            }
        }
    }

    public static void Publish<TSignal>(TSignal signal) where TSignal : ISignal
    {
        var type = typeof(TSignal);
        if (listeners.ContainsKey(type))
        {
            foreach (var listener in listeners[type])
            {
                ((Action<TSignal>)listener)?.Invoke(signal);
            }
        }
    }

    // Reset signal listeners (can be used when reloading scenes or resetting game state)
    public static void Reset()
    {
        listeners.Clear();
    }
}