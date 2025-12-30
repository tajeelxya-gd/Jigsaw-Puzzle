using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.Content;
using UniTx.Runtime.IoC;

namespace Client.Runtime
{
    public sealed class PuzzleService : IPuzzleService, IInjectable
    {
        private IContentService _contentService;

        public void Inject(IResolver resolver) => _contentService = resolver.Resolve<IContentService>();

        public UniTask LoadPuzzleAsync(CancellationToken cToken = default)
        {
            var levelData = GetCurrentLevelData();
            var boardData = _contentService.GetData<JigSawBoardData>(levelData.BoardId);

            return UniTask.CompletedTask;
        }

        private JigSawLevelData GetCurrentLevelData()
        {
            var data = _contentService.GetAllData<JigSawLevelData>();

            // TODO: load idx from saves later
            return data.First();
        }
    }
}