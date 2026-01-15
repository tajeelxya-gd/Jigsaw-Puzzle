using UnityEngine;

namespace Client.Runtime
{
    public static class RendererExt
    {
        public static void SetTexture(this Renderer source, Texture2D texture)
        {
            source.material.EnableKeyword("_EMISSION");
            var sharedMaterials = source.materials;

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

            source.materials = sharedMaterials;
        }
    }
}