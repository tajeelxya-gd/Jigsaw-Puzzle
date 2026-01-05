using System.Collections.Generic;

namespace Client.Runtime
{
    public interface IPuzzleTray
    {
        
        void ShufflePieces(IReadOnlyList<JigSawPiece> pieces);
    }
}