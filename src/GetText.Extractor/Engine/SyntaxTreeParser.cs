using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using GetText.Extractor.Engine.SourceResolver;
using GetText.Extractor.Template;

using Microsoft.CodeAnalysis.CSharp;

namespace GetText.Extractor.Engine
{
    internal class SyntaxTreeParser : ParserBase
    {
        private readonly IList<FileInfo> sources;

        public SyntaxTreeParser(CatalogTemplate catalog, IList<FileInfo> sources, bool unixStyle, bool verbose, Aliases aliases) :
            base(catalog, unixStyle, verbose, aliases)
        {
            this.sources = sources;
            catalog.Header.ProjectIdVersion = Path.GetFileNameWithoutExtension(sources.First().Name);
        }

        public override async Task Parse()
        {
            await Parallel.ForEachAsync(sources, async (fileInfo, token) =>
            {
                DirectorySourceResolver sourceResolver = new DirectorySourceResolver(fileInfo);
                await Parallel.ForEachAsync(sourceResolver.GetInput(), async (fileName, token) =>
                {
                    using (StreamReader reader = File.OpenText(fileName))
                    {
                        string sourceCode = await reader.ReadToEndAsync().ConfigureAwait(false);
                        GetStrings(CSharpSyntaxTree.ParseText(sourceCode, null, fileName));
                    }

                    Interlocked.Increment(ref Counter);
                }).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }
}
