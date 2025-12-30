using System;
using UnityEngine;

namespace Client.Runtime
{

    [Serializable]
    public class NeighbourPiece
    {
        [SerializeField] private string _id;
        [SerializeField] private PlacementDirection _placement;

        // Public Getters
        public string Id => _id;
        public PlacementDirection Placement => _placement;
    }
}