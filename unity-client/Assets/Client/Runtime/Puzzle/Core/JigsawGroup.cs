using System.Collections.Generic;

namespace Client.Runtime
{
    public sealed class JigsawGroup : HashSet<JigSawPiece>
    {
        public readonly string Id;

        public JigsawGroup(string id) : base() => Id = id;

        public JigsawGroup(string id, IEnumerable<JigSawPiece> pieces) : base(pieces) => Id = id;
    }
}