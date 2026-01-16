using UnityEngine;

namespace Client.Runtime
{
    public interface IJigsawHelper
    {
        void SetTexture(Texture2D texture);
        
        void ToggleImage();

        Material GetBaseMaterial();

        Material GetOutlineMaterial();
    }
}