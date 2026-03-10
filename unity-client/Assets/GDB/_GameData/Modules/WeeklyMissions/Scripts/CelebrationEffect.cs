using System;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;

public class CelebrationEffect : MonoBehaviour
{
    [SerializeField] private GameObject[] _allObjects;
    [SerializeField] private int _minSpawns = 5;
    [SerializeField] private int _maxSpawns = 10;
    [SerializeField] private AnimationCurve _curve;
    [SerializeField] private Transform[] _spawnPoints;

    [SerializeField] private float _minOffset = 50f;          // Random horizontal offset
    [SerializeField] private float _maxOffset = 150f;
    [SerializeField] private float _moveDuration = 2f;  
    [Range(20, 90)]
    [SerializeField] private float screenDistancePer = 50;
    [ReadOnly,SerializeField]
     private float _baseMoveDistance = 300f;  // Base distance upward
    [SerializeField] private float _randomDistanceOffset = 100f; // Random extra distance per balloon
    [SerializeField] private float _delayBetweenSpawns = 0.1f;
    [SerializeField] private AudioClip []celebrationAudioEffects;

    private void Awake()
    {
        SignalBus.Subscribe<OnCelebrationAchievementSignal>(OnCelebrationAchievementReceived);
        _baseMoveDistance = Screen.height * (screenDistancePer/100);
    }

    private void OnCelebrationAchievementReceived(OnCelebrationAchievementSignal signal)
    {
        PlayEffect();
    }

    [Button("Play Effect")]
    public void PlayEffect()
    {
        int spawnCount = Random.Range(_minSpawns, _maxSpawns + 1);

        for (int i = 0; i < spawnCount; i++)
        {
            float delay = i * _delayBetweenSpawns + Random.Range(0f, _delayBetweenSpawns);
            Invoke(nameof(SpawnBalloon), delay);
            OnPlaySfx();
        }
    }

    void OnPlaySfx()
    {
        foreach (var audEffect in celebrationAudioEffects)   
            AudioController.PlaySFX(audEffect);
    }

    private void SpawnBalloon()
    {
        // Random prefab and spawn point
        GameObject prefab = _allObjects[Random.Range(0, _allObjects.Length)];
        Transform spawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Length)];

        // Random horizontal offset
        Vector3 offset = new Vector3(
            Random.Range(-_maxOffset, _maxOffset),
            Random.Range(-_minOffset, _minOffset), // slight vertical variation at start
            0f
        );

        GameObject balloon = Instantiate(prefab, spawnPoint.position + offset, Quaternion.identity, transform);
        balloon.transform.localScale = Vector3.zero;
       
        // Pop animation
        balloon.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

        // Move upward with base distance + random extra
        float randomExtra = Random.Range(0f, _randomDistanceOffset);
        Vector3 startPos = balloon.transform.position;
        Vector3 endPos = startPos + Vector3.up * (_baseMoveDistance + randomExtra);

        // Animate using AnimationCurve
         Tween tween =  DOTween.To(
            () => 0f,
            t =>
            {
                float evaluated = _curve.Evaluate(t);
                balloon.transform.position = Vector3.Lerp(startPos, endPos, evaluated);
            },
            1f,
            _moveDuration
        ).SetDelay(0.15f).OnComplete(() => Destroy(balloon));
        balloon.GetComponent<ConfettiBallon>().Init(_moveDuration,tween);
    }

    private void Update()
    {
        _baseMoveDistance = Screen.height * (screenDistancePer/100);

    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<OnCelebrationAchievementSignal>(OnCelebrationAchievementReceived);
    }
}


public class OnCelebrationAchievementSignal: ISignal{}