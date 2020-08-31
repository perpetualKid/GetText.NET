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
        internal static readonly List<string> ControlTextProperties = new List<string>() { "Text", "HeaderText", "ToolTipText", };
        internal static readonly List<string> ControlTextMethods = new List<string>() { "SetToolTip" };

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
            foreach (InvocationExpressionSyntax item in root.DescendantNodes().OfType<InvocationExpressionSyntax>().
                Where((item) => CatalogMethods.Contains(methodName = ((item.Expression as MemberAccessExpressionSyntax)?.Name as IdentifierNameSyntax)?.Identifier.ValueText)))
            {
                List<ArgumentSyntax> arguments = item.DescendantNodes().OfType<ArgumentSyntax>().ToList();
                switch (methodName)
                {
                    case "GetString":   //first argument is message id
                        messageId = ExtractText(arguments[0]);
                        catalog.AddOrUpdateEntry(null, messageId, $"{pathRelative}:{arguments[0].GetLocation().GetLineSpan().StartLinePosition.Line + 1}");
                        break;
                    case "GetParticularString": //first argument is context, second is message id
                        context = ExtractText(arguments[0]);
                        messageId = ExtractText(arguments[1]);
                        catalog.AddOrUpdateEntry(context, messageId, $"{pathRelative}:{arguments[1].GetLocation().GetLineSpan().StartLinePosition.Line + 1}");
                        break;
                    case "GetPluralString": //first argument is message id, second is plural message
                        messageId = ExtractText(arguments[0]);
                        plural = ExtractText(arguments[1]);
                        catalog.AddOrUpdateEntry(null, messageId, plural, $"{pathRelative}:{arguments[0].GetLocation().GetLineSpan().StartLinePosition.Line + 1}");
                        break;
                    case "GetParticularPluralString": //first argument is context, second is message id, third is plural message
                        context = ExtractText(arguments[0]);
                        messageId = ExtractText(arguments[1]);
                        plural = ExtractText(arguments[2]);
                        catalog.AddOrUpdateEntry(context, messageId, plural, $"{pathRelative}:{arguments[1].GetLocation().GetLineSpan().StartLinePosition.Line + 1}");
                        break;
                }
            }
            foreach (AssignmentExpressionSyntax item in root.DescendantNodes().OfType<AssignmentExpressionSyntax>().
                Where((item) => ControlTextProperties.Contains(((item.Left as MemberAccessExpressionSyntax)?.Name as IdentifierNameSyntax)?.Identifier.ValueText)))
            {
                if (item.Right.IsKind(SyntaxKind.InvocationExpression)) //maybe log for verbose output?
                    continue;

                messageId = ExtractText(item.Right);
                //this skips the case when Windows Forms Controls still have there default text which is set to the control name (identifier) 
                if (item.Left.DescendantNodes().OfType<IdentifierNameSyntax>().Reverse().ElementAtOrDefault(1)?.Identifier.ValueText.Equals(messageId, StringComparison.OrdinalIgnoreCase) ?? false)
                    continue;
                catalog.AddOrUpdateEntry(null, messageId, $"{pathRelative}:{item.Right.GetLocation().GetLineSpan().StartLinePosition.Line + 1}");
            }
            foreach (InvocationExpressionSyntax item in root.DescendantNodes().OfType<InvocationExpressionSyntax>().
                Where((item) => ControlTextMethods.Contains(methodName = ((item.Expression as MemberAccessExpressionSyntax)?.Name as IdentifierNameSyntax)?.Identifier.ValueText)))
            {
                List<ArgumentSyntax> arguments = item.DescendantNodes().OfType<ArgumentSyntax>().ToList();
                switch (methodName)
                {
                    case "SetToolTip":
                        messageId = ExtractText(arguments[1]);
                        catalog.AddOrUpdateEntry(null, messageId, $"{pathRelative}:{arguments[1].GetLocation().GetLineSpan().StartLinePosition.Line + 1}");
                        break;
                }
            }
        }

        protected static string ToLiteral(string valueTextForCompiler) => SymbolDisplay.FormatLiteral(valueTextForCompiler, false);

        private static string ExtractText(CSharpSyntaxNode syntaxNode)
        {
            StringBuilder builder = new StringBuilder();

            void ExtractFromStringNode(CSharpSyntaxNode stringNode)
            {
                int i = 0;
                switch (stringNode.Kind())
                {
                    case SyntaxKind.InterpolatedStringExpression:
                        foreach (InterpolatedStringContentSyntax item in (stringNode as InterpolatedStringExpressionSyntax).Contents)
                        {
                            if (item.Kind() == SyntaxKind.InterpolatedStringText)
                                builder.Append(ToLiteral((item as InterpolatedStringTextSyntax)?.TextToken.ValueText));
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

            if (syntaxNode.IsKind(SyntaxKind.InterpolatedStringExpression) || syntaxNode.IsKind(SyntaxKind.StringLiteralExpression))
            {
                ExtractFromStringNode(syntaxNode);
            }
            else
            {
                foreach (CSharpSyntaxNode stringNode in syntaxNode.DescendantNodes().
                    Where((node) => node.IsKind(SyntaxKind.InterpolatedStringExpression) || node.IsKind(SyntaxKind.StringLiteralExpression)))
                {
                    ExtractFromStringNode(stringNode);
                }
            }
            return builder.ToString();
        }
    }
}
