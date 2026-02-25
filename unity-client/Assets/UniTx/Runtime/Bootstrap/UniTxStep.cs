using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UniTx.Runtime.Audio;
using UniTx.Runtime.Events;
using UniTx.Runtime.ResourceManagement;
using UniTx.Runtime.IoC;
using UniTx.Runtime.Widgets;

namespace UniTx.Runtime.Bootstrap
{
    public class UniTxStep : LoadingStepBase
    {
        [SerializeField] private UniWidgetsManager _widgetsManager;

        public async sealed override UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            SetupRoot();
            LoadConfig();
            SetupIoC();
            await UniEvents.InitialiseAsync(CreateEventBus(), cToken);
            await UniResources.InitialiseAsync(CreateResourceLoadingStrategy(), cToken);
            await UniWidgets.InitialiseAsync(CreateWidgetsManager(), cToken);
            await UniAudio.InitialiseAsync(CreateAudioService(), cToken);
        }

        protected virtual IEventBus CreateEventBus() => new UniEventBus();

        protected virtual IResourceLoadingStrategy CreateResourceLoadingStrategy() => new AddressablesLoadingStrategy();

        protected virtual IWidgetsManager CreateWidgetsManager()
        {
            _widgetsManager.Inject(UniStatics.Resolver);
            return _widgetsManager;
        }

        protected virtual UniTx.Runtime.Audio.IAudioService CreateAudioService() => new UniAudioService();

        private void LoadConfig() => UniStatics.Config = Resources.Load<UniTxConfig>("UniTxConfig");

        private void SetupRoot()
        {
            UniStatics.Root = new GameObject("UniTx - Root");
        }

        private void SetupIoC()
        {
            var container = new UniContainer();
            container.Bind(container).AsSingleton();
            UniStatics.Resolver = container.Resolve<IResolver>(); // ensure resolve
        }
    }
}