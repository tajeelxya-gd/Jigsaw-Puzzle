using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime;
using UnityEngine;

namespace Client.Runtime
{
    public interface IPuzzleTray : IResettable, ISceneEntity
    {
        BoxCollider TrayCollider { get; }

        Transform BgTransform { get; }

        void ShufflePieces(IEnumerable<JigsawPiece> pieces);
        void SetHoverPiece(JigsawPiece piece);
        void SubmitPiece(JigsawPiece piece);
        UniTask DropPiecesAsync(CancellationToken cToken = default);
        UniTask DropPieceAsync(JigsawPiece piece, int cellIdx, CancellationToken cToken = default);
        void PickPieces();
        bool CanDropPieces();
        bool CanPickPieces();
    }
}