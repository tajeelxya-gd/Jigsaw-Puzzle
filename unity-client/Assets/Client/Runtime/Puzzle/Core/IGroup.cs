using System.Collections.Generic;

namespace Client.Runtime
{
    public interface IGroup
    {
        List<IGroupable> Members { get; }
        void AddMember(IGroupable member);
        void RemoveMember(IGroupable member);
        void Merge(IGroup other);
        void Disband();
    }
}