using System;
using System.Collections.Generic;
using System.Linq;

namespace McdfReader
{
    internal class Directory
    {
        private readonly DirectoryEntry[] _entries;

        internal Directory(IEnumerable<DirectoryEntry> entries)
        {
            _entries = entries.ToArray();
        }

        internal DirectoryEntry GetRoot() => GetEntry(0u);
        
        internal DirectoryEntry GetEntry(uint id)
        {
            if (id >= _entries.Length)
                throw new ArgumentOutOfRangeException(nameof(id), $"Invalid stream ID, max = {_entries.Length - 1}");
            return _entries[id];
        }
        
        internal void Visit(Action<DirectoryEntry> visitor)
        {
            VisitEntry(visitor, GetRoot(), true);
        }
        
        internal void VisitEntry(Action<DirectoryEntry> visitor, DirectoryEntry entry, bool recurse)
        {
            var children = new List<uint>();
            var collectingVisitor = CreateChildCollectingVisitor(visitor, children);
            
            // First visit without recursion, and collect children
            if (entry.LeftSiblingID != StreamIDs.NoStream)
                VisitEntry(collectingVisitor, GetEntry(entry.LeftSiblingID), false);

            collectingVisitor(entry);
            
            if (entry.RightSiblingID != StreamIDs.NoStream)
                VisitEntry(collectingVisitor, GetEntry(entry.RightSiblingID), false);

            if (recurse)
            {
                foreach (var child in children)
                {
                    VisitEntry(visitor, GetEntry(child), true);
                }
            }
        }

        private static Action<DirectoryEntry> CreateChildCollectingVisitor(Action<DirectoryEntry> visitor, IList<uint> children)
        {
            return e =>
            {
                visitor(e);
                if (e.ChildID != StreamIDs.NoStream)
                    children.Add(e.ChildID);
            };
        }
    }
}