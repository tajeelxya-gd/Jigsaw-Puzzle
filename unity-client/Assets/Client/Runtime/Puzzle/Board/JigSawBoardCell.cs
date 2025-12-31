using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigSawBoardCell
    {
        private string _pieceId;
        private Transform _pieceTransform;

        public JigSawBoardCell(string pieceId, Transform pieceTransform)
        {
            _pieceId = pieceId;
            _pieceTransform = pieceTransform;
        }

        public string PieceId => _pieceId;

        public Transform PieceTransform => _pieceTransform;
    }
}