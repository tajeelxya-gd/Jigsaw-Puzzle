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
            var boardRoot = _puzzleService.PuzzleRoot;
            var frameMesh = _puzzleService.FrameMesh;

            if (boardRoot != null && frameMesh != null)
            {
                boardRoot.position = frameMesh.position;
            }

            FitBoardToSection(frameMesh, _boardSection);

            return UniTask.CompletedTask;
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
    }
}