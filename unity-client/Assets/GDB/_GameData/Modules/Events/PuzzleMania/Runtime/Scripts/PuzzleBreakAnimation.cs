using Coffee.UIExtensions;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;

public class PuzzleBreakAnimation : MonoBehaviour
{
    [SerializeField] private GameObject _hammer;
    [SerializeField] private GameObject _brokenSpriteImage;
    [SerializeField] private RectTransform[] _brokenSprites;
    [SerializeField] private UIParticle _particleSystem;

    [Header("Impact")]
    [SerializeField] private float impactDistance = 40f;
    [SerializeField] private float impactDuration = 0.12f;

    [Header("Fall")]
    [SerializeField] private float fallDistance = 300f;
    [SerializeField] private float fallDuration = 0.7f;

    [Header("Rotation")]
    [SerializeField] private float maxRotation = 35f;

    private Vector2[] _startPositions;
    private Quaternion[] _startRotations;
    private Vector3[] _startScales;

    private RectTransform _selfRect;

    private void Awake()
    {
        _selfRect = transform as RectTransform;
        CacheInitialState();
    }

    private void Start()
    {
        SignalBus.Subscribe<PuzzleManiaBreakAnimationSignal>(PlayBreakAnimation);
    }

    private void CacheInitialState()
    {
        int count = _brokenSprites.Length;

        _startPositions = new Vector2[count];
        _startRotations = new Quaternion[count];
        _startScales = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            _startPositions[i] = _brokenSprites[i].anchoredPosition;
            _startRotations[i] = _brokenSprites[i].localRotation;
            _startScales[i] = _brokenSprites[i].localScale;
        }
    }

    [Button]
    public void Test()
    {
        SignalBus.Publish(new PuzzleManiaBreakAnimationSignal{_transform = this.transform});
    }
    
    public void PlayBreakAnimation(PuzzleManiaBreakAnimationSignal signal)
    {
        ResetPieces();

        // Move UI root correctly
        _selfRect.position = signal._transform.position;

        _hammer.SetActive(true);
        
        Sequence hammerSeq = DOTween.Sequence();
        hammerSeq.Append(
            _hammer.transform.DORotate(new Vector3(0, 0, -45), 0.75f).SetEase(Ease.OutBack)
        ).OnComplete(() =>
        {
            AudioController.PlaySFX(AudioType.PuzzleUnlock);
            HapticController.Vibrate(HapticType.Puzzle);
        });
        _brokenSpriteImage.SetActive(true);
        hammerSeq.AppendInterval(0.3f); // delay in seconds
        hammerSeq.AppendCallback(() =>
        {
            AudioController.PlaySFX(AudioType.ButtonClick);
            HapticController.Vibrate(HapticType.Btn);
            _brokenSpriteImage.SetActive(false);
            _particleSystem.gameObject.SetActive(true);
            _particleSystem.Play();
            _hammer.SetActive(false);
            BreakPieces();
        });

    }

    private void BreakPieces()
    {
        for (int i = 0; i < _brokenSprites.Length; i++)
        {
            RectTransform piece = _brokenSprites[i];
            piece.gameObject.SetActive(true);

            Vector2 startPos = piece.anchoredPosition;

            Vector2 impactDir = new Vector2(
                Random.Range(-0.4f, 0.4f),
                Random.Range(0.2f, 0.5f)
            ).normalized;

            Sequence seq = DOTween.Sequence();

            seq.Append(
                piece.DOAnchorPos(
                    startPos + impactDir * impactDistance,
                    impactDuration
                ).SetEase(Ease.OutQuad)
            );

            seq.Append(
                piece.DOAnchorPosY(
                    startPos.y - fallDistance,
                    fallDuration
                ).SetEase(Ease.InQuad)
            );

            piece.DORotate(
                new Vector3(0, 0, Random.Range(-maxRotation, maxRotation)),
                impactDuration + fallDuration,
                RotateMode.FastBeyond360
            );

            piece.DOPunchScale(Vector3.one * 0.2f, impactDuration, 8, 0.8f);
        }
    }

    public void ResetPieces()
    {
        _hammer.SetActive(false);
        _brokenSpriteImage.SetActive(false);
        _hammer.transform.localRotation = Quaternion.Euler(0, 0, 0);
        _particleSystem.gameObject.SetActive(false);
        for (int i = 0; i < _brokenSprites.Length; i++)
        {
            RectTransform piece = _brokenSprites[i];

            piece.DOKill();

            piece.anchoredPosition = _startPositions[i];
            piece.localRotation = _startRotations[i];
            piece.localScale = _startScales[i];

            piece.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<PuzzleManiaBreakAnimationSignal>(PlayBreakAnimation);
    }
}

public class PuzzleManiaBreakAnimationSignal : ISignal
{
    public Transform _transform;
}
