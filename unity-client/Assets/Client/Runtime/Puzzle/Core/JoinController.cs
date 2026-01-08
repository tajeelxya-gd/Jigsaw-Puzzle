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
        [SerializeField] private float _tabScaleRatio = 0.2f;

        [Tooltip("The actual height/thickness of the joiner tabs")]
        [SerializeField] private float _tabThickness = 0.01f;

        public void Init(BoxCollider main, JigsawBoardCell[] neighbours, JigSawPiece owner)
        {
            Vector3 mSize = main.size;
            Vector3 mCenter = main.center;

            float surfaceY = mCenter.y + (mSize.y / 2f);
            float joinerCenterY = surfaceY + (_tabThickness / 2f);

            float tbWidth = mSize.x * _tabScaleRatio;
            float tbDepth = tbWidth;
            var tbSize = new Vector3(tbWidth, _tabThickness, tbDepth);

            _top.BoxCollider.size = tbSize;
            _bottom.BoxCollider.size = tbSize;

            _top.BoxCollider.center = new Vector3(mCenter.x, joinerCenterY, mCenter.z + (mSize.z / 2f));
            _bottom.BoxCollider.center = new Vector3(mCenter.x, joinerCenterY, mCenter.z - (mSize.z / 2f));

            var lrHeight = mSize.z * _tabScaleRatio;
            var lrDepth = lrHeight;
            var lrSize = new Vector3(lrDepth, _tabThickness, lrHeight);

            _left.BoxCollider.size = lrSize;
            _right.BoxCollider.size = lrSize;

            _left.BoxCollider.center = new Vector3(mCenter.x - (mSize.x / 2f), joinerCenterY, mCenter.z);
            _right.BoxCollider.center = new Vector3(mCenter.x + (mSize.x / 2f), joinerCenterY, mCenter.z);

            _top.Init(neighbours[0], owner);
            _bottom.Init(neighbours[1], owner);
            _left.Init(neighbours[2], owner);
            _right.Init(neighbours[3], owner);
        }
    }
}