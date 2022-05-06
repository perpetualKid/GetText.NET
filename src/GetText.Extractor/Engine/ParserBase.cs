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
        internal static readonly List<string> CatalogMethods = new List<string>() {
            "GetString", "GetParticularString", "GetPluralString", "GetParticularPluralString",
            "_", "_p", "_n", "_pn"
        };
        internal static readonly List<string> ControlTextProperties = new List<string>() { "Text", "HeaderText", "ToolTipText", };

        internal static readonly List<string> ControlTextMethods = new List<string>() { "SetToolTip" };

        internal static readonly List<string> DescriptionAttributes = new List<string>() { "Description", "DescriptionAttribute" };

        protected CatalogTemplate catalog;
        protected SourceResolverBase<T> sourceResolver;
        protected FileInfo sourceRoot;
        protected bool verbose;
        protected bool unixStyle;

        public ParserBase(CatalogTemplate catalog, FileInfo sourceRoot, bool unixStyle, bool verbose)
        {
            this.catalog = catalog;
            this.sourceRoot = sourceRoot;
            this.verbose = verbose;
            this.unixStyle = unixStyle;
        }

        public abstract Task Parse();

        protected void GetStrings(SyntaxTree tree)
        {
            string pathRelative = Path.GetRelativePath(catalog.FileName, tree.FilePath);
            string messageId, context, plural;
            string methodName = null;
            bool isFormatString;
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
            foreach (InvocationExpressionSyntax item in root.DescendantNodes().OfType<InvocationExpressionSyntax>().
                Where((item) => CatalogMethods.Contains(methodName = ((item.Expression as MemberAccessExpressionSyntax)?.Name as IdentifierNameSyntax)?.Identifier.ValueText) ||
                    CatalogMethods.Contains(methodName = ((item.Expression as  IdentifierNameSyntax)?.Identifier.ValueText))
                    ))
            {
                List<ArgumentSyntax> arguments = item.DescendantNodes().OfType<ArgumentSyntax>().ToList();
                switch (methodName)
                {
                    case "GetString": //first argument is message id
                    case "_":
                        messageId = ExtractText(arguments[0]);
                        isFormatString = arguments.Count > 1 || arguments[0].DescendantNodes().OfType<InterpolationSyntax>().Any();
                        catalog.AddOrUpdateEntry(null, messageId, $"{pathRelative}:{arguments[0].GetLocation().GetLineSpan().StartLinePosition.Line + 1}", isFormatString);
                        ;
                        break;
                    case "GetParticularString": //first argument is context, second is message id
                    case "_p":
                        context = ExtractText(arguments[0]);
                        messageId = ExtractText(arguments[1]);
                        isFormatString = arguments.Count > 2 || arguments[1].DescendantNodes().OfType<InterpolationSyntax>().Any();
                        catalog.AddOrUpdateEntry(context, messageId, $"{pathRelative}:{arguments[1].GetLocation().GetLineSpan().StartLinePosition.Line + 1}", isFormatString);
                        break;
                    case "GetPluralString": //first argument is message id, second is plural message
                    case "_n":
                        messageId = ExtractText(arguments[0]);
                        plural = ExtractText(arguments[1]);
                        isFormatString = arguments.Count > 2 || arguments[0].DescendantNodes().OfType<InterpolationSyntax>().Any() || arguments[1].DescendantNodes().OfType<InterpolationSyntax>().Any();
                        catalog.AddOrUpdateEntry(null, messageId, plural, $"{pathRelative}:{arguments[0].GetLocation().GetLineSpan().StartLinePosition.Line + 1}", isFormatString);
                        break;
                    case "GetParticularPluralString": //first argument is context, second is message id, third is plural message
                    case "_pn":
                        context = ExtractText(arguments[0]);
                        messageId = ExtractText(arguments[1]);
                        plural = ExtractText(arguments[2]);
                        isFormatString = arguments.Count > 3 || arguments[1].DescendantNodes().OfType<InterpolationSyntax>().Any() || arguments[2].DescendantNodes().OfType<InterpolationSyntax>().Any();
                        catalog.AddOrUpdateEntry(context, messageId, plural, $"{pathRelative}:{arguments[1].GetLocation().GetLineSpan().StartLinePosition.Line + 1}", isFormatString);
                        break;
                }
            }
            foreach (AssignmentExpressionSyntax item in root.DescendantNodes().OfType<AssignmentExpressionSyntax>().
                Where((item) => ControlTextProperties.Contains(((item.Left as MemberAccessExpressionSyntax)?.Name as IdentifierNameSyntax)?.Identifier.ValueText)))
            {
                if (item.Right.IsKind(SyntaxKind.InvocationExpression)) //maybe log for verbose output?
                {
                    continue;
                }
                if (item.Right.IsKind(SyntaxKind.ConditionalExpression))
                {
                    ExpressionSyntax whenTrue = (item.Right as ConditionalExpressionSyntax).WhenTrue;
                    messageId = ExtractText(whenTrue);
                    isFormatString = whenTrue.DescendantNodes().OfType<ArgumentSyntax>().Any() || whenTrue.DescendantNodes().OfType<InterpolationSyntax>().Any();
                    catalog.AddOrUpdateEntry(null, messageId, $"{pathRelative}:{whenTrue.GetLocation().GetLineSpan().StartLinePosition.Line + 1}", isFormatString);

                    ExpressionSyntax whenFalse = (item.Right as ConditionalExpressionSyntax).WhenFalse;
                    messageId = ExtractText(whenFalse);
                    isFormatString = whenFalse.DescendantNodes().OfType<ArgumentSyntax>().Any() || whenFalse.DescendantNodes().OfType<InterpolationSyntax>().Any();
                    catalog.AddOrUpdateEntry(null, messageId, $"{pathRelative}:{whenFalse.GetLocation().GetLineSpan().StartLinePosition.Line + 1}", isFormatString);
                }
                else
                {
                    messageId = ExtractText(item.Right);
                    isFormatString = item.Right.DescendantNodes().OfType<InterpolationSyntax>().Any();
                    //this skips the case when Windows Forms Controls still have there default text which is set to the control name (identifier) 
                    if (item.Left.DescendantNodes().OfType<IdentifierNameSyntax>().Reverse().ElementAtOrDefault(1)?.Identifier.ValueText.Equals(messageId, StringComparison.OrdinalIgnoreCase) ?? false)
                        continue;

                    catalog.AddOrUpdateEntry(null, messageId, $"{pathRelative}:{item.Right.GetLocation().GetLineSpan().StartLinePosition.Line + 1}", isFormatString);
                }
            }
            foreach (InvocationExpressionSyntax item in root.DescendantNodes().OfType<InvocationExpressionSyntax>().
                Where((item) => ControlTextMethods.Contains(methodName = ((item.Expression as MemberAccessExpressionSyntax)?.Name as IdentifierNameSyntax)?.Identifier.ValueText)))
            {
                List<ArgumentSyntax> arguments = item.DescendantNodes().OfType<ArgumentSyntax>().ToList();
                switch (methodName)
                {
                    case "SetToolTip":
                        messageId = ExtractText(arguments[1]);
                        isFormatString = arguments[1].DescendantNodes().OfType<InterpolationSyntax>().Any();
                        catalog.AddOrUpdateEntry(null, messageId, $"{pathRelative}:{arguments[1].GetLocation().GetLineSpan().StartLinePosition.Line + 1}", isFormatString);
                        break;
                }
            }
            foreach (EnumDeclarationSyntax item in root.DescendantNodes().OfType<EnumDeclarationSyntax>().
                Where((item) =>
                    item.DescendantNodes().OfType<AttributeSyntax>().
                    SelectMany((attribute) => attribute.DescendantNodes().OfType<IdentifierNameSyntax>()).
                    Select((identifier) => identifier.Identifier.ValueText).Any((identifier) =>
                    DescriptionAttributes.Contains(identifier))))
            {
                // check if there is a description attribute right on the enum declaration itself
                AttributeSyntax descriptionAttribute = item.ChildNodes().OfType<AttributeListSyntax>().FirstOrDefault()?.Attributes.
                    Where((attribute) => attribute.DescendantNodes().OfType<IdentifierNameSyntax>().
                    Any((identifier) => DescriptionAttributes.Contains(identifier.Identifier.ValueText)))?.FirstOrDefault();
                context = (null != descriptionAttribute) ? ExtractText(descriptionAttribute.ArgumentList) : null;

                foreach (EnumMemberDeclarationSyntax member in item.Members)
                {
                    descriptionAttribute = member.ChildNodes().OfType<AttributeListSyntax>().FirstOrDefault()?.Attributes.
                    Where((attribute) => attribute.DescendantNodes().OfType<IdentifierNameSyntax>().
                    Any((identifier) => DescriptionAttributes.Contains(identifier.Identifier.ValueText)))?.FirstOrDefault();
                    messageId = (null != descriptionAttribute) ? ExtractText(descriptionAttribute.ArgumentList) : null;

                    catalog.AddOrUpdateEntry(context, messageId, $"{pathRelative}:{member.GetLocation().GetLineSpan().StartLinePosition.Line + 1}", false);
                }
            }
        }

        protected static string ToLiteral(string valueTextForCompiler)
        {
            return SymbolDisplay.FormatLiteral(valueTextForCompiler, false);
        }

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
                            else if (item.Kind() == SyntaxKind.Interpolation)
                            {
                                InterpolationAlignmentClauseSyntax alignment = item.DescendantNodes().OfType<InterpolationAlignmentClauseSyntax>().FirstOrDefault();
                                InterpolationFormatClauseSyntax format = item.DescendantNodes().OfType<InterpolationFormatClauseSyntax>().FirstOrDefault();
                                builder.Append($"{{{i++}{alignment}{format}}}");
                            }
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
