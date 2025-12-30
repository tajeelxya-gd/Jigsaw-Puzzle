using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.Bootstrap;
using UniTx.Runtime.Content;
using UniTx.Runtime.IoC;

namespace Client.Runtime
{
    public sealed class BindDependenciesStep : LoadingStepBase, IInjectable
    {
        private IBinder _binder;

        public void Inject(IResolver resolver)
        {
            _binder = resolver.Resolve<IBinder>();
        }

        public override UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            _binder.BindAsSingleton<ContentService>();
            return UniTask.CompletedTask;
        }
    }
}