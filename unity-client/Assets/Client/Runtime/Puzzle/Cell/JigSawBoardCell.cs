using System.Collections.Generic;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigsawBoardCell : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _particleSystem;

        private const float HeightFactor = 0.00015f;

        private readonly Stack<JigsawPiece> _stack = new();
        private JigsawBoard _board;
        private ICellActionData _actionData;
        private bool _once = true;

        public int Idx { get; private set; }
        public Vector3 Size { get; private set; }
        public bool IsLocked { get; private set; }
        public IEnumerable<JigsawPiece> AllPieces => _stack;
        public bool HasAnyPiece => _stack.Count > 0;

        public bool Contains(JigsawPiece piece) => _stack.Contains(piece);

        public void SetData(int idx, Vector3 size, JigsawBoard board, ICellActionData actionData)
        {
            Idx = idx;
            Size = size;
            _board = board;
            _actionData = actionData;
        }

        public bool Push(JigsawPiece piece)
        {
            var newY = GetNextHeight();
            _stack.Push(piece);
            SetHeight(piece, newY);

            if (piece.CorrectIdx == Idx)
            {
                IsLocked = true;
                if (_once)
                {
                    _particleSystem.Play();
                    CellActionProcessor.Process(_actionData);
                    _once = false;
                }
                return true;
            }

            return false;
        }

        public JigsawPiece TryPop()
        {
            if (_stack.TryPop(out var result))
            {
                if (result.CorrectIdx == Idx) IsLocked = false;
                return result;
            }
            return null;
        }

        public JigsawPiece GetCorrectPiece() => _board.Pieces[Idx];

        public float GetNextHeight() => _stack.Count * HeightFactor;

        private void SetHeight(JigsawPiece piece, float height)
        {
            var pTransform = piece.transform;
            pTransform.position = new Vector3(pTransform.position.x, height, pTransform.position.z);
        }
    }
}