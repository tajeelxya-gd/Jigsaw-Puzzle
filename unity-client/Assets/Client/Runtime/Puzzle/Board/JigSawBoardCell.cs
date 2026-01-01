using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigsawBoardCell : MonoBehaviour
    {
        private int _idx;

        public int Idx => _idx;

        public void SetIdx(int idx) => _idx = idx;
    }
}