using UniTx.Runtime.Events;

namespace Client.Runtime
{
    public readonly struct GroupPlacedEvent : IEvent
    {
        public readonly JigsawGroup Group;

        public GroupPlacedEvent(JigsawGroup group)
        {
            Group = group;
        }
    }
}