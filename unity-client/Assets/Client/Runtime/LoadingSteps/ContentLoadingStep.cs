using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.Bootstrap;
using UniTx.Runtime.Content;
using UniTx.Runtime.IoC;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class ContentLoadingStep : LoadingStepBase, IInjectable
    {
        [SerializeField] private string[] _tags;

        private IContentLoader _contentLoader;

        public void Inject(IResolver resolver)
        {
            _contentLoader = resolver.Resolve<IContentLoader>();
        }

        public override UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            return _contentLoader.LoadContentAsync(_tags);
        }
    }
}