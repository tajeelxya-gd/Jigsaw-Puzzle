using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Client.Runtime
{
    public interface IPuzzleTray
    {
        void ShufflePieces(IEnumerable<JigsawPiece> pieces);
        void SetHoverPiece(JigsawPiece piece);
        void SubmitPiece(JigsawPiece piece);
        UniTask DropPiecesAsync(CancellationToken cToken = default);
        void PickPieces();
    }
}