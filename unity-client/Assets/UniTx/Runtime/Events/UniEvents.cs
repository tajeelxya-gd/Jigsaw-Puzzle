using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace UniTx.Runtime.Events
{
    /// <summary>
    /// Event bus for subscribing, unsubscribing, and raising value-type events.
    /// </summary>
    public static class UniEvents
    {
        private static IEventBus _eventBus = null;

        internal static UniTask InitialiseAsync(IEventBus eventBus, CancellationToken cToken = default)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            return _eventBus.InitialiseAsync(cToken);
        }

        /// <summary>
        /// Removes all subscriptions.
        /// </summary>
        public static void Reset() => _eventBus.Reset();

        /// <summary>
        /// Subscribes a listener to an event type with an optional priority.
        /// </summary>
        /// <typeparam name="TEvent">The event type to subscribe to.</typeparam>
        /// <param name="action">The callback invoked when the event is raised.</param>
        /// <param name="priority">Optional priority for invocation order.</param>
        public static void Subscribe<TEvent>(Action<TEvent> action, Priority priority = default)
            where TEvent : struct, IEvent
            => _eventBus.Subscribe(action, priority);

        /// <summary>
        /// Unsubscribes a previously registered listener for the event type.
        /// </summary>
        /// <typeparam name="TEvent">The event type to unsubscribe from.</typeparam>
        /// <param name="action">The callback to remove.</param>
        public static void Unsubscribe<TEvent>(Action<TEvent> action)
            where TEvent : struct, IEvent
            => _eventBus.Unsubscribe(action);

        /// <summary>
        /// Raises an event, invoking all subscribed listeners for its type.
        /// </summary>
        /// <typeparam name="TEvent">The event type being raised.</typeparam>
        /// <param name="event">The event instance to dispatch.</param>
        public static void Raise<TEvent>(TEvent @event)
            where TEvent : struct, IEvent
            => _eventBus.Raise(@event);
    }
}