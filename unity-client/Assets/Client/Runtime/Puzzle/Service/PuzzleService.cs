using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.Content;
using UniTx.Runtime.Entity;
using UniTx.Runtime.IoC;

namespace Client.Runtime
{
    public sealed class PuzzleService : IPuzzleService, IInjectable
    {
        private IContentService _contentService;
        private IEntityService _entityService;
        private JigSawBoard _board;

        public void Inject(IResolver resolver)
        {
            _contentService = resolver.Resolve<IContentService>();
            _entityService = resolver.Resolve<IEntityService>();
        }

        public async UniTask LoadPuzzleAsync(CancellationToken cToken = default)
        {
            var levelData = GetCurrentLevelData();
            _board = _entityService.Get<JigSawBoard>(levelData.BoardId);
            await _board.LoadPuzzleAsync(levelData.ImageKey, null, cToken);
            _board.SetActiveFullImage(false);
        }

        private JigSawLevelData GetCurrentLevelData()
        {
            var data = _contentService.GetAllData<JigSawLevelData>();

            // TODO: load idx from saves later
            return data.First();
        }
    }
}