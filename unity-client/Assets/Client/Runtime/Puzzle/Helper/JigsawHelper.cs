using UniTx.Runtime;
using UniTx.Runtime.Events;
using UniTx.Runtime.IoC;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigsawHelper : MonoBehaviour, IJigsawHelper, IInjectable, IInitialisable, IResettable
    {
        [SerializeField] private Renderer _renderer;

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

        public void SetFullImage(Texture2D texture) => _renderer.SetTexture(texture);

        public void ToggleImage() => gameObject.SetActive(!gameObject.activeSelf);

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