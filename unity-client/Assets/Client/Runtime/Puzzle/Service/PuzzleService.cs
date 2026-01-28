using System;
using System.Collections.Generic;
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
    public sealed class PuzzleService : MonoBehaviour, IPuzzleService, IInjectable, IInitialisable, IResettable
    {
        [SerializeField] private Transform _puzzleBoard;
        [SerializeField] private Transform _puzzleBounds;
        [SerializeField] private Transform _frameMesh;

        private IContentService _contentService;
        private IEntityService _entityService;
        private IWinConditionChecker _winConditionChecker;
        private IVFXController _vfxController;
        private IPuzzleTray _puzzleTray;
        private IJigsawResourceLoader _helper;
        private IUserSavedData _savedData;
        private JigsawBoard _board;
        private JigsawLevelData[] _levelsData;

        public Transform PuzzleRoot => _puzzleBoard;

        public Transform FrameMesh => _frameMesh;

        public void Inject(IResolver resolver)
        {
            _contentService = resolver.Resolve<IContentService>();
            _entityService = resolver.Resolve<IEntityService>();
            _winConditionChecker = resolver.Resolve<IWinConditionChecker>();
            _puzzleTray = resolver.Resolve<IPuzzleTray>();
            _vfxController = resolver.Resolve<IVFXController>();
            _helper = resolver.Resolve<IJigsawResourceLoader>();
            _savedData = resolver.Resolve<IUserSavedData>();
        }

        public void Initialise() => _winConditionChecker.OnWin += HandleOnWin;

        public void Reset() => _winConditionChecker.OnWin -= HandleOnWin;

        public async UniTask LoadPuzzleAsync(CancellationToken cToken = default)
        {
            var levelData = GetCurrentLevelData();
            _board = _entityService.Get<JigsawBoard>(levelData.GridId);
            await _helper.CreateGridAsync(levelData.GridId, _puzzleBoard, cToken);
            await _helper.LoadImageAsync(levelData.ImageKey);
            await _board.LoadPuzzleAsync(_puzzleBoard, _puzzleBounds, levelData.CellActionIds, cToken);
            _winConditionChecker.SetBoard(_board);
            _puzzleTray.ShufflePieces(_board.Pieces);
            JigsawBoardCalculator.SetBoard(_board);
            await SetLevelStateAsync(cToken);
            UniEvents.Raise(new LevelStartEvent());
        }

        public void UnLoadPuzzle()
        {
            _board.UnLoadPuzzle();
            _helper.UnLoadImage();
            _helper.DestroyGrid();
            _board = null;
            _winConditionChecker.SetBoard(null);
            JigsawBoardCalculator.SetBoard(null);
            _puzzleTray.Reset();
            UniEvents.Raise(new LevelResetEvent());
        }

        public UniTask RestartPuzzleAsync(CancellationToken cToken = default)
        {
            UnLoadPuzzle();
            return LoadPuzzleAsync(cToken);
        }

        public JigsawBoard GetCurrentBoard() => _board;

        public JigsawLevelData GetCurrentLevelData()
        {
            _levelsData ??= _contentService.GetAllData<JigsawLevelData>().ToArray();
            return _levelsData[GetCurrentIdx()];
        }

        public JigsawLevelData GetNextLevelData()
        {
            var currentIdx = GetCurrentIdx();
            var nextIdx = (currentIdx + 1) >= _levelsData.Length ? _levelsData.Length - 1 : currentIdx;
            return _levelsData[nextIdx];
        }

        private void HandleOnWin() => UniTask.Void(HandleOnWinAsync, default);

        private async UniTaskVoid HandleOnWinAsync(CancellationToken cToken = default)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: cToken);
            await _vfxController.AnimateBoardCompletionAsync(cToken);
            await UniWidgets.PushAsync<LevelCompletedWidget>();
        }

        private int GetCurrentIdx() => Math.Clamp(_savedData.CurrentLevel, 0, _levelsData.Length - 1);

        private async UniTask SetLevelStateAsync(CancellationToken cToken = default)
        {
            var state = _savedData.CurrentLevelState;

            if (!IsStateValid(state)) return;

            var tasks = new List<UniTask>();
            foreach (var pieceState in state.Pieces)
            {
                if (pieceState.CorrectIdx == -1 || pieceState.CurrentIdx == -1) continue;

                var piece = _board.Pieces[pieceState.CorrectIdx];
                tasks.Add(_puzzleTray.DropPieceAsync(piece, pieceState.CurrentIdx, cToken));
            }

            await UniTask.WhenAll(tasks);
        }

        private bool IsStateValid(JigsawLevelState state)
            => state != null && state.Pieces != null && state.LevelId.Equals(GetCurrentLevelData().Id);
    }
}