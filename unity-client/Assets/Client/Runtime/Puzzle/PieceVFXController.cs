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

            // 1. Start with the current piece's group
            JigsawGroup allToNotify = new(piece.Group);

            // 2. Prepare BFS to find all connected placed pieces
            Queue<JigSawPiece> searchQueue = new();

            // Add all current group members to the queue to check their neighbors too
            foreach (var p in piece.Group)
            {
                searchQueue.Enqueue(p);
            }

            // 3. Crawl through neighbors of neighbors
            while (searchQueue.Count > 0)
            {
                var current = searchQueue.Dequeue();
                var neighbors = board.GetNeighbours(current.Data.OriginalIdx);

                foreach (var neighborCell in neighbors)
                {
                    var neighborPiece = neighborCell?.Piece;

                    // If the neighbor is placed and we haven't processed it yet
                    if (neighborPiece != null && neighborPiece.IsPlaced && !allToNotify.Contains(neighborPiece))
                    {
                        allToNotify.Add(neighborPiece);
                        searchQueue.Enqueue(neighborPiece); // Add to queue to check ITS neighbors
                    }
                }
            }

            // 4. Play VFX
            foreach (var p in allToNotify)
            {
                p.PlayVfx();
            }
        }
    }
}