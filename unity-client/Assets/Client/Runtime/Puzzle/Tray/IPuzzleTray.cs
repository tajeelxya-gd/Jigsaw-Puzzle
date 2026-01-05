using System.Collections.Generic;
using UnityEngine;

namespace Client.Runtime
{
    public interface IPuzzleTray
    {
        void ShufflePieces(IReadOnlyList<JigSawPiece> pieces);
        bool IsOverTray(Vector3 worldPosition);
        void SetHoverPiece(JigSawPiece piece);
        void SubmitPiece(JigSawPiece piece);
    }
}