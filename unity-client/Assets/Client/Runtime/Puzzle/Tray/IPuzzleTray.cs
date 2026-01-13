using System.Collections.Generic;

namespace Client.Runtime
{
    public interface IPuzzleTray
    {
        void ShufflePieces(IEnumerable<JigsawPiece> pieces);
        void SetHoverPiece(JigsawPiece piece);
        void SubmitPiece(JigsawPiece piece);
        void DropPieces();
        void PickPieces();
    }
}