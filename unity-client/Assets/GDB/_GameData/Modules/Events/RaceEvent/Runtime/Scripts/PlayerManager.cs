using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class PlayerManager : MonoBehaviour, IGetWins
{
    [SerializeField] private CarMovement _carMovement;
    [SerializeField] private RaceProgressBar _raceProgressBar;
    [SerializeField] private TextMeshProUGUI _winsText;
    [SerializeField] private int _totalWinsToWin = 5;
    private int _playerWins = 0;
    private DataBaseService<CarData> _carDataService;
    private CarData _carData;
    public bool CanAct { get; set; } = false;
    private void Start()
    {
        _carDataService = new DataBaseService<CarData>();
        _carData = _carDataService.Load_Get();
        if (_carData != null)
        {
            _playerWins = _carData._playerWins;
            _carMovement.SetCarProgression(_carData._raceProgress);
            _raceProgressBar.SetCurrentProgress(_carData._currentProgress);
            UpdateText();
        }
        SignalBus.Subscribe<LevelWin>(OnLevelWin);
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<LevelWin>(OnLevelWin);
    }

    private void OnLevelWin(LevelWin signal)
    {
        if (!CanAct) return;
        _playerWins++;
        UpdateText();
        _carMovement.MoveCar(0.15f);
        _carData._raceProgress = _carMovement.GetCurrentRaceProgress();
        _raceProgressBar.UpdateFillBar(_playerWins);
        _carData._playerWins = _playerWins;
        _carData._currentProgress = _raceProgressBar.GetCurrentProgress();
        _carDataService.Save(_carData);
        SignalBus.Publish(new WinCheck());
    }
    
    private void UpdateText()
    {
        _winsText.text = _playerWins.ToString() + "/" + _totalWinsToWin.ToString();
    }
    
    public int GetWins()
    {
        return _playerWins;
    }
    [Button]
    public void Reset()
    {
        _playerWins = 0;
        _carMovement.Reset();
        _raceProgressBar.Reset();
        _carData._raceProgress = 0.25f;
        _carData._playerWins = 0;
        _carData._currentProgress = 0.25f;
        _carDataService.Save(_carData);
        UpdateText();
    }

    [Button]
    public void TestPlayer()
    {
        SignalBus.Publish(new LevelWin());
    }
}

public class LevelWin : ISignal
{
}

public class CarData
{
    public int _playerWins;
    public float _currentProgress;
    public float _raceProgress;
}

public interface IGetWins
{
    int GetWins();
}
