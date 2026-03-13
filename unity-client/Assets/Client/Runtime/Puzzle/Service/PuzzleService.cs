using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UniTx.Runtime;
using UniTx.Runtime.Content;
using UniTx.Runtime.Entity;
using UniTx.Runtime.Events;
using UniTx.Runtime.IoC;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class PuzzleService : MonoBehaviour, IPuzzleService, IInjectable, IInitialisable, IResettable
    {
        [SerializeField] private Transform _puzzleBoard;
        [SerializeField] private Transform _puzzleBounds;
        [SerializeField] private Transform _frameMesh;
        [SerializeField] private TMP_Text _timerText;

        private IContentService _contentService;
        private IEntityService _entityService;
        private IWinConditionChecker _winConditionChecker;
        private IVFXController _vfxController;
        private IPuzzleTray _puzzleTray;
        private IJigsawResourceLoader _helper;
        private IFullImageHandler _fullImageHandler;
        private IUserSavedData _savedData;
        private JigsawBoard _board;
        private JigsawLevelData[] _levelsData;
        private LevelType _clearLevelType;
        private string _levelId;

        public event Action<float> OnTimerTick;

        public float RemainingTime => _winConditionChecker.RemainingTime;

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
            _fullImageHandler = resolver.Resolve<IFullImageHandler>();
            _savedData = resolver.Resolve<IUserSavedData>();
        }

        public void Initialise()
        {
            _winConditionChecker.OnWin += HandleOnWin;
            _winConditionChecker.OnLose += HandleOnLose;
            _winConditionChecker.OnTimerTick += HandleOnTimerTick;
        }

        public void Reset()
        {
            _winConditionChecker.OnWin -= HandleOnWin;
            _winConditionChecker.OnLose -= HandleOnLose;
            _winConditionChecker.OnTimerTick -= HandleOnTimerTick;
        }
        public async UniTask LoadPuzzleAsync(bool reloadState, CancellationToken cToken = default)
        {
            var levelData = GetCurrentLevelData();
            _clearLevelType = levelData.DifficultyType;
            _board = _entityService.Get<JigsawBoard>(levelData.GridId);
            _levelId = levelData.Id;
            await _helper.CreateGridAsync(levelData.GridId, _puzzleBoard, cToken);
            await _helper.CreateFullImageAsync($"{levelData.GridId}_full_image", _puzzleBoard, cToken);
            await _helper.LoadOutlineGridAsync($"{levelData.GridId}_mat_outline", cToken);
            await _helper.LoadImageAsync(levelData.ImageKey);
            await _board.LoadPuzzleAsync(_puzzleBoard, _puzzleBounds, levelData.CellActionIds, cToken);
            JigsawBoardCalculator.SetBoard(_board);
            _winConditionChecker.SetBoard(_board, reloadState);

            _puzzleTray.ShufflePieces(_board.Pieces);
            if (reloadState)
            {
                await SetLevelStateAsync(cToken);
            }
            UniEvents.Raise(new LevelStartEvent(GetCurrentIdx()));
            InputHandler.SetActive(true);
        }

        public void UnLoadPuzzle()
        {
            _board.UnLoadPuzzle();
            _helper.UnLoadImage();
            _helper.DestroyGrid();
            _helper.DestroyFullImage();
            _helper.UnLoadOutlineGrid();
            _board = null;
            _winConditionChecker.SetBoard(null, false);
            JigsawBoardCalculator.SetBoard(null);
            _puzzleTray.Reset();
            UniEvents.Raise(new LevelResetEvent(_levelId));
        }

        public UniTask RestartPuzzleAsync(bool reloadState, CancellationToken cToken = default)
        {
            UnLoadPuzzle();
            return LoadPuzzleAsync(reloadState, cToken);
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

        public int GetBrushesCount()
        {
            var levelData = _contentService.GetData<JigsawLevelData>(_levelId);
            var gridData = _contentService.GetData<JigsawGridData>(levelData.GridId);
            return gridData.BrushesCount;
        }

        private void HandleOnWin() => UniTask.Void(HandleOnWinAsync, default);

        private async UniTaskVoid HandleOnWinAsync(CancellationToken cToken = default)
        {
            InputHandler.SetActive(false);
            await _vfxController.AnimateBoardCompletionAsync(cToken);
            _fullImageHandler.PlayLevelCompletedAnimationAsync(cToken).Forget();
            SignalBus.Publish(new OnLevelCompleteSignal() { levelType = _clearLevelType });
        }

        private void HandleOnLose() => UniTask.Void(HandleOnLoseAsync, default);

        private async UniTaskVoid HandleOnLoseAsync(CancellationToken cToken = default)
        {
            InputHandler.SetActive(false);
            SignalBus.Publish(new OnlevelFailSignal() { levelFailType = LevelFailType.none });
        }
        private void HandleOnTimerTick(float time)
        {
            _timerText.SetText($"{(int)time}");
            OnTimerTick?.Invoke(time);
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