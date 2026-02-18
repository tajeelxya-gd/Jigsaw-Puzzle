using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]

public class Outline : MonoBehaviour
{
    private static HashSet<Mesh> registeredMeshes = new HashSet<Mesh>();

    public enum Mode
    {
        OutlineAll,
        OutlineVisible,
        OutlineHidden,
        OutlineAndSilhouette,
        SilhouetteOnly
    }

    [Serializable]
    private class ListVector3
    {
        public List<Vector3> data;
    }

    [SerializeField]
    private Mode outlineMode;

    [SerializeField]
    private Color outlineColor;
    public void ChangeColor(Color color)
    {
        if (outlineColor == color) return;
        outlineColor = color;
        UpdateMaterialProperties();
    }

    [SerializeField, Range(0f, 20f)]
    private float outlineWidth = 20f;

    [Header("Optional")]

    [SerializeField, Tooltip("Precompute enabled: Per-vertex calculations are performed in the editor and serialized with the object. "
    + "Precompute disabled: Per-vertex calculations are performed at runtime in Awake(). This may cause a pause for large meshes.")]
    private bool precomputeOutline;

    [SerializeField, HideInInspector]
    private List<Mesh> bakeKeys = new List<Mesh>();

    [SerializeField, HideInInspector]
    private List<ListVector3> bakeValues = new List<ListVector3>();

    public Renderer[] renderers;
    Material outlineMaskMaterial;
    Material outlineFillMaterial;

    private bool needsUpdate;

    void Awake()
    {
        outlineMaskMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineMask"));
        outlineMaskMaterial.name = "OutlineMask (Instance)";
        outlineFillMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineFill"));
        outlineFillMaterial.name = "OutlineFill (Instance)";
        // Retrieve or generate smooth normals
        LoadSmoothNormals();
        // Apply material properties immediately
        needsUpdate = true;
        UpdateMaterialProperties();
       // ChangeColor(outlineColor);
    }

    void OnEnable()
    {
        foreach (var renderer in renderers)
        {
            // Append outline shaders
            var materials = renderer.sharedMaterials.ToList();

            materials.Add(outlineMaskMaterial);
            materials.Add(outlineFillMaterial);

            renderer.materials = materials.ToArray();
        }
    }

    void OnValidate()
    {

        //if (enabled)
        {
            if (renderers == null || renderers.Length <= 0)
                renderers = GetComponentsInChildren<MeshRenderer>();
        }
        // Update material properties
        needsUpdate = true;

        // Clear cache when baking is disabled or corrupted
        if (!precomputeOutline && bakeKeys.Count != 0 || bakeKeys.Count != bakeValues.Count)
        {
            bakeKeys.Clear();
            bakeValues.Clear();
        }

        // Generate smooth normals when baking is enabled
        if (precomputeOutline && bakeKeys.Count == 0)
        {
            Bake();
        }
    }

    // public void UpdateOutline()
    // {
    //     if (needsUpdate)
    //     {
    //         needsUpdate = false;

    //         UpdateMaterialProperties();
    //     }
    // }

    void OnDisable()
    {
        foreach (var renderer in renderers)
        {

            // Remove outline shaders
            var materials = renderer.sharedMaterials.ToList();

            materials.Remove(outlineMaskMaterial);
            materials.Remove(outlineFillMaterial);

            renderer.materials = materials.ToArray();
        }
    }

    void OnDestroy()
    {
        Destroy(outlineMaskMaterial);
        Destroy(outlineFillMaterial);
    }

    void Bake()
    {

        // Generate smooth normals for each mesh
        var bakedMeshes = new HashSet<Mesh>();

        foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
        {

            // Skip duplicates
            if (!bakedMeshes.Add(meshFilter.sharedMesh))
            {
                continue;
            }

            // Serialize smooth normals
            var smoothNormals = SmoothNormals(meshFilter.sharedMesh);

            bakeKeys.Add(meshFilter.sharedMesh);
            bakeValues.Add(new ListVector3() { data = smoothNormals });
        }
    }

    void LoadSmoothNormals()
    {
        foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
        {
            var mesh = meshFilter.sharedMesh;
            if (mesh == null) continue;

            // Skip if already processed
            if (!registeredMeshes.Add(mesh))
                continue;

            // Get smooth normals
            var index = bakeKeys.IndexOf(mesh);
            var smoothNormals = (index >= 0)
                ? bakeValues[index].data
                : SmoothNormals(mesh);

            // 🔒 CRITICAL FIX: ensure correct size
            if (smoothNormals == null || smoothNormals.Count != mesh.vertexCount)
            {
                Debug.LogWarning(
                    $"Smooth normal mismatch on {mesh.name}. " +
                    $"Expected {mesh.vertexCount}, got {smoothNormals?.Count ?? 0}. Rebuilding."
                );

                smoothNormals = new List<Vector3>(mesh.vertexCount);
                smoothNormals.AddRange(mesh.normals);
            }

            // Store in UV4 (SetUVs channel 3)
            mesh.SetUVs(3, smoothNormals);

            // Combine submeshes
            var renderer = meshFilter.GetComponent<Renderer>();
            if (renderer != null)
            {
                CombineSubmeshes(mesh, renderer.sharedMaterials);
            }
        }

        // Handle skinned meshes
        foreach (var skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            var mesh = skinnedMeshRenderer.sharedMesh;
            if (mesh == null) continue;

            if (!registeredMeshes.Add(mesh))
                continue;

            // Clear UV4 safely
            mesh.uv4 = new Vector2[mesh.vertexCount];

            CombineSubmeshes(mesh, skinnedMeshRenderer.sharedMaterials);
        }
    }


    List<Vector3> SmoothNormals(Mesh mesh)
    {

        // Group vertices by location
        var groups = mesh.vertices.Select((vertex, index) => new KeyValuePair<Vector3, int>(vertex, index)).GroupBy(pair => pair.Key);

        // Copy normals to a new list
        var smoothNormals = new List<Vector3>(mesh.normals);

        // Average normals for grouped vertices
        foreach (var group in groups)
        {

            // Skip single vertices
            if (group.Count() == 1)
            {
                continue;
            }

            // Calculate the average normal
            var smoothNormal = Vector3.zero;

            foreach (var pair in group)
            {
                smoothNormal += smoothNormals[pair.Value];
            }

            smoothNormal.Normalize();

            // Assign smooth normal to each vertex
            foreach (var pair in group)
            {
                smoothNormals[pair.Value] = smoothNormal;
            }
        }

        return smoothNormals;
    }

    void CombineSubmeshes(Mesh mesh, Material[] materials)
    {

        // Skip meshes with a single submesh
        if (mesh.subMeshCount == 1)
        {
            return;
        }

        // Skip if submesh count exceeds material count
        if (mesh.subMeshCount > materials.Length)
        {
            return;
        }

        // Append combined submesh
        mesh.subMeshCount++;
        mesh.SetTriangles(mesh.triangles, mesh.subMeshCount - 1);
    }

    int _OutlineColor = Shader.PropertyToID("_OutlineColor");
    int _ZTest = Shader.PropertyToID("_ZTest");
    int _OutlineWidth = Shader.PropertyToID("_OutlineWidth");

    void UpdateMaterialProperties()
    {

        // Apply properties according to mode
        outlineFillMaterial.SetColor(_OutlineColor, outlineColor);

        switch (outlineMode)
        {
            case Mode.OutlineAll:
                outlineMaskMaterial.SetFloat(_ZTest, (float)UnityEngine.Rendering.CompareFunction.Always);
                outlineFillMaterial.SetFloat(_ZTest, (float)UnityEngine.Rendering.CompareFunction.Always);
                outlineFillMaterial.SetFloat(_OutlineWidth, outlineWidth);
                break;

            case Mode.OutlineVisible:
                outlineMaskMaterial.SetFloat(_ZTest, (float)UnityEngine.Rendering.CompareFunction.Always);
                outlineFillMaterial.SetFloat(_ZTest, (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                outlineFillMaterial.SetFloat(_OutlineWidth, outlineWidth);
                break;

            case Mode.OutlineHidden:
                outlineMaskMaterial.SetFloat(_ZTest, (float)UnityEngine.Rendering.CompareFunction.Always);
                outlineFillMaterial.SetFloat(_ZTest, (float)UnityEngine.Rendering.CompareFunction.Greater);
                outlineFillMaterial.SetFloat(_OutlineWidth, outlineWidth);
                break;

            case Mode.OutlineAndSilhouette:
                outlineMaskMaterial.SetFloat(_ZTest, (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                outlineFillMaterial.SetFloat(_ZTest, (float)UnityEngine.Rendering.CompareFunction.Always);
                outlineFillMaterial.SetFloat(_OutlineWidth, outlineWidth);
                break;

            case Mode.SilhouetteOnly:
                outlineMaskMaterial.SetFloat(_ZTest, (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                outlineFillMaterial.SetFloat(_ZTest, (float)UnityEngine.Rendering.CompareFunction.Greater);
                outlineFillMaterial.SetFloat(_OutlineWidth, 0f);
                break;
        }
    }
    public void HandleOutline(bool Enable, Color color = default)
    {
        this.enabled = Enable;
        if (Enable)
            ChangeColor(color == default ? Color.green : color);
    }
}
