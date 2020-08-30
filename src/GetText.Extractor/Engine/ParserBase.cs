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
        //keep the order as we access some logic through index lookup for performance reason
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
            string pathRelative = PathExtension.GetRelativePath(catalog.FileName, tree.FilePath);
            string messageId, context, plural;
            string methodName = null;
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
            foreach(InvocationExpressionSyntax item in root.DescendantNodes().OfType<InvocationExpressionSyntax>().
                Where((item) => CatalogMethods.Contains(methodName = ((item.Expression as MemberAccessExpressionSyntax)?.Name as IdentifierNameSyntax)?.Identifier.ValueText)))
            {
                List<ArgumentSyntax> arguments = item.DescendantNodes().OfType<ArgumentSyntax>().ToList();
                switch (methodName)
                {
                    case "GetString":   //first argument is message id
                        messageId = ExtractFromArgument(arguments[0]);
                        catalog.AddOrUpdateEntry(null, messageId, $"{pathRelative}:{arguments[0].GetLocation().GetLineSpan().StartLinePosition.Line + 1}");
                        break;
                    case "GetParticularString": //first argument is context, second is message id
                        context = ExtractFromArgument(arguments[0]);
                        messageId = ExtractFromArgument(arguments[1]);
                        catalog.AddOrUpdateEntry(context, messageId, $"{pathRelative}:{arguments[0].GetLocation().GetLineSpan().StartLinePosition.Line + 1}");
                        break;
                    case "GetPluralString": //first argument is message id, second is plural message
                        messageId = ExtractFromArgument(arguments[0]);
                        plural = ExtractFromArgument(arguments[1]);
                        catalog.AddOrUpdateEntry(null, messageId, plural, $"{pathRelative}:{arguments[0].GetLocation().GetLineSpan().StartLinePosition.Line + 1}");
                        break;
                    case "GetParticularPluralString": //first argument is context, second is message id, third is plural message
                        context = ExtractFromArgument(arguments[0]);
                        messageId = ExtractFromArgument(arguments[1]);
                        plural = ExtractFromArgument(arguments[2]);
                        catalog.AddOrUpdateEntry(context, messageId, plural, $"{pathRelative}:{arguments[0].GetLocation().GetLineSpan().StartLinePosition.Line + 1}");
                        break;
                }
            }
        }

        protected static string ToLiteral(string valueTextForCompiler) => SymbolDisplay.FormatLiteral(valueTextForCompiler, false);

        private static string ExtractFromArgument(ArgumentSyntax argument)
        {
            StringBuilder builder = new StringBuilder();
            int i = 0;
            foreach(SyntaxNode stringNode in argument.DescendantNodes().Where((node) => node.IsKind(SyntaxKind.InterpolatedStringExpression) || node.IsKind(SyntaxKind.StringLiteralExpression)))
            {
                switch (stringNode.Kind())
                {
                    case SyntaxKind.InterpolatedStringExpression:
                        foreach (InterpolatedStringContentSyntax item in (stringNode as InterpolatedStringExpressionSyntax).Contents)
                        {
                            if (item.Kind() == SyntaxKind.InterpolatedStringText)
                                builder.Append((item as InterpolatedStringTextSyntax)?.TextToken.ValueText);
                            else if (item.Kind() == SyntaxKind.Interpolation) //TODO 20200830 add format expression
                                builder.Append($"{{{i++}}}");
                            else
                                throw new InvalidDataException(item.Kind().ToString());
                        }
                        break;
                    case SyntaxKind.StringLiteralExpression:
                        builder.Append(ToLiteral((stringNode as LiteralExpressionSyntax).Token.Value?.ToString()));
                        break;
                }
            }
            return builder.ToString();
        }
    }
}
