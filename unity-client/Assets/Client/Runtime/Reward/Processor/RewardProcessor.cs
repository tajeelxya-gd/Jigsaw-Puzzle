using UniTx.Runtime.Content;
using UniTx.Runtime.Entity;
using UniTx.Runtime.Extensions;
using UniTx.Runtime.IoC;
using UnityEngine;

namespace Client.Runtime
{
    public class RewardProcessor : IRewardProcessor, IInjectable
    {
        private IContentService _contentService;
        private IEntityService _entityService;

        public event System.Action<string> RewardProcessed;

        public void Inject(IResolver resolver)
        {
            _contentService = resolver.Resolve<IContentService>();
            _entityService = resolver.Resolve<IEntityService>();
        }

        public string GetImageKey(string rewardId)
        {
            if (string.IsNullOrEmpty(rewardId)) return null;

            var data = _contentService.GetData<IRewardData>(rewardId);

            switch (data)
            {
                case CurrencyRewardData currencyRewardData:
                    var currency = _entityService.Get<ICurrency>(currencyRewardData.CurrencyId);
                    return currency.ImageKey;
                default:
                    return null;
            }
        }

        public void Process(string rewardId)
        {
            if (string.IsNullOrEmpty(rewardId)) return;

            var data = _contentService.GetData<IRewardData>(rewardId);

            switch (data)
            {
                case CurrencyRewardData currencyRewardData:
                    ProcessCurrencyReward(currencyRewardData);
                    break;
                default:
                    break;
            }

            RewardProcessed.Broadcast(rewardId);
        }

        private void ProcessCurrencyReward(CurrencyRewardData data)
        {
            var currency = _entityService.Get<ICurrency>(data.CurrencyId);
            var amount = Random.Range(data.AmountMin, data.AmountMax + 1);
            currency.Add(amount);
        }
    }
}