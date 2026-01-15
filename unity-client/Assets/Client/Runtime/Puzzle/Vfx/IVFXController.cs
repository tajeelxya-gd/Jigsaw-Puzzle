using Cysharp.Threading.Tasks;
using System.Threading;

namespace Client.Runtime
{
    public interface IVFXController
    {
        void HighlightGroupAndNeighbours(JigsawGroup group);

        UniTask AnimateBoardCompletionAsync(CancellationToken cToken = default);
    }
}