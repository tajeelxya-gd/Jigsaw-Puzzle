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
        public IEnumerable<JigsawPiece> AllPieces => _stack;
        public bool HasAnyPiece => _stack.Count > 0;

        public bool Contains(JigsawPiece piece) => _stack.Contains(piece);

        public void SetData(int idx, Vector3 size, JigsawBoard board)
        {
            Idx = idx;
            Size = size;
            _board = board;
        }

        public bool Push(JigsawPiece piece)
        {
            var newY = GetNextHeight();
            _stack.Push(piece);
            SetHeight(piece, newY);

            if (piece.CorrectIdx == Idx)
            {
                IsLocked = true;
                return true;
            }

            return false;
        }

        public JigsawPiece TryPop()
        {
            if (_stack.TryPop(out var result))
            {
                if (result.CorrectIdx == Idx) IsLocked = false;
                return result;
            }
            return null;
        }

        public JigsawPiece GetCorrectPiece() => _board.Pieces[Idx];

        public float GetNextHeight() => _stack.Count * 0.0001f;

        private void SetHeight(JigsawPiece piece, float height)
        {
            var pTransform = piece.transform;
            pTransform.position = new Vector3(pTransform.position.x, height, pTransform.position.z);
        }
    }
}