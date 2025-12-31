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

        public void WrapAndSetup(Transform parent, JigSawPieceData data)
        {
            // Create a parent wrapper
            GameObject pieceRoot = new GameObject(MeshTransform.name + "_Piece");
            pieceRoot.transform.SetParent(parent);
            pieceRoot.transform.position = MeshTransform.position;
            pieceRoot.transform.rotation = MeshTransform.rotation;

            // Reparent the mesh under wrapper
            MeshTransform.SetParent(pieceRoot.transform);
            MeshTransform.localPosition = Vector3.zero;
            MeshTransform.localRotation = Quaternion.identity;

            var renderer = MeshTransform.GetComponent<Renderer>();
            BoxCollider collider = pieceRoot.AddComponent<BoxCollider>();
            pieceRoot.AddComponent<DragController>();
            var jigSawPiece = pieceRoot.AddComponent<JigSawPiece>();
            pieceRoot.AddComponent<PieceSnapController>();

            if (renderer != null)
                collider.size = renderer.bounds.size;

            jigSawPiece.SetData(data);
            JigSawPieceRegistry.Register(jigSawPiece);
        }
    }
}