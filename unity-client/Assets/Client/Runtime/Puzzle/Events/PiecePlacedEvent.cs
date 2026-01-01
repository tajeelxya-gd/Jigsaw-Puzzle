using UniTx.Runtime.Events;

namespace Client.Runtime
{
    public readonly struct PiecePlacedEvent : IEvent
    {
        // Empty yet
        public readonly JigSawPiece jigSawPiece;

        public PiecePlacedEvent(JigSawPiece jigSawPiece)
        {
            this.jigSawPiece = jigSawPiece;
        }
    }
}