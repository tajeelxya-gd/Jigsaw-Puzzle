using UniTx.Runtime.Events;

namespace Client.Runtime
{
    public readonly struct PieceSelectedEvent : IEvent
    {
        public readonly bool Selected;

        public PieceSelectedEvent(bool selected) => Selected = selected;
    }
}