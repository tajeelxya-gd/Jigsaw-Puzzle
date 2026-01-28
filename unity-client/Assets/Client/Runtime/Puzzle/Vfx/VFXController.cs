using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UniTx.Runtime;
using UniTx.Runtime.Audio;
using UniTx.Runtime.Events;
using UniTx.Runtime.IoC;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class VFXController : MonoBehaviour, IVFXController, IInitialisable, IResettable, IInjectable
    {
        [SerializeField] private float _liftAmount;
        [SerializeField] private float _duration;
        [SerializeField] private float _delayBetweenPiecesInGroup;
        [SerializeField] private float _delayBetweenGroup;
        [SerializeField] private float _vfxDelayInPieces;
        [SerializeField] private float _vfxDelayInClockwise;
        [SerializeField] private int _maxHighlightPieces = -1;
        [SerializeField] private ScriptableObject _pieceLocked;

        private readonly List<JigsawPiece> _vfxQueue = new();
        private bool _isBatching;
        private ICameraEffects _cameraEffects;
        private IPuzzleService _puzzleService;

        public void Inject(IResolver resolver)
        {
            _cameraEffects = resolver.Resolve<ICameraEffects>();
            _puzzleService = resolver.Resolve<IPuzzleService>();
        }

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

        public void HighlightGroupAndNeighbours(JigsawGroup group)
        {
            // 1. Run BFS to find all connected locked pieces
            Queue<JigsawPiece> searchQueue = new();
            HashSet<JigsawPiece> processed = new();

            foreach (var p in group)
            {
                searchQueue.Enqueue(p);
                processed.Add(p);
                if (!_vfxQueue.Contains(p)) _vfxQueue.Add(p);
            }

            while (searchQueue.Count > 0)
            {
                if (_maxHighlightPieces != -1 && _vfxQueue.Count >= _maxHighlightPieces) break;

                var current = searchQueue.Dequeue();
                var neighbors = JigsawBoardCalculator.GetNeighboursCells(current.CorrectIdx);

                foreach (var cell in neighbors)
                {
                    if (cell.IsLocked)
                    {
                        var piece = cell.GetCorrectPiece();
                        if (processed.Add(piece))
                        {
                            searchQueue.Enqueue(piece);
                            if (!_vfxQueue.Contains(piece)) _vfxQueue.Add(piece);
                        }
                    }
                }
            }

            TriggerBatchVfx(_vfxDelayInPieces);
        }

        public async UniTask AnimateBoardCompletionAsync(CancellationToken cToken = default)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.35f), cancellationToken: cToken);

            var allTasks = new List<UniTask>();
            var order = AnimationOrder.RowByRow;
            var board = _puzzleService.GetCurrentBoard();
            var cols = board.Data.YConstraint;

            var indexedPieces = board.Pieces.Select((piece, index) => new { piece, index });
            var groups = order == AnimationOrder.RowByRow
                ? indexedPieces.GroupBy(x => x.index / cols).OrderBy(g => g.Key)
                : indexedPieces.GroupBy(x => x.index % cols).OrderBy(g => g.Key);
            var delay = 0f;
            var groupDelay = 0f;
            foreach (var group in groups)
            {
                if (cToken.IsCancellationRequested) break;

                delay = groupDelay;

                foreach (var item in group)
                {
                    allTasks.Add(ManualBounceAsync(item.piece, _liftAmount, _duration, delay, cToken));
                    delay += _delayBetweenPiecesInGroup;
                }

                groupDelay += _delayBetweenGroup;
            }

            await UniTask.WhenAll(allTasks);
            await UniTask.Delay(TimeSpan.FromSeconds(0.35f), cancellationToken: cToken);
        }

        public UniTask WaitForHighlightsAsync(CancellationToken cToken = default)
        {
            return UniTask.WaitWhile(() => _vfxQueue.Count > 0, cancellationToken: cToken);
        }

        private void HandlePiecePlaced(GroupPlacedEvent ev)
        {
            _cameraEffects.PlayGroupingEffect();
            UniAudio.Play2D((IAudioConfig)_pieceLocked);
            if(JigsawBoardCalculator.IsEdge(ev.Anchor.CorrectIdx) && AllEdgesLocked(out var edges))
            {
                HighlightClockwise(edges, ev.Anchor);
                return;
            }
            HighlightGroupAndNeighbours(ev.Group);
        }

        private bool AllEdgesLocked(out IEnumerable<JigsawPiece> edgePieces)
        {
            var board = _puzzleService.GetCurrentBoard();
            var allPieces = board.Pieces;
            var edges = new List<JigsawPiece>();
            bool allLocked = true;

            for (var i = 0; i < allPieces.Count; i++)
            {
                var piece = allPieces[i];

                if (JigsawBoardCalculator.IsEdge(piece.CorrectIdx)) 
                {
                    edges.Add(piece);

                    if (!piece.IsLocked)
                    {
                        allLocked = false;
                    }
                }
            }

            edgePieces = edges;
            return allLocked;
        }

        private void HighlightClockwise(IEnumerable<JigsawPiece> pieces, JigsawPiece anchor)
        {
            var board = _puzzleService.GetCurrentBoard();
            var data = board.Data;
            int rows = data.XConstraint;
            int cols = data.YConstraint;

            var sorted = pieces.OrderBy(p => 
            {
                int r = p.CorrectIdx / cols;
                int col = p.CorrectIdx % cols;
                if (r == 0) return col; 
                if (col == cols - 1) return cols + r;
                if (r == rows - 1) return cols + rows + (cols - 1 - col);
                return cols + rows + cols + (rows - 1 - r);
            }).ToList();

            int anchorIndex = sorted.IndexOf(anchor);
            if (anchorIndex != -1)
            {
                var rotated = sorted.Skip(anchorIndex).Concat(sorted.Take(anchorIndex)).ToList();
                sorted = rotated;
            }

            foreach(var piece in sorted)
            {
                if (!_vfxQueue.Contains(piece)) _vfxQueue.Add(piece);
            }

            TriggerBatchVfx(_vfxDelayInClockwise);
        }

        private async UniTask ManualBounceAsync(JigsawPiece piece, float amount, float duration, float delay, CancellationToken cToken)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: cToken);
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

        private void TriggerBatchVfx(float vfxDelay)
        {
            if (vfxDelay > 0)
            {
                TriggerBatchVfxAsync(vfxDelay, this.GetCancellationTokenOnDestroy()).Forget();
                return;
            }

            if (_isBatching) return;
            _isBatching = true;

            foreach (var p in _vfxQueue)
            {
                p.PlayVfx();
            }

            _vfxQueue.Clear();
            _isBatching = false;
        }

        private async UniTaskVoid TriggerBatchVfxAsync(float vfxDelay,CancellationToken cToken = default)
        {
            if (_isBatching) return;
            _isBatching = true;

            await Task.Yield();

            var delay = 0f;
            var tasks =  new List<UniTask>();
            foreach (var p in _vfxQueue)
            {
                tasks.Add(UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: cToken).ContinueWith(() => p.PlayVfx()));
                delay += vfxDelay;
            }
            await UniTask.WhenAll(tasks);

            _vfxQueue.Clear();
            _isBatching = false;
        }

#if UNITY_EDITOR
        [ContextMenu("PlayAnim")]
        private void PlayAnim()
        {
            AnimateBoardCompletionAsync(this.GetCancellationTokenOnDestroy()).Forget();
        }
#endif
    }
}