using System.Collections.Generic;
using UnityEngine;

namespace Client.Runtime
{
    public interface IPuzzleTray
    {
        void ShufflePieces(JigsawBoard board);
        bool IsOverTray(Vector3 worldPosition);
        void SetHoverPiece(JigsawPiece piece);
        void SubmitPiece(JigsawPiece piece);
    }
}