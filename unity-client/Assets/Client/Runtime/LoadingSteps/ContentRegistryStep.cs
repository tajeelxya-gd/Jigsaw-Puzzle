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
            ContentRegistry.Register<JigSawLevelData>("JigSawLevelData");
            ContentRegistry.Register<JigSawBoardData>("JigSawBoardData");
            return UniTask.CompletedTask;
        }
    }
}