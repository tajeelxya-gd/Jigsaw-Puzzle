using Cysharp.Threading.Tasks;
using System.Threading;
using Unity;
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

        protected virtual IUnityContainer CreateContainer() => new UnityContainer();

        protected virtual IResolver CreateResolver(IUnityContainer container) => new UniResolver(container);

        protected virtual IBinder CreateBinder(IUnityContainer container) => new UniBinder(container);

        protected virtual IEventBus CreateEventBus() => new UniEventBus();

        protected virtual IResourceLoadingStrategy CreateResourceLoadingStrategy() => new AddressablesLoadingStrategy();

        protected virtual IWidgetsManager CreateWidgetsManager() => new UniWidgetsManager();

        protected virtual IAudioService CreateAudioService() => new UniAudioService();

        private void LoadConfig() => UniStatics.Config = Resources.Load<UniTxConfig>("UniTxConfig");

        private void SetupRoot()
        {
            UniStatics.Root = new GameObject("UniTx - Root");
            DontDestroyOnLoad(UniStatics.Root);
        }

        private void SetupIoC()
        {
            var container = CreateContainer();
            var resolver = CreateResolver(container);
            var binder = CreateBinder(container);
            binder.BindAsSingleton(binder.GetType(), binder);
            binder.BindAsSingleton(resolver.GetType(), resolver);
            UniStatics.Resolver = resolver.Resolve<IResolver>(); // ensure resolve
        }
    }
}