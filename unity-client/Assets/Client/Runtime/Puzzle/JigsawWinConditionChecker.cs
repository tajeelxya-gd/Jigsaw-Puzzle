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

        private JigsawBoard _board;

        public bool CheckLose() => false;

        public bool CheckWin()
        {
            var placed = _board.Cells.Count(itm => itm.IsLocked);
            return placed == _board.Cells.Count;
        }

        public void SetBoard(JigsawBoard board) => _board = board;

        public void Initialise()
        {
            UniEvents.Subscribe<GroupPlacedEvent>(OnPiecePlaced);
        }

        public void Reset()
        {
            UniEvents.Unsubscribe<GroupPlacedEvent>(OnPiecePlaced);
        }

        private void OnPiecePlaced(GroupPlacedEvent @event)
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