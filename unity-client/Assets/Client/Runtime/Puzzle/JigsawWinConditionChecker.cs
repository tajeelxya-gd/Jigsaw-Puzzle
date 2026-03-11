using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime;
using UniTx.Runtime.Events;
using UniTx.Runtime.Extensions;
using UniTx.Runtime.IoC;

namespace Client.Runtime
{
    public sealed class JigsawWinConditionChecker : IWinConditionChecker, IInitialisable, IResettable, IInjectable
    {
        public event Action OnWin;
        public event Action OnLose;
        public event Action<float> OnAdvance;
        public event Action<float> OnTimerTick;

        public float RemainingTime { get; private set; }

        private IPuzzleService _puzzleService;
        private JigsawBoard _board;
        private CancellationTokenSource _timerCts;
        private bool _isTimeOut;
        private bool _isLoseBroadcasted;

        public void Inject(IResolver resolver)
        {
            _puzzleService = resolver.Resolve<IPuzzleService>();
        }

        public bool CheckLose() => _isTimeOut;

        public bool CheckWin()
        {
            var placed = _board.Cells.Count(itm => itm.IsLocked);
            return placed == _board.Cells.Count;
        }

        public void SetBoard(JigsawBoard board)
        {
            StopTimer();

            _board = board;
            _isTimeOut = false;
            _isLoseBroadcasted = false;
            RemainingTime = 0;

            if (_board == null) return;

            var maxDuration = _puzzleService.GetCurrentLevelData().MaxDuration;
            if (maxDuration > 0)
            {
                RemainingTime = maxDuration;
                _timerCts = new CancellationTokenSource();
                StartTimer(_timerCts.Token).Forget();
            }
        }

        private void StopTimer()
        {
            _timerCts?.Cancel();
            _timerCts?.Dispose();
            _timerCts = null;
        }

        private async UniTaskVoid StartTimer(CancellationToken token)
        {
            try
            {
                while (RemainingTime > 0)
                {
                    OnTimerTick?.Invoke(RemainingTime);
                    await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);

                    if (token.IsCancellationRequested) return;

                    RemainingTime -= 1;
                }

                if (token.IsCancellationRequested) return;

                RemainingTime = 0;
                OnTimerTick?.Invoke(RemainingTime);
                _isTimeOut = true;

                if (!_isLoseBroadcasted)
                {
                    _isLoseBroadcasted = true;
                    OnLose.Broadcast();
                }
            }
            catch (OperationCanceledException)
            {
                // Timer cancelled
            }
        }

        public void Initialise()
        {
            UniEvents.Subscribe<GroupPlacedEvent>(OnPiecePlaced);
            SignalBus.Subscribe<OnlevelFailSignal>(OnLevelFailed);
        }

        public void Reset()
        {
            StopTimer();
            UniEvents.Unsubscribe<GroupPlacedEvent>(OnPiecePlaced);
            SignalBus.Unsubscribe<OnlevelFailSignal>(OnLevelFailed);
        }

        private void OnLevelFailed(OnlevelFailSignal signal)
        {
            StopTimer();
        }

        private void OnPiecePlaced(GroupPlacedEvent @event)
        {
            var progress = (float)_board.Cells.Count(itm => itm.IsLocked) / _board.Cells.Count;
            OnAdvance.Broadcast(progress);

            HandleJoinPiecesMissionSignal(@event.Group);

            if (CheckWin())
            {
                StopTimer();
                OnWin.Broadcast();
                HandleWinMissionSignals();
                return;
            }

            if (CheckLose())
            {
                if (!_isLoseBroadcasted)
                {
                    _isLoseBroadcasted = true;
                    StopTimer();
                    OnLose.Broadcast();
                }
                return;
            }
        }
        private void HandleJoinPiecesMissionSignal(JigsawGroup group)
        {
            if (group.Count <= 1) return;

            SignalBus.Publish(new OnMissionObjectiveCompleteSignal()
            {
                MissionType = MissionType.MergePieces,
                Amount = group.Count - 1
            });
        }

        private void HandleWinMissionSignals()
        {
            SignalBus.Publish(new OnMissionObjectiveCompleteSignal()
            {
                MissionType = MissionType.WinLevel,
                Amount = 1
            });

            var difficultyType = _puzzleService.GetCurrentLevelData().DifficultyType;
            switch (difficultyType)
            {
                case LevelType.Hard:
                    SignalBus.Publish(new OnMissionObjectiveCompleteSignal()
                    {
                        MissionType = MissionType.WinHardLevel,
                        Amount = 1
                    });
                    break;
                case LevelType.SuperHard:
                    SignalBus.Publish(new OnMissionObjectiveCompleteSignal()
                    {
                        MissionType = MissionType.WinSuperHardLevel,
                        Amount = 1
                    });
                    break;
                default:
                    break;
            }
        }
    }
}