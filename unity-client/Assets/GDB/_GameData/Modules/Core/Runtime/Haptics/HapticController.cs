using Lofelt.NiceVibrations;
using UniTx.Runtime;
using UnityEngine;
public static class HapticController
{
    private static IVibrationService _vibrationService;

    private static bool _supportAdvanceHaptics;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void Initialize()
    {
        if (_vibrationService == null)
            _vibrationService = new VibrationService();

        if (Lofelt.NiceVibrations.HapticController.Init())
            _supportAdvanceHaptics = true;
        UniStatics.LogInfo("Device Vibration " + Lofelt.NiceVibrations.HapticController.Init());
    }

    public static void Vibrate(HapticPatterns.PresetType presetType)
    {
        if (!GlobalService.GameData.Data.HapticsOn) return;
        if (!_supportAdvanceHaptics) return;
        _vibrationService.Vibrate(presetType);
    }
    public static void Vibrate(HapticType hapticType)
    {
        if (!GlobalService.GameData.Data.HapticsOn) return;
        if (!_supportAdvanceHaptics) return;
        _vibrationService.Vibrate(hapticType);
    }
    public static void VibrateForcefully(HapticType hapticType)
    {
        if (!GlobalService.GameData.Data.HapticsOn) return;
        _vibrationService.Vibrate(hapticType);
    }

    public class VibrationService : IVibrationService
    {
        private HapticData _hapticData;

        public VibrationService()
        {
            if (_hapticData == null)
                LoadHapticData();
        }

        public void Vibrate(HapticPatterns.PresetType presetType)
        {
            Lofelt.NiceVibrations.HapticPatterns.PlayPreset(presetType);
        }

        public void Vibrate(HapticType hapticType)
        {
            if (_hapticData == null)
                LoadHapticData();

            Lofelt.NiceVibrations.HapticController.fallbackPreset = HapticPatterns.PresetType.LightImpact;
            Lofelt.NiceVibrations.HapticController.Play(_hapticData.GetClip(hapticType));
        }

        private void LoadHapticData()
        {
            _hapticData = Resources.Load<HapticData>("HapticData");
        }
    }
}