using Sirenix.OdinInspector;
using UnityEngine;

public class TestAnimMaterialOveride : MonoBehaviour
{
    public Renderer renderer;
    public int animIndex = 0;

    MaterialPropertyBlock mpb;

    static readonly int AnimIndexID = Shader.PropertyToID("_AnimIndex");

    void Awake()
    {
        mpb = new MaterialPropertyBlock();
    }
    [Button]
    public void Apply()
    {
        if (renderer == null)
        {
            Debug.LogError("Renderer not assigned!");
            return;
        }

        mpb ??= new MaterialPropertyBlock(); // ensure MPB is created
        renderer.GetPropertyBlock(mpb);
        mpb.SetFloat(AnimIndexID, animIndex);

        renderer.SetPropertyBlock(mpb);
    }
}