using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime;
using UniTx.Runtime.Content;
using UniTx.Runtime.Entity;
using UniTx.Runtime.Events;
using UniTx.Runtime.IoC;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class PuzzleService : IPuzzleService, IInjectable, IInitialisable
    {
        private IContentService _contentService;
        private IEntityService _entityService;
        private IWinConditionChecker _winConditionChecker;
        private JigSawBoard _board;
        private Transform _puzzleRoot;

        public void Inject(IResolver resolver)
        {
            _contentService = resolver.Resolve<IContentService>();
            _entityService = resolver.Resolve<IEntityService>();
            _winConditionChecker = resolver.Resolve<IWinConditionChecker>();
        }

        public void Initialise()
        {
            _puzzleRoot = GameObject.FindGameObjectWithTag("PuzzleRoot").transform;
            _winConditionChecker.OnWin += () => _board.SetActiveFullImage(true);
        }

        public async UniTask LoadPuzzleAsync(CancellationToken cToken = default)
        {
            var levelData = GetCurrentLevelData();
            _board = _entityService.Get<JigSawBoard>(levelData.BoardId);
            await _board.LoadPuzzleAsync(levelData.ImageKey, _puzzleRoot, cToken);
            _board.SetActiveFullImage(false);
            _winConditionChecker.SetBoard(_board);
            // ShufflePieces();
        }

        public void UnLoadPuzzle()
        {
            _board.UnLoadPuzzle();
            _board = null;
            _winConditionChecker.SetBoard(null);
        }

        public JigSawBoard GetCurrentBoard() => _board;

        private JigSawLevelData GetCurrentLevelData()
        {
            var data = _contentService.GetAllData<JigSawLevelData>();

            // TODO: load idx from saves later
            return data.First();
        }

        private void ShufflePieces()
        {
            foreach (var piece in _board.Pieces)
            {
                var randomPos = _puzzleRoot.position + new Vector3(
                    Random.Range(-0.05f, 0.05f),
                    0,
                    Random.Range(-0.2f, -0.16f)
                );

                var pieceTransform = piece.transform;
                pieceTransform.position = randomPos;
            }
        }
    }
}