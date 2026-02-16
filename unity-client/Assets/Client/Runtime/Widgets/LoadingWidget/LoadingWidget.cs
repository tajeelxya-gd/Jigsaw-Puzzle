using DG.Tweening;
using TMPro;
using UniTx.Runtime.Widgets;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class LoadingWidget : MonoBehaviour, IWidget
    {
        [SerializeField] private RectTransform _fillBarRect;
        [SerializeField] private TMP_Text _versionTxt;
        [SerializeField] private float _minSplashTime = 5;
        private float _maxBarWidth;
        private Tween _tween;

        public GameObject GameObject => gameObject;
        public Transform Transform => transform;

        public void Initialise()
        {
            _maxBarWidth = _fillBarRect.sizeDelta.x;
            _fillBarRect.sizeDelta = new Vector2(0, _fillBarRect.sizeDelta.y);
            _versionTxt.text = $"V{Application.version}";

            var endValue = new Vector2(_maxBarWidth, _fillBarRect.sizeDelta.y);
            _tween = _fillBarRect.DOSizeDelta(endValue, _minSplashTime).SetEase(Ease.Linear).Play();
        }

        public void Reset()
        {
            _tween.Complete();
        }

        private void OnDestroy()
        {
            _tween.Complete();
        }
    }
}