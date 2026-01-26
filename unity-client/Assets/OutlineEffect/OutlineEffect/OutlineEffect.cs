using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace cakeslice
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class OutlineEffect : MonoBehaviour
    {
        public static OutlineEffect Instance { get; private set; }

        private readonly LinkedSet<Outline> outlines = new LinkedSet<Outline>();

        [Range(0.1f, 6.0f)]
        public float lineThickness = 1.25f;
        [Range(0, 10)]
        public float lineIntensity = .5f;
        
        // --- ADDED: Softness Property ---
        [Range(1.0f, 20.0f)]
        [Tooltip("Higher values make the outline sharper, lower values make it softer/blurry.")]
        public float lineSoftness = 10.0f; 
        // --------------------------------

        [Range(0, 1)]
        public float fillAmount = 0.2f;

        public Color lineColor0 = Color.red;
        public Color lineColor1 = Color.green;
        public Color lineColor2 = Color.blue;

        public bool additiveRendering = false;
        public bool backfaceCulling = true;

        public Color fillColor = Color.blue;
        public bool useFillColor = false;

        [Header("These settings can affect performance!")]
        public bool cornerOutlines = false;
        public bool addLinesBetweenColors = false;

        [Header("Advanced settings")]
        public bool scaleWithScreenSize = true;
        [Range(0.0f, 1.0f)]
        public float alphaCutoff = .5f;
        public bool flipY = false;
        public Camera sourceCamera;
        public bool autoEnableOutlines = false;

        [HideInInspector]
        public Camera outlineCamera;
        Material outline1Material;
        Material outline2Material;
        Material outline3Material;
        Material outlineEraseMaterial;
        Shader outlineShader;
        Shader outlineBufferShader;
        [HideInInspector]
        public Material outlineShaderMaterial;
        [HideInInspector]
        public RenderTexture renderTexture;
        [HideInInspector]
        public RenderTexture extraRenderTexture;

        CommandBuffer commandBuffer;

        Material GetMaterialFromID(int ID)
        {
            if (ID == 0) return outline1Material;
            else if (ID == 1) return outline2Material;
            else if (ID == 2) return outline3Material;
            else return outline1Material;
        }

        List<Material> materialBuffer = new List<Material>();
        Material CreateMaterial(Color emissionColor)
        {
            Material m = new Material(outlineBufferShader);
            m.SetColor("_Color", emissionColor);
            m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.DisableKeyword("_ALPHATEST_ON");
            m.EnableKeyword("_ALPHABLEND_ON");
            m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            m.renderQueue = 3000;
            return m;
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                throw new System.Exception("you can only have one outline camera in the scene");
            }
            Instance = this;
        }

        void Start()
        {
            CreateMaterialsIfNeeded();
            UpdateMaterialsPublicProperties();

            if (sourceCamera == null)
            {
                sourceCamera = GetComponent<Camera>();
                if (sourceCamera == null) sourceCamera = Camera.main;
            }

            if (outlineCamera == null)
            {
                foreach (Camera c in GetComponentsInChildren<Camera>())
                {
                    if (c.name == "Outline Camera")
                    {
                        outlineCamera = c;
                        c.enabled = false;
                        break;
                    }
                }

                if (outlineCamera == null)
                {
                    GameObject cameraGameObject = new GameObject("Outline Camera");
                    cameraGameObject.transform.parent = sourceCamera.transform;
                    outlineCamera = cameraGameObject.AddComponent<Camera>();
                    outlineCamera.enabled = false;
                }
            }

            if (renderTexture != null) renderTexture.Release();
            if (extraRenderTexture != null) extraRenderTexture.Release();
            
            renderTexture = new RenderTexture(sourceCamera.pixelWidth, sourceCamera.pixelHeight, 16, RenderTextureFormat.Default);
            extraRenderTexture = new RenderTexture(sourceCamera.pixelWidth, sourceCamera.pixelHeight, 16, RenderTextureFormat.Default);
            UpdateOutlineCameraFromSource();

            commandBuffer = new CommandBuffer();
            outlineCamera.AddCommandBuffer(CameraEvent.BeforeImageEffects, commandBuffer);
        }

        bool RenderTheNextFrame;
        public void OnPreRender()
        {
            if (commandBuffer == null) return;

            if (outlines.Count == 0)
            {
                if (!RenderTheNextFrame) return;
                RenderTheNextFrame = false;
            }
            else
            {
                RenderTheNextFrame = true;
            }

            CreateMaterialsIfNeeded();

            if (renderTexture == null || renderTexture.width != sourceCamera.pixelWidth || renderTexture.height != sourceCamera.pixelHeight)
            {
                if (renderTexture != null) renderTexture.Release();
                if (extraRenderTexture != null) extraRenderTexture.Release();
                renderTexture = new RenderTexture(sourceCamera.pixelWidth, sourceCamera.pixelHeight, 16, RenderTextureFormat.Default);
                extraRenderTexture = new RenderTexture(sourceCamera.pixelWidth, sourceCamera.pixelHeight, 16, RenderTextureFormat.Default);
                outlineCamera.targetTexture = renderTexture;
            }

            UpdateMaterialsPublicProperties();
            UpdateOutlineCameraFromSource();
            outlineCamera.targetTexture = renderTexture;
            commandBuffer.SetRenderTarget(renderTexture);
            commandBuffer.Clear();

            foreach (Outline outline in outlines)
            {
                LayerMask l = sourceCamera.cullingMask;
                if (outline != null && l == (l | (1 << outline.gameObject.layer)))
                {
                    for (int v = 0; v < outline.SharedMaterials.Length; v++)
                    {
                        Material m = null;
                        if (outline.SharedMaterials[v].HasProperty("_MainTex") && outline.SharedMaterials[v].mainTexture != null)
                        {
                            foreach (Material g in materialBuffer)
                            {
                                if (g.mainTexture == outline.SharedMaterials[v].mainTexture)
                                {
                                    if (outline.eraseRenderer && g.color == outlineEraseMaterial.color) m = g;
                                    else if (!outline.eraseRenderer && g.color == GetMaterialFromID(outline.color).color) m = g;
                                }
                            }

                            if (m == null)
                            {
                                m = outline.eraseRenderer ? new Material(outlineEraseMaterial) : new Material(GetMaterialFromID(outline.color));
                                m.mainTexture = outline.SharedMaterials[v].mainTexture;
                                materialBuffer.Add(m);
                            }
                        }
                        else
                        {
                            m = outline.eraseRenderer ? outlineEraseMaterial : GetMaterialFromID(outline.color);
                        }

                        m.SetInt("_Culling", backfaceCulling ? (int)CullMode.Back : (int)CullMode.Off);

                        if (outline.MeshFilter && outline.MeshFilter.sharedMesh != null)
                        {
                            if (v < outline.MeshFilter.sharedMesh.subMeshCount)
                                commandBuffer.DrawRenderer(outline.Renderer, m, v, 0);
                        }
                        else if (outline.SkinnedMeshRenderer && outline.SkinnedMeshRenderer.sharedMesh != null)
                        {
                            if (v < outline.SkinnedMeshRenderer.sharedMesh.subMeshCount)
                                commandBuffer.DrawRenderer(outline.Renderer, m, v, 0);
                        }
                        else if (outline.SpriteRenderer)
                        {
                            commandBuffer.DrawRenderer(outline.Renderer, m, v, 0);
                        }
                    }
                }
            }

            outlineCamera.Render();
        }

        private void OnEnable()
        {
            Outline[] o = FindObjectsOfType<Outline>();
            if (autoEnableOutlines)
            {
                foreach (Outline oL in o) { oL.enabled = false; oL.enabled = true; }
            }
            else
            {
                foreach (Outline oL in o) { if (!outlines.Contains(oL)) outlines.Add(oL); }
            }
        }

        void OnDestroy()
        {
            if (renderTexture != null) renderTexture.Release();
            if (extraRenderTexture != null) extraRenderTexture.Release();
            DestroyMaterials();
        }

        [ImageEffectOpaque]
        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (outlineShaderMaterial != null)
            {
                outlineShaderMaterial.SetTexture("_OutlineSource", renderTexture);
                if (addLinesBetweenColors)
                {
                    Graphics.Blit(source, extraRenderTexture, outlineShaderMaterial, 0);
                    outlineShaderMaterial.SetTexture("_OutlineSource", extraRenderTexture);
                }
                Graphics.Blit(source, destination, outlineShaderMaterial, 1);
            }
        }

        private void CreateMaterialsIfNeeded()
        {
            if (outlineShader == null) outlineShader = Resources.Load<Shader>("OutlineShader");
            if (outlineBufferShader == null) outlineBufferShader = Resources.Load<Shader>("OutlineBufferShader");
            
            if (outlineShaderMaterial == null)
            {
                outlineShaderMaterial = new Material(outlineShader);
                outlineShaderMaterial.hideFlags = HideFlags.HideAndDontSave;
                UpdateMaterialsPublicProperties();
            }
            if (outlineEraseMaterial == null) outlineEraseMaterial = CreateMaterial(new Color(0, 0, 0, 0));
            if (outline1Material == null) outline1Material = CreateMaterial(new Color(1, 0, 0, 0));
            if (outline2Material == null) outline2Material = CreateMaterial(new Color(0, 1, 0, 0));
            if (outline3Material == null) outline3Material = CreateMaterial(new Color(0, 0, 1, 0));
        }

        private void DestroyMaterials()
        {
            foreach (Material m in materialBuffer) DestroyImmediate(m);
            materialBuffer.Clear();
            DestroyImmediate(outlineShaderMaterial);
            DestroyImmediate(outlineEraseMaterial);
            DestroyImmediate(outline1Material);
            DestroyImmediate(outline2Material);
            DestroyImmediate(outline3Material);
            outlineShader = outlineBufferShader = null;
            outlineShaderMaterial = outlineEraseMaterial = outline1Material = outline2Material = outline3Material = null;
        }

        public void UpdateMaterialsPublicProperties()
        {
            if (outlineShaderMaterial)
            {
                float scalingFactor = 1;
                if (scaleWithScreenSize) scalingFactor = Screen.height / 360.0f;

                float thicknessX, thicknessY;
                if (scaleWithScreenSize && scalingFactor < 1)
                {
                    thicknessX = 1.0f / Screen.width;
                    thicknessY = 1.0f / Screen.height;
                }
                else
                {
                    thicknessX = scalingFactor * (lineThickness / 1000.0f) * (1.0f / Screen.width) * 1000.0f;
                    thicknessY = scalingFactor * (lineThickness / 1000.0f) * (1.0f / Screen.height) * 1000.0f;
                }

                outlineShaderMaterial.SetFloat("_LineThicknessX", thicknessX);
                outlineShaderMaterial.SetFloat("_LineThicknessY", thicknessY);
                outlineShaderMaterial.SetFloat("_LineIntensity", lineIntensity);
                outlineShaderMaterial.SetFloat("_FillAmount", fillAmount);
                outlineShaderMaterial.SetColor("_FillColor", fillColor);
                outlineShaderMaterial.SetFloat("_UseFillColor", useFillColor ? 1 : 0);
                outlineShaderMaterial.SetColor("_LineColor1", lineColor0 * lineColor0);
                outlineShaderMaterial.SetColor("_LineColor2", lineColor1 * lineColor1);
                outlineShaderMaterial.SetColor("_LineColor3", lineColor2 * lineColor2);
                
                // --- ADDED: Pass softness to shader ---
                outlineShaderMaterial.SetFloat("_Softness", lineSoftness);
                // --------------------------------------

                outlineShaderMaterial.SetInt("_FlipY", flipY ? 1 : 0);
                outlineShaderMaterial.SetInt("_Dark", !additiveRendering ? 1 : 0);
                outlineShaderMaterial.SetInt("_CornerOutlines", cornerOutlines ? 1 : 0);

                Shader.SetGlobalFloat("_OutlineAlphaCutoff", alphaCutoff);
            }
        }

        void UpdateOutlineCameraFromSource()
        {
            outlineCamera.CopyFrom(sourceCamera);
            outlineCamera.renderingPath = RenderingPath.Forward;
            outlineCamera.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            outlineCamera.clearFlags = CameraClearFlags.SolidColor;
            outlineCamera.rect = new Rect(0, 0, 1, 1);
            outlineCamera.cullingMask = 0;
            outlineCamera.targetTexture = renderTexture;
            outlineCamera.enabled = false;
            outlineCamera.allowHDR = false;
        }

        public void AddOutline(Outline outline) => outlines.Add(outline);
        public void RemoveOutline(Outline outline) => outlines.Remove(outline);
    }
}