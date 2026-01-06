using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigsawBoardCell : MonoBehaviour
    {
        private int _idx;
        
        private JigSawPiece _piece;

        public int Idx => _idx;

        public JigSawPiece Piece => _piece;

        public void SetIdx(int idx) => _idx = idx;

        public void SetPiece(JigSawPiece piece) => _piece = piece;
    }
}