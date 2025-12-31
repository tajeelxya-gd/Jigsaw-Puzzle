using System.Collections.Generic;

namespace Client.Runtime
{
    public static class JigSawPieceRegistry
    {
        private static readonly Dictionary<string, JigSawPiece> _map = new();

        public static void Register(JigSawPiece piece)
        {
            _map[piece.Data.Id] = piece;
        }

        public static void Unregister(JigSawPiece piece)
        {
            _map.Remove(piece.Data.Id);
        }

        public static JigSawPiece Get(string id)
        {
            _map.TryGetValue(id, out var piece);
            return piece;
        }
    }
}