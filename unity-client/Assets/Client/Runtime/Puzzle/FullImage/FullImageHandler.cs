
using System;
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
        [SerializeField] private float _fadeDuration = 0.5f;

        private CancellationTokenSource _fadeCts;

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
            _fadeCts?.Cancel();
            _fadeCts?.Dispose();
            _fadeCts = null;
        }

        public void ToggleFullImage() => Fade(!gameObject.activeSelf);

        public async UniTask PlayLevelCompletedAnimationAsync(CancellationToken cToken = default)
        {
            await FadeAsync(true, cToken);
            await UniTask.Delay(TimeSpan.FromSeconds(_levelCompletedDuration), cancellationToken: cToken);
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
            _renderer.sharedMaterial = _loader.FullImageMaterial;
            Fade(false, true);
        }

        private void HandleGroupPlaced(GroupPlacedEvent @event) => Fade(false);

        private void HandleOnWin() => Fade(false);

        private void Fade(bool active, bool instant = false)
        {
            _fadeCts?.Cancel();
            _fadeCts = new CancellationTokenSource();
            FadeAsync(active, _fadeCts.Token, instant).Forget();
        }

        private async UniTask FadeAsync(bool active, CancellationToken cToken = default, bool instant = false)
        {
            if (_renderer == null) return;

            float targetAlpha = active ? 1 : 0;

            if (active) gameObject.SetActive(true);

            if (!instant)
            {
                float elapsed = 0;
                float startAlpha = _renderer.material.color.a;

                while (elapsed < _fadeDuration)
                {
                    elapsed += Time.deltaTime;
                    float progress = elapsed / _fadeDuration;
                    Color color = _renderer.material.color;
                    color.a = Mathf.Lerp(startAlpha, targetAlpha, progress);
                    _renderer.material.color = color;
                    await UniTask.Yield(PlayerLoopTiming.Update, cToken);
                }
            }

            Color finalColor = _renderer.material.color;
            finalColor.a = targetAlpha;
            _renderer.material.color = finalColor;

            if (!active) gameObject.SetActive(false);
        }
    }
}