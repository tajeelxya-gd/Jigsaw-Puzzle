using System;
using UniTx.Runtime.Content;
using UnityEngine;

namespace Client.Runtime
{
    [Serializable]
    public class JigSawPieceData : IData
    {
        [SerializeField] private string _id;
        [SerializeField] private int _placementOnBoard;
        [SerializeField] private NeighbourPiece[] _neighbourPieces;

        public string Id => _id;
        public int PlacementOnBoard => _placementOnBoard;
        public NeighbourPiece[] NeighbourPieces => _neighbourPieces;
    }
}
