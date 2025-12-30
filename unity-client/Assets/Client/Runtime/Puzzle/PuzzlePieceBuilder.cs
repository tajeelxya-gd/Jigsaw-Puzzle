using UnityEngine;

namespace Client.Runtime
{
    [ExecuteAlways]
    public class PuzzlePieceBuilder : MonoBehaviour
    {
        [Header("Piece Base Settings")]
        [SerializeField] private GameObject pieceBasePrefab;
        [SerializeField] private Transform piecesRoot; // Transform containing all meshes in correct positions
        [SerializeField] private bool generatePieces = false;

        private void Update()
        {
            if (generatePieces)
            {
                generatePieces = false;
                BuildPieces();
            }
        }

        private void BuildPieces()
        {
            if (pieceBasePrefab == null || piecesRoot == null)
            {
                Debug.LogWarning("PieceBasePrefab or PiecesRoot not set.");
                return;
            }

            foreach (Transform meshTransform in piecesRoot)
            {
                // Skip already created pieces
                if (meshTransform.GetComponent<PuzzlePiece>() != null)
                    continue;

                // Instantiate piece base prefab
                GameObject pieceObj = Instantiate(pieceBasePrefab, meshTransform.position, meshTransform.rotation, transform);
                pieceObj.name = meshTransform.name + "_Piece";

                // Add PuzzlePiece component if not present
                PuzzlePiece piece = pieceObj.GetComponent<PuzzlePiece>();
                if (piece == null)
                    piece = pieceObj.AddComponent<PuzzlePiece>();

                // Assign target transform (original mesh position)
                GameObject targetObj = new GameObject(meshTransform.name + "_Target");
                targetObj.transform.position = meshTransform.position;
                targetObj.transform.rotation = meshTransform.rotation;
                targetObj.transform.parent = pieceObj.transform;
                piece.GetType().GetField("target", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                     .SetValue(piece, targetObj.transform);

                // Create snap anchor at the **world center of the mesh bounds**
                GameObject snapAnchor = new GameObject(meshTransform.name + "_SnapAnchor");
                snapAnchor.transform.parent = pieceObj.transform;

                MeshFilter mf = meshTransform.GetComponent<MeshFilter>();
                if (mf != null)
                {
                    Bounds bounds = mf.sharedMesh.bounds;
                    Vector3 localCenter = bounds.center;

                    // Transform local mesh center to world space
                    Vector3 worldCenter = meshTransform.TransformPoint(localCenter);

                    // Transform world center into pieceObj's local space
                    snapAnchor.transform.localPosition = pieceObj.transform.InverseTransformPoint(worldCenter);
                }
                else
                {
                    snapAnchor.transform.localPosition = Vector3.zero;
                }

                piece.GetType().GetField("snapAnchor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                     .SetValue(piece, snapAnchor.transform);

                // Copy the mesh into the piece base prefab
                MeshRenderer originalMR = meshTransform.GetComponent<MeshRenderer>();
                if (mf != null)
                {
                    GameObject meshGO = new GameObject("Mesh");
                    meshGO.transform.parent = pieceObj.transform;
                    meshGO.transform.localPosition = Vector3.zero;
                    meshGO.transform.localRotation = Quaternion.identity;

                    MeshFilter mfNew = meshGO.AddComponent<MeshFilter>();
                    mfNew.sharedMesh = mf.sharedMesh;

                    if (originalMR != null)
                    {
                        MeshRenderer mrNew = meshGO.AddComponent<MeshRenderer>();
                        mrNew.sharedMaterials = originalMR.sharedMaterials;
                    }
                }

                // Optional: destroy original mesh to clean up scene
                DestroyImmediate(meshTransform.gameObject);
            }

            Debug.Log("Puzzle pieces generated successfully.");
        }
    }
}
