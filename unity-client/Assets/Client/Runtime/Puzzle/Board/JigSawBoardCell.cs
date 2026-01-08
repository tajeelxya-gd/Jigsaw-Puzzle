using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigsawBoardCell : MonoBehaviour
    {
        public int Idx { get; private set; }
        public Vector3 Size { get; private set; }
        public JigSawPiece Piece { get; private set; }

        public void SetData(int idx, Vector3 size)
        {
            Idx = idx;
            Size = size;
        }

        public void SetPiece(JigSawPiece piece) => Piece = piece;
    }
}