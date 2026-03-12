using UniTx.Runtime;
using UniTx.Runtime.Events;
using UniTx.Runtime.IoC;

namespace Client.Runtime
{
    public sealed class CellActionProcessor : ICellActionProcessor, IInjectable, IInitialisable, IResettable
    {
        private IRewardProcessor _rewardProcessor;

        public bool CanProcess { get; private set; }

        public void Inject(IResolver resolver) => _rewardProcessor = resolver.Resolve<IRewardProcessor>();

        public void Initialise()
        {
            UniEvents.Subscribe<LevelStartEvent>(HandleLevelStart);
            UniEvents.Subscribe<LevelResetEvent>(HandleLevelReset);
        }

        public void Reset()
        {
            UniEvents.Unsubscribe<LevelStartEvent>(HandleLevelStart);
            UniEvents.Unsubscribe<LevelResetEvent>(HandleLevelReset);
        }

        private void HandleLevelStart(LevelStartEvent @event) => CanProcess = true;

        private void HandleLevelReset(LevelResetEvent @event) => CanProcess = false;

        public string GetImageKey(ICellActionData data)
        {
            if (data == null) return null;

            switch (data)
            {
                case CellActionRewardData rewardData:
                    return "3d Coin";
                case CellActionPuzzleManiaData puzzleManiaData:
                    return "Brush";
                default:
                    return null;
            }
        }

        public void Process(ICellActionData data)
        {
            if (!CanProcess || data == null) return;

            switch (data)
            {
                case CellActionRewardData rewardData:
                    ProcessCellActionReward(rewardData);
                    break;
                case CellActionPuzzleManiaData puzzleManiaData:
                    ProcessCellActionPuzzleMania(puzzleManiaData);
                    break;
                default:
                    break;
            }
        }

        private void ProcessCellActionReward(CellActionRewardData data) => _rewardProcessor.Process(data.RewardId);
        private void ProcessCellActionPuzzleMania(CellActionPuzzleManiaData data)
        {
            var signal = new OnEnemyDieSignal();
            signal.Count = data.RewardAmount;
            SignalBus.Publish(signal);
        }
    }
}