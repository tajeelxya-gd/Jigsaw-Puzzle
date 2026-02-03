using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime.Entity;
using UniTx.Runtime.IoC;
using UniTx.Runtime.ResourceManagement;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigsawBoard : EntityBase<JigsawGridData, JigsawBoardSavedData>
    {
        private readonly List<JigsawPiece> _pieces = new();
        private readonly List<JigsawBoardCell> _cells = new();

        private IResolver _resolver;
        private IJigsawResourceLoader _helper;
        private IPuzzleTray _tray;

        public IReadOnlyList<JigsawBoardCell> Cells => _cells;

        public IReadOnlyList<JigsawPiece> Pieces => _pieces;

        private float _sortingY = 0f;
        private const float SortingYStep = 0.00001f;
        private const float MaxSortingY = 0.005f;

        public JigsawBoard(string id) : base(id)
        {
            // Empty yet
        }

        public float GetSortingY(int groupSize)
        {
            const float baseHeight = 0.0001f;
            const float bandRange = 0.004f;
            float sizeWeight = 1f / groupSize;
            float heightFromSize = sizeWeight * bandRange;
            _sortingY += SortingYStep;

            if (_sortingY > 0.0005f) _sortingY = 0f;

            return baseHeight + heightFromSize + _sortingY;
        }

        public async UniTask LoadPuzzleAsync(Transform parent, Transform bounds, string[] cellActionIds, CancellationToken cToken = default)
        {
            await SpawnCellsAsync(parent, bounds, cellActionIds, cToken);
            var grid = _helper.Grid;

            foreach (var cell in _cells)
            {
                var mesh = grid.GetChild(0);
                await WrapMeshesInPuzzlePieceAsync(cell, mesh, cToken);
            }
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
            _sortingY = 0;
        }

        protected override void OnInject(IResolver resolver)
        {
            _resolver = resolver;
            _tray = resolver.Resolve<IPuzzleTray>();
            _helper = resolver.Resolve<IJigsawResourceLoader>();
        }

        protected override void OnInit()
        {
            // Empty yet
        }

        protected override void OnReset()
        {
            // Empty yet
        }

        private async UniTask SpawnCellsAsync(Transform parent, Transform bounds, string[] cellActionIds, CancellationToken cToken = default)
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
            var actionData = _contentservice.GetData<ICellActionData>(cellActionIds).ToArray();

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
                cell.Inject(_resolver);
                cell.SetData(i, cellSize, this, action);
                cell.name = $"Cell_{i}";
                cell.transform.SetPositionAndRotation(worldPos, parent.rotation);
                _cells.Add(cell);
            }
        }

        private async UniTask WrapMeshesInPuzzlePieceAsync(JigsawBoardCell cell, Transform mesh, CancellationToken cToken = default)
        {
            var piece = await UniResources.CreateInstanceAsync<JigsawPiece>("PuzzlePiece - Root", cell.transform, null, cToken);
            piece.name = $"Piece_{cell.Idx}";
            mesh.gameObject.layer = piece.gameObject.layer;
            mesh.SetParent(piece.transform);
            var pos = mesh.localPosition;
            mesh.localPosition = new Vector3(pos.x, 0f, pos.z);
            var meshRenderer = mesh.GetComponent<Renderer>();
            var rendererData = new JigsawPieceRendererData(meshRenderer, _tray.Transform.rotation.eulerAngles, Data.IdleShadowOffset, Data.HoverShadowOffset);
            piece.Inject(_resolver);
            piece.Init(cell, rendererData);
            _pieces.Add(piece);
        }
    }
}