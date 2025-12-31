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
        private AssetData _assetData;
        private Texture2D _texture;

        public IList<Transform> Pieces { get; private set; } = new List<Transform>();
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

            foreach (var id in Data.PiecesIds)
            {
                var asset = _assetData.GetAsset(id);
                var piece = await UniResources.CreateInstanceAsync<Transform>(asset.RuntimeKey, parent, null, cToken);
                Pieces.Add(piece);
            }

            var fullImgAsset = _assetData.GetAsset(Data.FullImageId);
            FullImg = await UniResources.CreateInstanceAsync<Transform>(fullImgAsset.RuntimeKey, parent, null, cToken);

            if (Pieces.First().TryGetComponent<Renderer>(out var renderer))
            {
                _texture = await UniResources.LoadAssetAsync<Texture2D>(imageKey, cToken: cToken);
                renderer.material.SetTexture("_BaseMap", _texture);
            }
        }

        public void UnLoadPuzzle()
        {
            foreach (var piece in Pieces)
            {
                UniResources.DisposeInstance(piece.gameObject);
            }
            Pieces.Clear();
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
    }
}