using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime;
using UniTx.Runtime.Audio;
using UniTx.Runtime.Bootstrap;
using UniTx.Runtime.IoC;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class InitDependenciesStep : LoadingStepBase, IInjectable
    {
        [SerializeField] private ScriptableObject _pieceLocked;

        private IEnumerable<IInitialisable> _initialisables;
        private IEnumerable<IInitialisableAsync> _initialisablesAsync;

        public void Inject(IResolver resolver)
        {
            var injectables = resolver.ResolveAll<IInjectable>();
            foreach (var injectable in injectables)
            {
                injectable.Inject(resolver);
            }
            _initialisables = resolver.ResolveAll<IInitialisable>();
            _initialisablesAsync = resolver.ResolveAll<IInitialisableAsync>();
            resolver.Resolve<IVFXController>().SetPiecePlacedAudioConfig((IAudioConfig)_pieceLocked);
        }

        public async override UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            foreach (var initialisable in _initialisables)
            {
                UniStatics.LogInfo($"Initialising: concrete<{initialisable.GetType().Name}>", this);
                initialisable.Initialise();
            }

            foreach (var initialisable in _initialisablesAsync)
            {
                UniStatics.LogInfo($"Initialising: concrete<{initialisable.GetType().Name}>", this);
                await initialisable.InitialiseAsync(cToken);
            }
        }
    }
}