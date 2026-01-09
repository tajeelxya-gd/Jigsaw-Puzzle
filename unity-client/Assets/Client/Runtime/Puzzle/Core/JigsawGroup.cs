using System.Collections.Generic;

namespace Client.Runtime
{
    public sealed class JigsawGroup : HashSet<JigsawPiece>
    {
        public readonly string Id;

        public JigsawGroup(string id) : base() => Id = id;

        public JigsawGroup(string id, IEnumerable<JigsawPiece> pieces) : base(pieces) => Id = id;
    }
}