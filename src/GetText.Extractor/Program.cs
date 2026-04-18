using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
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
            // Typed locals are required — parseResult.GetValue(option) needs the same instance
            Option<IList<FileInfo>> sourceOption = CommandLineOptions.SourceOption;
            Option<FileInfo> outFile = CommandLineOptions.OutFile;
            Option<bool> merge = CommandLineOptions.Merge;
            Option<bool> verbose = CommandLineOptions.Verbose;
            Option<bool> unixPathSeparator = CommandLineOptions.UseUnixPathSeparator;
            Option<bool> sort = CommandLineOptions.SortOutput;
            Option<List<string>> aliasString = CommandLineOptions.GetStringAliases;
            Option<List<string>> aliasDefinite = CommandLineOptions.GetParticularStringAliases;
            Option<List<string>> aliasPlural = CommandLineOptions.GetPluralStringAliases;
            Option<List<string>> aliasDefinitePlural = CommandLineOptions.GetParticularPluralStringAliases;

            RootCommand rootCommand = CommandLineOptions.RootCommand;
            rootCommand.Options.Add(sourceOption);
            rootCommand.Options.Add(outFile);
            rootCommand.Options.Add(merge);
            rootCommand.Options.Add(verbose);
            rootCommand.Options.Add(unixPathSeparator);
            rootCommand.Options.Add(sort);
            rootCommand.Options.Add(aliasString);
            rootCommand.Options.Add(aliasDefinite);
            rootCommand.Options.Add(aliasPlural);
            rootCommand.Options.Add(aliasDefinitePlural);

            rootCommand.SetAction(async (parseResult, cancellationToken) =>
            {
                await Execute(
                    parseResult.GetValue(sourceOption),
                    parseResult.GetValue(outFile),
                    parseResult.GetValue(unixPathSeparator),
                    parseResult.GetValue(sort),
                    parseResult.GetValue(verbose),
                    parseResult.GetValue(aliasString),
                    parseResult.GetValue(aliasDefinite),
                    parseResult.GetValue(aliasPlural),
                    parseResult.GetValue(aliasDefinitePlural),
                    cancellationToken
                ).ConfigureAwait(false);
            });

            return await rootCommand.Parse(args).InvokeAsync().ConfigureAwait(false);
        }

        private static async Task Execute(IList<FileInfo> sources, FileInfo target, bool unixStyle, bool sortOutput, bool verbose,
            List<string> getStringAliases, List<string> getParticularStringAliases, List<string> getPluralStringAliases,
            List<string> getParticularPluralStringAliases, CancellationToken cancellationToken = default)
        {
            Stopwatch stopwatch = null;
            if (verbose)
            {
                stopwatch = Stopwatch.StartNew();
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
                Console.WriteLine($"Processed {parser.Counter} files in {stopwatch.Elapsed.TotalSeconds:N2}sec.");
                Console.WriteLine($"Found {catalog.entries.Count} distinct messages in {catalog.entries.Sum(entry => entry.Value.References.Count)} source references.");
            }
        }
    }
}
