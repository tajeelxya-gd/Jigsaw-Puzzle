using System;
using System.Collections.Generic;
using UnityEditor;

[InitializeOnLoad]
public static class EditorPostCompileTasks
{
    private static Queue<Action> tasks = new Queue<Action>();

    static EditorPostCompileTasks()
    {
        // Run tasks when scripts reloaded
        EditorApplication.delayCall += RunPendingTasks;
    }

    public static void Enqueue(Action action)
    {
        tasks.Enqueue(action);
    }

    private static void RunPendingTasks()
    {
        while (tasks.Count > 0)
        {
            try
            {
                tasks.Dequeue()?.Invoke();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e);
            }
        }
    }
}