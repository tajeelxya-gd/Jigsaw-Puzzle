using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigSawBoardCell
    {
        private string _pieceId;
        private Transform _meshTransform;

        public string PieceId => _pieceId;
        public Transform MeshTransform => _meshTransform;

        public JigSawBoardCell(string pieceId, Transform pieceTransform)
        {
            _pieceId = pieceId;
            _meshTransform = pieceTransform;
        }
    }
}