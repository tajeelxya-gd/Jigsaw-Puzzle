using System;

namespace UniTx.Runtime.Events
{
    internal readonly struct Listener<TEvent> : IListener
        where TEvent : struct, IEvent
    {
        public readonly Action<TEvent> Action;
        public readonly Priority Priority { get; }

        public Listener(Action<TEvent> action, Priority priority = default)
        {
            Action = action;
            Priority = priority;
        }
    }
}