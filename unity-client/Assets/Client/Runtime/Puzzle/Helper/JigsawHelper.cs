using UniTx.Runtime;
using UniTx.Runtime.Events;
using UniTx.Runtime.IoC;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigsawHelper : MonoBehaviour, IJigsawHelper, IInjectable, IInitialisable, IResettable
    {
        [SerializeField] private Renderer _fullImage;
        [SerializeField] private Material[] _materials;

        private Camera _cam;

        private IWinConditionChecker _checker;

        public void Inject(IResolver resolver) => _checker = resolver.Resolve<IWinConditionChecker>();

        public void Initialise()
        {
            _checker.OnWin += HandleOnWin;
            UniEvents.Subscribe<LevelStartEvent>(HandleLevelStart);
        }

        public void Reset()
        {
            _checker.OnWin -= HandleOnWin;
            UniEvents.Unsubscribe<LevelStartEvent>(HandleLevelStart);
        }

        public void SetTexture(Texture2D texture)
        {
            foreach (var mat in _materials)
            {
                mat.SetTexture("_BaseMap", texture);
                mat.SetTexture("_DetailAlbedoMap", texture);
            }

            _fullImage.sharedMaterials = new[] { GetBaseMaterial() };
        }

        public void ToggleImage() => gameObject.SetActive(!gameObject.activeSelf);

        public Material GetBaseMaterial() => _materials[0];

        public Material GetOutlineMaterial() => _materials[1];

        private void Awake() => _cam = Camera.main;

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