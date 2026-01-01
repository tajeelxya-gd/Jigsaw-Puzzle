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
            var grid = await SpawnGridAsync(parent, cToken);
            var len = grid.childCount;

            for (int i = 0; i < len; i++)
            {
                var mesh = grid.GetChild(0);
                SetLoadedTexture(mesh);
                var piece = await SpawnPuzzlePieceAsync(mesh, parent, cToken);
                SetPlacement(i, piece);
            }

            var fullImgAsset = _assetData.GetAsset(Data.FullImageId);
            FullImg = await UniResources.CreateInstanceAsync<Transform>(fullImgAsset.RuntimeKey, parent, null, cToken);
            SetLoadedTexture(FullImg);
            UniResources.DisposeInstance(grid.gameObject);
        }

        public void UnLoadPuzzle()
        {
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

        private UniTask<Transform> SpawnGridAsync(Transform parent, CancellationToken cToken = default)
        {
            var gridAsset = _assetData.GetAsset(Data.GridId);
            return UniResources.CreateInstanceAsync<Transform>(gridAsset.RuntimeKey, parent, null, cToken);
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
    }
}