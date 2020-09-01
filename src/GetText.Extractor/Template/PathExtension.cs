using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace GetText.Extractor.Template
{
    internal static class PathExtension
    {
        private const int FILE_ATTRIBUTE_DIRECTORY = 0x10;
        private const int FILE_ATTRIBUTE_NORMAL = 0x80;
        private const int MaximumPath = 260;

        /// <summary>
        /// Returns a relative path from one path to another.
        /// </summary>
        /// <param name="relativeTo">The source path the result should be relative to. This path is always considered to be a directory.</param>
        /// <param name="path">The destination path.</param>
        /// <returns></returns>
        public static string GetRelativePath(string relativeTo, string path)
        {
            int sourceAttribute = GetPathAttribute(relativeTo);
            int targetAttribute = GetPathAttribute(path);

            StringBuilder result = new StringBuilder(MaximumPath);
            if (NativeMethods.PathRelativePathTo(result, relativeTo, sourceAttribute, path, targetAttribute) == 0)
            {
                throw new ArgumentException("Paths must have a common prefix.");
            }

            return result.ToString();
        }

        private static int GetPathAttribute(string path)
        {
            if (Directory.Exists(path))
            {
                return FILE_ATTRIBUTE_DIRECTORY;
            }
            else if (File.Exists(path))
            {
                return FILE_ATTRIBUTE_NORMAL;
            }
            else
                return FILE_ATTRIBUTE_NORMAL;
            //throw new FileNotFoundException("A file or directory with the specified path was not found.", path);
        }
    }

    internal static class NativeMethods
    {
        [DllImport("shlwapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern int PathRelativePathTo(StringBuilder pszPath, string pszFrom, int dwAttrFrom, string pszTo, int dwAttrTo);
    }
}
