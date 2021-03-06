﻿using System.CommandLine;
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
            rootCommand.Add(CommandLineOptions.SourceOption);
            rootCommand.Add(CommandLineOptions.OutFile);
            //            rootCommand.Add(CommandLineOptions.Merge);
            rootCommand.Add(CommandLineOptions.Verbose);
            rootCommand.Add(CommandLineOptions.UseUnixPathSeparator);

            rootCommand.Handler = CommandHandler.Create(async (FileInfo source, FileInfo target, bool unixstyle, bool verbose) =>
            {
                await Execute(source, target, unixstyle, verbose).ConfigureAwait(false);
            });

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
