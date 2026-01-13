using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.Bootstrap;
using UniTx.Runtime.IoC;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class SceneInjectStep : LoadingStepBase, IInjectable
    {
        [SerializeField] private GameObject[] _objects;

        public void Inject(IResolver resolver)
        {
            for (var i = 0; i < _objects.Length; i++)
            {
                var obj = _objects[i];
                if (obj.TryGetComponent<IInjectable>(out var injectable))
                {
                    injectable.Inject(resolver);
                }
            }
        }

        public override UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            return UniTask.CompletedTask;
        }
    }
}