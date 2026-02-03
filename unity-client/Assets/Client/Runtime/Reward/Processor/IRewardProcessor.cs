using System;

namespace Client.Runtime
{
    public interface IRewardProcessor
    {
        event Action<string> RewardProcessed; // rewardId

        void Process(string rewardId);

        string GetImageKey(string rewardId);
    }
}