using System;
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
        private IWinConditionChecker _winConditionChecker;
        private IPuzzleTray _puzzleTray;
        private JigSawBoard _board;
        private Transform _puzzleRoot;
        private int _idx = -1;

        public void Inject(IResolver resolver)
        {
            _contentService = resolver.Resolve<IContentService>();
            _entityService = resolver.Resolve<IEntityService>();
            _winConditionChecker = resolver.Resolve<IWinConditionChecker>();
            _puzzleTray = resolver.Resolve<IPuzzleTray>();
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
            _puzzleTray.ShufflePieces(_board);
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
            var data = _contentService.GetAllData<JigSawLevelData>().ToArray();
            var idx = Math.Clamp(++_idx, 0, data.Length - 1);

            // TODO: load idx from saves later
            return data[idx];
        }
    }
}