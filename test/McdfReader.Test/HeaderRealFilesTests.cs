using System.Buffers;
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
        }
    }
}