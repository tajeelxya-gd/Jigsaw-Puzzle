using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.Audio;

namespace Client.Runtime
{
    public interface IVFXController
    {
        void HighlightGroupAndNeighbours(JigsawGroup group);

        UniTask AnimateBoardCompletionAsync(IEnumerable<JigsawPiece> pieces, int cols, AnimationOrder order, CancellationToken cToken = default);

        void SetPiecePlacedAudioConfig(IAudioConfig config);
    }
}