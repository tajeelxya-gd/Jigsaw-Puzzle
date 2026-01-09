using UniTx.Runtime.Events;

namespace Client.Runtime
{
    public readonly struct PiecePlacedEvent : IEvent
    {
        // Empty yet
        public readonly JigsawPiece jigSawPiece;

        public PiecePlacedEvent(JigsawPiece jigSawPiece)
        {
            this.jigSawPiece = jigSawPiece;
        }
    }
}