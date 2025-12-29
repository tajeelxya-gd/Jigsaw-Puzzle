using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;

namespace UniTx.Runtime.Events
{
    internal sealed class UniEventBus : IEventBus
    {
        private IDictionary<Type, List<IListener>> _subscriptions;
        private Comparer _comparer;

        public UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            _subscriptions = new Dictionary<Type, List<IListener>>();
            _comparer = new();
            return UniTask.CompletedTask;
        }

        public void Reset()
        {
            foreach (var kvp in _subscriptions)
            {
                kvp.Value.Clear();
            }

            _subscriptions.Clear();
        }

        public void Subscribe<TEvent>(Action<TEvent> action, Priority priority = default)
            where TEvent : struct, IEvent
        {
            var eventType = typeof(TEvent);
            var listener = new Listener<TEvent>(action, priority);

            if (_subscriptions.TryGetValue(eventType, out var listeners))
            {
                listeners.Add(listener);
                listeners.Sort(_comparer);
                return;
            }

            _subscriptions.Add(eventType, new List<IListener> { listener });
        }

        public void Unsubscribe<TEvent>(Action<TEvent> action)
            where TEvent : struct, IEvent
        {
            if (!_subscriptions.TryGetValue(typeof(TEvent), out var listeners)) return;

            for (var i = 0; i < listeners.Count; i++)
            {
                if (listeners[i] is Listener<TEvent> typed && typed.Action == action)
                {
                    listeners.RemoveAt(i);
                    if (listeners.Count > 1)
                    {
                        listeners.Sort(_comparer);
                    }

                    break;
                }
            }
        }

        public void Raise<TEvent>(TEvent @event)
            where TEvent : struct, IEvent
        {
            if (!_subscriptions.TryGetValue(typeof(TEvent), out var listeners)) return;

            foreach (var listener in listeners)
            {
                if (listener is Listener<TEvent> typed)
                {
                    typed.Action(@event);
                }
            }
        }
    }
}