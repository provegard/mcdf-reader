using System;
using System.Text;

namespace McdfReader
{
    internal enum ObjectType
    {
        UnknownOrUnallocated = 0,
        StorageObject = 1,
        StreamObject = 2,
        RootStorageObject = 5
    }

    internal class DirectoryEntry
    {
        internal const int Size = 128;

        internal DirectoryEntry(ReadOnlyMemory<byte> entryData)
        {
            var span = entryData.Span;
            var entryNameBytes = span[..64].ToArray();

            var offset = 64;
            var nameLength = BitConverter.ToUInt16(span[NextBytes(2)]);
            ValidateNameLength(nameLength);
            
            Name = nameLength > 0
                ? Encoding.Unicode.GetString(entryNameBytes, 0, nameLength - 2) // exclude the null terminator
                : "";
            ObjectType = ConvertUndefinedToUnknown((ObjectType)span[NextBytes(1)][0]);
            
            _ = NextBytes(1); // ignore color flag

            LeftSiblingID = BitConverter.ToUInt32(span[NextBytes(4)]);
            RightSiblingID = BitConverter.ToUInt32(span[NextBytes(4)]);
            ChildID = BitConverter.ToUInt32(span[NextBytes(4)]);

            Range NextBytes(int len)
            {
                var range = new Range(offset, offset + len);
                offset += len;
                return range;
            }

            static ObjectType ConvertUndefinedToUnknown(ObjectType input)
                => Enum.IsDefined(input) ? input : ObjectType.UnknownOrUnallocated;
        }

        private static void ValidateNameLength(ushort nameLength)
        {
            if (nameLength > 64)
                throw new McdfException($"Name length must be at most 64, found {nameLength}");
        }

        internal string Name { get; }
        internal ObjectType ObjectType { get; }
        
        internal uint LeftSiblingID { get; }
        internal uint RightSiblingID { get; }
        internal uint ChildID { get; }

        public override string ToString()
        {
            return $"'{Name}' ({ObjectType}) (left = {LeftSiblingID}, right = {RightSiblingID}, child = {ChildID}";
        }
    }
}