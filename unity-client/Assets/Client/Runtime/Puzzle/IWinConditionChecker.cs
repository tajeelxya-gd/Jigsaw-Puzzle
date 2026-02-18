using System;

namespace Client.Runtime
{
    public interface IWinConditionChecker
    {
        event Action OnWin;
        event Action OnLose;
        event Action<float> OnAdvance;

        bool CheckWin();
        bool CheckLose();
        void SetBoard(JigsawBoard board);
    }
}