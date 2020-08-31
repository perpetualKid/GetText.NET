using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using GetText.Extractor.Engine.SourceResolver;
using GetText.Extractor.Template;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace GetText.Extractor.Engine
{
    internal class SyntaxTreeParser: ParserBase<string>
    {
        public SyntaxTreeParser(CatalogTemplate catalog, FileInfo sourceFolder, bool verbose): base(catalog, sourceFolder, verbose)
        {
            sourceResolver = new DirectorySourceResolver(sourceFolder);
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

            foreach (string fileName in sourceResolver.GetInput()) 
                inputBlock.Post(fileName);

            inputBlock.Complete();
            await actionBlock.Completion.ConfigureAwait(false);
        }
    }
}
