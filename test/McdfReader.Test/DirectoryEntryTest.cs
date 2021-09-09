using System;
using NUnit.Framework;
using static McdfReader.Test.DirectoryEntryTestUtil;

namespace McdfReader.Test
{
    [TestFixture]
    public class DirectoryEntryTest
    {
        [Test]
        public void Extracts_name()
        {
            var data = DataWithName("abc");
            
            var entry = Entry(data);
            
            Assert.That(entry.Name, Is.EqualTo("abc"));
        }
        
        [Test]
        public void Sets_name_to_empty_when_data_is_blank()
        {
            var data = EmptyData();
            
            var entry = Entry(data);
            
            Assert.That(entry.Name, Is.EqualTo(""));
        }
        
        [TestCase(0, ObjectType.UnknownOrUnallocated)]
        [TestCase(1, ObjectType.StorageObject)]
        [TestCase(2, ObjectType.StreamObject)]
        [TestCase(5, ObjectType.RootStorageObject)]
        [TestCase(3, ObjectType.UnknownOrUnallocated)]
        public void Reads_object_type(byte value, object expected)
        {
            var data = DataWithName("test");
            Array.Copy(new[] { value }, 0, data, 66, 1);
            
            var entry = Entry(data);
            
            Assert.That(entry.ObjectType, Is.EqualTo(expected));
        }
        
        [Test]
        public void Reads_left_sibling()
        {
            var data = DataWithName("test");
            Array.Copy(new byte[] { 0x01, 0x00, 0x00, 0x00 }, 0, data, 68, 4);
            
            var entry = Entry(data);
            
            Assert.That(entry.LeftSiblingID, Is.EqualTo(1u));
        }
        
        [Test]
        public void Reads_right_sibling()
        {
            var data = DataWithName("test");
            Array.Copy(new byte[] { 0x02, 0x00, 0x00, 0x00 }, 0, data, 72, 4);
            
            var entry = Entry(data);
            
            Assert.That(entry.RightSiblingID, Is.EqualTo(2u));
        }
        
        [Test]
        public void Reads_child()
        {
            var data = DataWithName("test");
            Array.Copy(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, 0, data, 76, 4);
            
            var entry = Entry(data);
            
            Assert.That(entry.ChildID, Is.EqualTo(StreamIDs.NoStream));
        }
        
        [Test]
        public void Rejects_invalid_name_length()
        {
            var data = DataWithName("test");
            Array.Copy(new byte[] { 0x41, 0x00 }, 0, data, 64, 2);
            
            Assert.Multiple(() =>
            {
                var ex = Assert.Catch<McdfException>(() => Entry(data));
                Assert.That(ex?.Message, Is.EqualTo("Name length must be at most 64, found 65"));
            });
        }
    }
}