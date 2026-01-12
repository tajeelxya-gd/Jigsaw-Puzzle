using UniTx.Runtime;
using UnityEngine;

namespace Client.Runtime
{
    public static class CellActionProcessor
    {
        public static void Process(ICellActionData data)
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

        private static void ProcessCellActionReward(CellActionRewardData data)
        {
            UniStatics.LogInfo($"CellActionReward processed with id: {data.Id} at index: {data.CellIdx} with reward: {data.RewardId}.", "CellActionProcessor", Color.green);
        }
    }
}