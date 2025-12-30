using UnityEngine;

namespace Client.Runtime
{
    [RequireComponent(typeof(Collider))]
    public class SlotController : MonoBehaviour
    {
        private Collider _slotCollider;

        private void Awake()
        {
            _slotCollider = GetComponent<Collider>();
        }

        private void OnTriggerExit(Collider other)
        {
            // Re-enable slot collider when piece exits
            if (other.gameObject.layer == LayerMask.NameToLayer("JigsawPiece"))
            {
                _slotCollider.enabled = true;
            }
        }
    }
}
