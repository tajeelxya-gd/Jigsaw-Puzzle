using UnityEngine;

namespace Client.Runtime
{
    public interface IJigsawHelper
    {
        void SetFullImage(Texture2D texture);
        void ShowFullImage(bool value);
    }
}