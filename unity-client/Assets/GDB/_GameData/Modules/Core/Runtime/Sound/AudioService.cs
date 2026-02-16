using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
public class AudioService : IAudioService
{
    private GameObject _audioRoot;
    AudioSource MusicSource;
    AudioSource SFXSource;
    AudioSource DemandSFXSource;
    private SoundData _soundData;

    private Dictionary<AudioType, AudioData> _sfxLookup;
    private Dictionary<AudioType, AudioData> _bgLookup;

    List<AudioSource> PooledAudioSources = new List<AudioSource>();

    int _poolQuantity = 10;
    int _currentPoolIndex = 0;
    public AudioService()
    {
        LoadSoundData();

        BuildLookupTables();

        _audioRoot = new GameObject("AudioServiceRoot");
        UnityEngine.Object.DontDestroyOnLoad(_audioRoot);

        MusicSource = CreateSource("MusicSource", _audioRoot.transform);
        SFXSource = CreateSource("SFXSource", _audioRoot.transform);
        DemandSFXSource = CreateSource("DemandSFXSource", _audioRoot.transform);
        for (int i = 0; i < _poolQuantity; i++)
        {
            AudioSource _pSfx = CreateSource($"PooledAudio_{i}", _audioRoot.transform);
            PooledAudioSources.Add(_pSfx);
        }
    }

    private void LoadSoundData()
    {
        _soundData = Resources.Load<SoundData>("SoundData");
    }

    private void BuildLookupTables()
    {
        _sfxLookup = new Dictionary<AudioType, AudioData>();
        _bgLookup = new Dictionary<AudioType, AudioData>();

        foreach (var data in _soundData.AudioData)
        {
            if (!_sfxLookup.ContainsKey(data.ClipName))
                _sfxLookup.Add(data.ClipName, data);
        }

        foreach (var data in _soundData.BgsSoundData)
        {
            if (!_bgLookup.ContainsKey(data.ClipName))
                _bgLookup.Add(data.ClipName, data);
        }
    }

    private AudioSource CreateSource(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        AudioSource source = go.AddComponent<AudioSource>();
        return source;
    }

    float musicDefaultVolu = 0;
    public void PlayBG(AudioType audioType, float vol = 1)
    {
        if (!GlobalService.GameData.Data.MusicOn) return;
        if (MusicSource == null) return;
        if (BGDataExists(audioType, out AudioData result))
        {
            MusicSource.volume = result.Volume;
            MusicSource.clip = result.audio;
            MusicSource.loop = true;
            MusicSource.time = 0;
            MusicSource.Play();
            musicDefaultVolu = MusicSource.volume;
        }
    }

    // private float _lastSfxTime;
    // private const float MinSfxInterval = 0.08f;
    // public void PlaySFX(AudioType audioType, float vol = 1, float spatialBlend = 0)
    // {
    //     if (!GlobalService.GameData.Data.SoundOn) return;
    //     if (Time.unscaledTime - _lastSfxTime < MinSfxInterval)
    //         return;

    //     _lastSfxTime = Time.unscaledTime;

    //     if (AudioDataExists(audioType, out AudioData result))
    //     {
    //         if (!DemandSFXSource) return;
    //         if (PooledAudioSources == null) return;

    //         // PooledAudioSources[_currentPoolIndex].volume = vol;
    //         // PooledAudioSources[_currentPoolIndex].clip = result.audio;
    //         // PooledAudioSources[_currentPoolIndex].time = 0;
    //         PooledAudioSources[_currentPoolIndex].pitch = Random.Range(0.95f, 1.05f);
    //         float spamDamp = Mathf.Clamp01((Time.time - _lastSfxTime) / 0.15f);
    //         float finalVolume = vol * Mathf.Lerp(0.7f, 1f, spamDamp);
    //         PooledAudioSources[_currentPoolIndex].PlayOneShot(result.audio, finalVolume);
    //         ++_currentPoolIndex;
    //         _currentPoolIndex %= PooledAudioSources.Count;
    //     }
    // }

    private float _lastHitSfxTime;
    private const float MinHitInterval = 0.08f;

    private float _lastSfxPlayTime;
    private const float SpamDampWindow = 0.15f;

    public void PlaySFX(AudioType audioType, float vol = 1, float spatialBlend = 1)
    {
        if (!GlobalService.GameData.Data.SoundOn) return;

        if (audioType == AudioType.Hit)
        {
            if (Time.unscaledTime - _lastHitSfxTime < MinHitInterval)
                return;

            _lastHitSfxTime = Time.unscaledTime;
        }

        if (!AudioDataExists(audioType, out AudioData result))
            return;

        if (PooledAudioSources == null || PooledAudioSources.Count == 0)
            return;

        AudioSource source = GetAvailableSource();

        source.pitch = spatialBlend;

        float timeSinceLast = Time.unscaledTime - _lastSfxPlayTime;
        float spamDamp = Mathf.Clamp01(timeSinceLast / SpamDampWindow);
        float finalVolume = vol * Mathf.Lerp(0.75f, 1f, spamDamp);

        source.PlayOneShot(result.audio, finalVolume);

        _lastSfxPlayTime = Time.unscaledTime;
    }

    public void PlaySFX(AudioClip audioClip, float vol = 1, float spatialBlend = 1)
    {
        if (!GlobalService.GameData.Data.SoundOn) return;

        if (audioClip == null)
        {
            if (Time.unscaledTime - _lastHitSfxTime < MinHitInterval)
                return;

            _lastHitSfxTime = Time.unscaledTime;
        }

        if (PooledAudioSources == null || PooledAudioSources.Count == 0)
            return;

        AudioSource source = GetAvailableSource();

        source.pitch = spatialBlend;

        float timeSinceLast = Time.unscaledTime - _lastSfxPlayTime;
        float spamDamp = Mathf.Clamp01(timeSinceLast / SpamDampWindow);
        float finalVolume = vol * Mathf.Lerp(0.75f, 1f, spamDamp);

        source.PlayOneShot(audioClip, finalVolume);

        _lastSfxPlayTime = Time.unscaledTime;
    }

    private AudioType _lastDemadAudioType;
    public void PlayOnDemandSFX(AudioType audioType, float vol = 1)
    {
        if (!GlobalService.GameData.Data.SoundOn) return;
        if (AudioDataExists(audioType, out AudioData result))
        {
            if (!SFXSource) return;
            if (SFXSource.isPlaying) return;

            // _lastDemadAudioType = audioType;
            SFXSource.volume = result.Volume;
            SFXSource.clip = result.audio;
            SFXSource.time = 0;
            SFXSource.Play();
        }
    }

    public void StopBG()
    {
        MusicSource.Stop();
        MusicSource.time = 0;
    }

    private bool AudioDataExists(AudioType type, out AudioData result)
    {
        return _sfxLookup.TryGetValue(type, out result);
    }

    private bool BGDataExists(AudioType type, out AudioData result)
    {
        return _bgLookup.TryGetValue(type, out result);
    }

    public void ChangeBGVolumeLevel(float vol)
    {
        float targetVolume = musicDefaultVolu * vol;
        MusicSource.DOKill();
        MusicSource.DOFade(targetVolume, 0.5f);
    }
    private AudioSource GetAvailableSource()
    {
        for (int i = 0; i < PooledAudioSources.Count; i++)
        {
            if (!PooledAudioSources[i].isPlaying)
                return PooledAudioSources[i];
        }
        AudioSource lowest = PooledAudioSources[0];
        for (int i = 1; i < PooledAudioSources.Count; i++)
        {
            if (PooledAudioSources[i].volume < lowest.volume)
                lowest = PooledAudioSources[i];
        }

        return lowest;
    }
}
