using UniTx.Runtime.Events;
using UnityEngine;

namespace Client.Runtime
{
    public readonly struct CurrencyCollectHudEffectEvent : IEvent
    {
        public readonly string ImageKey;
        public readonly Vector3 WorldPos;

        public CurrencyCollectHudEffectEvent(string imageKey, Vector3 worldPos)
        {
            ImageKey = imageKey;
            WorldPos = worldPos;
        }
    }
}