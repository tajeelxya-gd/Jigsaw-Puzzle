using System.Collections.Generic;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigsawBoardCell : MonoBehaviour
    {
        private readonly Stack<JigsawPiece> _stack = new();

        public int Idx { get; private set; }
        public Vector3 Size { get; private set; }
        public bool IsLocked { get; private set; }

        public void SetData(int idx, Vector3 size)
        {
            Idx = idx;
            Size = size;
        }

        public void Lock() => IsLocked = true;
    }
}