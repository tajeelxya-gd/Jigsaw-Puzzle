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
        private JigsawBoard _board;
        private int _idx;
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
        }

        public UniTask RestartPuzzleAsync(CancellationToken cToken = default)
        {
            UnLoadPuzzle();
            return LoadPuzzleAsync(cToken);
        }

        public JigsawBoard GetCurrentBoard() => _board;
        public JigsawLevelData GetCurrentLevelData() => _levelsData[_idx];

        public void LoadCurrentLevelData()
        {
            _levelsData = _contentService.GetAllData<JigsawLevelData>().ToArray();
            // TODO: load from saves
            _idx = 0;
        }

        public JigsawLevelData GetNextLevelData()
        {
            var idx = (_idx + 1) >= _levelsData.Length ? 0 : _idx + 1;
            return _levelsData[idx];
        }

        private void IncrementLevel()
        {
            if (++_idx >= _levelsData.Length) _idx = 0;
        }

        private void HandleOnWin() => UniTask.Void(HandleOnWinAsync, default);

        private async UniTaskVoid HandleOnWinAsync(CancellationToken cToken = default)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.4f), cancellationToken: cToken);
            await _vfxController.AnimateBoardCompletionAsync(cToken);
            await UniWidgets.PushAsync<LevelCompletedWidget>();
            IncrementLevel();
        }
    }
}