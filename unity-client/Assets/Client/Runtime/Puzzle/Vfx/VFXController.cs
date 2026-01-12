using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UniTx.Runtime;
using UniTx.Runtime.Events;
using UniTx.Runtime.IoC;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class VFXController : IVFXController, IInitialisable, IResettable, IInjectable
    {
        private readonly HashSet<JigsawPiece> _vfxQueue = new();
        private bool _isBatching;
        private ICameraEffects _cameraEffects;

        public void Inject(IResolver resolver) => _cameraEffects = resolver.Resolve<ICameraEffects>();

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
            _cameraEffects.PlayGroupingEffect();
            HighlightGroupAndNeighbours(ev.Group);
        }

        public void HighlightGroupAndNeighbours(JigsawGroup group)
        {
            // 1. Run BFS to find all connected locked pieces
            Queue<JigsawPiece> searchQueue = new();

            foreach (var p in group)
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

        public async UniTask AnimateBoardCompletionAsync(IEnumerable<JigsawPiece> pieces, int cols, AnimationOrder order, CancellationToken cToken = default)
        {
            var allTasks = new List<UniTask>();

            float liftAmount = 0.017f;
            float duration = 0.5f;
            float delayBetweenGroups = 0.1f;
            float delayBetweenPiecesInGroup = 0.05f;

            // Use Select to keep track of the original index for math
            var indexedPieces = pieces.Select((piece, index) => new { piece, index });

            // Group by calculating Row or Col from the index
            var groups = order == AnimationOrder.RowByRow
                ? indexedPieces.GroupBy(x => x.index / cols).OrderBy(g => g.Key)
                : indexedPieces.GroupBy(x => x.index % cols).OrderBy(g => g.Key);

            foreach (var group in groups)
            {
                if (cToken.IsCancellationRequested) break;

                foreach (var item in group)
                {
                    allTasks.Add(ManualBounceAsync(item.piece, liftAmount, duration, cToken));

                    if (delayBetweenPiecesInGroup > 0)
                        await UniTask.Delay(TimeSpan.FromSeconds(delayBetweenPiecesInGroup), cancellationToken: cToken);
                }

                await UniTask.Delay(TimeSpan.FromSeconds(delayBetweenGroups), cancellationToken: cToken);
            }

            await UniTask.WhenAll(allTasks);
        }

        private async UniTask ManualBounceAsync(JigsawPiece piece, float amount, float duration, CancellationToken cToken)
        {
            Vector3 startPos = piece.transform.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (cToken.IsCancellationRequested) return;

                elapsed += Time.deltaTime;
                float normalizedTime = elapsed / duration;
                float yOffset = Mathf.Sin(normalizedTime * Mathf.PI) * amount;

                piece.transform.localPosition = startPos + new Vector3(0, yOffset, 0);
                await UniTask.Yield(PlayerLoopTiming.Update, cToken);
            }

            piece.transform.localPosition = startPos;
        }

        private async void TriggerBatchVfx()
        {
            if (_isBatching) return;
            _isBatching = true;

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