using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using GetText.Extractor.CommandLine;
using GetText.Extractor.Engine;
using GetText.Extractor.Template;

[assembly: InternalsVisibleTo("GetText.Extractor.Tests")]

namespace GetText.Extractor
{
    internal class Program
    {
        internal static CatalogTemplate catalog;

        private static async Task<int> Main(string[] args)
        {
            RootCommand rootCommand = CommandLineOptions.RootCommand;
            Option sourceOption = CommandLineOptions.SourceOption;
            Option outFile = CommandLineOptions.OutFile;
            Option verbose = CommandLineOptions.Verbose;
            Option unixPathSeparator = CommandLineOptions.UseUnixPathSeparator;
            rootCommand.Add(sourceOption);
            rootCommand.Add(outFile);
            rootCommand.Add(verbose);
            rootCommand.Add(unixPathSeparator);

            rootCommand.SetHandler(async (FileInfo source, FileInfo target, bool unixstyle, bool sortOutput, bool verbose) =>
            {
                await Execute(source, target, unixstyle, verbose).ConfigureAwait(false);
            }, sourceOption, outFile, verbose, unixPathSeparator);

            return await rootCommand.InvokeAsync(args).ConfigureAwait(false);

        }

        private static async Task Execute(FileInfo source, FileInfo target, bool unixStyle, bool verbose)
        {
            catalog = new CatalogTemplate(target.FullName);

            SyntaxTreeParser parser = new SyntaxTreeParser(catalog, source, unixStyle, verbose);
            await parser.Parse().ConfigureAwait(false);

            await catalog.WriteAsync().ConfigureAwait(false);

        }
    }
}
