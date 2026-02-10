using System;

namespace UniTx.Runtime
{
    /// <summary>
    /// Defines the contract for a state machine that manages transitions between registered states.
    /// </summary>
    public interface IStateMachine
    {
        /// <summary>
        /// Gets the current active state of the state machine.
        /// </summary>
        IState CurrentState { get; }

        /// <summary>
        /// Registers a state in the state machine.
        /// </summary>
        /// <param name="state">The state instance to register.</param>
        void RegisterState(IState state);

        /// <summary>
        /// Unregisters a state of the specified type from the state machine.
        /// </summary>
        /// <typeparam name="T">The type of the state to unregister.</typeparam>
        void UnregisterState<T>() where T : IState;

        /// <summary>
        /// Switches the state machine to a registered state of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the state to switch to.</typeparam>
        void SwitchState<T>() where T : IState;
    }
}
