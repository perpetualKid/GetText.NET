using System.Collections.Generic;
using System.CommandLine;
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
            Option sort = CommandLineOptions.SortOutput;

            rootCommand.Add(sourceOption);
            rootCommand.Add(outFile);
            rootCommand.Add(verbose);
            rootCommand.Add(unixPathSeparator);
            rootCommand.Add(sort);

            rootCommand.SetHandler(async (IList<FileInfo> sources, FileInfo target, bool unixstyle, bool sortOutput, bool verbose) =>
            {
                await Execute(sources, target, unixstyle, sortOutput, verbose).ConfigureAwait(false);
            }, sourceOption, outFile, unixPathSeparator, sort, verbose);

            return await rootCommand.InvokeAsync(args).ConfigureAwait(false);

        }

        private static async Task Execute(IList<FileInfo> sources, FileInfo target, bool unixStyle, bool sortOutput, bool verbose)
        {
            catalog = new CatalogTemplate(target.FullName);

            SyntaxTreeParser parser = new SyntaxTreeParser(catalog, sources, unixStyle, verbose);
            await parser.Parse().ConfigureAwait(false);

            await catalog.WriteAsync(sortOutput).ConfigureAwait(false);

        }
    }
}
