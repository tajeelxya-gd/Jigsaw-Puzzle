using UnityEngine;

namespace Client.Runtime
{
    public sealed class JoinController : MonoBehaviour
    {
        public enum ConnectionType { Simple, Join }

        [SerializeField] private BoxCollider _collider;
        [SerializeField] private Join _top;
        [SerializeField] private Join _bottom;
        [SerializeField] private Join _left;
        [SerializeField] private Join _right;

        [Header("Settings")]
        [Range(0.1f, 0.5f)]
        [SerializeField] private float _tabScaleRatio;

        [Tooltip("The actual height/thickness of the joiner tabs")]
        [SerializeField] private float _tabThickness;
        [SerializeField] private float _scaleFactor;

        public void Init(BoxCollider main, JigsawBoardCell[] neighbours, JigSawPiece owner)
        {
            var mSize = main.size;
            var mCenter = main.center;
            _collider.size = new Vector3(mSize.x * _scaleFactor, mSize.y, mSize.z * _scaleFactor);
            _collider.center = mCenter;

            var surfaceY = mCenter.y + (mSize.y / 2f);
            var joinerCenterY = surfaceY;

            // TOP & BOTTOM (Horizontal edges)
            float tbWidth = mSize.x * _tabScaleRatio;
            float tbDepth = tbWidth;
            var tbSize = new Vector3(tbWidth, _tabThickness, tbDepth);

            _top.BoxCollider.size = tbSize;
            _bottom.BoxCollider.size = tbSize;

            // Z positions remain at the front and back edges
            _top.BoxCollider.center = new Vector3(mCenter.x, joinerCenterY, mCenter.z + (mSize.z / 2f));
            _bottom.BoxCollider.center = new Vector3(mCenter.x, joinerCenterY, mCenter.z - (mSize.z / 2f));

            // LEFT & RIGHT (Vertical edges)
            float lrHeight = mSize.z * _tabScaleRatio;
            float lrDepth = lrHeight;
            var lrSize = new Vector3(lrDepth, _tabThickness, lrHeight);

            _left.BoxCollider.size = lrSize;
            _right.BoxCollider.size = lrSize;

            // X positions remain at the left and right edges
            _left.BoxCollider.center = new Vector3(mCenter.x - (mSize.x / 2f), joinerCenterY, mCenter.z);
            _right.BoxCollider.center = new Vector3(mCenter.x + (mSize.x / 2f), joinerCenterY, mCenter.z);

            _top.Init(neighbours[0], owner);
            _bottom.Init(neighbours[1], owner);
            _left.Init(neighbours[2], owner);
            _right.Init(neighbours[3], owner);
        }


        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<PuzzleTray>(out var _))
            {
                SetActiveJoins(false);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<PuzzleTray>(out var _))
            {
                SetActiveJoins(true);
            }
        }

        private void SetActiveJoins(bool active)
        {
            _top.gameObject.SetActive(active);
            _bottom.gameObject.SetActive(active);
            _left.gameObject.SetActive(active);
            _right.gameObject.SetActive(active);
        }
    }
}