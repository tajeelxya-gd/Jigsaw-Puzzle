using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinFailManager : MonoBehaviour
{
    [SerializeField] private GameObject _user;
    [SerializeField] private GameObject[] _ai;
    [SerializeField] private GameObject _rewardButton;
    [SerializeField] private GameObject _tryAgainButton;
    [SerializeField] private Sprite _userBox;
    [SerializeField] private Sprite _aiBox;
    [SerializeField] private Sprite[] _playerProfileSprites;
    [SerializeField] private Image[] _boxes;
    [SerializeField] private Image[] _leaderBoardImages;
    [SerializeField] private TextMeshProUGUI[] _playerName;
    [SerializeField] private TextMeshProUGUI[] _rankText;

    private DataBaseService<PlayerProfileData> _profileDataBase;
    private PlayerProfileData _playerProfileData;
    private IGetRank _getRank;
    private IGetRank[] _getAIRank;
    private void OnEnable()
    {
        _profileDataBase = new DataBaseService<PlayerProfileData>();
        _playerProfileData = _profileDataBase.Load_Get();
        _getRank = _user.GetComponent<IGetRank>();
        int playerRankValue = (int)_getRank.GetRank();
        if (playerRankValue >= 1 && playerRankValue <= 3)
        {
            _rewardButton.SetActive(true);
            _tryAgainButton.SetActive(false);
        }
        else
        {
            _rewardButton.SetActive(false);
            _tryAgainButton.SetActive(true);
        }
        _getAIRank = new IGetRank[_ai.Length];
        for (int i = 0; i < _ai.Length; i++)
        {
            _getAIRank[i] = _ai[i].GetComponent<IGetRank>();
        }
        ApplyRank(_getRank.GetRank());
        GameAnalytics.PublishAnalytic(AnalyticEventType.GameData, "Events", nameof(AnalyticEventType.RaceEvent), "End", ((int)_getRank.GetRank()).ToString());
    }

    private void OnDisable()
    {
        SignalBus.Publish(new OnRaceEventEndSignal());
        //Debug.LogError("Signal Published");
    }

    void PublishOnMissionCompleteSignal()
    {
        SignalBus.Publish(new OnMissionObjectiveCompleteSignal { MissionType = MissionType.WinTinyRace, Amount = 1 });
    }

    private void SetPlayerName(int index)
    {
        _playerName[index].text = _playerProfileData._playerName;
        _playerName[index].color = Color.white;
        _rankText[index].color = Color.white;
    }

    private void SetPlayerProfileImage(int index)
    {
        _leaderBoardImages[index].sprite = _playerProfileSprites[_playerProfileData._pictureIndex];
    }

    private void ApplyRank(Ranks playerRank)
    {
        int rankValue = (int)playerRank;
        if (rankValue - 1 == 0)
        {
            SignalBus.Publish(new RaceEventResultSignal { _hasWin = true, _rank = (int)_getRank.GetRank() });
            AudioController.PlaySFX(AudioType.RaceWin);
        }
        else
        {
            SignalBus.Publish(new RaceEventResultSignal { _hasWin = false, _rank = (int)_getRank.GetRank() });
            AudioController.PlaySFX(AudioType.RaceFail);
        }

        for (int i = 0; i < _boxes.Length; i++)
        {
            if (i == rankValue - 1) //player wins here
            {
                _boxes[i].sprite = _userBox;
                SetPlayerName(i);
                SetPlayerProfileImage(i);
                PublishOnMissionCompleteSignal();
            }
            else
            {
                _boxes[i].sprite = _aiBox;
                string aiName = "AI";
                Sprite aiProfileSprite = null;
                for (int j = 0; j < _ai.Length; j++)
                {
                    if ((int)_getAIRank[j].GetRank() == i + 1)
                    {
                        IAIRacerName aiRacerName = _ai[j].GetComponent<IAIRacerName>();
                        IAiProfilePicture aiProfilePicture = _ai[j].GetComponent<IAiProfilePicture>();
                        if (aiRacerName != null)
                        {
                            aiName = aiRacerName.GetName();
                            aiProfileSprite = aiProfilePicture.GetProfilePicture();
                            break;
                        }
                    }
                }
                _playerName[i].text = aiName;
                _leaderBoardImages[i].sprite = aiProfileSprite;
            }
        }
    }
}

public class OnRaceEventEndSignal : ISignal
{
}

public class RaceEventResultSignal : ISignal
{
    public bool _hasWin;
    public int _rank;
}