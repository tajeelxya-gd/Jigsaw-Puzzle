using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime;
using UniTx.Runtime.Database;
using UniTx.Runtime.Entity;
using UniTx.Runtime.IoC;
using UniTx.Runtime.ResourceManagement;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigsawBoard : EntityBase<JigsawBoardData, JigsawBoardSavedData>
    {
        private readonly List<JigsawPiece> _pieces = new();
        private readonly List<JigsawBoardCell> _cells = new();

        private AssetData _assetData;
        private IResolver _resolver;
        private JigSawLevelData _levelData;
        private IJigsawHelper _helper;

        public IReadOnlyList<JigsawBoardCell> Cells => _cells;

        public IReadOnlyList<JigsawPiece> Pieces => _pieces;

        private float _sortingY = 0f;
        private const float SortingYStep = 0.00001f;
        private const float MaxSortingY = 0.005f;

        public JigsawBoard(string id) : base(id)
        {
            // Empty yet
        }

        public float GetNextSortingY()
        {
            _sortingY += SortingYStep;
            if (_sortingY > MaxSortingY) _sortingY = SortingYStep;
            return _sortingY;
        }

        public async UniTask LoadPuzzleAsync(JigSawLevelData levelData, Transform parent, Transform bounds, CancellationToken cToken = default)
        {
            _levelData = levelData;
            await SpawnCellsAsync(parent, bounds, cToken);
            _assetData = await UniResources.LoadAssetAsync<AssetData>(Data.AssetDataId, cToken: cToken);
            var gridAsset = _assetData.GetAsset(Data.GridId);
            var grid = await UniResources.CreateInstanceAsync<Transform>(gridAsset.RuntimeKey, parent, null, cToken);
            var flatGridAsset = _assetData.GetAsset(Data.FlatGridId);
            var flatGrid = await UniResources.CreateInstanceAsync<Transform>(flatGridAsset.RuntimeKey, parent, null, cToken);
            await _helper.LoadMaterialsAsync(levelData.ImageKey, cToken);

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
            _helper.UnLoadMaterials();
            UniResources.DisposeAsset(_assetData);
            _levelData = null;
        }

        protected override void OnInject(IResolver resolver)
        {
            _resolver = resolver;
            _helper = resolver.Resolve<IJigsawHelper>();
        }

        protected override void OnInit()
        {
            // Empty yet
        }

        protected override void OnReset()
        {
            // Empty yet
        }

        private async UniTask SpawnCellsAsync(Transform parent, Transform bounds, CancellationToken cToken = default)
        {
            var boundsSize = bounds.lossyScale;
            var size = new Vector3(boundsSize.x, 0f, boundsSize.z);
            var r = Data.XConstraint;
            var c = Data.YConstraint;
            var len = r * c;
            float cellLocalWidth = size.x / c;
            float cellLocalHeight = size.z / r;
            float startX = (-size.x / 2f) + (cellLocalWidth / 2f);
            float startZ = (size.z / 2f) - (cellLocalHeight / 2f);
            var cellSize = new Vector3(cellLocalWidth * parent.lossyScale.x, 0.001f, cellLocalHeight * parent.lossyScale.z);
            var actionData = _contentservice.GetData<ICellActionData>(_levelData.CellActionIds).ToArray();

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
                var action = actionData.FirstOrDefault(itm => itm.CellIdx == i);
                cell.SetData(i, cellSize, this, action);
                cell.name = $"Cell_{i}";
                cell.transform.SetPositionAndRotation(worldPos, parent.rotation);
                _cells.Add(cell);
            }
        }

        private async UniTask WrapMeshesInPuzzlePieceAsync(JigsawBoardCell cell, Transform mesh, Transform flatMesh, CancellationToken cToken = default)
        {
            var piece = await UniResources.CreateInstanceAsync<JigsawPiece>("PuzzlePiece - Root", cell.transform, null, cToken);
            piece.name = $"Piece_{cell.Idx}";
            mesh.gameObject.layer = piece.gameObject.layer;
            mesh.SetParent(piece.transform);
            flatMesh.SetParent(piece.transform);
            var meshRenderer = mesh.GetComponent<Renderer>();
            var flatMeshRenderer = flatMesh.GetComponent<Renderer>();
            meshRenderer.sharedMaterials = new[] { _helper.OutlineMaterial, _helper.BaseMaterial };
            flatMeshRenderer.sharedMaterials = new[] { _helper.BaseMaterial };
            var rendererData = new JigsawPieceRendererData(meshRenderer, flatMeshRenderer);
            piece.Inject(_resolver);
            piece.Init(cell, rendererData);
            _pieces.Add(piece);
        }
    }
}