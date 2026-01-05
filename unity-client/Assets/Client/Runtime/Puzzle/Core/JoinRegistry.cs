using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Client.Runtime
{
    public static class JoinRegistry
    {
        private static readonly Dictionary<JigSawPiece, Transform> _registry = new();

        public static void Register(JigSawPiece piece, Transform mergeTransform) => _registry.TryAdd(piece, mergeTransform);

        public static void UnRegister(JigSawPiece piece) => _registry.Remove(piece);

        public static void Clear() => _registry.Clear();

        public static IEnumerable<(JigSawPiece piece, Transform transform)> Get() => _registry.Select(kvp => (kvp.Key, kvp.Value));

        public static bool Has() => _registry.Count > 0;
    }
}