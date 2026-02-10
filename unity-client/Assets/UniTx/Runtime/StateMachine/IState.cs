namespace UniTx.Runtime.StateMachine
{
    /// <summary>
    /// Defines the contract for a state in a state machine.
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        void OnEnter();

        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        void OnExit();
    }
}
