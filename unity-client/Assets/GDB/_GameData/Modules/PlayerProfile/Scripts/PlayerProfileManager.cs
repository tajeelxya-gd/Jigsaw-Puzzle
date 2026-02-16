using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerProfileManager : MonoBehaviour
{
    [SerializeField] private GameObject _profilePanel;
    [SerializeField] private PlayerPicture _playerPicture;
    [SerializeField] private Image[] _profileImage;
    [SerializeField] private PlayerName _playerName;
    [SerializeField] private TextMeshProUGUI[] _nameDisplayText;
    [SerializeField] private GameObject[] _greenBg;
    [SerializeField] private GameObject[] _ticks;
    [SerializeField] private Button _openProfileButton;
    private int _currentPictureIndex = 0;
    private DataBaseService<PlayerProfileData> _profileDataBase;
    private PlayerProfileData _playerProfileData;
    private PanelScaling _scaling;
    private void Awake()
    {
        _profileDataBase = new DataBaseService<PlayerProfileData>();
        SignalBus.Subscribe<PlayerProfilePanelOpenSignal>(LoadNameAndPicture);
        _scaling = _profilePanel.GetComponent<PanelScaling>();
    }

    private void Start()
    {
        LoadNameAndPicture(null);
        _openProfileButton.onClick.RemoveAllListeners();
        _openProfileButton.onClick.AddListener(OpenPanel);
    }

    private void LoadNameAndPicture(PlayerProfilePanelOpenSignal signal)
    {
        _playerProfileData = _profileDataBase.Load_Get();

        if (_playerProfileData == null)
        {
            _playerProfileData = new PlayerProfileData();
            _playerProfileData._pictureIndex = 0;
            _playerProfileData._playerName = "";
        }

        _currentPictureIndex = _playerProfileData._pictureIndex;
        for (int i = 0; i < _profileImage.Length; i++)
        {
            _profileImage[i].sprite = _playerPicture.GetSprite(_currentPictureIndex);
        }
        ChangeBackground();
        for (int i = 0; i < _nameDisplayText.Length; i++)
        {
            _nameDisplayText[i].text = _playerProfileData._playerName;
        }
    }

    public void ChangePicture(int index)
    {
        AudioController.PlaySFX(AudioType.ButtonClick);
        _currentPictureIndex = index;
        for (int i = 0; i < _profileImage.Length; i++)
        {
            _profileImage[i].sprite = _playerPicture.GetSprite(index);
        }
        ChangeBackground();
    }

    private void ChangeBackground()
    {
        for (int x = 0; x < _greenBg.Length; x++)
        {
            if (x == _currentPictureIndex)
            {
                _greenBg[x].SetActive(true);
                _ticks[x].SetActive(true);
            }
            else
            {
                _greenBg[x].SetActive(false);
                _ticks[x].SetActive(false);
            }
        }
    }

    public void ChangeName()
    {
        _playerProfileData._playerName = _playerName.GetPlayerName();
        _profileDataBase.Save(_playerProfileData);
        for (int i = 0; i < _nameDisplayText.Length; i++)
        {
            _nameDisplayText[i].text = _playerProfileData._playerName;
        }
        SignalBus.Publish(new OnPlayerNameChange());
    }

    public void SaveButton()
    {
        _playerProfileData._pictureIndex = _currentPictureIndex;
        _profileDataBase.Save(_playerProfileData);
        StartCoroutine(ClosingPanel());
    }

    private IEnumerator ClosingPanel()
    {
        AudioController.PlaySFX(AudioType.ButtonClick);
        HapticController.Vibrate(HapticType.Btn);
        _scaling.ScaleOut();
        yield return new WaitForSeconds(0.5f);
        _profilePanel.SetActive(false);
    }

    void OpenPanel()
    {
        _profilePanel.SetActive(true);
        AudioController.PlaySFX(AudioType.ButtonClick);
        HapticController.Vibrate(HapticType.Btn);
    }
}

public class OnPlayerNameChange : ISignal { }