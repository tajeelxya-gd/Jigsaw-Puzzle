using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.Bootstrap;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Client.Runtime
{
    public sealed class SwitchSceneStep : LoadingStepBase
    {
        [SerializeField] private string _sceneName;

        public override UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            return Addressables.LoadSceneAsync(_sceneName).ToUniTask(cancellationToken: cToken);
        }
    }
}