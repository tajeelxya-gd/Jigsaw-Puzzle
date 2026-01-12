using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Client.Runtime
{
    public interface IVFXController
    {
        void HighlightGroupAndNeighbours(JigsawGroup group);

        UniTask AnimateBoardCompletionAsync(IEnumerable<JigsawPiece> pieces, int cols, AnimationOrder order, CancellationToken cToken = default);
    }
}