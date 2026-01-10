using System.Collections.Generic;
using System.Linq;
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
            if (piece.CorrectCell.Idx == Idx)
            {
                if (!IsLocked) _stack.Push(piece);
                IsLocked = true;
                return true;
            }

            _stack.Push(piece);
            return false;
        }

        public JigsawPiece Pop() => _stack.Pop();

        public JigsawPiece GetPieceOrDefaultWithIdx(int idx) => _stack.FirstOrDefault(itm => itm.CorrectCell.Idx == idx);

        public IEnumerable<JigsawBoardCell> GetNeighbours()
        {
            var boardData = _board.Data;
            var data = JigsawBoardCalculator.GetNeighbours(Idx, boardData.YConstraint, boardData.XConstraint);
            var neighbours = new List<JigsawBoardCell>();
            if (data.Top != -1) neighbours.Add(_board.Cells[data.Top]);
            if (data.Bottom != -1) neighbours.Add(_board.Cells[data.Bottom]);
            if (data.Left != -1) neighbours.Add(_board.Cells[data.Left]);
            if (data.Right != -1) neighbours.Add(_board.Cells[data.Right]);
            return neighbours;
        }

        public JigsawPiece GetCorrectPiece() => _board.Pieces[Idx];
    }
}