using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime;
using UniTx.Runtime.Content;
using UniTx.Runtime.Entity;
using UniTx.Runtime.IoC;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class PuzzleService : IPuzzleService, IInjectable, IInitialisable
    {
        private IContentService _contentService;
        private IEntityService _entityService;
        private JigSawBoard _board;
        private Transform _puzzleRoot;

        public void Inject(IResolver resolver)
        {
            _contentService = resolver.Resolve<IContentService>();
            _entityService = resolver.Resolve<IEntityService>();
        }

        public void Initialise()
        {
            _puzzleRoot = GameObject.FindGameObjectWithTag("PuzzleRoot").transform;
        }

        public async UniTask LoadPuzzleAsync(CancellationToken cToken = default)
        {
            var levelData = GetCurrentLevelData();
            _board = _entityService.Get<JigSawBoard>(levelData.BoardId);
            await _board.LoadPuzzleAsync(levelData.ImageKey, _puzzleRoot, cToken);
            _board.SetActiveFullImage(false);
            foreach (var cell in _board.Cells)
            {
                var data = _contentService.GetData<JigSawPieceData>(cell.PieceId);
                cell.WrapAndSetup(_puzzleRoot, data);
            }

            // ShufflePieces();
        }

        public void UnLoadPuzzle()
        {
            _board.UnLoadPuzzle();
            _board = null;
        }

        private JigSawLevelData GetCurrentLevelData()
        {
            var data = _contentService.GetAllData<JigSawLevelData>();

            // TODO: load idx from saves later
            return data.First();
        }

        private void ShufflePieces()
        {
            foreach (var cell in _board.Cells)
            {
                var randomPos = _puzzleRoot.position + new Vector3(
                    Random.Range(-0.05f, 0.05f),
                    0,
                    Random.Range(-0.15f, -0.03f)
                );

                var parentTransform = cell.MeshTransform.parent;
                parentTransform.position = randomPos;
            }
        }
    }
}