using System;
using System.IO;

namespace McdfReader.Test
{
    using Directory=System.IO.Directory;
    internal class TestFile
    {
        internal static readonly TestFile HelloWorldDoc = new("hello-world.doc");

        private readonly string _filename;

        private TestFile(string filename)
        {
            _filename = filename;
        }

        internal Stream Open() => File.OpenRead(Path.Combine(RootPath, "test", "files", _filename));

        private static string? _rootPath;
        private static string RootPath
        {
            get
            {
                if (_rootPath == null)
                {
                    var path = Directory.GetCurrentDirectory();
                    while (path != null && !IsRoot(path))
                    {
                        path = Directory.GetParent(path)?.FullName;
                    }

                    _rootPath = path ?? throw new Exception("Failed to find the repository root path");
                }

                return _rootPath;

                static bool IsRoot(string p)
                    => File.Exists(Path.Combine(p, "Main.sln"));
            }
        }
    }
}