using System.Collections.Generic;
using System.Threading.Tasks;
using UniTx.Runtime;
using UniTx.Runtime.Events;

namespace Client.Runtime
{
    public sealed class PieceVFXController : IInitialisable, IResettable
    {
        private readonly HashSet<JigsawPiece> _vfxQueue = new();
        private bool _isBatching;

        public void Initialise()
        {
            UniEvents.Subscribe<GroupPlacedEvent>(HandlePiecePlaced);
        }

        public void Reset()
        {
            UniEvents.Unsubscribe<GroupPlacedEvent>(HandlePiecePlaced);
            _vfxQueue.Clear();
            _isBatching = false;
        }

        private void HandlePiecePlaced(GroupPlacedEvent ev)
        {
            // 1. Run BFS to find all connected locked pieces
            var pieceGroup = ev.Group;
            Queue<JigsawPiece> searchQueue = new();

            foreach (var p in pieceGroup)
            {
                searchQueue.Enqueue(p);
                _vfxQueue.Add(p);
            }

            while (searchQueue.Count > 0)
            {
                var current = searchQueue.Dequeue();
                var neighbors = JigsawBoardCalculator.GetNeighboursCells(current.CorrectIdx);

                foreach (var cell in neighbors)
                {
                    if (cell.IsLocked)
                    {
                        var piece = cell.GetCorrectPiece();
                        // Only add to search if not already in our global VFX batch
                        if (_vfxQueue.Add(piece))
                        {
                            searchQueue.Enqueue(piece);
                        }
                    }
                }
            }

            // 2. Trigger the batch play once at the end of the frame
            TriggerBatchVfx();
        }

        private async void TriggerBatchVfx()
        {
            if (_isBatching) return;
            _isBatching = true;

            // Wait until the end of the frame/next tick so all simultaneous 
            // events have finished adding their pieces to _vfxQueue
            await Task.Yield();

            foreach (var p in _vfxQueue)
            {
                if (p != null) p.PlayVfx();
            }

            _vfxQueue.Clear();
            _isBatching = false;
        }
    }
}