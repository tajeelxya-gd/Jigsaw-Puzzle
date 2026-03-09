using Client.Runtime;
using UnityEngine;

public class SettingManagerGameplay : SettingsManager
{
    private float _lastTimeScale;
    public override void OpenSettingPanel()
    {
        _lastTimeScale = Time.timeScale;
        Time.timeScale = 0;

        base.OpenSettingPanel();
        SignalBus.Publish(new InputRestrictSignal { restrict = true });
        InputHandler.SetActive(false);
    }
    public override void CloseSettingPanel()
    {
        Time.timeScale = _lastTimeScale;
        _settingPanel.SetActive(false);
        SignalBus.Publish(new InputRestrictSignal { restrict = false });
        InputHandler.SetActive(true);
    }
}