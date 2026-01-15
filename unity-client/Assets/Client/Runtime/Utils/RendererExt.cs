using UnityEngine;

namespace Client.Runtime
{
    public static class RendererExt
    {
        public static void SetTexture(this Renderer renderer, Texture2D texture)
        {
            renderer.material.EnableKeyword("_EMISSION");
            var sharedMaterials = renderer.materials;

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

            renderer.materials = sharedMaterials;
        }
    }
}