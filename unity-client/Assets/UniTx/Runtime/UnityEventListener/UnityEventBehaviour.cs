using UnityEngine;

namespace UniTx.Runtime.UnityEventListener
{
    internal sealed class UnityEventBehaviour : MonoBehaviour
    {
        private UnityEventListener _listener;

        public void SetListener(UnityEventListener listener) => _listener = listener;

        private void Update() => _listener.BroadcastOnUpdate();

        private void LateUpdate() => _listener.BroadcastOnLateUpdate();

        private void FixedUpdate() => _listener.BroadcastOnFixedUpdate();

        private void OnApplicationPause(bool status) => _listener.BroadcastOnPause(status);

        private void OnApplicationQuit() => _listener.BroadcastOnQuit();
    }
}