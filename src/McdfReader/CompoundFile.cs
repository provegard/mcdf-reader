using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace McdfReader
{
    public sealed class CompoundFile : IDisposable
    {
        private readonly Stream _stream;
        private readonly long _streamStartPosition;
        private readonly bool _dispose;

        public Header Header { get; } // TODO: temp public

        private CompoundFile(Stream stream, Header header, long streamStartPosition, bool dispose)
        {
            _stream = stream;
            Header = header;
            _streamStartPosition = streamStartPosition;
            _dispose = dispose;
        }

        public static async Task<CompoundFile> FromStream(Stream stream, CancellationToken ct)
        {
            // TODO: Check read + seek
            var startPos = stream.Position;
            
            using var headerMemoryOwner = MemoryPool<byte>.Shared.Rent(512);
            var memory = headerMemoryOwner.Memory;
            await stream.ReadAsync(memory, ct);
            var header = new Header(memory);
            return new CompoundFile(stream, header, startPos, false);
        }

        public async Task<T> ReadSectorAsync<T>(uint sectorNumber, Func<ReadOnlyMemory<byte>, T> reader, CancellationToken ct)
        {
            // Sector 0 starts after the header
            var offset = Header.SectorSize * (sectorNumber + 1);
            SeekTo(offset);

            using var sectorMemoryOwner = MemoryPool<byte>.Shared.Rent(Header.SectorSize);
            var memory = sectorMemoryOwner.Memory;
            await _stream.ReadAsync(memory, ct);
            return reader(memory);
        }

        public IAsyncEnumerable<uint> ReadFATSectorNumbers(CancellationToken ct)
        {
            if (Header.DIFATSectorCount > 0)
                throw new NotImplementedException("TODO: Implement DIFAT sector reading");

            // Read the sector numbers that are included in the header.
            var sectorNumbers = Header.DIFAT.TakeWhile(secNum => secNum <= SectorNumbers.MaxRegularSector);
            return sectorNumbers.ToAsyncEnumerable();
        }

        public async Task<uint[]> ReadFAT(CancellationToken ct)
        {
            var fatSectorNumbers = await ReadFATSectorNumbers(ct).ToListAsync(ct);
            var sectorNumCountInFAT = fatSectorNumbers.Count * (Header.SectorSize / 4);
            var fat = new uint[sectorNumCountInFAT];
            var idx = 0;
            foreach (var fatSecNum in fatSectorNumbers)
            {
                await ReadSectorAsync(fatSecNum, mem =>
                {
                    var span = mem.Span;
                    foreach (var secNum in SectorUtil.ReadSectorNumbers(ref span))
                    {
                        fat[idx++] = secNum;
                    }
                    return 0;
                }, default);
            }

            return fat;
        }

        private void SeekTo(long pos)
        {
            _stream.Seek(_streamStartPosition + pos, SeekOrigin.Begin);
        }

        // public IAsyncEnumerable<ReadOnlyMemory<byte>> ReadSectorsAsync(IEnumerable<uint> sectorNumbers, CancellationToken ct)
        // {
        //     // TODO: Optimize for contiguous reading.
        //     
        // }

        public void Dispose()
        {
            if (_dispose)
            {
                _stream.Dispose();
            }
        }
    }
}