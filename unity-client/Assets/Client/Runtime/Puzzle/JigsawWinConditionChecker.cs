using System;
using System.Linq;
using UniTx.Runtime;
using UniTx.Runtime.Events;
using UniTx.Runtime.Extensions;

namespace Client.Runtime
{
    public sealed class JigsawWinConditionChecker : IWinConditionChecker, IInitialisable, IResettable
    {
        public event Action OnWin;
        public event Action OnLose;
        public event Action OnAdvance;

        private JigSawBoard _board;

        public bool CheckLose() => false;

        public bool CheckWin()
        {
            var placed = _board.Pieces.Count(itm => itm.IsPlaced);
            return placed == _board.Pieces.Count;
        }

        public void SetBoard(JigSawBoard board) => _board = board;

        public void Initialise()
        {
            UniEvents.Subscribe<PiecePlacedEvent>(OnPiecePlaced);
        }

        public void Reset()
        {
            UniEvents.Unsubscribe<PiecePlacedEvent>(OnPiecePlaced);
        }

        private void OnPiecePlaced(PiecePlacedEvent @event)
        {
            if (CheckWin())
            {
                OnWin.Broadcast();
                UniStatics.LogInfo("Board completed.", this);
                return;
            }

            if (CheckLose())
            {
                OnLose.Broadcast();
                return;
            }

            OnAdvance.Broadcast();
        }
    }
}