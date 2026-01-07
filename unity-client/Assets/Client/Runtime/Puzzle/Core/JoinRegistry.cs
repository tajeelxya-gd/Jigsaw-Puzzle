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

        public static List<(JigSawPiece piece, Join join)> Flush()
        {
            if (_registry.Count == 0) return new List<(JigSawPiece, Join)>();

            var data = _registry.Select(kvp => (kvp.Key, kvp.Value)).ToList();
            _registry.Clear();
            return data;
        }

        public static bool HasCorrectContacts() => _registry.Count > 0;
    }
}