using System;
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
        private readonly List<GameObject> _meshes = new();
        private readonly List<JigSawPiece> _pieces = new();
        private readonly List<Transform> _placements = new();

        private AssetData _assetData;
        private Texture2D _texture;

        public Transform FullImg { get; private set; }

        public IReadOnlyList<Transform> Placements => _placements;
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

            for (int i = 0; i < Data.PiecesIds.Length; i++)
            {
                var id = Data.PiecesIds[i];
                var mesh = await SpawnMeshAsync(id, parent, cToken);
                var piece = await SpawnPuzzlePieceAsync(mesh, parent, cToken);
                SetPlacement(i, piece);
            }

            var fullImgAsset = _assetData.GetAsset(Data.FullImageId);
            FullImg = await UniResources.CreateInstanceAsync<Transform>(fullImgAsset.RuntimeKey, parent, null, cToken);
            SetLoadedTexture(FullImg);
        }

        public void UnLoadPuzzle()
        {
            foreach (var mesh in _meshes)
            {
                UniResources.DisposeInstance(mesh);
            }
            _meshes.Clear();

            foreach (var piece in _pieces)
            {
                UniResources.DisposeInstance(piece.gameObject);
            }
            _pieces.Clear();

            foreach (var placement in _placements)
            {
                GameObject.Destroy(placement.gameObject);
            }
            _placements.Clear();

            UniResources.DisposeAsset(_texture);
            UniResources.DisposeInstance(FullImg.gameObject);
        }

        public void SetActiveFullImage(bool active) => FullImg.gameObject.SetActive(active);

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

        private async UniTask<Transform> SpawnMeshAsync(string id, Transform parent, CancellationToken cToken = default)
        {
            var meshAsset = _assetData.GetAsset(id);
            var mesh = await UniResources.CreateInstanceAsync<Transform>(meshAsset.RuntimeKey, parent, null, cToken);
            SetLoadedTexture(mesh);
            _meshes.Add(mesh.gameObject);
            return mesh;
        }

        private async UniTask<JigSawPiece> SpawnPuzzlePieceAsync(Transform mesh, Transform parent, CancellationToken cToken = default)
        {
            var piece = await UniResources.CreateInstanceAsync<JigSawPiece>("PuzzlePiece - Root", parent, null, cToken);
            piece.transform.SetPositionAndRotation(mesh.position, mesh.rotation);
            mesh.SetParent(piece.transform);
            piece.SetColliderSize(mesh.GetComponent<Renderer>());
            piece.SetPlacements(_placements);
            _pieces.Add(piece);

            return piece;
        }

        private void SetPlacement(int idx, JigSawPiece piece)
        {
            var go = new GameObject($"Placement - {idx}");
            var placement = go.transform;
            var pieceTransform = piece.transform;
            placement.SetParent(pieceTransform.parent);
            placement.SetPositionAndRotation(pieceTransform.position, pieceTransform.rotation);
            _placements.Add(go.transform);
        }

        private void SetLoadedTexture(Transform obj)
        {
            if (obj.TryGetComponent<Renderer>(out var renderer))
            {
                renderer.material.SetTexture("_BaseMap", _texture);
            }
        }
    }
}