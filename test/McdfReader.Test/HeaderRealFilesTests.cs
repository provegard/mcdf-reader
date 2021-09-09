using System.Buffers;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace McdfReader.Test
{
    public abstract class HeaderRealFilesTests
    {
        public class DocFile
        {
            private Header _header = default!;

            [OneTimeSetUp]
            public async Task Read_header()
            {
                await using var fileStream = TestFile.HelloWorldDoc.Open();
                using var memOwner = MemoryPool<byte>.Shared.Rent(512);
                var memory = memOwner.Memory;
                
                await fileStream.ReadAsync(memory);

                _header = new Header(memory);
            }

            [Test]
            public void Reads_revision()
                => Assert.That(_header.Revision, Is.EqualTo(0x3E));

            [Test]
            public void Reads_version()
                => Assert.That(_header.Version, Is.EqualTo(0x3));

            [Test]
            public void Reads_sector_size()
                => Assert.That(_header.SectorSize, Is.EqualTo(512));

            [Test]
            public void Reads_mini_sector_size()
                => Assert.That(_header.MiniSectorSize, Is.EqualTo(64));

            [Test]
            public void Reads_directory_sector_count()
                => Assert.That(_header.DirectorySectorCount, Is.EqualTo(0));

            [Test]
            public void Reads_FAT_sector_count()
                => Assert.That(_header.FATSectorCount, Is.EqualTo(1));

            [Test]
            public void Reads_first_directory_sector_number()
                => Assert.That(_header.FirstDirectorySectorSectorNumber, Is.EqualTo(47));

            [Test]
            public void Reads_mini_stream_cutoff_size()
                => Assert.That(_header.MiniStreamCutoffSize, Is.EqualTo(4096));

            [Test]
            public void Reads_first_mini_FAT_sector_number()
                => Assert.That(_header.FirstMiniFATSectorNumber, Is.EqualTo(49));

            [Test]
            public void Reads_mini_FAT_sector_count()
                => Assert.That(_header.MiniFATSectorCount, Is.EqualTo(1));

            [Test]
            public void Reads_first_DIFAT_sector_number()
                => Assert.That(_header.FirstDIFATSectorNumber, Is.EqualTo(SectorNumbers.EndOfChain));

            [Test]
            public void Reads_DIFAT_sector_count()
                => Assert.That(_header.DIFATSectorCount, Is.EqualTo(0));

            [Test]
            public void Reads_DIFAT_embedded_in_header()
            {
                var difat = _header.DIFAT;
                Assert.Multiple(() =>
                {
                    Assert.That(difat.Count, Is.EqualTo(109));

                    var firstTwo = difat.Take(2).ToList();
                    Assert.That(firstTwo, Is.EqualTo(new[] { 46ul, SectorNumbers.FreeSector }));
                });
            }
        }
    }
}