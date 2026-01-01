using System.Collections.Generic;
using UnityEngine;

namespace Client.Runtime
{
    public sealed class JigSawGroup : MonoBehaviour, IGroup
    {
        public List<IGroupable> Members { get; private set; } = new List<IGroupable>();

        public void AddMember(IGroupable member)
        {
            if (!Members.Contains(member))
            {
                Members.Add(member);
                member.SetGroup(this);
            }
        }

        public void RemoveMember(IGroupable member)
        {
            if (Members.Contains(member))
            {
                Members.Remove(member);
                member.SetGroup(null);
            }
        }

        public void Merge(IGroup other)
        {
            foreach (var member in other.Members)
            {
                AddMember(member);
            }
            other.Disband();
        }

        public void Disband()
        {
            foreach (var member in Members)
            {
                member.SetGroup(null);
            }
            Members.Clear();
        }

        public void Move(Vector3 delta)
        {
            foreach (var member in Members)
            {
                if (member is MonoBehaviour mb)
                    mb.transform.position += delta;
            }
        }
    }
}