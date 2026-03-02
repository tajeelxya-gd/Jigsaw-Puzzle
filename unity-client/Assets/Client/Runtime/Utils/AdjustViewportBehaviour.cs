using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Client.Runtime
{
    public class AdjustViewportBehaviour : MonoBehaviour
    {
        [SerializeField] private Vector3 _viewPort;

        private void Update()
        {
            UpdateViewPortAsync().Forget();
        }

        [Button]
        private void UpdateViewPort()
        {
            UpdateViewPortAsync().Forget();
        }

        private async UniTask UpdateViewPortAsync(CancellationToken cToken = default)
        {

            await UniTask.Yield(PlayerLoopTiming.Update);
            transform.position = Camera.main.ViewportToWorldPoint(_viewPort);
        }
    }
}