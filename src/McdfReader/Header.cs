using System;

namespace McdfReader
{
    public class Header
    {
        private const ulong ExpectedFileId = 0xE11AB1A1E011CFD0;
        private const ushort LittleEndianByteOrder = 0xFFFE;
        
        public Header(ReadOnlyMemory<byte> headerData)
        {
            if (!BitConverter.IsLittleEndian)
                throw new NotSupportedException("Only little-endian systems are supported");
            if (headerData.Length < 512)
                throw new ArgumentOutOfRangeException(nameof(headerData), "Expected at least 512 header bytes");

            var span = headerData.Span;
            
            // File ID is in the first 8 bytes
            var fileIdBytes = span[..8];
            var fileId = BitConverter.ToUInt64(fileIdBytes);
            if (fileId != ExpectedFileId)
            {
                var expectedHex = Convert.ToHexString(BitConverter.GetBytes(ExpectedFileId));
                var actualHex = Convert.ToHexString(fileIdBytes);
                throw new McdfException($"Invalid file ID, expected {expectedHex} but found {actualHex}");
            }
            
            // Skip UID (16 bytes) - must be 0 but we don't check it

            var offset = 24;
            
            // Revision and Version, 2 bytes each
            Revision = BitConverter.ToUInt16(span[NextBytes(2)]);
            Version = BitConverter.ToUInt16(span[NextBytes(2)]);
            
            if (Version != 3 && Version != 4)
                throw new McdfException($"Version must be 3 or 4, found {Version}");
            
            // Check byte order identifier
            var byteOrder = BitConverter.ToUInt16(span[NextBytes(2)]);
            ValidateByteOrder(byteOrder);
            
            // Sector size is 2^ssz
            var ssz = BitConverter.ToUInt16(span[NextBytes(2)]);
            SectorSize = 1 << ssz;
            ValidateSectorSize();

            // Mini sector size is 2^sssz
            var sssz = BitConverter.ToUInt16(span[NextBytes(2)]);
            MiniSectorSize = 1 << sssz;
            ValidateMiniSectorSize();
            
            // Skip 6 bytes (reserved) - must be 0 but we don't check
            _ = NextBytes(6);

            DirectorySectorCount = BitConverter.ToUInt32(span[NextBytes(4)]);
            ValidateDirectorySectorCount();
            
            FATSectorCount = BitConverter.ToUInt32(span[NextBytes(4)]);
            FirstDirectorySectorSectorNumber = BitConverter.ToUInt32(span[NextBytes(4)]);

            Range NextBytes(int len)
            {
                var range = new Range(offset, offset + len);
                offset += len;
                return range;
            }
        }

        private void ValidateDirectorySectorCount()
        {
            if (Version == 3 && DirectorySectorCount != 0)
                throw new McdfException($"Directory sector count must be 0, found {DirectorySectorCount}");
        }

        private void ValidateMiniSectorSize()
        {
            if (MiniSectorSize != 64)
                throw new McdfException($"Mini sector size must be 64, found {MiniSectorSize}");
        }

        private static void ValidateByteOrder(ushort byteOrder)
        {
            if (byteOrder != LittleEndianByteOrder)
                throw new NotSupportedException("Big-endian files are not supported");
        }

        private void ValidateSectorSize()
        {
            var expectedSectorSize = Version == 3 ? 512 : 4096;
            if (SectorSize != expectedSectorSize)
                throw new McdfException($"Sector size must be {expectedSectorSize}, found {SectorSize}");
        }

        /// <summary>
        /// File revision, is usually 0x3E (62).
        /// </summary>
        public ushort Revision { get; }
        
        /// <summary>
        /// File version, is always 3 or 4.
        /// </summary>
        public ushort Version { get; }
        
        /// <summary>
        /// Sector size of regular streams. Is always 512 for version 3 files and 4096 for version 4 files.
        /// </summary>
        public int SectorSize { get; }
        
        /// <summary>
        /// Sector size of mini streams. Is always 64.
        /// </summary>
        public int MiniSectorSize { get; }
        
        /// <summary>
        /// Sector count of the directory. Is always 0 for version 3 files.
        /// </summary>
        public uint DirectorySectorCount { get; }
        
        /// <summary>
        /// Sector count of the File Allocation Table.
        /// </summary>
        public uint FATSectorCount { get; }

        /// <summary>
        /// Sector number of the first directory sector.
        /// </summary>
        public uint FirstDirectorySectorSectorNumber { get; }
    }
}