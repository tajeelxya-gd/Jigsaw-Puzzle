using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.Bootstrap;
using UniTx.Runtime.Widgets;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class LoadingWidgetStep : LoadingStepBase
    {
        [SerializeField] private bool _activateLoading;

        public override UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            return _activateLoading ? UniWidgets.PushAsync<LoadingWidget>(cToken) : UniWidgets.PopWidgetsStackAsync(cToken);
        }
    }
}