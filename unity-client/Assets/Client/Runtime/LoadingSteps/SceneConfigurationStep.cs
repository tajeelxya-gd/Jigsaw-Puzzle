using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.Bootstrap;
using UniTx.Runtime.IoC;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class SceneConfigurationStep : LoadingStepBase, IInjectable
    {
        [Header("Reference Settings")]
        [SerializeField] private RectTransform _boardSection;
        [SerializeField] private RectTransform _puzzleTraySection;
        [SerializeField] private float _fitPadding = 0.95f;

        private IPuzzleService _puzzleService;
        private IPuzzleTray _puzzleTray;

        public void Inject(IResolver resolver)
        {
            _puzzleService = resolver.Resolve<IPuzzleService>();
            _puzzleTray = resolver.Resolve<IPuzzleTray>();
        }

        public override UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            ConfigureScene();
            return UniTask.CompletedTask;
        }

        private void ConfigureScene()
        {
            var boardRoot = _puzzleService.PuzzleRoot;
            var frameMesh = _puzzleService.FrameMesh;

            if (boardRoot != null && frameMesh != null)
            {
                boardRoot.position = frameMesh.position;
            }

            FitBoardToSection(frameMesh, _boardSection);

            if (_puzzleTray != null)
            {
                FitTrayToSection(_puzzleTray.MeshTransform, _puzzleTray.TrayCollider, _puzzleTraySection);
            }
        }

        private void FitBoardToSection(Transform frameTransform, RectTransform section)
        {
            var cam = Camera.main;
            if (cam == null || section == null || frameTransform == null) return;

            // 1. Get the actual visual bounds of the frame mesh (encapsulating all children renderers)
            Renderer[] renderers = frameTransform.GetComponentsInChildren<Renderer>();
            Bounds bounds;
            if (renderers.Length > 0)
            {
                bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }
            else
            {
                // Fallback to lossyScale if no renderers found
                bounds = new Bounds(frameTransform.position, new Vector3(frameTransform.lossyScale.x, 0.01f, frameTransform.lossyScale.z));
            }

            Vector3 worldCenter = bounds.center;
            float worldWidth = bounds.size.x;
            float worldHeight = bounds.size.z;

            // 2. Calculate the viewport area occupied by the UI section
            Vector3[] corners = new Vector3[4];
            section.GetWorldCorners(corners);
            Canvas canvas = section.GetComponentInParent<Canvas>();
            Camera uiCam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;

            Vector2 screenMin = RectTransformUtility.WorldToScreenPoint(uiCam, corners[0]);
            Vector2 screenMax = RectTransformUtility.WorldToScreenPoint(uiCam, corners[2]);

            Vector2 viewportMin = cam.ScreenToViewportPoint(screenMin);
            Vector2 viewportMax = cam.ScreenToViewportPoint(screenMax);

            // Apply padding to the target viewport area
            Vector2 viewportSize = (viewportMax - viewportMin) * _fitPadding;
            Vector2 viewportCenter = (viewportMin + viewportMax) / 2f;

            if (viewportSize.x <= 0 || viewportSize.y <= 0) return;

            // 3. Adjust camera distance or size to fit the visual dimensions
            if (cam.orthographic)
            {
                float size_w = worldWidth / (viewportSize.x * 2f * cam.aspect);
                float size_h = worldHeight / (viewportSize.y * 2f);
                cam.orthographicSize = Mathf.Max(size_w, size_h);
            }
            else
            {
                float halfFovRad = cam.fieldOfView * 0.5f * Mathf.Deg2Rad;
                float tanHalfFov = Mathf.Tan(halfFovRad);

                float d_w = worldWidth / (viewportSize.x * 2f * tanHalfFov * cam.aspect);
                float d_h = worldHeight / (viewportSize.y * 2f * tanHalfFov);
                float targetDistance = Mathf.Max(d_w, d_h);

                // Move camera to the required distance from the bounds center plane
                Plane boardPlane = new Plane(Vector3.up, worldCenter);
                Ray cameraRay = new Ray(cam.transform.position, cam.transform.forward);
                if (boardPlane.Raycast(cameraRay, out float currentDistance))
                {
                    cam.transform.position += cam.transform.forward * (currentDistance - targetDistance);
                }
                else
                {
                    cam.transform.position = worldCenter - cam.transform.forward * targetDistance;
                }
            }

            // 4. Center the bounds center in the viewport section
            Vector3 centerViewportPos = cam.WorldToViewportPoint(worldCenter);
            Vector2 viewportOffset = (Vector2)centerViewportPos - viewportCenter;

            float frustumHeight, frustumWidth;
            if (cam.orthographic)
            {
                frustumHeight = 2f * cam.orthographicSize;
            }
            else
            {
                Plane boardPlane = new Plane(Vector3.up, worldCenter);
                Ray cameraRay = new Ray(cam.transform.position, cam.transform.forward);
                boardPlane.Raycast(cameraRay, out float currentFrustumDistance);
                frustumHeight = 2f * currentFrustumDistance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            }
            frustumWidth = frustumHeight * cam.aspect;

            cam.transform.position += cam.transform.right * (viewportOffset.x * frustumWidth);
            cam.transform.position += cam.transform.up * (viewportOffset.y * frustumHeight);
        }

        private void FitTrayToSection(Transform meshTransform, BoxCollider trayCollider, RectTransform section)
        {
            var cam = Camera.main;
            if (cam == null || section == null || meshTransform == null || trayCollider == null) return;

            // 1. Get the screen bounds of the UI section
            Vector3[] corners = new Vector3[4];
            section.GetWorldCorners(corners);
            Canvas canvas = section.GetComponentInParent<Canvas>();
            Camera uiCam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;

            Vector2 screenMin = RectTransformUtility.WorldToScreenPoint(uiCam, corners[0]);
            Vector2 screenMax = RectTransformUtility.WorldToScreenPoint(uiCam, corners[2]);

            // 2. Project screen corners to the XZ world plane (using current tray height)
            float worldY = meshTransform.position.y;
            Plane trayPlane = new Plane(Vector3.up, new Vector3(0, worldY, 0));

            // Calculate rays for all 4 corners to handle camera angles/perspective correctly
            Ray rayBL = cam.ScreenPointToRay(new Vector3(screenMin.x, screenMin.y, 0));
            Ray rayTR = cam.ScreenPointToRay(new Vector3(screenMax.x, screenMax.y, 0));
            Ray rayTL = cam.ScreenPointToRay(new Vector3(screenMin.x, screenMax.y, 0));
            Ray rayBR = cam.ScreenPointToRay(new Vector3(screenMax.x, screenMin.y, 0));

            if (trayPlane.Raycast(rayBL, out float dBL) &&
                trayPlane.Raycast(rayTR, out float dTR) &&
                trayPlane.Raycast(rayTL, out float dTL) &&
                trayPlane.Raycast(rayBR, out float dBR))
            {
                Vector3 pBL = rayBL.GetPoint(dBL);
                Vector3 pTR = rayTR.GetPoint(dTR);
                Vector3 pTL = rayTL.GetPoint(dTL);
                Vector3 pBR = rayBR.GetPoint(dBR);

                // Find the min/max extents on the XZ plane
                float minX = Mathf.Min(pBL.x, pTL.x, pTR.x, pBR.x);
                float maxX = Mathf.Max(pBL.x, pTL.x, pTR.x, pBR.x);
                float minZ = Mathf.Min(pBL.z, pTL.z, pTR.z, pBR.z);
                float maxZ = Mathf.Max(pBL.z, pTL.z, pTR.z, pBR.z);

                Vector3 worldCenter = new Vector3((minX + maxX) * 0.5f, worldY, (minZ + maxZ) * 0.5f);
                float worldWidth = maxX - minX;
                float worldDepth = maxZ - minZ;

                // 3. Centralize and Stretch the Root (Collider Transform)
                trayCollider.transform.position = worldCenter;

                // 4. Reset Mesh Local Position as requested by user
                meshTransform.localPosition = Vector3.zero;

                // 5. Detect base dimensions of the mesh for scaling
                float baseWidth = 1f;
                float baseDepth = 1f;
                var filter = meshTransform.GetComponent<MeshFilter>();
                if (filter != null && filter.sharedMesh != null)
                {
                    baseWidth = filter.sharedMesh.bounds.size.x;
                    baseDepth = filter.sharedMesh.bounds.size.z;
                }

                if (Mathf.Abs(baseWidth) < 0.001f) baseWidth = 1f;
                if (Mathf.Abs(baseDepth) < 0.001f) baseDepth = 1f;

                // 6. Apply Scale to Mesh
                Vector3 targetLocalScale = new Vector3(worldWidth / baseWidth, meshTransform.localScale.y, worldDepth / baseDepth);

                // Compensate for parent scale if nested
                if (meshTransform.parent != null)
                {
                    Vector3 parentScale = meshTransform.parent.lossyScale;
                    // Divide world dimension by parent scale to get target local scale
                    targetLocalScale.x = worldWidth / (baseWidth * parentScale.x);
                    targetLocalScale.z = worldDepth / (baseDepth * parentScale.z);
                }
                meshTransform.localScale = targetLocalScale;

                // 7. Scale and align the Box Collider
                // Size is world-space dimensions divided by lossyScale of the collider object
                Vector3 colliderLossyScale = trayCollider.transform.lossyScale;
                trayCollider.size = new Vector3(
                    worldWidth / Mathf.Max(colliderLossyScale.x, 0.001f),
                    trayCollider.size.y,
                    worldDepth / Mathf.Max(colliderLossyScale.z, 0.001f)
                );
                trayCollider.center = Vector3.zero;
            }
        }
    }
}