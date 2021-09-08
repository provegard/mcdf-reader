using System;
using NUnit.Framework;

namespace McdfReader.Test
{
    public class HeaderTest
    {
        private const ulong FileId = 0xE11AB1A1E011CFD0;

        [Test]
        public void Rejects_span_less_than_512_bytes()
        {
            var mem = new ReadOnlyMemory<byte>(new byte[511]);
            Assert.Catch<ArgumentOutOfRangeException>(() => _ = new Header(mem));
        }
        
        [Test]
        public void Rejects_invalid_file_identifier()
        {
            var mem = new ReadOnlyMemory<byte>(new byte[512]);
            Assert.Multiple(() =>
            {
                var ex = Assert.Catch<McdfException>(() => _ = new Header(mem));
                Assert.That(ex?.Message, Is.EqualTo("Invalid file ID, expected D0CF11E0A1B11AE1 but found 0000000000000000"));
            });
        }
        
        [Test]
        public void Rejects_big_endian_file()
        {
            var data = HeaderDataWithFileId();
            Array.Copy(new byte[] { 0xFF, 0xFE }, 0, data, 28, 2);
            Array.Copy(new byte[] { 0x03, 0x00 }, 0, data, 26, 2);
            
            var mem = new ReadOnlyMemory<byte>(data);
            Assert.Multiple(() =>
            {
                var ex = Assert.Catch<NotSupportedException>(() => _ = new Header(mem));
                Assert.That(ex?.Message, Is.EqualTo("Big-endian files are not supported"));
            });
        }
        
        [Test]
        public void Rejects_wrong_version()
        {
            var data = HeaderDataWithFileId();
            Array.Copy(new byte[] { 0xFE, 0xFF }, 0, data, 28, 2);
            Array.Copy(new byte[] { 0x05, 0x00 }, 0, data, 26, 2);
            
            var mem = new ReadOnlyMemory<byte>(data);
            Assert.Multiple(() =>
            {
                var ex = Assert.Catch<McdfException>(() => _ = new Header(mem));
                Assert.That(ex?.Message, Is.EqualTo("Version must be 3 or 4, found 5"));
            });
        }
        
        [TestCase(3, 512)]
        [TestCase(4, 4096)]
        public void Rejects_wrong_sector_size(int version, int expected)
        {
            var data = HeaderDataWithFileId();
            Array.Copy(new byte[] { 0xFE, 0xFF }, 0, data, 28, 2);
            Array.Copy(new byte[] { (byte) version, 0x00 }, 0, data, 26, 2);
            Array.Copy(new byte[] { 0x0A, 0x00 }, 0, data, 30, 2); // 2 << 10 => 1024 bytes
            
            var mem = new ReadOnlyMemory<byte>(data);
            Assert.Multiple(() =>
            {
                var ex = Assert.Catch<McdfException>(() => _ = new Header(mem));
                Assert.That(ex?.Message, Is.EqualTo($"Sector size must be {expected}, found 1024"));
            });
        }
        
        [Test]
        public void Rejects_wrong_mini_sector_size()
        {
            var data = HeaderDataWithFileId();
            Array.Copy(new byte[] { 0xFE, 0xFF }, 0, data, 28, 2);
            Array.Copy(new byte[] { 0x03, 0x00 }, 0, data, 26, 2);
            Array.Copy(new byte[] { 0x09, 0x00 }, 0, data, 30, 2);
            Array.Copy(new byte[] { 0x05, 0x00 }, 0, data, 32, 2);
            
            var mem = new ReadOnlyMemory<byte>(data);
            Assert.Multiple(() =>
            {
                var ex = Assert.Catch<McdfException>(() => _ = new Header(mem));
                Assert.That(ex?.Message, Is.EqualTo("Mini sector size must be 64, found 32"));
            });
        }
        
        [Test]
        public void Rejects_wrong_directory_sector_count()
        {
            var data = HeaderDataWithFileId();
            Array.Copy(new byte[] { 0xFE, 0xFF }, 0, data, 28, 2);
            Array.Copy(new byte[] { 0x03, 0x00 }, 0, data, 26, 2);
            Array.Copy(new byte[] { 0x09, 0x00 }, 0, data, 30, 2);
            Array.Copy(new byte[] { 0x06, 0x00 }, 0, data, 32, 2);
            Array.Copy(new byte[] { 0x01, 0x00, 0x00, 0x00 }, 0, data, 40, 4);
            
            var mem = new ReadOnlyMemory<byte>(data);
            Assert.Multiple(() =>
            {
                var ex = Assert.Catch<McdfException>(() => _ = new Header(mem));
                Assert.That(ex?.Message, Is.EqualTo("Directory sector count must be 0, found 1"));
            });
        }
        
        [Test]
        public void Accepts_non_zero_directory_sector_count_for_version_4()
        {
            var data = HeaderDataWithFileId();
            Array.Copy(new byte[] { 0xFE, 0xFF }, 0, data, 28, 2);
            Array.Copy(new byte[] { 0x04, 0x00 }, 0, data, 26, 2);
            Array.Copy(new byte[] { 0x0C, 0x00 }, 0, data, 30, 2);
            Array.Copy(new byte[] { 0x06, 0x00 }, 0, data, 32, 2);
            Array.Copy(new byte[] { 0x01, 0x00, 0x00, 0x00 }, 0, data, 40, 4);
            Array.Copy(new byte[] { 0x00, 0x10, 0x00, 0x00 }, 0, data, 56, 4);
            
            var mem = new ReadOnlyMemory<byte>(data);
            var header = new Header(mem);
            Assert.That(header.DirectorySectorCount, Is.EqualTo(1));
        }

        private byte[] HeaderDataWithFileId()
        {
            var data = new byte[512];
            Array.Copy(BitConverter.GetBytes(FileId), data, 8);
            return data;
        }
    }
}