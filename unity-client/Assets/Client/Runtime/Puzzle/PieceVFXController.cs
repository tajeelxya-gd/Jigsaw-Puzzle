using System.Collections.Generic;
using UniTx.Runtime;
using UniTx.Runtime.Events;

namespace Client.Runtime
{
    public sealed class PieceVFXController : IInitialisable, IResettable
    {
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
            // 1. Start with the current piece's group
            var pieceGroup = ev.jigSawPiece.Group;
            JigsawGroup allToNotify = new("group_tmp", pieceGroup);

            // 2. Prepare BFS to find all connected placed pieces
            Queue<JigSawPiece> searchQueue = new();

            // Add all current group members to the queue to check their neighbors too
            foreach (var p in pieceGroup)
            {
                searchQueue.Enqueue(p);
            }

            // 3. Crawl through neighbors of neighbors
            while (searchQueue.Count > 0)
            {
                var current = searchQueue.Dequeue();
                var idx = current.Data.OriginalCell.Idx;
                var neighbors = JigsawBoardCalculator.GetNeighbours(idx);

                foreach (var cell in neighbors)
                {
                    var piece = cell?.Piece;

                    // If the neighbor is placed and we haven't processed it yet
                    if (piece != null && piece.IsLocked && !allToNotify.Contains(piece))
                    {
                        allToNotify.Add(piece);
                        searchQueue.Enqueue(piece); // Add to queue to check ITS neighbors
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