using System;
using UniTx.Runtime.Extensions;

namespace UniTx.Runtime.UnityEventListener
{
    public sealed class UnityEventListener : IUnityEventListener, IInitialisable, IResettable
    {
        public event Action OnUpdate;
        public event Action OnLateUpdate;
        public event Action OnFixedUpdate;
        public event Action<bool> OnPause;
        public event Action OnQuit;

        public void Initialise()
        {
            var behaviour = UniStatics.Root.AddComponent<UnityEventBehaviour>();
            behaviour.SetListener(this);
        }

        public void Reset()
        {
            if (UniStatics.Root.TryGetComponent<UnityEventBehaviour>(out var behaviour))
            {
                UnityEngine.Object.Destroy(behaviour);
            }
        }

        public void BroadcastOnUpdate() => OnUpdate.Broadcast();
        public void BroadcastOnLateUpdate() => OnLateUpdate.Broadcast();
        public void BroadcastOnFixedUpdate() => OnFixedUpdate.Broadcast();
        public void BroadcastOnPause(bool status) => OnPause.Broadcast(status);
        public void BroadcastOnQuit() => OnQuit.Broadcast();
    }
}