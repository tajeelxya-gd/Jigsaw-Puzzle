using System.Collections.Generic;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class JoinController : MonoBehaviour
    {
        public enum ConnectionType { Simple, Join }

        [SerializeField] private Join _top;
        [SerializeField] private Join _bottom;
        [SerializeField] private Join _left;
        [SerializeField] private Join _right;

        [Header("Settings")]
        [Range(0.1f, 0.5f)]
        [SerializeField] private float _tabScaleRatio = 0.25f; // Tab is 25% of the side length

        public void Init(BoxCollider main, JigsawBoardCell[] neighbours, JigSawPiece piece)
        {
            Vector3 mSize = main.size;
            Vector3 mCenter = main.center;

            // The thickness (Y) must always match the main piece
            float thickness = mSize.y;

            // TOP & BOTTOM (Horizontal edges)
            // We want the tab to be a 'square' based on a ratio of the width
            float tbWidth = mSize.x * _tabScaleRatio;
            float tbDepth = tbWidth; // Keeps the tab square

            Vector3 tbSize = new Vector3(tbWidth, thickness, tbDepth);
            _top.BoxCollider.size = tbSize;
            _bottom.BoxCollider.size = tbSize;

            // Position them exactly on the edge (Z axis)
            _top.BoxCollider.center = new Vector3(mCenter.x, mCenter.y, mCenter.z + (mSize.z / 2f));
            _bottom.BoxCollider.center = new Vector3(mCenter.x, mCenter.y, mCenter.z - (mSize.z / 2f));

            // LEFT & RIGHT (Vertical edges)
            // We want the tab to be a 'square' based on a ratio of the height (Z in your view)
            float lrHeight = mSize.z * _tabScaleRatio;
            float lrDepth = lrHeight; // Keeps the tab square

            Vector3 lrSize = new Vector3(lrDepth, thickness, lrHeight);
            _left.BoxCollider.size = lrSize;
            _right.BoxCollider.size = lrSize;

            // Position them exactly on the edge (X axis)
            _left.BoxCollider.center = new Vector3(mCenter.x - (mSize.x / 2f), mCenter.y, mCenter.z);
            _right.BoxCollider.center = new Vector3(mCenter.x + (mSize.x / 2f), mCenter.y, mCenter.z);

            _top.Init(neighbours[0], piece);
            _bottom.Init(neighbours[1], piece);
            _left.Init(neighbours[2], piece);
            _right.Init(neighbours[3], piece);
        }
    }
}