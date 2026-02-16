using System;
using UnityEngine;
using UnityEngine.Events;

public class OnBoardingMenuCommand : PopUpExecutionCommand
{
    public OnBoardingMenuCommand(PopCommandExecutionResponder.PopupPriority priority, UnityAction<Action> action) : base(priority, action)
    {
    }
}
