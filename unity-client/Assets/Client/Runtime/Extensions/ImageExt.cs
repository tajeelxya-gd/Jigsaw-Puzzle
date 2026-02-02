using UnityEngine;
using UnityEngine.UI;

namespace Client.Runtime
{
    public static class ImageExt
    {
        public static void SetAlpha(this Image image, float alpha)
        {
            var color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }
}