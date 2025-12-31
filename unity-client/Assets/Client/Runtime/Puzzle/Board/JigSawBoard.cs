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
        private readonly IList<Transform> _pieces = new List<Transform>();
        private AssetData _assetData;
        private Texture2D _texture;

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
                _pieces.Add(piece);
            }

            if (_pieces.First().TryGetComponent<Renderer>(out var renderer))
            {
                _texture = await UniResources.LoadAssetAsync<Texture2D>(imageKey, cToken: cToken);
                renderer.material.SetTexture("_BaseMap", _texture);
            }
        }

        public void UnLoadPuzzle()
        {
            foreach (var piece in _pieces)
            {
                UniResources.DisposeInstance(piece.gameObject);
            }

            _pieces.Clear();

            if (_texture != null)
            {
                UniResources.DisposeAsset(_texture);
            }
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
    }
}