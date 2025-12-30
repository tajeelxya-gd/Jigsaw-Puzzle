using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.Bootstrap;
using UniTx.Runtime.Content;
using UniTx.Runtime.Entity;
using UniTx.Runtime.IoC;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class ContentLoadingStep : LoadingStepBase, IInjectable
    {
        [SerializeField] private string[] _tags;

        private IContentLoader _contentLoader;
        private IEntityLoader _entityLoader;

        public void Inject(IResolver resolver)
        {
            _contentLoader = resolver.Resolve<IContentLoader>();
            _entityLoader = resolver.Resolve<IEntityLoader>();
        }

        public async override UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            await _contentLoader.LoadContentAsync(_tags, cToken);
            _entityLoader.LoadEntities();
        }
    }
}