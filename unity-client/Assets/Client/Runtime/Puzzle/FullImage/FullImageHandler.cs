
using Cysharp.Threading.Tasks;
using System.Threading;
using UniTx.Runtime;
using UniTx.Runtime.IoC;
using UniTx.Runtime.Events;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class FullImageHandler : MonoBehaviour, IFullImageHandler, IInitialisable, IResettable, IInjectable
    {
        [SerializeField] private Renderer _fullImage;

        private IWinConditionChecker _checker;
        private Camera _cam;

        public void Inject(IResolver resolver) => _checker = resolver.Resolve<IWinConditionChecker>();

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
        
        private void HandleGroupPlaced(GroupPlacedEvent @event) => Fade(false);

        private void HandleOnWin() => Fade(false);

        private void Fade(bool active) => gameObject.SetActive(active);
    }
}