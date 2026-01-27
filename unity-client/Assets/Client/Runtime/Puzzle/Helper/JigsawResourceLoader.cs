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
    public sealed class JigsawResourceLoader : MonoBehaviour, IJigsawResourceLoader, IInjectable, IInitialisableAsync, IResettable
    {
        [SerializeField] private string _imageAssetDataKey;
        [SerializeField] private string _gridAssetDataKey;
        [SerializeField] private Material _base;
        [SerializeField] private Material _outlineBoard;
        [SerializeField] private Material _outlineTray;
        [SerializeField] private Renderer _fullImage;

        private IWinConditionChecker _checker;
        private AssetData _imageAssetData;
        private AssetData _gridAssetData;
        private Texture2D _image;
        private Transform _grid;
        private Camera _cam;

        public Material Base => _base;
        public Material OutlineTray => _outlineTray;
        public Material OutlineBoard => _outlineBoard;

        public Transform Grid => _grid;

        public void Inject(IResolver resolver) => _checker = resolver.Resolve<IWinConditionChecker>();

        public async UniTask InitialiseAsync(CancellationToken cToken = default)
        {
            _imageAssetData = await UniResources.LoadAssetAsync<AssetData>(_imageAssetDataKey, cToken: cToken);
            _gridAssetData = await UniResources.LoadAssetAsync<AssetData>(_gridAssetDataKey, cToken: cToken);
            _checker.OnWin += HandleOnWin;
            UniEvents.Subscribe<LevelStartEvent>(HandleLevelStart);
            _cam = Camera.main;
        }

        public void Reset()
        {
            _checker.OnWin -= HandleOnWin;
            UniEvents.Unsubscribe<LevelStartEvent>(HandleLevelStart);
            UniResources.DisposeAsset(_imageAssetData);
            UniResources.DisposeAsset(_gridAssetData);
        }

        public async UniTask LoadImageAsync(string key, CancellationToken cToken = default)
        {
            var imgAsset = _imageAssetData.GetAsset(key);
            _image = await UniResources.LoadAssetAsync<Texture2D>(imgAsset.RuntimeKey, cToken: cToken);
            var property = "_MainTex";
            _base.SetTexture(property, _image);
            _outlineTray.SetTexture(property, _image);
            _outlineBoard.SetTexture(property, _image);
            _fullImage.sharedMaterials = new[] { _base };
        }

        public void UnLoadImage()
        {
            UniResources.DisposeAsset(_image);
            _image = null;
        }

        public void ToggleFullImage() => gameObject.SetActive(!gameObject.activeSelf);

        public async UniTask CreateGridAsync(string key, Transform parent, CancellationToken cToken = default)
        {
            var gridAsset = _gridAssetData.GetAsset(key);
            _grid = await UniResources.CreateInstanceAsync<Transform>(gridAsset.RuntimeKey, parent, null, cToken: cToken);
        }

        public void DestroyGrid()
        {
            UniResources.DisposeInstance(_grid.gameObject);
            _grid = null;
        }

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