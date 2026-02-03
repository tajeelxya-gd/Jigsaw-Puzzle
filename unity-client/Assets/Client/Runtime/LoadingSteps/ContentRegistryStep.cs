using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.Bootstrap;
using UniTx.Runtime.Content;

namespace Client.Runtime
{
    public sealed class ContentRegistryStep : LoadingStepBase
    {
        public override UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            ContentRegistry.Register<JigsawLevelData>("JigsawLevelData");
            ContentRegistry.Register<JigsawGridData>("JigsawGridData");
            ContentRegistry.Register<CellActionRewardData>("CellActionRewardData");
            ContentRegistry.Register<CurrencyData>("CurrencyData");
            ContentRegistry.Register<CurrencyRewardData>("CurrencyRewardData");
            return UniTask.CompletedTask;
        }
    }
}