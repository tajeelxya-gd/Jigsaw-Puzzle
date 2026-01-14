using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime;

namespace Client.Runtime
{
    public interface IPuzzleTray : IResettable
    {
        void ShufflePieces(IEnumerable<JigsawPiece> pieces);
        void SetHoverPiece(JigsawPiece piece);
        void SubmitPiece(JigsawPiece piece);
        UniTask DropPiecesAsync(CancellationToken cToken = default);
        void PickPieces();
        bool CanDropPieces();
        bool CanPickPieces();
    }
}