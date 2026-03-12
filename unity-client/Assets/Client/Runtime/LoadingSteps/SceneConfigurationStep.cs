using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.Bootstrap;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class SceneConfigurationStep : LoadingStepBase
    {
        [SerializeField] private Vector3 _trayViewPort;
        [SerializeField] private BoxCollider _trayCollider;

        public override async UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            await UniTask.Yield(PlayerLoopTiming.Update);
            SetTrayCollider();
        }

        private void SetTrayCollider()
        {
            if (_trayCollider == null) return;

            var cam = Camera.main;
            if (cam == null) return;

            // Define a plane matching the tray's surface (using its transform.up as normal)
            Plane trayPlane = new Plane(_trayCollider.transform.up, _trayCollider.transform.position);

            // Create rays from the left and right screen edges at the target viewport Y
            float viewportY = _trayViewPort.y;
            Ray leftRay = cam.ViewportPointToRay(new Vector3(0f, viewportY, 0f));
            Ray rightRay = cam.ViewportPointToRay(new Vector3(1f, viewportY, 0f));

            // Intersect rays with the tray plane to find the exact world-space boundaries
            if (trayPlane.Raycast(leftRay, out float distanceL) && trayPlane.Raycast(rightRay, out float distanceR))
            {
                Vector3 leftWorld = leftRay.GetPoint(distanceL);
                Vector3 rightWorld = rightRay.GetPoint(distanceR);

                // Transform these world boundaries into the collider's local space
                Vector3 leftLocal = _trayCollider.transform.InverseTransformPoint(leftWorld);
                Vector3 rightLocal = _trayCollider.transform.InverseTransformPoint(rightWorld);

                // Update collider size (X dimension only)
                Vector3 size = _trayCollider.size;
                size.x = Mathf.Abs(rightLocal.x - leftLocal.x);
                _trayCollider.size = size;

                // Update collider center (X dimension only) to align with calculated center
                Vector3 centerLocal = (leftLocal + rightLocal) * 0.5f;
                Vector3 colliderCenter = _trayCollider.center;
                colliderCenter.x = centerLocal.x;
                _trayCollider.center = colliderCenter;
            }
        }
    }
}