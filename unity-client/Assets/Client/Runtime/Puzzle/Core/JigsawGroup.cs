using System.Collections.Generic;

namespace Client.Runtime
{
    public sealed class JigsawGroup : HashSet<JigsawPiece>
    {
        public JigsawGroup() : base()
        {
            // Empty
        }

        public JigsawGroup(IEnumerable<JigsawPiece> pieces) : base(pieces)
        {
            // Empty
        }
    }
}