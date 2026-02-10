using System;
using System.Collections.Generic;

namespace UniTx.Runtime.StateMachine
{
    /// <summary>
    /// A state machine implementation that manages transitions between registered states.
    /// </summary>
    public class StateMachine : IStateMachine
    {
        private readonly Dictionary<Type, IState> _states = new();

        /// <summary>
        /// Gets the current active state.
        /// </summary>
        public IState CurrentState { get; private set; }

        /// <summary>
        /// Registers a state instance. If a state of the same type is already registered, it will be replaced.
        /// </summary>
        /// <param name="state">The state instance to register.</param>
        public void RegisterState(IState state)
        {
            var type = state.GetType();
            _states[type] = state;
        }

        /// <summary>
        /// Unregisters a state of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of state to unregister.</typeparam>
        public void UnregisterState<T>() where T : IState
        {
            var type = typeof(T);
            if (_states.ContainsKey(type))
            {
                if (CurrentState != null && CurrentState.GetType() == type)
                {
                    CurrentState.OnExit();
                    CurrentState = null;
                }
                _states.Remove(type);
            }
        }

        /// <summary>
        /// Switches to the registered state of type T. 
        /// Throws an exception if the state is not registered.
        /// </summary>
        /// <typeparam name="T">The type of state to switch to.</typeparam>
        public void SwitchState<T>() where T : IState
        {
            var type = typeof(T);
            if (!_states.TryGetValue(type, out var newState))
            {
                throw new InvalidOperationException($"State of type {type.Name} is not registered in the state machine.");
            }

            if (CurrentState == newState)
            {
                return;
            }

            CurrentState?.OnExit();
            CurrentState = newState;
            CurrentState?.OnEnter();
        }
    }
}
