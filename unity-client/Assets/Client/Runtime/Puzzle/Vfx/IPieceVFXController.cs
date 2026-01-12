using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Client.Runtime
{
    public interface IPieceVFXController
    {
        void HighlightGroupAndNeighbours(JigsawGroup group);

        UniTask AnimateBoardCompletionAsync(IEnumerable<JigsawPiece> pieces, CancellationToken cToken = default);
    }
}