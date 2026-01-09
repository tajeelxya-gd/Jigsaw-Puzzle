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
        private IResolver _resolver;

        public IReadOnlyList<JigsawBoardCell> Cells => _cells;

        public IReadOnlyList<JigSawPiece> Pieces => _pieces;

        public JigSawBoard(string id) : base(id)
        {
            // Empty yet
        }

        public async UniTask LoadPuzzleAsync(string imageKey, Transform parent, CancellationToken cToken = default)
        {
            await SpawnCellsAsync(parent, cToken);
            _assetData = await UniResources.LoadAssetAsync<AssetData>(Data.AssetDataId, cToken: cToken);
            _texture = await UniResources.LoadAssetAsync<Texture2D>(imageKey, cToken: cToken);
            var gridAsset = _assetData.GetAsset(Data.GridId);
            var grid = await UniResources.CreateInstanceAsync<Transform>(gridAsset.RuntimeKey, parent, null, cToken);
            var flatGridAsset = _assetData.GetAsset(Data.FlatGridId);
            var flatGrid = await UniResources.CreateInstanceAsync<Transform>(flatGridAsset.RuntimeKey, parent, null, cToken);

            foreach (var cell in _cells)
            {
                var mesh = grid.GetChild(0);
                var flatMesh = flatGrid.GetChild(0);

                await WrapMeshesInPuzzlePieceAsync(cell, mesh, flatMesh, cToken);
            }

            for (int i = 0; i < _pieces.Count; i++)
            {
                var piece = _pieces[i];
                if (piece.Data.PieceType == PieceType.Join)
                {
                    await SetJoinAsync(i, piece, cToken);
                }
            }

            UniResources.DisposeInstance(grid.gameObject);
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
            UniResources.DisposeAsset(_assetData);
        }

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

        protected override void OnInject(IResolver resolver) => _resolver = resolver;

        protected override void OnInit()
        {
            // Empty yet
        }

        protected override void OnReset()
        {
            // Empty yet
        }

        private async UniTask SpawnCellsAsync(Transform parent, CancellationToken cToken = default)
        {
            var size = new Vector3(0.1857f, 0f, 0.2387f);
            var r = Data.XConstraint;
            var c = Data.YConstraint;
            var len = r * c;
            float cellLocalWidth = size.x / c;
            float cellLocalHeight = size.z / r;
            float startX = (-size.x / 2f) + (cellLocalWidth / 2f);
            float startZ = (size.z / 2f) - (cellLocalHeight / 2f);
            var cellSize = new Vector3(cellLocalWidth * parent.lossyScale.x, 0.001f, cellLocalHeight * parent.lossyScale.z);

            for (var i = 0; i < len; i++)
            {
                int currentRow = i / c;
                int currentCol = i % c;

                var localPos = new Vector3(
                    startX + (currentCol * cellLocalWidth),
                    0.001f,
                    startZ - (currentRow * cellLocalHeight)
                );

                var worldPos = parent.TransformPoint(localPos);
                var cell = await UniResources.CreateInstanceAsync<JigsawBoardCell>("JigsawBoardCell", parent, null, cToken);

                cell.SetData(i, cellSize);
                cell.name = $"Cell_{i}";
                cell.transform.SetPositionAndRotation(worldPos, parent.rotation);
                _cells.Add(cell);
            }
        }

        private async UniTask SetJoinAsync(int idx, JigSawPiece piece, CancellationToken cToken = default)
        {
            var join = await UniResources.CreateInstanceAsync<JoinController>("Join - Controller", piece.transform, null, cToken);
            var neighbours = GetNeighbours(idx).ToArray();
            join.Init(piece.BoxCollider, neighbours, piece);
        }

        private async UniTask WrapMeshesInPuzzlePieceAsync(JigsawBoardCell cell, Transform mesh, Transform flatMesh, CancellationToken cToken = default)
        {
            var piece = await UniResources.CreateInstanceAsync<JigSawPiece>("PuzzlePiece - Root", cell.transform, null, cToken);
            piece.name = $"Piece_{cell.Idx}";
            mesh.SetParent(piece.transform);
            flatMesh.SetParent(piece.transform);
            var rendererData = new JigsawPieceRendererData(mesh.GetComponent<Renderer>(), flatMesh.GetComponent<Renderer>(), _texture);
            var pieceData = new JigSawPieceData(cell, _cells, GetPieceType(cell.Idx, Data.YConstraint));
            piece.Inject(_resolver);
            piece.Init(pieceData, rendererData);
            _pieces.Add(piece);
        }

        private PieceType GetPieceType(int index, int numCols)
        {
            int row = index / numCols;
            int col = index % numCols;

            // If the sum of row and column indices is even, it's Basic. Otherwise, it's Join.
            return (row + col) % 2 == 0 ? PieceType.Basic : PieceType.Join;
        }
    }
}