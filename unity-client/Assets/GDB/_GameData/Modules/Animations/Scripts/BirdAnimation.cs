using System;
using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class BirdAnimation : MonoBehaviour
{
    [SerializeField] private Sprite[] _birdAnimation;
    [SerializeField] private float _timer = 0.1f;
    [SerializeField] private Image _bird;
    [SerializeField] private RectTransform _startPosition;
    [SerializeField] private RectTransform _endPosition;
    [SerializeField] private float _duration;
    [SerializeField] private bool playLoopOnAwake = false;
    private WaitForSeconds _wait;

    private RectTransform _rect;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _wait = new WaitForSeconds(_timer);
    }

    private void Start()
    {
        if (playLoopOnAwake) AlwaysPlay();
    }

    [Button]
    public void PlayAnimation()
    {
        StopAllCoroutines();
        Reset();
        _rect.DOAnchorPos(_endPosition.anchoredPosition, _duration);
        StartCoroutine(PlaySpriteAnimation());
    }

     void AlwaysPlay()
    {
        StartCoroutine(PlaySpriteAnimation(true));

    }

    private IEnumerator PlaySpriteAnimation()
    {
        for (int i = 0; i < _birdAnimation.Length; i++)
        {
            _bird.sprite = _birdAnimation[i];
            yield return _wait;
        }
    }
    
    private IEnumerator PlaySpriteAnimation(bool loop)
    {
        while (true)
        {
            for (int i = 0; i < _birdAnimation.Length; i++)
            {
                _bird.sprite = _birdAnimation[i];
                yield return _wait;
            }
            if(!loop) yield break;
        }
    
    }

    [Button]
    private void Reset()
    {
        _rect.anchoredPosition = _startPosition.anchoredPosition;
    }
}