using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UniTx.Runtime;
using UniTx.Runtime.Entity;
using UniTx.Runtime.Events;
using UniTx.Runtime.IoC;
using UniTx.Runtime.ResourceManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Client.Runtime
{
    public sealed class CurrencyHUDComponent : MonoBehaviour, IInitialisable, IResettable, IInjectable
    {
        [SerializeField] private string _currencyId;
        [SerializeField] private TMP_Text _txtAmount;
        [SerializeField] private Image _currencyImg;
        [SerializeField] private float _duration;

        private IEntityService _entityService;
        private ICurrency _currency;
        private Sprite _sprite;
        private Camera _cam;

        public void Inject(IResolver resolver) => _entityService = resolver.Resolve<IEntityService>();

        public void Initialise()
        {
            _currency = _entityService.Get<ICurrency>(_currencyId);
            UpdateText(_currency.Amount);
            RegisterEvents();
            LoadSpriteAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }

        public void Reset()
        {
            UnRegisterEvents();
            _currency = null;
            UpdateText(0);
        }

        private void RegisterEvents()
        {
            UniEvents.Subscribe<CurrencyCollectHudEffectEvent>(HandleCollectEffect);
        }

        private void UnRegisterEvents()
        {
            UniEvents.Unsubscribe<CurrencyCollectHudEffectEvent>(HandleCollectEffect);
        }

        private void UpdateText(double amount) => _txtAmount.SetText($"{amount}");

        private async UniTask LoadSpriteAsync(CancellationToken cToken = default)
        {
            var spriteKey = _currency.ImageKey;
            _sprite = await UniResources.LoadAssetAsync<Sprite>(spriteKey, null, cToken);
            _currencyImg.sprite = _sprite;
        }

        private void UnloadSprite()
        {
            if (_sprite != null) UniResources.DisposeAsset(_sprite);
            _currencyImg.sprite = null;
        }

        private void HandleCollectEffect(CurrencyCollectHudEffectEvent @event)
        {
            if (_currency == null || !@event.ImageKey.Equals(_currency.ImageKey) || _sprite == null) return;

            AnimateCollectEffectAsync(@event.WorldPos, this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTaskVoid AnimateCollectEffectAsync(Vector3 worldPos, CancellationToken cToken = default)
        {
            var duplicate = Instantiate(_currencyImg, _currencyImg.transform.parent);
            duplicate.sprite = _sprite;

            var screenPos = _cam.WorldToScreenPoint(worldPos);
            duplicate.transform.position = screenPos;

            var startPos = duplicate.transform.position;
            var endPos = _currencyImg.transform.position;
            var elapsed = 0f;

            while (elapsed < _duration)
            {
                if (cToken.IsCancellationRequested) break;

                elapsed += Time.deltaTime;
                float t = elapsed / _duration;
                float tEase = 1 - (1 - t) * (1 - t);

                duplicate.transform.position = Vector3.Lerp(startPos, endPos, tEase);
                duplicate.transform.localScale = Vector3.Lerp(Vector3.one * 1.5f, Vector3.one, tEase);
                await UniTask.Yield(PlayerLoopTiming.Update, cToken);
            }

            UpdateText(_currency.Amount);

            if (duplicate != null) Destroy(duplicate.gameObject);
        }

        private void Awake() => _cam = Camera.main;
    }
}