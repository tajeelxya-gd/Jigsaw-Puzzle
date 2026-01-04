using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.Database;
using UniTx.Runtime.Entity;
using UniTx.Runtime.IoC;
using UniTx.Runtime.ResourceManagement;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigSawBoard : EntityBase<JigSawBoardData, JigSawBoardSavedData>
    {
        private readonly List<JigSawPiece> _pieces = new();
        private readonly List<JigsawBoardCell> _cells = new();

        private AssetData _assetData;
        private Texture2D _texture;

        public Transform FullImg { get; private set; }

        public IReadOnlyList<JigsawBoardCell> Cells => _cells;

        public IReadOnlyList<JigSawPiece> Pieces => _pieces;

        public JigSawBoard(string id) : base(id)
        {
            // Empty yet
        }

        public async UniTask LoadPuzzleAsync(string imageKey, Transform parent, CancellationToken cToken = default)
        {
            if (_assetData == null)
            {
                _assetData = await UniResources.LoadAssetAsync<AssetData>(Data.AssetDataId, cToken: cToken);
            }

            _texture = await UniResources.LoadAssetAsync<Texture2D>(imageKey, cToken: cToken);
            var grid = await SpawnGridAsync(parent, cToken);
            var len = grid.childCount;

            for (int i = 0; i < len; i++)
            {
                var mesh = grid.GetChild(0);
                SetLoadedTexture(mesh);
                var piece = await SpawnPuzzlePieceAsync(i, mesh, parent, cToken);
                await SpawnCellAsync(i, piece, cToken);
            }

            for (int i = 0; i < _pieces.Count; i++)
            {
                var piece = _pieces[i];
                if (piece.Data.PieceType == PieceType.Join)
                {
                    SetJoin(i, piece);
                }
            }

            var fullImgAsset = _assetData.GetAsset(Data.FullImageId);
            FullImg = await UniResources.CreateInstanceAsync<Transform>(fullImgAsset.RuntimeKey, parent, null, cToken);
            SetLoadedTexture(FullImg);
            UniResources.DisposeInstance(grid.gameObject);
        }

        private void SetJoin(int idx, JigSawPiece piece)
        {
            var join = piece.GetComponentInChildren<JoinController>();
            var neighbours = GetNeighbours(idx).ToArray();
            join.Init(piece.BoxCollider, neighbours);
        }

        public void UnLoadPuzzle()
        {
            foreach (var piece in _pieces)
            {
                UniResources.DisposeInstance(piece.gameObject);
            }
            _pieces.Clear();

            foreach (var cell in _cells)
            {
                UniResources.DisposeInstance(cell.gameObject);
            }
            _cells.Clear();

            UniResources.DisposeAsset(_texture);
            UniResources.DisposeInstance(FullImg.gameObject);
        }

        public void SetActiveFullImage(bool active) => FullImg.gameObject.SetActive(active);

        public IEnumerable<JigsawBoardCell> GetNeighbours(int idx)
        {
            var cols = Data.YConstraint;
            var rows = Data.XConstraint;

            // Fixed size: [0]=Top, [1]=Bottom, [2]=Left, [3]=Right
            var neighbours = new JigsawBoardCell[4];

            if (idx < 0 || idx >= Cells.Count)
                return neighbours; // All null

            int row = idx / cols;
            int col = idx % cols;

            // Top
            neighbours[0] = (row > 0) ? Cells[idx - cols] : null;

            // Bottom
            neighbours[1] = (row < rows - 1) ? Cells[idx + cols] : null;

            // Left
            neighbours[2] = (col > 0) ? Cells[idx - 1] : null;

            // Right
            neighbours[3] = (col < cols - 1) ? Cells[idx + 1] : null;

            return neighbours;
        }

        protected override void OnInject(IResolver resolver)
        {
            // Empty yet
        }

        protected override void OnInit()
        {
            // Empty yet
        }

        protected override void OnReset()
        {
            // Empty yet
        }

        private UniTask<Transform> SpawnGridAsync(Transform parent, CancellationToken cToken = default)
        {
            var gridAsset = _assetData.GetAsset(Data.GridId);
            return UniResources.CreateInstanceAsync<Transform>(gridAsset.RuntimeKey, parent, null, cToken);
        }

        private async UniTask<JigSawPiece> SpawnPuzzlePieceAsync(int idx, Transform mesh, Transform parent, CancellationToken cToken = default)
        {
            var piece = await UniResources.CreateInstanceAsync<JigSawPiece>("PuzzlePiece - Root", parent, null, cToken);
            var pieceTransform = piece.transform;
            pieceTransform.SetPositionAndRotation(mesh.position, mesh.rotation);
            mesh.SetParent(pieceTransform);
            var pieceType = GetPieceType(idx, Data.YConstraint);
            piece.Init(new JigSawPieceData(idx, mesh.GetComponent<Renderer>(), _cells, pieceType));
            _pieces.Add(piece);
            if (pieceType == PieceType.Join)
            {
                await UniResources.CreateInstanceAsync<JoinController>("Join - Controller", parent, null, cToken);
            }
            return piece;
        }

        private async UniTask SpawnCellAsync(int idx, JigSawPiece piece, CancellationToken cToken = default)
        {
            var pieceTransform = piece.transform;
            var cell = await UniResources.CreateInstanceAsync<JigsawBoardCell>("JigsawBoardCell", pieceTransform.parent, null, cToken);
            cell.SetIdx(idx);
            cell.transform.SetPositionAndRotation(pieceTransform.position, pieceTransform.rotation);
            _cells.Add(cell);
        }

        private void SetLoadedTexture(Transform obj)
        {
            if (obj.TryGetComponent<Renderer>(out var renderer))
            {
                var sharedMaterials = renderer.materials;

                for (int i = 0; i < sharedMaterials.Length; i++)
                {
                    var sharedMaterial = sharedMaterials[i];
                    if (sharedMaterial != null)
                    {
                        sharedMaterial.SetTexture("_BaseMap", _texture);
                    }
                }

                renderer.materials = sharedMaterials;
            }
        }

        private PieceType GetPieceType(int index, int numCols)
        {
            int row = index / numCols;
            int col = index % numCols;

            // If the sum of row and column indices is even, it's Basic.
            // Otherwise, it's Join.
            return (row + col) % 2 == 0 ? PieceType.Basic : PieceType.Join;
        }
    }
}