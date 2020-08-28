using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using GetText.Extractor.CommandLine;
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

        private static async Task<int> Main(string[] args)
        {
            RootCommand rootCommand = CommandLineOptions.RootCommand;
            rootCommand.Add(CommandLineOptions.SourceOption);
            rootCommand.Add(CommandLineOptions.OutFile);
            rootCommand.Add(CommandLineOptions.Merge);

            rootCommand.Handler = CommandHandler.Create(async (FileInfo source, FileInfo target, bool merge) =>
            {
                await Execute(source, target).ConfigureAwait(false);
            });

            return await rootCommand.InvokeAsync(args).ConfigureAwait(false);

        }

        static async Task Execute(FileInfo source, FileInfo target)
        {

            string solutionDir;
            if ((source.Attributes | FileAttributes.Directory) == FileAttributes.Directory)
            {
                solutionDir = source.FullName;
            }
            else
            {
                solutionDir = source.DirectoryName;
            }
            catalog = new CatalogTemplate(target.FullName);
            catalogPath = Path.GetDirectoryName(catalog.FileName);
            var allCSharpFiles = GetAllCSharpCode(solutionDir);
            PrintStrings(allCSharpFiles);

            allCSharpFiles.ForEach(f =>
            {
                Console.WriteLine($"\r\n{f}");
                PrintStrings(GetStrings(File.ReadAllText(f), f));
            });

            await catalog.WriteAsync().ConfigureAwait(false);

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
            return GetStrings(tree);
        }

        private static List<string> GetStrings(SyntaxTree tree)
        {
            List<string> result = new List<string>();
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
            foreach (var interpolationString in root.DescendantNodes().OfType<InterpolatedStringExpressionSyntax>().
                Where((item) => (((item.Ancestors().OfType<InvocationExpressionSyntax>()?.FirstOrDefault()?.Expression as MemberAccessExpressionSyntax)?.Name as IdentifierNameSyntax)?.Identifier.ValueText == "GetString")))
            {
                StringBuilder builder = new StringBuilder();
                int i = 0;
                foreach (var item in interpolationString.Contents)
                {
                    if (item.Kind() == SyntaxKind.InterpolatedStringText)
                        builder.Append((item as InterpolatedStringTextSyntax)?.TextToken.ValueText);
                    else if (item.Kind() == SyntaxKind.Interpolation)
                        builder.Append($"{{{i++}}}");
                    else
                        Console.WriteLine(item.Kind());
                }
                CatalogEntry entry = catalog.AddOrUpdateEntry(null, ToLiteral(builder.ToString()));
                string pathRelative = PathExtension.GetRelativePath(catalogPath, tree.FilePath);
                entry.Comments.References.Add($"{pathRelative}:{interpolationString.GetLocation().GetLineSpan().StartLinePosition.Line + 1}");
                result.Add($"{builder} ({interpolationString.GetLocation().GetLineSpan().StartLinePosition.Line})");
            }
            foreach (var literalString in root.DescendantNodes().OfType<LiteralExpressionSyntax>().Where((node) => node.IsKind(SyntaxKind.StringLiteralExpression)).
                Where((item) => (((item.Ancestors().OfType<InvocationExpressionSyntax>()?.FirstOrDefault()?.Expression as MemberAccessExpressionSyntax)?.Name as IdentifierNameSyntax)?.Identifier.ValueText == "GetString")))
            {
                CatalogEntry entry = catalog.AddOrUpdateEntry(null, ToLiteral(literalString.Token.Value?.ToString()));
                string pathRelative = PathExtension.GetRelativePath(catalogPath, tree.FilePath);
                entry.Comments.References.Add($"{pathRelative}:{literalString.GetLocation().GetLineSpan().StartLinePosition.Line + 1}");
                result.Add($"{(string)literalString.Token.Value} ({literalString.GetLocation().GetLineSpan().StartLinePosition.Line})");
            }

            return result;
        }

        private static string ToLiteral(string valueTextForCompiler) => SymbolDisplay.FormatLiteral(valueTextForCompiler, false);
    }
}
