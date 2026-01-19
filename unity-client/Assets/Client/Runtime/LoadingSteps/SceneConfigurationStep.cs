using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.Bootstrap;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class SceneConfigurationStep : LoadingStepBase
    {
        [SerializeField] private float _refHeight;
        [SerializeField] private float _refWidth;
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _board;

        public override UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            // set the camera position such that for ref screen height and width, it matches exactly the same. And for other screen ref and height it adjusts accordingly
            return UniTask.CompletedTask;
        }
    }
}
