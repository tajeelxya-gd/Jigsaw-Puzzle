namespace Client.Runtime
{
    public interface IGroupable
    {
        IGroup Group { get; }
        void AttachTo(IGroup other);
        void SetGroup(IGroup group);
    }
}