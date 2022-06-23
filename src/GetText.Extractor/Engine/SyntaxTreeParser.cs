using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using GetText.Extractor.Engine.SourceResolver;
using GetText.Extractor.Template;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace GetText.Extractor.Engine
{
    internal class SyntaxTreeParser : ParserBase
    {
        private readonly IList<FileInfo> sources;

        public SyntaxTreeParser(CatalogTemplate catalog, IList<FileInfo> sources, bool unixStyle, bool verbose) :
            base(catalog, unixStyle, verbose)
        {
            this.sources = sources;
            catalog.Header.ProjectIdVersion = Path.GetFileNameWithoutExtension(sources.First().Name);
        }

        public override async Task Parse()
        {
            //https://stackoverflow.com/questions/11564506/nesting-await-in-parallel-foreach?rq=1
            TransformBlock<string, SyntaxTree> inputBlock = new TransformBlock<string, SyntaxTree>
                (async fileName =>
                {
                    using (StreamReader reader = File.OpenText(fileName))
                    {
                        string sourceCode = await reader.ReadToEndAsync().ConfigureAwait(false);
                        return CSharpSyntaxTree.ParseText(sourceCode, null, fileName);
                    }
                },
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 });

            ActionBlock<SyntaxTree> actionBlock = new ActionBlock<SyntaxTree>(tree => GetStrings(tree));
            inputBlock.LinkTo(actionBlock, new DataflowLinkOptions { PropagateCompletion = true });

            foreach (var source in sources)
            {
                DirectorySourceResolver sourceResolver = new DirectorySourceResolver(source);
                foreach (string fileName in sourceResolver.GetInput())
                {
                    inputBlock.Post(fileName);
                }
            }

            inputBlock.Complete();
            await actionBlock.Completion.ConfigureAwait(false);
        }
    }
}
