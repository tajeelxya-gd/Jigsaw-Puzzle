using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public class VAT_Texture2DArrayCreator : MonoBehaviour
{
    public Texture2D[] items;
    [Button]
    private void CreateArray()
    {
        Texture2D[] src = items;

        int width = src[0].width;
        int maxHeight = 0;
        TextureFormat format = src[0].format;
        bool mipmaps = false;

        foreach (var t in src)
        {
            if (t.width != width)
            {
                return;
            }

            if (t.format != format)
            {
                return;
            }

            maxHeight = Mathf.Max(maxHeight, t.height);
        }

        Texture2DArray array = new Texture2DArray(
            width,
            maxHeight,
            src.Length,
            format,
            mipmaps,
            false
        );

        array.wrapMode = TextureWrapMode.Clamp;
        array.filterMode = FilterMode.Point;

        for (int i = 0; i < src.Length; i++)
        {
            Texture2D padded = PadTexture(src[i], width, maxHeight);
            Graphics.CopyTexture(padded, 0, array, i);
        }
        #if UNITY_EDITOR
                AssetDatabase.CreateAsset(array, "Assets/VAT_TextureArray.asset");
                AssetDatabase.SaveAssets();
        #endif

        Debug.Log($"Texture2DArray created. Width={width}, Height={maxHeight}, Layers={src.Length}");
    }

    private Texture2D PadTexture(Texture2D source, int width, int height)
    {
        RenderTexture rt = RenderTexture.GetTemporary(
            width,
            height,
            0,
            RenderTextureFormat.ARGBHalf
        );

        Graphics.Blit(source, rt);

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D padded = new Texture2D(width, height, source.format, false, true);
        padded.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        padded.Apply();

        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);

        return padded;
    }
}
