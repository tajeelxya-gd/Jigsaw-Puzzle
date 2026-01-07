using System.Collections.Generic;
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

            // 1. Initialize the set with the current group
            HashSet<JigSawPiece> allToNotify = new(piece.Group);

            // 2. Iterate through every piece in the group to find their immediate neighbors
            foreach (var groupMember in piece.Group)
            {
                var neighbors = board.GetNeighbours(groupMember.Data.OriginalIdx);

                foreach (var neighborCell in neighbors)
                {
                    var neighborPiece = neighborCell?.Piece;

                    // Only add if the neighbor exists, is placed, and isn't already in our set
                    if (neighborPiece != null && neighborPiece.IsPlaced)
                    {
                        allToNotify.Add(neighborPiece);
                    }
                }
            }

            // 3. Trigger VFX for the group and their immediate neighbors
            foreach (var p in allToNotify)
            {
                p.PlayVfx();
            }
        }
    }
}