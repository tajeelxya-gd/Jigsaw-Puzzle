using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace UniTx.Runtime.Bootstrap
{
    public abstract class LoadingStepBase : MonoBehaviour, IInitialisableAsync
    {
        public abstract UniTask InitialiseAsync(CancellationToken cToken = default);
    }
}
