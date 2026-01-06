using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.Bootstrap;
using UniTx.Runtime.ResourceManagement;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class LoadingWidgetStep : LoadingStepBase
    {
        [SerializeField] private GameObject _loadingWidget;
        [SerializeField] private bool _activateLoading;

        public override UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            if (_activateLoading)
            {
                _loadingWidget.SetActive(true);
                return UniTask.CompletedTask;
            }

            if (!UniResources.DisposeInstance(_loadingWidget))
            {
                Destroy(_loadingWidget);
            }

            return UniTask.CompletedTask;
        }
    }
}