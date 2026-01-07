using System.Collections.Generic;

namespace Client.Runtime
{
    public sealed class JigsawGroup : HashSet<JigSawPiece>
    {
        public JigsawGroup() : base() { }
        public JigsawGroup(IEnumerable<JigSawPiece> pieces) : base(pieces) { }
    }
}