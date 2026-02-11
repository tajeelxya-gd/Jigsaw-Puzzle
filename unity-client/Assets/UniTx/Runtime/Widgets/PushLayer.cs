using UnityEngine;
using System;

namespace UniTx.Runtime.Widgets
{
    [Serializable]
    public enum PushLayer
    {
        Background,
        HUD,
        Widgets,
        Overlay
    }

    [Serializable]
    internal sealed class PushLayerTransform
    {
        [SerializeField] private Transform _transform;
        [SerializeField] private PushLayer _layer;

        public Transform Transform => _transform;
        public PushLayer Layer => _layer;
    }
}