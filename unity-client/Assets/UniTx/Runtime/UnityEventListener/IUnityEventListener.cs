using System;

namespace UniTx.Runtime.UnityEventListener
{
    /// <summary>
    /// Listener for common Unity lifecycle events (Update, LateUpdate, Pause, Quit).
    /// </summary>
    public interface IUnityEventListener
    {
        /// <summary>
        /// Invoked every frame during the <c>Update</c> phase.
        /// </summary>
        event Action OnUpdate;

        /// <summary>
        /// Invoked every frame during the <c>LateUpdate</c> phase.
        /// </summary>
        event Action OnLateUpdate;

        /// <summary>
        /// Invoked every frame during the <c>FixedUpdate</c> phase.
        /// </summary>
        event Action OnFixedUpdate;

        /// <summary>
        /// Invoked when the application is paused or resumed.
        /// The boolean argument is <c>true</c> when paused, and <c>false</c> when resumed.
        /// </summary>
        event Action<bool> OnPause;

        /// <summary>
        /// Invoked when the application is quit.
        /// </summary>
        event Action OnQuit;
    }
}