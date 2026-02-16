using System;
using UnityEngine;
using UnityEngine.Events;

public class LeaderBoardUnlockCommand : PopUpExecutionCommand
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public LeaderBoardUnlockCommand(PopCommandExecutionResponder.PopupPriority priority, UnityAction<Action> action) : base(priority, action)
    {
    }
}
