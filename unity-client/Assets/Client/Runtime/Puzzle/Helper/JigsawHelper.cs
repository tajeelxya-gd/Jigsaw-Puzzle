using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigsawHelper : MonoBehaviour, IJigsawHelper
    {
        [SerializeField] private Renderer _renderer;

        public void SetTexture(Texture2D texture)
        {
            _renderer.material.EnableKeyword("_EMISSION");
            var sharedMaterials = _renderer.materials;

            for (int i = 0; i < sharedMaterials.Length; i++)
            {
                var sharedMaterial = sharedMaterials[i];
                if (sharedMaterial != null)
                {
                    sharedMaterial.SetTexture("_BaseMap", texture);
                    sharedMaterial.SetTexture("_DetailAlbedoMap", texture);
                    sharedMaterial.SetFloat("_DetailAlbedoMapScale", 0.75f);
                    sharedMaterial.EnableKeyword("_DETAIL_MULX2");
                }
            }

            _renderer.materials = sharedMaterials;
        }

        public void ShowFullImage(bool value)
        {
            _renderer.gameObject.SetActive(value);
        }
    }
}