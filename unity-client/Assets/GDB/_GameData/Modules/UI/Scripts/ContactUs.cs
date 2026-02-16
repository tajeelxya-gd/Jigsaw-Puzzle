using System;
using UnityEngine;
using UnityEngine.UI;

public class ContactUs : MonoBehaviour
{
    [SerializeField] private Button _contactUsButton;
    private string _emailAddress = "kokozonegames@gmail.com";
    private void Start()
    {
        _contactUsButton.onClick.AddListener(OnContactUsClicked);
    }

    private void OnContactUsClicked()
    {
        AudioController.PlaySFX(AudioType.ButtonClick);
        HapticController.Vibrate(HapticType.Btn);
        string url = "mailto:" + _emailAddress;
        Application.OpenURL(url);
    }
}
