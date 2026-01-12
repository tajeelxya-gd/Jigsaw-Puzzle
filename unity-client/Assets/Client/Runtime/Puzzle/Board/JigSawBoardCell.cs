using System.Collections.Generic;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigsawBoardCell : MonoBehaviour
    {
        private readonly Stack<JigsawPiece> _stack = new();
        private JigsawBoard _board;

        public int Idx { get; private set; }
        public Vector3 Size { get; private set; }
        public bool IsLocked { get; private set; }

        public void SetData(int idx, Vector3 size, JigsawBoard board)
        {
            Idx = idx;
            Size = size;
            _board = board;
        }

        /// <summary>
        /// Returns true when correct piece is pushed to the cell.
        /// </summary>
        /// <param name="piece"></param>
        /// <returns></returns>
        public bool Push(JigsawPiece piece)
        {
            if (piece.CorrectIdx == Idx)
            {
                IsLocked = true;
                return true;
            }

            _stack.Push(piece);
            return false;
        }

        public JigsawPiece Pop() => _stack.Pop();

        public JigsawPiece GetCorrectPiece() => _board.Pieces[Idx];
    }
}