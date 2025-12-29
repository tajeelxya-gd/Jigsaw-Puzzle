using System.Collections.Generic;

namespace UniTx.Runtime.Events
{
    internal readonly struct Comparer : IComparer<IListener>
    {
        private static readonly Dictionary<Priority, int> _order = new()
        {
            { Priority.Highest, 0 },
            { Priority.High, 1 },
            { Priority.Medium, 2 },
            { Priority.Low, 3 },
            { Priority.Lowest, 4 }
        };

        public int Compare(IListener x, IListener y)
            => _order[x.Priority].CompareTo(_order[y.Priority]);
    }
}