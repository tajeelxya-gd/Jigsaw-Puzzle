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
        [SerializeField] private float _tabScaleRatio = 0.25f;

        [Tooltip("Extra height added to the top and bottom of the joiner collider")]
        [SerializeField] private float _verticalExpansion = 0.1f;

        public void Init(BoxCollider main, JigsawBoardCell[] neighbours)
        {
            Vector3 mSize = main.size;
            Vector3 mCenter = main.center;

            // Apply expansion: size.y + (expansion * 2) if you want 'expansion' to be the amount added to EACH side
            float expandedThickness = mSize.y + (_verticalExpansion * 2f);

            // TOP & BOTTOM (Horizontal edges)
            float tbWidth = mSize.x * _tabScaleRatio;
            float tbDepth = tbWidth;

            // Using expandedThickness for the Y value
            Vector3 tbSize = new Vector3(tbWidth, expandedThickness, tbDepth);
            _top.BoxCollider.size = tbSize;
            _bottom.BoxCollider.size = tbSize;

            // Positions (Y remains mCenter.y so it expands equally up and down)
            _top.BoxCollider.center = new Vector3(mCenter.x, mCenter.y, mCenter.z + (mSize.z / 2f));
            _bottom.BoxCollider.center = new Vector3(mCenter.x, mCenter.y, mCenter.z - (mSize.z / 2f));

            // LEFT & RIGHT (Vertical edges)
            float lrHeight = mSize.z * _tabScaleRatio;
            float lrDepth = lrHeight;

            // Using expandedThickness for the Y value
            Vector3 lrSize = new Vector3(lrDepth, expandedThickness, lrHeight);
            _left.BoxCollider.size = lrSize;
            _right.BoxCollider.size = lrSize;

            _left.BoxCollider.center = new Vector3(mCenter.x - (mSize.x / 2f), mCenter.y, mCenter.z);
            _right.BoxCollider.center = new Vector3(mCenter.x + (mSize.x / 2f), mCenter.y, mCenter.z);

            _top.Init(neighbours[0]);
            _bottom.Init(neighbours[1]);
            _left.Init(neighbours[2]);
            _right.Init(neighbours[3]);
        }
    }
}