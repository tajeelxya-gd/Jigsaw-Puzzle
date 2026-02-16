using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerName : MonoBehaviour
{
    [SerializeField] private TMP_InputField _nameInputField;

    public string GetPlayerName()
    {
            return _nameInputField.text;
    }
}

[Serializable]
public class PlayerProfileData
{
    public string _playerName = "Player";
    public int _pictureIndex=0;
}
