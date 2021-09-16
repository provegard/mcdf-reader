using System;
using System.Collections.Generic;
using NUnit.Framework;
using static McdfReader.Test.DirectoryEntryTestUtil;

namespace McdfReader.Test
{
    [TestFixture]
    public class DirectoryTest
    {
        [Test]
        public void Identifies_root_entry()
        {
            var entry = Entry(DataWithName("root"));
            var directory = Directory(entry);

            var root = directory.GetRoot();
            
            Assert.That(root.Name, Is.EqualTo("root"));
        }
        
        [Test]
        public void Can_get_entry_by_ID()
        {
            var entry1 = Entry(DataWithName("root"));
            var entry2 = Entry(DataWithName("data"));
            var directory = Directory(entry1, entry2);

            var entry = directory.GetEntry(1);
            
            Assert.That(entry.Name, Is.EqualTo("data"));
        }
        
        [Test]
        public void Throws_if_asked_entry_ID_is_too_large()
        {
            var entry1 = Entry(DataWithName("root"));
            var entry2 = Entry(DataWithName("data"));
            var directory = Directory(entry1, entry2);

            Assert.Multiple(() =>
            {
                var ex = Assert.Catch<ArgumentOutOfRangeException>(() => directory.GetEntry(2));
                Assert.That(ex?.Message, Is.EqualTo("Invalid stream ID, max = 1 (Parameter 'id')"));
            });
        }

        [Test]
        public void Visits_child()
        {
            var root = NewEntry("root", StreamIDs.NoStream, StreamIDs.NoStream, 1);
            var child = NewEntry("child", StreamIDs.NoStream, StreamIDs.NoStream, StreamIDs.NoStream);

            var directory = Directory(root, child);
            var names = CollectNames(directory);

            Assert.That(names, Is.EqualTo(new[] { "root", "child" }));
        }
        
        [Test]
        public void Visits_left_sibling()
        {
            var root = NewEntry("root", StreamIDs.NoStream, StreamIDs.NoStream, 2);
            var leftSibling = NewEntry("left", StreamIDs.NoStream, StreamIDs.NoStream, StreamIDs.NoStream);
            var child = NewEntry("child", 1, StreamIDs.NoStream, StreamIDs.NoStream);

            var directory = Directory(root, leftSibling, child);
            var names = CollectNames(directory);

            Assert.That(names, Is.EqualTo(new[] { "root", "left", "child" }));
        }
        
        [Test]
        public void Visits_right_sibling()
        {
            var root = NewEntry("root", StreamIDs.NoStream, StreamIDs.NoStream, 2);
            var leftSibling = NewEntry("right", StreamIDs.NoStream, StreamIDs.NoStream, StreamIDs.NoStream);
            var child = NewEntry("child", StreamIDs.NoStream, 1, StreamIDs.NoStream);

            var directory = Directory(root, leftSibling, child);
            var names = CollectNames(directory);

            Assert.That(names, Is.EqualTo(new[] { "root", "child", "right" }));
        }
        
        [Test]
        public void Visits_all_siblings_before_child()
        {
            // Verify that the grand child (which is a child of 'child') is visited last.
            var root = NewEntry("root", StreamIDs.NoStream, StreamIDs.NoStream, 3);
            var leftOfLeftSibling = NewEntry("left-left", StreamIDs.NoStream, StreamIDs.NoStream, StreamIDs.NoStream);
            var leftSibling = NewEntry("left", 1, StreamIDs.NoStream, StreamIDs.NoStream);
            var child = NewEntry("child", 2, 4, 6);
            var rightSibling = NewEntry("right", StreamIDs.NoStream, 5, StreamIDs.NoStream);
            var rightOfRightSibling = NewEntry("right-right", StreamIDs.NoStream, StreamIDs.NoStream, StreamIDs.NoStream);
            var grandChild = NewEntry("grand-child", StreamIDs.NoStream, StreamIDs.NoStream, StreamIDs.NoStream);

            var directory = Directory(root, leftOfLeftSibling, leftSibling, child, rightSibling, rightOfRightSibling, grandChild);
            var names = CollectNames(directory);

            Assert.That(names, Is.EqualTo(new[] { "root", "left-left", "left", "child", "right", "right-right", "grand-child" }));
        }
        
        [Test]
        public void Visits_all_siblings_before_child_of_sibling()
        {
            // Verify that the grand child (which is a child of 'left-left') is visited last.
            var root = NewEntry("root", StreamIDs.NoStream, StreamIDs.NoStream, 3);
            var leftOfLeftSibling = NewEntry("left-left", StreamIDs.NoStream, StreamIDs.NoStream, 6);
            var leftSibling = NewEntry("left", 1, StreamIDs.NoStream, StreamIDs.NoStream);
            var child = NewEntry("child", 2, 4, StreamIDs.NoStream);
            var rightSibling = NewEntry("right", StreamIDs.NoStream, 5, StreamIDs.NoStream);
            var rightOfRightSibling = NewEntry("right-right", StreamIDs.NoStream, StreamIDs.NoStream, StreamIDs.NoStream);
            var grandChild = NewEntry("grand-child", StreamIDs.NoStream, StreamIDs.NoStream, StreamIDs.NoStream);

            var directory = Directory(root, leftOfLeftSibling, leftSibling, child, rightSibling, rightOfRightSibling, grandChild);
            var names = CollectNames(directory);

            Assert.That(names, Is.EqualTo(new[] { "root", "left-left", "left", "child", "right", "right-right", "grand-child" }));
        }

        private IList<string> CollectNames(Directory directory)
        {
            var names = new List<string>();
            directory.Visit(entry => names.Add(entry.Name));
            return names;
        }

        private Directory Directory(params DirectoryEntry[] entries)
            => new(entries);

        private DirectoryEntry NewEntry(string name, uint leftId, uint rightId, uint childId)
        {
            if (name.Length > 31) throw new ArgumentOutOfRangeException(nameof(name), "Max name length is 31");
            var data = DataWithName(name);
            Array.Copy(BitConverter.GetBytes(leftId), 0, data, 68, 4);
            Array.Copy(BitConverter.GetBytes(rightId), 0, data, 72, 4);
            Array.Copy(BitConverter.GetBytes(childId), 0, data, 76, 4);
            return Entry(data);
        }
    }
}