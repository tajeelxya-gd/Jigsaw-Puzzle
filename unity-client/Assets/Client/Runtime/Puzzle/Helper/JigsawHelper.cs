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

        public Material BaseMaterial { get; private set; }

        public Material PieceTrayOutline { get; private set; }

        public Material PieceBoardOutline { get; private set; }

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
            BaseMaterial = await UniResources.LoadAssetAsync<Material>(matAsset.RuntimeKey, cToken: cToken);

            var trayOutlineAsset = _assetData.GetAsset($"{key}_tray");
            PieceTrayOutline = await UniResources.LoadAssetAsync<Material>(trayOutlineAsset.RuntimeKey, cToken: cToken);

            var boardOutlineAsset = _assetData.GetAsset($"{key}_board");
            PieceBoardOutline = await UniResources.LoadAssetAsync<Material>(boardOutlineAsset.RuntimeKey, cToken: cToken);

            _fullImage.sharedMaterials = new[] { BaseMaterial };
        }

        public void UnLoadMaterials()
        {
            UniResources.DisposeAsset(BaseMaterial);
            UniResources.DisposeAsset(PieceTrayOutline);
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