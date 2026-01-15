using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigsawHelper : MonoBehaviour, IJigsawHelper
    {
        [SerializeField] private Renderer _renderer;

        public void SetFullImage(Texture2D texture) => _renderer.SetTexture(texture);

        public void ShowFullImage(bool value) => _renderer.gameObject.SetActive(value);
    }
}