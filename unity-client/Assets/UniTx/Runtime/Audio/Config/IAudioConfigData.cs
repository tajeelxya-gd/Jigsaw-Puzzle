using UnityEngine;
using UnityEngine.Audio;

namespace UniTx.Runtime.Audio
{
    public interface IAudioConfigData
    {
        AudioClip Clip { get; }

        float Volume { get; }

        float Pitch { get; }

        bool Loop { get; }

        float SpatialBlend { get; set; }

        float MinDistance { get; }

        float MaxDistance { get; }

        AudioMixerGroup MixerGroup { get; }
    }
}