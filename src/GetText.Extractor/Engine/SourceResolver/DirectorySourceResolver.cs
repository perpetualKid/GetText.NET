using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GetText.Extractor.Engine.SourceResolver
{
    internal class DirectorySourceResolver : SourceResolverBase<string>
    {
        private static readonly string[] excludedFolders = new string[] { "obj", "packages", "bin", ".vs", ".git" };
        private static readonly string[] fileTypes = new string[] { "*.cs", "*.razor", "*.cshtml" };

        public DirectorySourceResolver(FileInfo sourcePath) :
            base(sourcePath.Attributes.HasFlag(FileAttributes.Directory) ? sourcePath : new FileInfo(sourcePath.DirectoryName))
        {
        }

        public override IEnumerable<string> GetInput()
        {
            return GetCSharpFilesFromFolder(sourcePath.FullName);
        }

        private static IEnumerable<string> GetCSharpFilesFromFolder(string folder)
        {
            return fileTypes.SelectMany(fileType => Directory.EnumerateFiles(folder, fileType)).Concat(
                Directory.EnumerateDirectories(folder).Where(d => !excludedFolders.Contains(Path.GetFileName(d), StringComparer.OrdinalIgnoreCase)).
                SelectMany(subdir => GetCSharpFilesFromFolder(subdir)));
        }
    }
}
