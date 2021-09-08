using System;
using System.Text;

namespace McdfReader
{
    public enum ObjectType
    {
        UnknownOrUnallocated = 0,
        StorageObject = 1,
        StreamObject = 2,
        RootStorageObject = 5
    }

    public enum ColorFlag
    {
        Red = 0,
        Black = 1
    }
    
    public class DirectoryEntry
    {
        public DirectoryEntry(ReadOnlyMemory<byte> entryData)
        {
            var span = entryData.Span;
            var entryNameBytes = span[..64].ToArray();

            var offset = 64;
            var nameLength = BitConverter.ToUInt16(span[NextBytes(2)]); // TODO: Validate, max 64
            Name = nameLength > 0
                ? Encoding.Unicode.GetString(entryNameBytes, 0, nameLength - 2)
                : ""; // exclude the null terminator
            ObjectType = (ObjectType)span[NextBytes(1)][0];
            ColorFlag = (ColorFlag)span[NextBytes(1)][0];
            LeftSiblingID = BitConverter.ToUInt32(span[NextBytes(4)]);
            RightSiblingID = BitConverter.ToUInt32(span[NextBytes(4)]);
            ChildID = BitConverter.ToUInt32(span[NextBytes(4)]);

            Range NextBytes(int len)
            {
                var range = new Range(offset, offset + len);
                offset += len;
                return range;
            }
        }
        
        public string Name { get; }
        public ObjectType ObjectType { get; }
        public ColorFlag ColorFlag { get; }
        
        public uint LeftSiblingID { get; }
        public uint RightSiblingID { get; }
        public uint ChildID { get; }

        public override string ToString()
        {
            return $"'{Name}' ({ObjectType}, {ColorFlag})";
        }
    }
}