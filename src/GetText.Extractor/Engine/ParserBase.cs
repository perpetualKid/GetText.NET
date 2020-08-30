using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GetText.Extractor.Engine.SourceResolver;
using GetText.Extractor.Template;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GetText.Extractor.Engine
{
    internal abstract class ParserBase<T>
    {
        //hardcoded as we don't have a reference to GetText.ICatalog in this package
        internal static readonly List<string> CatalogMethods = new List<string>() { "GetString", "GetParticularString", "GetPluralString", "GetParticularPluralString" };

        protected CatalogTemplate catalog;
        protected SourceResolverBase<T> sourceResolver;
        protected FileInfo sourceRoot;

        public ParserBase(CatalogTemplate catalog, FileInfo sourceRoot)
        {
            this.catalog = catalog;
            this.sourceRoot = sourceRoot;
        }

        public abstract Task Parse();

        protected void GetStrings(SyntaxTree tree)
        {
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
            foreach (var interpolationString in root.DescendantNodes().OfType<InterpolatedStringExpressionSyntax>().
                Where((item) => CatalogMethods.Contains(((item.Ancestors().OfType<InvocationExpressionSyntax>()?.FirstOrDefault()?.Expression as MemberAccessExpressionSyntax)?.Name as IdentifierNameSyntax)?.Identifier.ValueText)))
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
                string pathRelative = PathExtension.GetRelativePath(catalog.FileName, tree.FilePath);
                entry.Comments.References.Add($"{pathRelative}:{interpolationString.GetLocation().GetLineSpan().StartLinePosition.Line + 1}");
            }
            foreach (var literalString in root.DescendantNodes().OfType<LiteralExpressionSyntax>().Where((node) => node.IsKind(SyntaxKind.StringLiteralExpression)).
                Where((item) => CatalogMethods.Contains(item.Ancestors().OfType<InvocationExpressionSyntax>().FirstOrDefault()?.DescendantNodes().OfType<IdentifierNameSyntax>().LastOrDefault()?.Identifier.ValueText)))
            {
                CatalogEntry entry = catalog.AddOrUpdateEntry(null, ToLiteral(literalString.Token.Value?.ToString()));
                string pathRelative = PathExtension.GetRelativePath(catalog.FileName, tree.FilePath);
                entry.Comments.References.Add($"{pathRelative}:{literalString.GetLocation().GetLineSpan().StartLinePosition.Line + 1}");
            }
        }

        protected static string ToLiteral(string valueTextForCompiler) => SymbolDisplay.FormatLiteral(valueTextForCompiler, false);

    }
}
