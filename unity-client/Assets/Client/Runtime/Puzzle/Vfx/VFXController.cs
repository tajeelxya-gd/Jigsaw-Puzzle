using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Codice.Client.BaseCommands;
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
        [SerializeField] private int _maxHighlightPieces = -1;
        [SerializeField] private ScriptableObject _pieceLocked;

        private readonly HashSet<JigsawPiece> _vfxQueue = new();
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

        private void HandlePiecePlaced(GroupPlacedEvent ev)
        {
            _cameraEffects.PlayGroupingEffect();
            UniAudio.Play2D((IAudioConfig)_pieceLocked);
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
                if (_maxHighlightPieces != -1 && _vfxQueue.Count >= _maxHighlightPieces) break;

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

            UniTask.Void(TriggerBatchVfxAsync, this.GetCancellationTokenOnDestroy());
        }

        public async UniTask AnimateBoardCompletionAsync(CancellationToken cToken = default)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.35f), cancellationToken: cToken);

            var allTasks = new List<UniTask>();
            var order = AnimationOrder.RowByRow;
            var board = _puzzleService.GetCurrentBoard();
            var cols = board.Data.YConstraint;

            // Use Select to keep track of the original index for math
            var indexedPieces = board.Pieces.Select((piece, index) => new { piece, index });

            // Group by calculating Row or Col from the index
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

        private async UniTaskVoid TriggerBatchVfxAsync(CancellationToken cToken = default)
        {
            if (_isBatching) return;
            _isBatching = true;

            await Task.Yield();

            var delay = 0f;

            foreach (var p in _vfxQueue)
            {
                if (p != null)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: cToken);
                    p.PlayVfx();
                    delay += _vfxDelayInPieces;
                }
            }

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