using UnityEngine;

namespace Client.Runtime
{
    public interface IJigsawHelper
    {
        void SetTexture(Texture2D texture);

        void ShowFullImage(bool value);
    }
}