using System;
using System.Buffers;
using System.Threading.Tasks;
using NUnit.Framework;

namespace McdfReader.Test
{
    public abstract class CompoundFileTests
    {
        public class DocFile
        {
            [Test]
            public async Task Read()
            {
                await using var fileStream = TestFile.HelloWorldDoc.Open();
                using var cf = await CompoundFile.FromStream(fileStream, default);

                var fatArray = await cf.ReadFAT(default);
                var directorySecNo = cf.Header.FirstDirectorySectorSectorNumber;
                while (directorySecNo != SectorNumbers.EndOfChain)
                {
                    await cf.ReadSectorAsync(directorySecNo, mem =>
                    {
                        const int dirEntrySize = 512 / 4;
                        var offs = 0;
                        while (offs < mem.Length)
                        {
                            var entryMem = mem[offs..(offs + dirEntrySize)];
                            var entry = new DirectoryEntry(entryMem);
                            
                            Console.WriteLine("Entry: " + entry);
                            
                            offs += dirEntrySize;
                        }

                        return 0;
                    }, default);

                    directorySecNo = fatArray[directorySecNo];
                }
            }
        }
    }
}