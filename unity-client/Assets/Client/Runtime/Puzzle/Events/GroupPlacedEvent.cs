using UniTx.Runtime.Events;

namespace Client.Runtime
{
    public readonly struct GroupPlacedEvent : IEvent
    {
        public readonly JigsawGroup Group;
        public readonly bool IsEdge;

        public GroupPlacedEvent(JigsawGroup group, bool isEdge)
        {
            Group = group;
            IsEdge = isEdge;
        }
    }
}