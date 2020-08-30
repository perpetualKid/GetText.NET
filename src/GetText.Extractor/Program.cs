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
using GetText.Extractor.Engine;
using GetText.Extractor.Engine.SourceResolver;
using GetText.Extractor.Template;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
            rootCommand.Add(CommandLineOptions.Merge);

            rootCommand.Handler = CommandHandler.Create(async (FileInfo source, FileInfo target, bool merge) =>
            {
                await Execute(source, target).ConfigureAwait(false);
            });

            return await rootCommand.InvokeAsync(args).ConfigureAwait(false);

        }

        private static async Task Execute(FileInfo source, FileInfo target)
        {
            catalog = new CatalogTemplate(target.FullName);

            SyntaxTreeParser parser = new SyntaxTreeParser(catalog, source);
            await parser.Parse().ConfigureAwait(false);

            await catalog.WriteAsync().ConfigureAwait(false);

        }
    }
}
