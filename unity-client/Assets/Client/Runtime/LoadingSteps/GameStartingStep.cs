using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.Bootstrap;
using UniTx.Runtime.Content;
using UniTx.Runtime.IoC;

namespace Client.Runtime
{
    public sealed class GameStartingStep : LoadingStepBase, IInjectable
    {
        private IContentService _contentService;

        public void Inject(IResolver resolver)
        {
            _contentService = resolver.Resolve<IContentService>();
        }

        public override UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            var piecesData = _contentService.GetAllData<JigSawPieceData>().ToList();
            return UniTask.CompletedTask;
        }

    }
}