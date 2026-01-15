using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime;
using UniTx.Runtime.Content;
using UniTx.Runtime.Entity;
using UniTx.Runtime.Events;
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
        private IVFXController _vfxController;
        private IPuzzleTray _puzzleTray;
        private JigsawBoard _board;
        private Transform _puzzleBoard;
        private JigSawLevelData _levelData;
        private int _idx = -1;

        public void Inject(IResolver resolver)
        {
            _contentService = resolver.Resolve<IContentService>();
            _entityService = resolver.Resolve<IEntityService>();
            _winConditionChecker = resolver.Resolve<IWinConditionChecker>();
            _puzzleTray = resolver.Resolve<IPuzzleTray>();
            _vfxController = resolver.Resolve<IVFXController>();
        }

        public void Initialise()
        {
            _puzzleBoard = GameObject.FindGameObjectWithTag("PuzzleBoard").transform;
            _winConditionChecker.OnWin += HandleOnWin;
        }

        public async UniTask LoadPuzzleAsync(CancellationToken cToken = default)
        {
            if (_levelData == null) IncrementLevel();
            _board = _entityService.Get<JigsawBoard>(_levelData.BoardId);
            await _board.LoadPuzzleAsync(_levelData, _puzzleBoard, cToken);
            _winConditionChecker.SetBoard(_board);
            _puzzleTray.ShufflePieces(_board.Pieces);
            JigsawBoardCalculator.SetBoard(_board);
            UniEvents.Raise(new LevelStartEvent());
        }

        public void UnLoadPuzzle()
        {
            _board.UnLoadPuzzle();
            _board = null;
            _winConditionChecker.SetBoard(null);
            JigsawBoardCalculator.SetBoard(null);
            _puzzleTray.Reset();
        }

        public UniTask RestartPuzzleAsync(CancellationToken cToken = default)
        {
            UnLoadPuzzle();
            return LoadPuzzleAsync(cToken);
        }

        public JigsawBoard GetCurrentBoard() => _board;
        public JigSawLevelData GetCurrentLevelData() => _levelData;

        private void IncrementLevel()
        {
            var data = _contentService.GetAllData<JigSawLevelData>().ToArray();
            if (++_idx >= data.Length) _idx = 0;

            _levelData = data[_idx];
        }

        private void HandleOnWin() => UniTask.Void(HandleOnWinAsync, default);

        private async UniTaskVoid HandleOnWinAsync(CancellationToken cToken = default)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: cToken);
            await _vfxController.AnimateBoardCompletionAsync(cToken);
            await UniWidgets.PushAsync<LevelCompletedWidget>();
            IncrementLevel();
        }
    }
}