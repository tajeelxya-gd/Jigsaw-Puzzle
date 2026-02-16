using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;


public abstract class PopUpExecutionCommand
{
    public PopCommandExecutionResponder.PopupPriority Priority { get; }
    public UnityAction<Action> Action { get; }

    protected PopUpExecutionCommand(
        PopCommandExecutionResponder.PopupPriority priority,
        UnityAction<Action> action)
    {
        Priority = priority;
        Action = action;
    }
}

public class PopCommandExecutionResponder
{
    public enum PopupPriority
    {
        Critical = 0,
        High = 1,
        Medium = 2,
        Low = 3
    }

    private static readonly List<PopUpExecutionCommand> queue = new();
    private static PopUpExecutionCommand current;

    public static bool DoCommandsExists() => queue != null && queue.Count > 0;
    // ADD (by instance)
    public static void AddCommand(PopUpExecutionCommand command)
    {
        var type = command.GetType();

        if (queue.Any(c => c.GetType() == type) ||
            current?.GetType() == type)
            return;

        Debug.Log("Command Received of Type :: "+type);
        queue.Add(command);
        Sort();
    }

    public static void ExecuteNext()
    {
        if (queue.Count == 0 || queue == null) return;
        current = null;
        TryExecuteNext();
    }

    public static bool HasCommand<T>() where T : PopUpExecutionCommand
    {
        if(current is T)
            return true;
        
        if (queue == null || queue.Count == 0)
        {
            Debug.Log("No Command Found");
            return false;
        }

        return queue.Any(c => c is T);
    }


    public static void RemoveCommand<T>() where T : PopUpExecutionCommand
    {
        Debug.Log("Removing Command of Type :: "+typeof(T));
        queue.RemoveAll(c => c is T);

        if (current is T)
        {
            current = null;
            TryExecuteNext();
        }
    }

    private static void Sort()
    {
        queue.Sort((a, b) => a.Priority.CompareTo(b.Priority));
    }

    private static void TryExecuteNext()
    {
        if (current != null || queue.Count == 0)
            return;

        current = queue[0];
        queue.RemoveAt(0);

        // Execute action and pass finish callback
        current.Action.Invoke(OnCommandFinished);
    }

    private static void OnCommandFinished()
    {
        current = null;
        TryExecuteNext();
    }
    
    public static void ClearAllCommands()
    {
        Debug.Log("Clearing all popup commands");

        queue.Clear();
        current = null;
    }
}