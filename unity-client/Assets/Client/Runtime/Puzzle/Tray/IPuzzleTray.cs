using System.Collections.Generic;
using UnityEngine;

namespace Client.Runtime
{
    public interface IPuzzleTray
    {
        void ShufflePieces(JigSawBoard board);
        bool IsOverTray(Vector3 worldPosition);
        void SetHoverPiece(JigSawPiece piece);
        void SubmitPiece(JigSawPiece piece);
    }
}