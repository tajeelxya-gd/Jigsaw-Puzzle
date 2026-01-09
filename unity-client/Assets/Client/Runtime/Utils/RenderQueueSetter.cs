using UnityEngine;

namespace Client.Runtime
{
    [RequireComponent(typeof(Renderer))]
    public sealed class RenderQueueSetter : MonoBehaviour
    {
        [SerializeField] private int _value = 2000;

        private Renderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            SetQueue(_value);
        }

        public void SetQueue(int value)
        {
            _value = value;
            var materials = _renderer.sharedMaterials;

            foreach (var mat in materials)
            {
                if (mat != null) mat.renderQueue = value;
            }
        }
    }
}
