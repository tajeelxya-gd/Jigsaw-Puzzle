using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.Bootstrap;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class DebuggerLoadingStep : LoadingStepBase
    {
        [SerializeField] private bool _activeDebugger;
        [SerializeField] private GameObject _debugger;

        public override UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            if (_debugger == null) return UniTask.CompletedTask;
            _debugger.SetActive(_activeDebugger);
            return UniTask.CompletedTask;
        }
    }
}
