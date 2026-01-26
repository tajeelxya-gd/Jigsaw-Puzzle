using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime;
using UniTx.Runtime.Database;
using UniTx.Runtime.Events;
using UniTx.Runtime.IoC;
using UniTx.Runtime.ResourceManagement;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigsawHelper : MonoBehaviour, IJigsawHelper, IInjectable, IInitialisableAsync, IResettable
    {
        [SerializeField] private string _assetDataKey;
        [SerializeField] private Renderer _fullImage;

        private IWinConditionChecker _checker;
        private AssetData _assetData;
        private Camera _cam;

        public Material PieceMaterial { get; private set; }
        public Material PieceBaseMaterial { get; private set; }

        public void Inject(IResolver resolver) => _checker = resolver.Resolve<IWinConditionChecker>();

        public async UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            _assetData = await UniResources.LoadAssetAsync<AssetData>(_assetDataKey, cToken: cToken);
            _checker.OnWin += HandleOnWin;
            UniEvents.Subscribe<LevelStartEvent>(HandleLevelStart);
            _cam = Camera.main;
        }

        public void Reset()
        {
            _checker.OnWin -= HandleOnWin;
            UniEvents.Unsubscribe<LevelStartEvent>(HandleLevelStart);
            UniResources.DisposeAsset(_assetData);
        }

        public async UniTask LoadMaterialsAsync(string key, CancellationToken cToken = default)
        {
            var matAsset = _assetData.GetAsset(key);
            PieceMaterial = await UniResources.LoadAssetAsync<Material>(matAsset.RuntimeKey, cToken: cToken);

            var baseMatAsset = _assetData.GetAsset($"{key}_base");
            PieceBaseMaterial = await UniResources.LoadAssetAsync<Material>(baseMatAsset.RuntimeKey, cToken: cToken);

            _fullImage.sharedMaterials = new[] { PieceMaterial };
        }

        public void UnLoadMaterials()
        {
            UniResources.DisposeAsset(PieceMaterial);
            UniResources.DisposeAsset(PieceBaseMaterial);
        }

        public void ToggleFullImage() => gameObject.SetActive(!gameObject.activeSelf);

        private void Update()
        {
            if (InputHandler._3DActive && Input.GetMouseButtonDown(0))
            {
                Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
                if (!Physics.Raycast(ray, out var hit)) return;
                if (hit.transform != transform) return;

                Fade(false);
            }
        }

        private void HandleLevelStart(LevelStartEvent @event) => Fade(false);

        private void HandleOnWin() => Fade(false);

        private void Fade(bool active) => gameObject.SetActive(active);
    }
}