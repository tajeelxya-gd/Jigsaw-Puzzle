using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.Bootstrap;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class DebuggerLoadingStep : LoadingStepBase
    {
        [SerializeField] private bool _activateDebugger;

        public override UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            if (_activateDebugger)
            {
                SRDebug.Init();
            }

            return UniTask.CompletedTask;
        }
    }
}
