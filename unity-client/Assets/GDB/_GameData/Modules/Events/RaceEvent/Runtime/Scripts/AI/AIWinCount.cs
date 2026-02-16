using System;
using TMPro;
using UnityEngine;

public class AIWinCount : MonoBehaviour, IGetWins, IAIWinCount, IUpdateText
{
    [SerializeField] private int _aiWins = 0;
    [SerializeField] private TextMeshProUGUI _aiWinsText;
    [SerializeField] private int _totalWinsToWin = 5;

    public void AddWin()
    {
        _aiWins = Mathf.Clamp(_aiWins + 1, 0, _totalWinsToWin);
        UpdateText();
    }

    public void UpdateText()
    {
        _aiWinsText.text = _aiWins.ToString() + "/" + _totalWinsToWin.ToString();
    }

    public int GetAIWins()
    {
        return _aiWins;
    }
    
    public void SetWins(int wins)
    {
        _aiWins = Mathf.Clamp(wins, 0, _totalWinsToWin);
        UpdateText();
    }

    public int GetWins()
    {
        return _aiWins;
    }

    public void Reset()
    {
        _aiWins = 0;
        UpdateText();
    }
}

public interface IAIWinCount
{
    public int GetAIWins();
    public void SetWins(int wins);
    public void AddWin();
}

public interface IUpdateText
{
    public void UpdateText();
}