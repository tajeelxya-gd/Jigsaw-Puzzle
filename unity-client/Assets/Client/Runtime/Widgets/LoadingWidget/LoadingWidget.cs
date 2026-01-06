using UniTx.Runtime.Widgets;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class LoadingWidget : MonoBehaviour, IWidget
    {
        public GameObject GameObject => gameObject;

        public Transform Transform => transform;

        public void Initialise()
        {
            // Empty yet.
        }

        public void Reset()
        {
            // Empty yet.
        }
    }
}
