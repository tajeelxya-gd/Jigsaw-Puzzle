using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class WallView : MonoBehaviour
{
    [SerializeField] private RectTransform _heartImage;
    [SerializeField] private Image _fillImg;
    [SerializeField] private GameObject _canvas;

    private RectTransform _fillRect;
    private CanvasGroup _canvasGroup;

    private float _maxBarHeight;
    private float _initHealth;

    private Tween _hideTween;
    private Tween _heartTween;
    private Tween _fadeTween;

    public void Initialize(int health)
    {
        _initHealth = health;

        _fillRect = _fillImg.GetComponent<RectTransform>();
        if (_maxBarHeight == 0)
            _maxBarHeight = _fillRect.sizeDelta.y;
        _fillRect.sizeDelta = new Vector2(_fillRect.sizeDelta.x, _maxBarHeight);

        _canvasGroup = _canvas.GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = _canvas.AddComponent<CanvasGroup>();

        _canvasGroup.alpha = 0f;
        _canvas.SetActive(false);

        if (TutorialManager.IsTutorialActivated && GlobalService.GameData.Data.LevelIndex == 2)
            ShowCanvas();
    }

    public void UpdateBar(float val)
    {
        ShowCanvas();

        float percent = Mathf.Clamp01(val / _initHealth);
        float targetHeight = _maxBarHeight * percent;

        _fillRect.sizeDelta = new Vector2(_fillRect.sizeDelta.x, targetHeight);

        _heartTween?.Kill(true);
        _heartImage.localScale = Vector3.one;
        _heartTween = _heartImage.DOPunchScale(
            Vector3.one * 0.25f,
            0.3f,
            vibrato: 8,
            elasticity: 0.8f
        );

        _hideTween?.Kill();
        _hideTween = DOVirtual.DelayedCall(2f, HideCanvas);
    }

    private void ShowCanvas()
    {
        _canvas.SetActive(true);

        _fadeTween?.Kill();
        _fadeTween = _canvasGroup.DOFade(1f, 0.25f);
    }

    private void HideCanvas()
    {
        _fadeTween?.Kill();
        _fadeTween = _canvasGroup
            .DOFade(0f, 0.3f)
            .OnComplete(() => _canvas.SetActive(false));
    }
}