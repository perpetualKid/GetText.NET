using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GetText.Extractor.Engine.SourceResolver
{
    internal class XamlSourceResolver : SourceResolverBase<string>
    {
        private static readonly string[] excludedFolders = new string[] { "obj", "packages", "bin", ".vs", ".git" };

        public XamlSourceResolver(FileInfo sourcePath) :
            base(sourcePath.Attributes.HasFlag(FileAttributes.Directory) ? sourcePath : new FileInfo(sourcePath.DirectoryName))
        {
        }

        public override IEnumerable<string> GetInput()
        {
            return GetXamlFilesFromFolder(sourcePath.FullName);
        }

        private static IEnumerable<string> GetXamlFilesFromFolder(string folder)
        {
            return Directory.EnumerateFiles(folder, "*.xaml").Concat(
                Directory.EnumerateDirectories(folder).Where(d => !excludedFolders.Contains(Path.GetFileName(d), StringComparer.OrdinalIgnoreCase)).
                SelectMany(subdir => GetXamlFilesFromFolder(subdir)));
        }
    }
}
