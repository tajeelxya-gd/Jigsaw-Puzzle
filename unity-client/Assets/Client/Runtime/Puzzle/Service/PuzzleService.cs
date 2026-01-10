using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime;
using UniTx.Runtime.Content;
using UniTx.Runtime.Entity;
using UniTx.Runtime.IoC;
using UniTx.Runtime.Widgets;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class PuzzleService : IPuzzleService, IInjectable, IInitialisable
    {
        private IContentService _contentService;
        private IEntityService _entityService;
        private IWinConditionChecker _winConditionChecker;
        private IPuzzleTray _puzzleTray;
        private JigsawBoard _board;
        private Transform _puzzleBoard;
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
            _puzzleBoard = GameObject.FindGameObjectWithTag("PuzzleBoard").transform;
            _winConditionChecker.OnWin += HandleOnWin;
        }

        public async UniTask LoadPuzzleAsync(CancellationToken cToken = default)
        {
            var levelData = GetCurrentLevelData();
            _board = _entityService.Get<JigsawBoard>(levelData.BoardId);
            await _board.LoadPuzzleAsync(levelData.ImageKey, _puzzleBoard, cToken);
            _winConditionChecker.SetBoard(_board);
            _puzzleTray.ShufflePieces(_board.Pieces);
        }

        public void UnLoadPuzzle()
        {
            _board.UnLoadPuzzle();
            _board = null;
            _winConditionChecker.SetBoard(null);
        }

        public JigsawBoard GetCurrentBoard() => _board;

        private JigSawLevelData GetCurrentLevelData()
        {
            var data = _contentService.GetAllData<JigSawLevelData>().ToArray();
            if (++_idx >= data.Length) _idx = 0;

            // TODO: load idx from saves later
            return data[_idx];
        }

        private void HandleOnWin() => UniTask.Void(HandleOnWinAsync, default);

        private async UniTaskVoid HandleOnWinAsync(CancellationToken cToken = default)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(2f), cancellationToken: cToken);
            await UniWidgets.PushAsync<LevelCompletedWidget>();
        }
    }
}