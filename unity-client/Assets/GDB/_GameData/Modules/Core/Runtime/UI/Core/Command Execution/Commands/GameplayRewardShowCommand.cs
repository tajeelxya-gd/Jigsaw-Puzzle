using System;
using UnityEngine;
using UnityEngine.Events;

public class GameplayRewardShowCommand : PopUpExecutionCommand
{
    public GameplayRewardShowCommand(PopCommandExecutionResponder.PopupPriority priority, UnityAction<Action> action) : base(priority, action)
    {
    }
}
