using UniTx.Runtime.Events;

namespace Client.Runtime
{
    public readonly struct GroupPlacedEvent : IEvent
    {
        public readonly JigsawGroup Group;
        public readonly JigsawPiece Anchor;

        public GroupPlacedEvent(JigsawGroup group, JigsawPiece anchor)
        {
            Group = group;
            Anchor = anchor;
        }
    }
}