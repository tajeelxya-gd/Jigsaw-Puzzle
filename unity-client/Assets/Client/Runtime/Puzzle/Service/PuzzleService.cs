using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniTx.Runtime;
using UniTx.Runtime.Content;
using UniTx.Runtime.Entity;
using UniTx.Runtime.IoC;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class PuzzleService : IPuzzleService, IInjectable, IInitialisable
    {
        private IContentService _contentService;
        private IEntityService _entityService;
        private JigSawBoard _board;
        private Transform _puzzleRoot;

        public void Inject(IResolver resolver)
        {
            _contentService = resolver.Resolve<IContentService>();
            _entityService = resolver.Resolve<IEntityService>();
        }

        public void Initialise()
        {
            _puzzleRoot = GameObject.FindGameObjectWithTag("PuzzleRoot").transform;
        }

        public async UniTask LoadPuzzleAsync(CancellationToken cToken = default)
        {
            var levelData = GetCurrentLevelData();
            _board = _entityService.Get<JigSawBoard>(levelData.BoardId);
            await _board.LoadPuzzleAsync(levelData.ImageKey, _puzzleRoot, cToken);
            _board.SetActiveFullImage(false);
            foreach (var cell in _board.Cells)
            {
                WrapAndSetupPiece(cell);
            }

            ShufflePieces();
        }

        public void UnLoadPuzzle()
        {
            _board.UnLoadPuzzle();
            _board = null;
        }

        private JigSawLevelData GetCurrentLevelData()
        {
            var data = _contentService.GetAllData<JigSawLevelData>();

            // TODO: load idx from saves later
            return data.First();
        }

        private void WrapAndSetupPiece(JigSawBoardCell cell)
        {
            var meshTransform = cell.PieceTransform;

            // Create a parent wrapper
            GameObject pieceRoot = new GameObject(meshTransform.name + "_Piece");
            pieceRoot.transform.SetParent(_puzzleRoot);
            pieceRoot.transform.position = meshTransform.position;
            pieceRoot.transform.rotation = meshTransform.rotation;

            // Reparent the mesh under wrapper
            meshTransform.SetParent(pieceRoot.transform);
            meshTransform.localPosition = Vector3.zero;
            meshTransform.localRotation = Quaternion.identity;

            // Add required components to wrapper
            var jigSawPiece = pieceRoot.AddComponent<JigSawPiece>();

            // Collider that matches mesh bounds
            var renderer = meshTransform.GetComponent<Renderer>();
            BoxCollider collider = pieceRoot.AddComponent<BoxCollider>();
            if (renderer != null)
                collider.size = renderer.bounds.size;

            var dragController = pieceRoot.AddComponent<DragController3D>();

            // Assign piece data
            var pieceData = _contentService.GetData<JigSawPieceData>(cell.PieceId);
            jigSawPiece.SetData(pieceData);
        }

        private void ShufflePieces()
        {
            foreach (var cell in _board.Cells)
            {
                var randomPos = _puzzleRoot.position + new Vector3(
                    Random.Range(-0.05f, 0.05f),
                    0,
                    Random.Range(-0.15f, -0.03f)
                );

                var parentTransform = cell.PieceTransform.parent;
                parentTransform.position = randomPos;
            }
        }
    }
}