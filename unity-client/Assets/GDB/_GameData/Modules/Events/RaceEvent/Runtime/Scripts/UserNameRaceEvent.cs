using System;
using TMPro;
using UnityEngine;

public class UserNameRaceEvent : MonoBehaviour
{
    private DataBaseService<PlayerProfileData> _profileDataBase;
    private PlayerProfileData _playerProfileData;
    [SerializeField] private TextMeshProUGUI _userNameText;
    
    private void Awake()
    {
        _profileDataBase = new DataBaseService<PlayerProfileData>();
        _playerProfileData = _profileDataBase.Load_Get();
    }

    private void OnEnable()
    {
        SetUserName();
    }

    private void SetUserName()
    {
        _userNameText.text = _playerProfileData._playerName;
    }
}
