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

        public JigsawPiece OccupyingPiece => _stack.Count > 0 ? _stack.Peek() : null;

        public void SetData(int idx, Vector3 size, JigsawBoard board)
        {
            Idx = idx;
            Size = size;
            _board = board;
        }

        public bool Push(JigsawPiece piece)
        {
            if (piece.CorrectIdx == Idx)
            {
                _stack.Push(piece);
                IsLocked = true;
                return true;
            }

            _stack.Push(piece);
            return false;
        }

        public JigsawPiece TryPop() => _stack.TryPop(out var result) ? result : null;

        public JigsawPiece GetCorrectPiece() => _board.Pieces[Idx];
    }
}