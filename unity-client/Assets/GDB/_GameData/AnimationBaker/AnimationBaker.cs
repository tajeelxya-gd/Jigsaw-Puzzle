using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AnimationBaker : MonoBehaviour
{
    public ComputeShader animTextGen;
    void Start()
    {
        StartCoroutine(Bake());
    }
    IEnumerator Bake()
    {
        var animator = GetComponent<Animator>();
        var clips = animator.runtimeAnimatorController.animationClips;
        var skin = GetComponentInChildren<SkinnedMeshRenderer>();
        var vCount = skin.sharedMesh.vertexCount;

        var mesh = new Mesh();
        animator.speed = 0;
        var textWidth = Mathf.NextPowerOfTwo(vCount);
        foreach (var c in clips)
        {
            var frames = Mathf.NextPowerOfTwo((int)(c.length / 0.05f));
            var info = new List<VertInfo>();

            var pRt = new RenderTexture(textWidth, frames, 0, RenderTextureFormat.ARGBHalf);
            var nRt = new RenderTexture(textWidth, frames, 0, RenderTextureFormat.ARGBHalf);
            pRt.name = string.Format("{0}.{1}.posText", name, c.name);
            nRt.name = string.Format("{0}.{1}.normText", name, c.name);

            foreach (var rt in new[] { pRt, nRt })
            {
                rt.enableRandomWrite = true;
                rt.Create();
                RenderTexture.active = rt;
                GL.Clear(true, true, Color.clear);
            }
            animator.Play(c.name);
            yield return 0;
            for (int i = 0; i < frames; i++)
            {
                animator.Play(c.name, 0, (float)i / frames);
                yield return 0;
                skin.BakeMesh(mesh);
                info.AddRange(Enumerable.Range(0, vCount).Select(idx => new VertInfo()
                {
                    position = mesh.vertices[idx],
                    normal = mesh.normals[idx]
                }));
            }
            var buffer = new ComputeBuffer(info.Count, System.Runtime.InteropServices.Marshal.SizeOf(typeof(VertInfo)));
            buffer.SetData(info);

            var Kernel = animTextGen.FindKernel("CSMain");
            uint x, y, z;
            animTextGen.GetKernelThreadGroupSizes(Kernel, out x, out y, out z);
            animTextGen.SetInt("VertCount", vCount);
            animTextGen.SetBuffer(Kernel, "meshInfo", buffer);
            animTextGen.SetTexture(Kernel, "OutPosition", pRt);
            animTextGen.SetTexture(Kernel, "OutNormal", nRt);

            animTextGen.Dispatch(Kernel, vCount / (int)x + 1, frames / (int)y + 1, (int)z);
            buffer.Release();

#if UNITY_EDITOR
            var posTex = Convert(pRt);
            var normTex = Convert(nRt);
            Graphics.CopyTexture(pRt, posTex);
            Graphics.CopyTexture(nRt, normTex);

            AssetDatabase.CreateAsset(posTex, Path.Combine("Assets", pRt.name + ".asset"));
            AssetDatabase.CreateAsset(normTex, Path.Combine("Assets", nRt.name + ".asset"));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }
        yield return null;
    }
    public Texture2D Convert(RenderTexture rt)
    {
        var texture = new Texture2D(rt.width, rt.height, TextureFormat.RGBAHalf, false);
        RenderTexture.active = rt;
        texture.ReadPixels(Rect.MinMaxRect(0, 0, rt.width, rt.height), 0, 0);
        RenderTexture.active = null;
        return texture;
    }
    public struct VertInfo
    {
        public Vector3 position;
        public Vector3 normal;
    }
}