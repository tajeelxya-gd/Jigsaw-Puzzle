using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class VibratingImage : MonoBehaviour,IMovable
{
    [Header("Settings")]
    [SerializeField] private RectTransform _imageRect; 
    [SerializeField] private float _vibrationDuration = 0.2f; 
    [SerializeField] private float _vibrationStrength = 10f;  
    [SerializeField] private int _vibrationLoops = 5;      
    [SerializeField] private float _repeatInterval = 5f;
    private Vector2 _originalPos;

    private void Awake()
    {
        if (_imageRect == null)
            _imageRect = GetComponent<RectTransform>();

        _originalPos = _imageRect.anchoredPosition;
    }

    private void StartVibration()
    {
        StartCoroutine(VibrationLoop());
    }

    private void OnDisable()
    {
        StopCoroutine(VibrationLoop());
    }

    private IEnumerator VibrationLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(_repeatInterval);
            Vibrate();
        }
    }

    private void Vibrate()
    {
        _imageRect.DOShakeAnchorPos(_vibrationDuration, _vibrationStrength, _vibrationLoops, 90, false, true)
            .OnComplete(() => _imageRect.anchoredPosition = _originalPos);
    }

    public void Move()
    {
        StartVibration();
    }
}