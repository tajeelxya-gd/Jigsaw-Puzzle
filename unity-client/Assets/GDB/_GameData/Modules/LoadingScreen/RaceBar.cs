using System;
using TMPro;
using UnityEngine;

public class RaceBar : MonoBehaviour
{
    [SerializeField] private GameObject _bar;
    [SerializeField] private TextMeshProUGUI _rank;
    [SerializeField] private TextMeshProUGUI _gamesWon;
    [SerializeField] private GameObject _user;
    private IGetRank _userRank;
    private CarData _playerCarData;
    private DataBaseService<CarData> _carDataService;

    private void Start()
    {
        SignalBus.Subscribe<OnRaceEventStartSignal>(OpenBarPanel);
        SignalBus.Subscribe<OnRaceEventEndSignal>(CloseBarPanel);
        _userRank=_user.GetComponent<IGetRank>();
        _carDataService = new DataBaseService<CarData>();
        _playerCarData = _carDataService.Load_Get();
        UpdateRaceBar();
    }

    private void UpdateRaceBar()
    {
        _gamesWon.text=_playerCarData._playerWins.ToString()+"/5";
        int playerRankValue = (int)_userRank.GetRank();
        switch (playerRankValue)
        {
            case 1:
                _rank.text = "1st";
                break;
            case 2:
                _rank.text = "2nd";
                break;
            case 3:
                _rank.text = "3rd";
                break;
            default:
                _rank.text = playerRankValue + "th";
                break;
        }
    }

    private void OpenBarPanel(OnRaceEventStartSignal signal)
    {
        _bar.SetActive(true);
        UpdateRaceBar();
    }
    
    private void CloseBarPanel(OnRaceEventEndSignal signal)
    {
        _bar.SetActive(false);
    }

    private void OnDestroy()
    {
        SignalBus.Unsubscribe<OnRaceEventStartSignal>(OpenBarPanel);
        SignalBus.Unsubscribe<OnRaceEventEndSignal>(CloseBarPanel);
    }
}
