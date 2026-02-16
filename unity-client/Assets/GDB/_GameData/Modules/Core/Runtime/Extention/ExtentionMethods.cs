using UnityEngine;

public static class ExtentionMethods
{
    public static void SetColor(this Renderer renderer, Color color, string id = "_Color", int matInd = 0)
    {
        MaterialPropertyBlock _block = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(_block);
        _block.SetColor(id, color);
        renderer.SetPropertyBlock(_block, matInd);
    }
    public static void SetColorDefault(this Renderer renderer, Color color, string id = "_Color")
    {
        MaterialPropertyBlock _block = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(_block);
        _block.SetColor(id, color);
        renderer.SetPropertyBlock(_block);
    }
    public static void SetTexture(this Renderer renderer, Texture texture, string id = "_BaseMap", int matInd = 0)
    {
        MaterialPropertyBlock _block = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(_block);
        _block.SetTexture(id, texture);
        renderer.SetPropertyBlock(_block);
    }
    public static void SetAlpha(this ref Color color, float alpha)
    {
        color.a = alpha;
    }

    public static void SetAlpha(this ref Color32 color, float alpha)
    {
        color.a = (byte)(Mathf.Clamp01(alpha) * 255f);
    }
}