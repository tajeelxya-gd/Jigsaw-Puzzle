using System.Collections.Generic;
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

            UniResources.DisposeInstance(grid.gameObject);
            UniResources.DisposeInstance(flatGrid.gameObject);
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

        private async UniTask WrapMeshesInPuzzlePieceAsync(JigsawBoardCell cell, Transform mesh, Transform flatMesh, CancellationToken cToken = default)
        {
            var piece = await UniResources.CreateInstanceAsync<JigSawPiece>("PuzzlePiece - Root", cell.transform, null, cToken);
            piece.name = $"Piece_{cell.Idx}";
            mesh.SetParent(piece.transform);
            flatMesh.SetParent(piece.transform);
            var rendererData = new JigsawPieceRendererData(mesh.GetComponent<Renderer>(), flatMesh.GetComponent<Renderer>(), _texture);
            var pieceData = new JigSawPieceData(cell, _cells);
            piece.Inject(_resolver);
            piece.Init(pieceData, rendererData);
            _pieces.Add(piece);
        }
    }
}