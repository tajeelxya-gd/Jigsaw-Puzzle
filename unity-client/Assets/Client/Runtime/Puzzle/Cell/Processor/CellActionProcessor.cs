using UniTx.Runtime;
using UniTx.Runtime.Content;
using UniTx.Runtime.Entity;
using UniTx.Runtime.IoC;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class CellActionProcessor : ICellActionProcessor, IInjectable
    {
        private IRewardProcessor _rewardProcessor;

        public void Inject(IResolver resolver) => _rewardProcessor = resolver.Resolve<IRewardProcessor>();

        public string GetImageKey(ICellActionData data)
        {
            if (data == null) return null;

            switch (data)
            {
                case CellActionRewardData rewardData:
                    return _rewardProcessor.GetImageKey(rewardData.RewardId);
                default:
                    return null;
            }
        }

        public void Process(ICellActionData data)
        {
            if (data == null) return;

            switch (data)
            {
                case CellActionRewardData rewardData:
                    ProcessCellActionReward(rewardData);
                    break;
                default:
                    break;
            }
        }

        private void ProcessCellActionReward(CellActionRewardData data) => _rewardProcessor.Process(data.RewardId);
    }
}