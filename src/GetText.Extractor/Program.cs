using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            Option aliasString = CommandLineOptions.GetStringAliases;
            Option aliasDefinite = CommandLineOptions.GetParticularStringAliases;
            Option aliasPlural = CommandLineOptions.GetPluralStringAliases;
            Option aliasDefinitePlural = CommandLineOptions.GetParticularPluralStringAliases;

            rootCommand.Add(sourceOption);
            rootCommand.Add(outFile);
            rootCommand.Add(verbose);
            rootCommand.Add(unixPathSeparator);
            rootCommand.Add(sort);
            rootCommand.Add(aliasString);
            rootCommand.Add(aliasDefinite);
            rootCommand.Add(aliasPlural);
            rootCommand.Add(aliasDefinitePlural);

            rootCommand.SetHandler(async (IList<FileInfo> sources, FileInfo target, bool unixstyle, bool sortOutput, bool verbose,
                List<string> @as, List<string> ad, List<string> ap, List<string> adp) =>
            {
                await Execute(sources, target, unixstyle, sortOutput, verbose, @as, ad, ap, adp).ConfigureAwait(false);
            }, sourceOption, outFile, unixPathSeparator, sort, verbose, aliasString, aliasDefinite, aliasPlural, aliasDefinitePlural);

            return await rootCommand.InvokeAsync(args).ConfigureAwait(false);

        }

        private static async Task Execute(IList<FileInfo> sources, FileInfo target, bool unixStyle, bool sortOutput, bool verbose,
            List<string> getStringAliases, List<string> getParticularStringAliases, List<string> getPluralStringAliases, List<string> getParticularPluralStringAliases)
        {
            Stopwatch stopwatch = null;
            if (verbose)
            {
                stopwatch = new Stopwatch();
                stopwatch.Start();
            }

            catalog = new CatalogTemplate(target.FullName);

            SyntaxTreeParser parser = new SyntaxTreeParser(catalog, sources, unixStyle, verbose, new Aliases()
            {
                GetString = getStringAliases,
                GetParticularString = getParticularStringAliases,
                GetPluralString = getPluralStringAliases,
                GetParticularPluralString = getParticularPluralStringAliases
            });
            await parser.Parse().ConfigureAwait(false);

            await catalog.WriteAsync(sortOutput).ConfigureAwait(false);
            if (verbose)
            {
                stopwatch.Stop();
                System.Console.WriteLine($"Processed {parser.Counter} files in {stopwatch.Elapsed.TotalSeconds:N2}sec.");
                System.Console.WriteLine($"Found {catalog.entries.Count} distinct messages in {catalog.entries.Sum(entry => entry.Value.References.Count)} source references.");
            }
        }
    }
}
