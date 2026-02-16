using UnityEngine;

[CreateAssetMenu(fileName = "SoundData", menuName = "Scriptable Objects/SoundData")]
public class SoundData : ScriptableObject
{
    public AudioData[] AudioData;
    public AudioData[] BgsSoundData;
}
[System.Serializable]
public class AudioData
{
    public AudioType ClipName;
    public float Volume = 1;
    public AudioClip audio;
}