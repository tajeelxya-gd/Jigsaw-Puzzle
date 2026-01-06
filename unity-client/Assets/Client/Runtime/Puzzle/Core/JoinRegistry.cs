using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Client.Runtime
{
    public static class JoinRegistry
    {
        private static readonly Dictionary<JigSawPiece, Join> _registry = new();

        public static void Register(JigSawPiece piece, Join join) => _registry.TryAdd(piece, join);

        public static void UnRegister(JigSawPiece piece) => _registry.Remove(piece);

        public static void Clear() => _registry.Clear();

        public static IEnumerable<(JigSawPiece piece, Join join)> Get() => _registry.Select(kvp => (kvp.Key, kvp.Value));

        public static bool HasCorrectContacts() => _registry.Count > 0;
    }
}