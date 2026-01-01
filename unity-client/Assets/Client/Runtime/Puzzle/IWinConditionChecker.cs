using System;

namespace Client.Runtime
{
    public interface IWinConditionChecker
    {
        event Action OnWin;
        event Action OnLose;
        event Action OnAdvance;

        bool CheckWin();
        bool CheckLose();
        void SetBoard(JigSawBoard board);
    }
}