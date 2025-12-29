using System;
using UnityEngine;
using UnityEngine.Audio;
using UniTx.Runtime.Pool;

namespace UniTx.Runtime.Audio
{
    [Serializable]
    internal sealed class UniAudioConfigData : IAudioConfigData, IPoolItemData
    {
        [Header("Clip")]
        [SerializeField] private AudioClip _clip;

        [Header("Settings")]
        [SerializeField, Range(0f, 1f)] private float _volume = 1f;
        [SerializeField, Range(-3f, 3f)] private float _pitch = 1f;
        [SerializeField] private bool _loop;

        [Header("3D Settings")]
        [SerializeField] private float _minDistance = 1f;
        [SerializeField] private float _maxDistance = 20f;

        [Header("Mixer")]
        [SerializeField] private AudioMixerGroup _mixerGroup;

        public AudioClip Clip => _clip;

        public float Volume => _volume;

        public float Pitch => _pitch;

        public bool Loop => _loop;

        public float SpatialBlend { get; set; } = 0f;

        public float MinDistance => _minDistance;

        public float MaxDistance => _maxDistance;

        public AudioMixerGroup MixerGroup => _mixerGroup;

        public Transform ToFollow { get; set; } = null;
    }
}