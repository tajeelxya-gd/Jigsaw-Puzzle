using System;
using UnityEngine;

namespace Client.Runtime
{

    [Serializable]
    public sealed class NeighbourPiece
    {
        [SerializeField] private string _id;
        [SerializeField] private string _placement;

        public string Id => _id;
        public PlacementDirection Placement => Enum.TryParse<PlacementDirection>(_placement, out var result) ? result : default;
    }
}