using Lofelt.NiceVibrations;
using UnityEngine;

[CreateAssetMenu(fileName = "HapticData", menuName = "Scriptable Objects/HapticData")]
public class HapticData : ScriptableObject
{
    [SerializeField] private Haptic[] _haptics;

    public HapticClip GetClip(HapticType hapticType)
    {
        for (int i = 0; i < _haptics.Length; i++)
        {
            if (_haptics[i].hapticType == hapticType)
                return _haptics[i].hapticClip;
        }
        return _haptics[0].hapticClip;
    }

    [System.Serializable]
    public class Haptic
    {
        public HapticType hapticType;
        public HapticClip hapticClip;
    }
}