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

        private AssetData _assetData;
        private Texture2D _texture;

        public Transform FullImg { get; private set; }

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

            foreach (var id in Data.PiecesIds)
            {
                var meshAsset = _assetData.GetAsset(id);
                var mesh = await UniResources.CreateInstanceAsync<Transform>(meshAsset.RuntimeKey, parent, null, cToken);
                SetLoadedTexture(mesh);
                _meshes.Add(mesh.gameObject);

                var piece = await UniResources.CreateInstanceAsync<JigSawPiece>("PuzzlePiece - Root", parent, null, cToken);
                var meshTransform = mesh.transform;
                piece.transform.SetPositionAndRotation(meshTransform.position, meshTransform.rotation);
                mesh.SetParent(piece.transform);
                piece.SetColliderSize(mesh.GetComponent<Renderer>());
                _pieces.Add(piece);
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

        private void SetLoadedTexture(Transform obj)
        {
            if (obj.TryGetComponent<Renderer>(out var renderer))
            {
                renderer.material.SetTexture("_BaseMap", _texture);
            }
        }
    }
}