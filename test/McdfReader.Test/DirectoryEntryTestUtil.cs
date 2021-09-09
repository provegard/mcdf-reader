using System;
using System.Text;

namespace McdfReader.Test
{
    internal static class DirectoryEntryTestUtil
    {
        internal static DirectoryEntry Entry(byte[] data)
        {
            var mem = new ReadOnlyMemory<byte>(data);
            return new DirectoryEntry(mem);
        }

        internal static byte[] EmptyData() => new byte[DirectoryEntry.Size];

        internal static byte[] DataWithName(string name)
        {
            var data = EmptyData();
            var bytes = Encoding.Unicode.GetBytes(name);
            
            Array.Copy(bytes, 0, data, 0, bytes.Length);

            var len = (ushort) (bytes.Length + 2); // 2 is null termination bytes
            var lenBytes = BitConverter.GetBytes(len);
            Array.Copy(lenBytes, 0, data, 64, 2);

            return data;
        }
    }
}