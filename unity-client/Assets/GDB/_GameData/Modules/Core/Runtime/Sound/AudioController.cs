using UnityEngine;

public static class AudioController
{
    private static IAudioService _audioService;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        _audioService = new AudioService();
    }

    public static void PlaySFX(AudioType audioType, float vol = 1, float Pitch = 1) { _audioService.PlaySFX(audioType, vol, Pitch); }
    public static void PlaySFX(AudioClip clip, float vol = 1, float Pitch = 1) { _audioService.PlaySFX(clip, vol, Pitch); }
    public static void PlayOnDemandSFX(AudioType audioType, float vol = 1) { _audioService.PlayOnDemandSFX(audioType, vol); }
    public static void PlayBG(AudioType audioType, float vol = 1) { _audioService.PlayBG(audioType, vol); }
    public static void StopBG() { _audioService.StopBG(); }
    public static void ChangeBGVolumeLevel(float vol) { _audioService.ChangeBGVolumeLevel(vol); }
}