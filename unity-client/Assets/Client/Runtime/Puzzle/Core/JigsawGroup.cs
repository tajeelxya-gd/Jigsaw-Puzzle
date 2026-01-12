using System.Collections.Generic;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigsawGroup : HashSet<JigsawPiece>
    {
        public JigsawGroup() : base()
        {
            // Empty
        }

        public JigsawGroup(IEnumerable<JigsawPiece> pieces) : base(pieces)
        {
            // Empty
        }

        public void Join(JigsawGroup other)
        {
            if (this == other) return;

            var groupToKeep = Count >= other.Count ? this : other;
            var groupToDiscard = (groupToKeep == this) ? other : this;

            foreach (var piece in groupToDiscard)
            {
                groupToKeep.Add(piece);
                piece.Group = groupToKeep;
            }

            foreach (var piece in groupToKeep)
            {
                piece.PlayVfx();
            }
        }

        public void Move(Vector3 delta)
        {
            foreach (var piece in this)
            {
                piece.transform.position += delta;
            }
        }

        public void SetPosY(float y)
        {
            foreach (var piece in this)
            {
                var pieceTransform = piece.transform;
                pieceTransform.position = new Vector3(pieceTransform.position.x, y, pieceTransform.position.z);
            }
        }

        public void Lock()
        {
            foreach (var piece in this)
            {
                piece.LockPiece();
            }
            SetPosY(0f);
        }
    }
}