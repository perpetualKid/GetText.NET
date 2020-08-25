using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using GetText.Extractor.Template;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[assembly: InternalsVisibleTo("GetText.Extractor.Tests")]

namespace GetText.Extractor
{
    class Program
    {
        internal static CatalogTemplate catalog;
        internal static string catalogPath;

        static async Task Main(string[] args)
        {

            string solutionDir = Path.GetFullPath(@"C:\Storage\Dev\Scratch\ConsoleApp3");
            catalog = new CatalogTemplate(Path.Combine(solutionDir, "messages.pot"));
            catalogPath = Path.GetDirectoryName(catalog.FileName);
            //            string solutionDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\..\examples\examples.HelloForms"));
            var allCSharpFiles = GetAllCSharpCode(solutionDir);
            PrintStrings(allCSharpFiles);

            allCSharpFiles.ForEach(f =>
            {
                Console.WriteLine($"\r\n{f}");
                PrintStrings(GetStrings(File.ReadAllText(f), f));
            });

            await catalog.WriteAsync().ConfigureAwait(false);

        }

        public static string GetRelativeUri(string uriString, string relativeUriString)
        {
            if ((!uriString.EndsWith("\\") || !uriString.EndsWith("/")) &&
                (relativeUriString.EndsWith("\\") || relativeUriString.EndsWith("/")))
                relativeUriString += "dummy";
            Uri fileUri = new Uri(uriString);
            Uri dirUri = new Uri(relativeUriString);
            Uri relativeUri = dirUri.MakeRelativeUri(fileUri);
            return relativeUri.ToString();
        }

        static void PrintStrings(List<string> list)
        {
            list.ForEach(Console.WriteLine);
        }

        private static string[] skippedDirs = new string[] { "obj", "packages" };
        static bool SkipDirectory(string directory)
        {
            var lcDir = directory.ToLower();
            return skippedDirs.Any(sd => directory.EndsWith("\\" + sd));
        }

        static List<string> GetAllCSharpCode(string folder)
        {
            List<string> result = new List<string>();
            result.AddRange(Directory.GetFiles(folder, "*.cs"));
            foreach (var directory in Directory.GetDirectories(folder).Where(d => !SkipDirectory(d)))
            {

                result.AddRange(GetAllCSharpCode(directory));
            }

            return result;
        }

        private static List<string> GetStrings(string sourceCode, string path)
        {
            SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceCode, null, path);
            var collector = new StringsCollector();
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
            collector.Visit(root);
            return collector._strings;
        }

        private static List<string> GetStrings(SyntaxTree tree)
        {
            List<string> result = new List<string>();
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
            foreach (var interpolationString in root.DescendantNodes().OfType<InterpolatedStringExpressionSyntax>())
            {
                StringBuilder builder = new StringBuilder();
                int i = 0;
                foreach (var item in interpolationString.Contents)
                {
                    if (item.Kind() == SyntaxKind.InterpolatedStringText)
                        builder.Append(item.ToString());
                    else if (item.Kind() == SyntaxKind.Interpolation)
                        builder.Append($"{{{i++}}}");
                    else
                        Console.WriteLine(item.Kind());
                }
                result.Add($"{builder} ({interpolationString.GetLocation().GetLineSpan().StartLinePosition.Line})");
            }
            foreach (var literalString in root.DescendantNodes().OfType<LiteralExpressionSyntax>().Where((node) => node.IsKind(SyntaxKind.StringLiteralExpression)))
            {
                // StringLiteralToken stringLiteralToken = node.Token;
                // Console.WriteLine(node.Token.Value);
                result.Add($"{(string)literalString.Token.Value} ({literalString.GetLocation().GetLineSpan().StartLinePosition.Line})");
            }

            return result;
        }

    }
    class StringsCollector : CSharpSyntaxWalker
    {
        public List<String> _strings = new List<string>();

        public override void VisitInterpolatedStringExpression(InterpolatedStringExpressionSyntax node)
        {
            if (node.IsKind(SyntaxKind.InterpolatedStringExpression))
            {
//                _strings.Add($"{node.Contents} ({node.GetLocation().GetLineSpan().StartLinePosition.Line})");
                var contents = node.Contents;
                StringBuilder builder = new StringBuilder();
                int i = 0;
                foreach (var item in node.Contents)
                {
                    if (item.Kind() == SyntaxKind.InterpolatedStringText)
                        builder.Append(item);//.ToString());
                    else if (item.Kind() == SyntaxKind.Interpolation)
                        builder.Append($"{{{i++}}}");
                    else
                        Console.WriteLine(item.Kind());
                }
                CatalogEntry entry = Program.catalog.AddOrUpdateEntry(null, builder.ToString());
                string pathRelative = PathExtended.GetRelativePath(Program.catalogPath, node.SyntaxTree.FilePath);
//                string relativeUrl = Program.GetRelativeUri(node.SyntaxTree.FilePath, AppDomain.CurrentDomain.BaseDirectory);
                entry.Comments.References.Add($"{pathRelative}:{node.GetLocation().GetLineSpan().StartLinePosition.Line + 1}");
//                entry.Comments.References.Add($"{node.SyntaxTree.FilePath}:{node.GetLocation().GetLineSpan().StartLinePosition.Line}");
                _strings.Add($"{builder} ({node.GetLocation().GetLineSpan().StartLinePosition.Line + 1})");
                FormattableString formattableString = FormattableStringFactory.Create(builder.ToString(), new object());
                //                string test = FormattableString.Invariant(FormattableStringFactory.Create(node.Contents.ToString()));
            }
            else
                Console.WriteLine("Something odd");
        }

        public override void VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            if (node.IsKind(SyntaxKind.StringLiteralExpression))
            {
                // StringLiteralToken stringLiteralToken = node.Token;
                // Console.WriteLine(node.Token.Value);
                _strings.Add($"{(string)node.Token.Value} ({node.GetLocation().GetLineSpan().StartLinePosition.Line})");
            }
        }
    }

    public static class PathExtended
    {
        private const int FILE_ATTRIBUTE_DIRECTORY = 0x10;
        private const int FILE_ATTRIBUTE_NORMAL = 0x80;
        private const int MaximumPath = 260;

        public static string GetRelativePath(string fromPath, string toPath)
        {
            var fromAttribute = GetPathAttribute(fromPath);
            var toAttribute = GetPathAttribute(toPath);

            var stringBuilder = new StringBuilder(MaximumPath);
            if (PathRelativePathTo(
                stringBuilder,
                fromPath,
                fromAttribute,
                toPath,
                toAttribute) == 0)
            {
                throw new ArgumentException("Paths must have a common prefix.");
            }

            return stringBuilder.ToString();
        }

        private static int GetPathAttribute(string path)
        {
            var directory = new DirectoryInfo(path);
            if (directory.Exists)
            {
                return FILE_ATTRIBUTE_DIRECTORY;
            }

            var file = new FileInfo(path);
            if (file.Exists)
            {
                return FILE_ATTRIBUTE_NORMAL;
            }

            throw new FileNotFoundException(
                "A file or directory with the specified path was not found.",
                path);
        }

        [DllImport("shlwapi.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        private static extern int PathRelativePathTo(
            StringBuilder pszPath,
            string pszFrom,
            int dwAttrFrom,
            string pszTo,
            int dwAttrTo);
    }

}
