using UniTx.Runtime.Events;
using UniTx.Runtime.IoC;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class DemoTester : MonoBehaviour
    {
        public void OnPressDemoButton()
        {
            UniEvents.Raise(new DemoTestEvent());
        }
    }
}
