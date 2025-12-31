using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigSawBoardCell
    {
        private string _pieceId;
        private Transform _pieceTransform;

        public string PieceId => _pieceId;
        public Transform PieceTransform => _pieceTransform;

        public JigSawBoardCell(string pieceId, Transform pieceTransform)
        {
            _pieceId = pieceId;
            _pieceTransform = pieceTransform;
        }

        public void WrapAndSetup(Transform parent, JigSawPieceData data)
        {
            // Create a parent wrapper
            GameObject pieceRoot = new GameObject(PieceTransform.name + "_Piece");
            pieceRoot.transform.SetParent(parent);
            pieceRoot.transform.position = PieceTransform.position;
            pieceRoot.transform.rotation = PieceTransform.rotation;

            // Reparent the mesh under wrapper
            PieceTransform.SetParent(pieceRoot.transform);
            PieceTransform.localPosition = Vector3.zero;
            PieceTransform.localRotation = Quaternion.identity;

            // Add required components to wrapper
            var jigSawPiece = pieceRoot.AddComponent<JigSawPiece>();

            // Collider that matches mesh bounds
            var renderer = PieceTransform.GetComponent<Renderer>();
            BoxCollider collider = pieceRoot.AddComponent<BoxCollider>();
            if (renderer != null)
                collider.size = renderer.bounds.size;

            var dragController = pieceRoot.AddComponent<DragController3D>();
            jigSawPiece.SetData(data);
        }
    }
}