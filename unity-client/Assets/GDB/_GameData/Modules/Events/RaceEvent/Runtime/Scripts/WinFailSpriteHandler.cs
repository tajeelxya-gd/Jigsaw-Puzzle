using System;
using TMPro;
using UnityEngine;

public class WinFailSpriteHandler : MonoBehaviour
{
    [SerializeField] private GameObject _winPanel;
    [SerializeField] private GameObject _failPanel;
    [SerializeField] private TextMeshProUGUI _rankText;

    private void Start()
    {
        SignalBus.Subscribe<RaceEventResultSignal>(ChangeSprite);
    }

    private void ChangeSprite(RaceEventResultSignal signal)
    {
        if (signal._hasWin)
        {
            _failPanel.SetActive(false);
            _winPanel.SetActive(true);
            
        }
        else
        {
            _failPanel.SetActive(true);
            _winPanel.SetActive(false);
            _rankText.text = signal._rank.ToString();
        }
    }

    private void OnDisable()
    {
        SignalBus.Unsubscribe<RaceEventResultSignal>(ChangeSprite);
    }
}