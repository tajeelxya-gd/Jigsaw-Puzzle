using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UniTx.Runtime.IoC;
using UnityEngine;

namespace UniTx.Runtime.Bootstrap
{
    internal sealed class AppLoader : MonoBehaviour
    {
        [SerializeField] private List<LoadingStepBase> _loadingSteps = default;

        private void Start() => UniTask.Void(StartAsync, this.GetCancellationTokenOnDestroy());

        private async UniTaskVoid StartAsync(CancellationToken cToken = default)
        {
            var len = _loadingSteps.Count;

            for (var i = 0; i < len; i++)
            {
                var loadingStep = _loadingSteps[i];

                UniStatics.LogInfo($"Executing ({i + 1}/{len}): {loadingStep.GetType().Name}.", this);
                if (loadingStep is IInjectable injectable)
                {
                    injectable.Inject(UniStatics.Resolver);
                }
                await loadingStep.InitialiseAsync(cToken);
            }

            UniStatics.LogInfo("Loading Steps executed.", this);
        }
    }
}