using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;
using UnityEngine.Events;
public class SceneLoader : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private CanvasGroup rootCanvasGroup;
    [SerializeField] private RectTransform fillBarRect;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Image[] _pieces;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float minLoadTime = 2.0f;
    [SerializeField] private float piecesAnimationSpeed = 5f;

    [Header("Visual")]
    [SerializeField] private LoadingScreenUIData _loadingScreenUIData;
    [SerializeField] private Image _mainLoadingImg, _loadingTxtImg, _emptBar, _raceImg;

    private float _visualProgress = 0f;
    private float _maxBarWidth;
    [SerializeField] private bool isGamePlay = false;

    void Awake()
    {
        float fillWidth = fillBarRect.rect.width;
        fillBarRect.anchoredPosition = new Vector2(-fillWidth, fillBarRect.anchoredPosition.y);
    }

    void Start()
    {
        SignalBus.Subscribe<OnSceneShiftSignal>(OnShiftScene);
        ResetUI(false);
    }

    private void ResetUI(bool active)
    {
        _visualProgress = 0;
        float fillWidth = fillBarRect.rect.width;
        fillBarRect.anchoredPosition = new Vector2(-fillWidth, fillBarRect.anchoredPosition.y);
        progressText.text = "0%";

        if (!active)
        {
            rootCanvasGroup.alpha = 0;
            rootCanvasGroup.gameObject.SetActive(false);
        }
        else
        {
            rootCanvasGroup.gameObject.SetActive(true);
        }
    }

    void OnShiftScene(OnSceneShiftSignal signal)
    {
        // We keep this at 1f to ensure the new scene starts running, 
        // but the logic below now works regardless of this value.
        Time.timeScale = 1f;

        if (signal.DoFakeLoad)
            StartFakeLoad(signal);
        else
            StartRealLoad(signal.SceneName);

        if (!isGamePlay)
            ChangeVisual(signal.levelType);
        AudioController.StopBG();
    }

    private void StartFakeLoad(OnSceneShiftSignal signal)
    {
        ResetUI(true);

        // Added .SetUpdate(true) to ignore timeScale
        rootCanvasGroup.DOFade(1, fadeDuration).From(0).SetUpdate(true);

        DOTween.To(() => _visualProgress, x => UpdateUI(x), 1f, signal.FakeLoadTime)
            .SetEase(Ease.Linear)
            .SetUpdate(true) // Ensure the bar moves during pause
            .OnComplete(() =>
            {
                signal.OnFakeLoadCompleteEven?.Invoke();
                rootCanvasGroup.DOFade(0, fadeDuration / 2)
                    .SetUpdate(true)
                    .OnComplete(() => ResetUI(false));
            });
    }

    private void ChangeVisual(LevelType levelType)
    {
        if (_loadingScreenUIData == null) return;

        LoadingScreenUIData.UI uI = _loadingScreenUIData.GetData(levelType);
        _mainLoadingImg.sprite = uI.LoadingPanelSprite;
        _loadingTxtImg.sprite = uI.LoadingTxtSp;
        _emptBar.sprite = uI.EmptBarSp;
        _raceImg.sprite = uI.RaceSp;
    }

    private void StartRealLoad(string sceneName)
    {
        ResetUI(true);
        // Added .SetUpdate(true)
        rootCanvasGroup.DOFade(1, fadeDuration).From(0).SetUpdate(true).OnComplete(() =>
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        });
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f || _visualProgress < 0.99f)
        {
            float target = Mathf.Clamp01(operation.progress / 0.9f);

            // Changed to Time.unscaledDeltaTime so the bar moves while paused
            _visualProgress = Mathf.MoveTowards(_visualProgress, target, Time.unscaledDeltaTime / minLoadTime);
            UpdateUI(_visualProgress);

            yield return null;
        }

        UpdateUI(1f);

        // Changed to WaitForSecondsRealtime so the coroutine doesn't hang while paused
        yield return new WaitForSecondsRealtime(0.2f);

        operation.allowSceneActivation = true;
    }

    private void UpdateUI(float val)
    {
        _visualProgress = val;
        float fillWidth = fillBarRect.rect.width;
        fillBarRect.anchoredPosition = new Vector2((val - 1) * fillWidth, fillBarRect.anchoredPosition.y);

        progressText.text = $"{(val * 100):0}%";
    }

    private void OnDisable() => SignalBus.Unsubscribe<OnSceneShiftSignal>(OnShiftScene);

    private void Update()
    {
        AnimatePieces();
    }

    private void AnimatePieces()
    {
        if (_pieces == null || _pieces.Length == 0) return;

        float phase = (Time.unscaledTime * piecesAnimationSpeed) % _pieces.Length;

        for (int i = 0; i < _pieces.Length; i++)
        {
            float dist = phase - i;

            // Wrap distance to [-2, 2] for 4 pieces
            float halfLen = _pieces.Length / 2f;
            while (dist > halfLen) dist -= _pieces.Length;
            while (dist < -halfLen) dist += _pieces.Length;

            // alpha = 1.5 - abs(dist)
            // This ensures:
            // dist 0     -> alpha 1.0 (clamped from 1.5)
            // dist 1/-1  -> alpha 0.5
            // dist 2/-2  -> alpha 0.0 (clamped from -0.5)
            // This keeps at most 3 pieces visible at any time.
            float alpha = Mathf.Clamp01(1.5f - Mathf.Abs(dist));

            Color c = _pieces[i].color;
            c.a = alpha;
            _pieces[i].color = c;
        }
    }
}
public class OnSceneShiftSignal : ISignal
{
    public string SceneName;
    public bool DoFakeLoad = false;
    public float FakeLoadTime = 2;
    public UnityAction OnFakeLoadCompleteEven;
    public LevelType levelType = LevelType.Easy;
}