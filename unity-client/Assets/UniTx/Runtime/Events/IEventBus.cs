using System;

namespace UniTx.Runtime.Events
{
    /// <summary>
    /// Event bus interface for subscribing, unsubscribing, and raising value-type events.
    /// </summary>
    public interface IEventBus : IInitialisableAsync, IResettable
    {
        /// <summary>
        /// Subscribes a listener to an event type with an optional priority.
        /// </summary>
        /// <typeparam name="TEvent">The event type to subscribe to.</typeparam>
        /// <param name="action">The callback invoked when the event is raised.</param>
        /// <param name="priority">Optional priority for invocation order.</param>
        void Subscribe<TEvent>(Action<TEvent> action, Priority priority = default)
            where TEvent : struct, IEvent;

        /// <summary>
        /// Unsubscribes a previously registered listener for the event type.
        /// </summary>
        /// <typeparam name="TEvent">The event type to unsubscribe from.</typeparam>
        /// <param name="action">The callback to remove.</param>
        void Unsubscribe<TEvent>(Action<TEvent> action)
            where TEvent : struct, IEvent;

        /// <summary>
        /// Raises an event, invoking all subscribed listeners for its type.
        /// </summary>
        /// <typeparam name="TEvent">The event type being raised.</typeparam>
        /// <param name="event">The event instance to dispatch.</param>
        void Raise<TEvent>(TEvent @event)
            where TEvent : struct, IEvent;
    }

}