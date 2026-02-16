using System;
using UnityEngine;
using UnityEngine.UI;

public class FailBundleButtons : MonoBehaviour
{
    [SerializeField] private Button _bundle1Button;
    [SerializeField] private Button _bundle2Button;
    [SerializeField] private Sprite _greenButton;
    [SerializeField] private Sprite _blankButton;
    private Image _button1Image;
    private Image _button2Image;

    private void Start()
    {
        SignalBus.Subscribe<OnBundleImageUpdate>(UpdateImages);
        _button1Image = _bundle1Button.gameObject.GetComponent<Image>();
        _button2Image = _bundle2Button.gameObject.GetComponent<Image>();
        RegisterButtons();
    }

    private void OnEnable()
    {
        SignalBus.Subscribe<OnBundleImageUpdate>(UpdateImages);
    }

    private void RegisterButtons()
    {
        _bundle1Button.onClick.AddListener(OnClickButton1);
        _bundle2Button.onClick.AddListener(OnClickButton2);
    }

    private void OnClickButton1()
    {
        UpdateImagesForButton1();
        SignalBus.Publish(new OnBundleButtonClick{_bundleIndex = 0});
    }

    private void OnClickButton2()
    {
        UpdateImagesForButton2();
        SignalBus.Publish(new OnBundleButtonClick{_bundleIndex = 1});
    }

    private void UpdateImagesForButton1()
    {
        _button1Image.sprite = _greenButton;
        _button2Image.sprite = _blankButton;
    }
    
    private void UpdateImagesForButton2()
    {
        _button1Image.sprite = _blankButton;
        _button2Image.sprite = _greenButton;
    }

    private void UpdateImages(OnBundleImageUpdate signal)
    {
        if(signal._bundleImageIndex == 0)
        {
            UpdateImagesForButton1();
        }
        else if(signal._bundleImageIndex == 1)
        {
            UpdateImagesForButton2();
        }
    }

    private void OnDisable()
    {
        SignalBus.Unsubscribe<OnBundleImageUpdate>(UpdateImages);
    }
}