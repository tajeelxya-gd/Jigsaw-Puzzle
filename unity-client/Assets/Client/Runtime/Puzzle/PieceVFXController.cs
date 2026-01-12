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
            UniEvents.Subscribe<GroupPlacedEvent>(HandlePiecePlaced);
        }

        public void Reset()
        {
            UniEvents.Unsubscribe<GroupPlacedEvent>(HandlePiecePlaced);
        }

        private void HandlePiecePlaced(GroupPlacedEvent ev)
        {
            // 1. Start with the current piece's group
            var pieceGroup = ev.Group;
            JigsawGroup allToNotify = new(pieceGroup);

            // 2. Prepare BFS to find all connected placed pieces
            Queue<JigsawPiece> searchQueue = new();

            // Add all current group members to the queue to check their neighbors too
            foreach (var p in pieceGroup)
            {
                searchQueue.Enqueue(p);
            }

            // 3. Crawl through neighbors of neighbors
            while (searchQueue.Count > 0)
            {
                var current = searchQueue.Dequeue();
                var neighbors = JigsawBoardCalculator.GetNeighboursCells(current.CorrectIdx);

                foreach (var cell in neighbors)
                {
                    // If the neighbor is placed and we haven't processed it yet
                    if (cell.IsLocked)
                    {
                        var piece = cell.GetCorrectPiece();
                        if (!allToNotify.Contains(piece))
                        {
                            allToNotify.Add(piece);
                            searchQueue.Enqueue(piece); // Add to queue to check ITS neighbors
                        }
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