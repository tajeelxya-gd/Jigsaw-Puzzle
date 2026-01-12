using UniTx.Runtime.Events;
using UniTx.Runtime.IoC;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigsawPiece : MonoBehaviour, IInjectable
    {
        [SerializeField] private DragController _dragController;
        [SerializeField] private PieceSnapController _snapController;
        [SerializeField] private BoxCollider _collider;
        [SerializeField] private JigsawPieceVFX _vfx;
        [SerializeField] private JigsawPieceRenderer _renderer;
        [SerializeField] private ScaleController _scaleController;

        private IPuzzleTray _puzzleTray;
        private IPuzzleService _puzzleService;

        public int CorrectIdx { get; private set; }
        public int CurrentIdx { get; private set; }
        public JigsawGroup Group { get; set; }

        public void Inject(IResolver resolver)
        {
            _puzzleService = resolver.Resolve<IPuzzleService>();
            _puzzleTray = resolver.Resolve<IPuzzleTray>();
        }

        public void Init(JigsawBoardCell cell, JigsawPieceRendererData rendererData)
        {
            CorrectIdx = cell.Idx;
            CurrentIdx = -1;
            _collider.size = cell.Size;
            _renderer.Init(rendererData);

            Group = new JigsawGroup()
            {
                this
            };
        }

        public void PlayVfx() => _vfx.Play();

        public void StartManualDrag() => _dragController.ForceStartDrag();

        public void ScaleUp() => _scaleController.ScaleTo(1f);

        public void ScaleDown() => _scaleController.ScaleTo(_puzzleService.GetCurrentBoard().Data.TrayScaleReduction);

        public void LockPiece()
        {
            _dragController.enabled = false;
            _collider.enabled = false;
            _snapController.enabled = false;
            _renderer.SetActive(isFlat: true);
        }

        private void Awake()
        {
            _dragController.OnDragStarted += HandleDragStarted;
            _dragController.OnDragged += HandleOnDragged;
            _dragController.OnDragEnded += HandleDraggedEnded;
            _snapController.OnSnapped += HandleSnapped;
        }

        private void OnDestroy()
        {
            _dragController.OnDragStarted -= HandleDragStarted;
            _dragController.OnDragged -= HandleOnDragged;
            _dragController.OnDragEnded -= HandleDraggedEnded;
            _snapController.OnSnapped -= HandleSnapped;
        }

        private void HandleDragStarted() => Group.SetPosY(0.01f);

        private void HandleOnDragged(Vector3 delta)
        {
            Group.Move(delta);
            _puzzleTray.SetHoverPiece(this);
        }

        private void HandleDraggedEnded()
        {
            _puzzleTray.SetHoverPiece(null);

            // 1. Physical Tray Check
            bool physicallyOverTray = _puzzleTray.IsOverTray(transform.position);

            // 2. Only submit if it's a single piece
            if (physicallyOverTray && Group.Count == 1)
            {
                _puzzleTray.SubmitPiece(this);
                return;
            }

            _snapController.SnapToClosestCell(Group, _puzzleService.GetCurrentBoard().Cells);
        }

        private void HandleSnapped(JigsawBoardCell cell)
        {
            if (cell.Push(this))
            {
                Group.Lock();

                UniEvents.Raise(new GroupPlacedEvent(Group));

                return;
            }
        }
    }
}