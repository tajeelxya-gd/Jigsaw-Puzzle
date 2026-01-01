using UniTx.Runtime;
using UniTx.Runtime.Events;
using UniTx.Runtime.IoC;

namespace Client.Runtime
{
    public sealed class PieceVFXController : IInjectable, IInitialisable, IResettable
    {
        private IPuzzleService _puzzleService;

        public void Inject(IResolver resolver)
        {
            _puzzleService = resolver.Resolve<IPuzzleService>();
        }

        public void Initialise()
        {
            UniEvents.Subscribe<PiecePlacedEvent>(HandlePiecePlaced);
        }

        public void Reset()
        {
            UniEvents.Unsubscribe<PiecePlacedEvent>(HandlePiecePlaced);
        }

        private void HandlePiecePlaced(PiecePlacedEvent ev)
        {
            var board = _puzzleService.GetCurrentBoard();
            var piece = ev.jigSawPiece;
            var idx = piece.Data.OriginalIdx;
            var neighbours = board.GetNeighbours(idx);

            piece.PlayVfx();
            foreach (var neighbour in neighbours)
            {
                var neighbourPiece = neighbour.Piece;
                if (neighbourPiece != null && neighbourPiece.IsPlaced)
                {
                    neighbourPiece.PlayVfx();
                }
            }
        }
    }
}