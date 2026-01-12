using System.Collections.Generic;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigsawGroup : HashSet<JigsawPiece>
    {
        public JigsawGroup() : base()
        {
            // Empty
        }

        public JigsawGroup(IEnumerable<JigsawPiece> pieces) : base(pieces)
        {
            // Empty
        }

        public void Join(JigsawGroup other)
        {
            if (this == other) return;

            var groupToKeep = Count >= other.Count ? this : other;
            var groupToDiscard = (groupToKeep == this) ? other : this;

            foreach (var piece in groupToDiscard)
            {
                groupToKeep.Add(piece);
                piece.Group = groupToKeep;
            }

            foreach (var piece in groupToKeep)
            {
                piece.PlayVfx();
            }
        }

        public void Move(Vector3 delta)
        {
            foreach (var piece in this)
            {
                piece.transform.position += delta;
            }
        }

        public void SetPosY(float y)
        {
            foreach (var piece in this)
            {
                var pieceTransform = piece.transform;
                pieceTransform.position = new Vector3(pieceTransform.position.x, y, pieceTransform.position.z);
            }
        }

        public void Lock()
        {
            foreach (var piece in this)
            {
                piece.LockPiece();
            }
            SetPosY(0f);
        }

        public void SetCurrentCells(int anchorIdx, JigsawPiece anchorPiece)
        {
            var board = JigsawBoardCalculator.Board;
            var boardData = board.Data;
            int cols = boardData.YConstraint;
            int rows = boardData.XConstraint;

            int anchorTargetRow = anchorIdx / cols;
            int anchorTargetCol = anchorIdx % cols;

            int anchorBaseRow = anchorPiece.CorrectIdx / cols;
            int anchorBaseCol = anchorPiece.CorrectIdx % cols;

            foreach (var piece in this)
            {
                int pieceBaseRow = piece.CorrectIdx / cols;
                int pieceBaseCol = piece.CorrectIdx % cols;

                int rowOffset = pieceBaseRow - anchorBaseRow;
                int colOffset = pieceBaseCol - anchorBaseCol;

                int targetRow = anchorTargetRow + rowOffset;
                int targetCol = anchorTargetCol + colOffset;

                // Verify the piece is within board boundaries
                if (targetRow >= 0 && targetRow < rows && targetCol >= 0 && targetCol < cols)
                {
                    int targetIdx = (targetRow * cols) + targetCol;
                    piece.CurrentIdx = targetIdx;

                    // Push the piece into the cell's stack so neighbors can find it
                    board.Cells[targetIdx].Push(piece);
                }
                else
                {
                    piece.CurrentIdx = -1;
                }
            }
        }

        public void RemoveFromCurrentCells()
        {
            foreach (var piece in this)
            {
                if (piece.CurrentIdx != -1)
                {
                    var cell = JigsawBoardCalculator.Board.Cells[piece.CurrentIdx];

                    // If the piece is in this cell, remove it regardless of stack position
                    if (cell.Contains(piece))
                    {
                        RemovePieceFromStack(cell, piece);
                    }

                    piece.CurrentIdx = -1;
                }
            }
        }

        private void RemovePieceFromStack(JigsawBoardCell cell, JigsawPiece pieceToRemove)
        {
            // Use a temporary list to filter out the piece
            // (Stacks don't allow removing from the middle, so we rebuild)
            var tempStorage = new List<JigsawPiece>();

            while (cell.HasAnyPiece)
            {
                var p = cell.TryPop();
                if (p != pieceToRemove) tempStorage.Add(p);
            }

            // Push back in reverse order to maintain original hierarchy
            for (int i = tempStorage.Count - 1; i >= 0; i--)
            {
                cell.Push(tempStorage[i]);
            }
        }
    }
}