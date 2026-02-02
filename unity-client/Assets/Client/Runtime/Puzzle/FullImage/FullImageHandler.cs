
using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime;
using UniTx.Runtime.Events;
using UniTx.Runtime.IoC;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class FullImageHandler : MonoBehaviour, IFullImageHandler, IInitialisable, IResettable, IInjectable
    {
        [SerializeField] private float _levelCompletedDuration = 1;

        private IJigsawResourceLoader _loader;
        private IWinConditionChecker _checker;
        private Camera _cam;
        private Renderer _renderer;

        public void Inject(IResolver resolver)
        {
            _checker = resolver.Resolve<IWinConditionChecker>();
            _loader = resolver.Resolve<IJigsawResourceLoader>();
        }

        public void Initialise()
        {
            _checker.OnWin += HandleOnWin;
            UniEvents.Subscribe<LevelStartEvent>(HandleLevelStart);
            UniEvents.Subscribe<GroupPlacedEvent>(HandleGroupPlaced);
        }

        public void Reset()
        {
            _checker.OnWin -= HandleOnWin;
            UniEvents.Unsubscribe<LevelStartEvent>(HandleLevelStart);
            UniEvents.Unsubscribe<GroupPlacedEvent>(HandleGroupPlaced);
        }

        public void ToggleFullImage() => Fade(!gameObject.activeSelf);

        public UniTask PlayLevelCompletedAnimationAsync(CancellationToken cToken = default)
        {
            return UniTask.CompletedTask;
        }

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

        private void HandleLevelStart(LevelStartEvent @event)
        {
            var fullImg = _loader.FullImage;
            fullImg.SetParent(transform);
            fullImg.gameObject.layer = gameObject.layer;
            _renderer = fullImg.GetComponent<Renderer>();
            Fade(false);
        }

        private void HandleGroupPlaced(GroupPlacedEvent @event) => Fade(false);

        private void HandleOnWin() => Fade(false);

        private void Fade(bool active) => gameObject.SetActive(active);
    }
}