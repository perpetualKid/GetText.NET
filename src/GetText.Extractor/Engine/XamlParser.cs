using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using GetText.Extractor.Engine.SourceResolver;
using GetText.Extractor.Template;

namespace GetText.Extractor.Engine
{
    internal class XamlParser : ParserBase
    {
        private static readonly HashSet<string> TranslatableAttributes = new HashSet<string>
        {
            "Content",
            "Header",
            "HeaderText",
            "Label",
            "Text",
            "Title",
            "ToolTip",
        };

        private static readonly HashSet<string> DirectTextElements = new HashSet<string>
        {
            "AccessText",
            "Run",
            "TextBlock",
        };

        private readonly IList<FileInfo> sources;

        public XamlParser(CatalogTemplate catalog, IList<FileInfo> sources, bool unixStyle, bool verbose, Aliases aliases) :
            base(catalog, unixStyle, verbose, aliases)
        {
            this.sources = sources;
        }

        public override async Task Parse()
        {
            await Parallel.ForEachAsync(sources, async (fileInfo, token) =>
            {
                XamlSourceResolver sourceResolver = new XamlSourceResolver(fileInfo);
                await Parallel.ForEachAsync(sourceResolver.GetInput(), async (fileName, token) =>
                {
                    string sourceCode = await File.ReadAllTextAsync(fileName, token).ConfigureAwait(false);
                    GetStrings(sourceCode, fileName);
                    Interlocked.Increment(ref Counter);
                }).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        private void GetStrings(string sourceCode, string fileName)
        {
            XDocument document;
            try
            {
                document = XDocument.Parse(sourceCode, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            }
            catch (XmlException)
            {
                return;
            }

            string pathRelative = Path.GetRelativePath(Path.GetDirectoryName(catalog.FileName), fileName);
            if (unixStyle)
                pathRelative = pathRelative.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            foreach (XElement element in document.Descendants())
            {
                ExtractAttributes(element, pathRelative);
                ExtractDirectText(element, pathRelative);
            }
        }

        private void ExtractAttributes(XElement element, string pathRelative)
        {
            foreach (XAttribute attribute in element.Attributes())
            {
                string value = attribute.Value;
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                XamlCatalogString markupExtensionString = TryParseGettextMarkupExtension(value);
                if (markupExtensionString != null)
                {
                    AddEntry(markupExtensionString, pathRelative, GetLineNumber(attribute));
                    continue;
                }

                if (!TranslatableAttributes.Contains(attribute.Name.LocalName) || IsMarkupExtension(value))
                    continue;

                catalog.AddOrUpdateEntry(null, ToLiteral(value), BuildReference(pathRelative, GetLineNumber(attribute)), false);
            }
        }

        private void ExtractDirectText(XElement element, string pathRelative)
        {
            if (!DirectTextElements.Contains(element.Name.LocalName))
                return;

            foreach (XText textNode in element.Nodes().OfType<XText>())
            {
                if (string.IsNullOrWhiteSpace(textNode.Value))
                    continue;

                catalog.AddOrUpdateEntry(null, ToLiteral(textNode.Value), BuildReference(pathRelative, GetLineNumber(textNode)), false);
            }
        }

        private void AddEntry(XamlCatalogString entry, string pathRelative, int lineNumber)
        {
            string reference = BuildReference(pathRelative, lineNumber);
            if (string.IsNullOrEmpty(entry.PluralText))
                catalog.AddOrUpdateEntry(entry.Context, ToLiteral(entry.Text), reference, false);
            else
                catalog.AddOrUpdateEntry(entry.Context, ToLiteral(entry.Text), ToLiteral(entry.PluralText), reference, false);
        }

        private XamlCatalogString TryParseGettextMarkupExtension(string value)
        {
            if (!IsMarkupExtension(value) || value.StartsWith("{}", StringComparison.Ordinal))
                return null;

            string expression = value.Substring(1, value.Length - 2).Trim();
            if (expression.Length == 0)
                return null;

            int separatorIndex = expression.IndexOfAny(new[] { ' ', '\t', '\r', '\n', ',' });
            string extensionName = separatorIndex < 0 ? expression : expression.Substring(0, separatorIndex);
            string extensionLocalName = GetLocalName(extensionName);
            if (!IsGettextExtensionName(extensionLocalName))
                return null;

            string argumentText = separatorIndex < 0 ? string.Empty : expression.Substring(separatorIndex).Trim();
            XamlCatalogString result = ParseMarkupExtensionArguments(argumentText);
            SplitContext(result);
            return string.IsNullOrEmpty(result.Text) ? null : result;
        }

        private XamlCatalogString ParseMarkupExtensionArguments(string argumentText)
        {
            XamlCatalogString result = new XamlCatalogString();
            if (string.IsNullOrEmpty(argumentText))
                return result;

            List<string> arguments = SplitMarkupExtensionArguments(argumentText).ToList();
            foreach (string argument in arguments)
            {
                int equalsIndex = argument.IndexOf('=');
                if (equalsIndex <= 0)
                    continue;

                string name = argument.Substring(0, equalsIndex).Trim();
                string value = Unquote(argument.Substring(equalsIndex + 1).Trim());
                switch (name)
                {
                    case "Context":
                        result.Context = value;
                        break;
                    case "PluralText":
                        result.PluralText = value;
                        break;
                    case "Text":
                        result.Text = value;
                        break;
                }
            }

            if (string.IsNullOrEmpty(result.Text))
            {
                List<string> positionalArguments = arguments.Where(argument => !argument.Contains("=")).Select(argument => Unquote(argument.Trim())).ToList();
                if (positionalArguments.Count >= 2)
                {
                    result.Context = string.IsNullOrEmpty(result.Context) ? positionalArguments[0] : result.Context;
                    result.Text = positionalArguments[1];
                }
                else
                {
                    result.Text = positionalArguments.FirstOrDefault();
                }
            }

            return result;
        }

        private static IEnumerable<string> SplitMarkupExtensionArguments(string argumentText)
        {
            int start = 0;
            char quote = '\0';
            for (int index = 0; index < argumentText.Length; index++)
            {
                char current = argumentText[index];
                if ((current == '\'' || current == '"') && (index == 0 || argumentText[index - 1] != '\\'))
                {
                    quote = quote == '\0' ? current : quote == current ? '\0' : quote;
                }
                else if (current == ',' && quote == '\0')
                {
                    string argument = argumentText.Substring(start, index - start).Trim();
                    if (argument.Length > 0)
                        yield return argument;

                    start = index + 1;
                }
            }

            string lastArgument = argumentText.Substring(start).Trim();
            if (lastArgument.Length > 0)
                yield return lastArgument;
        }

        private bool IsGettextExtensionName(string extensionName)
        {
            string normalizedName = extensionName.EndsWith("Extension", StringComparison.Ordinal)
                ? extensionName.Substring(0, extensionName.Length - "Extension".Length)
                : extensionName;

            return normalizedName == "Gettext" || GetStringAliases.Contains(normalizedName);
        }

        private static bool IsMarkupExtension(string value)
        {
            return value.Length > 1 && value[0] == '{' && value[value.Length - 1] == '}';
        }

        private static string GetLocalName(string name)
        {
            int separatorIndex = name.LastIndexOf(':');
            return separatorIndex < 0 ? name : name.Substring(separatorIndex + 1);
        }

        private static string Unquote(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            if (value.Length >= 2 && ((value[0] == '\'' && value[value.Length - 1] == '\'') || (value[0] == '"' && value[value.Length - 1] == '"')))
                return value.Substring(1, value.Length - 2);

            return value;
        }

        private static void SplitContext(XamlCatalogString result)
        {
            if (!string.IsNullOrEmpty(result.Context) || string.IsNullOrEmpty(result.Text))
                return;

            int separatorIndex = result.Text.IndexOf('|');
            if (separatorIndex <= 0 || separatorIndex == result.Text.Length - 1)
                return;

            result.Context = result.Text.Substring(0, separatorIndex);
            result.Text = result.Text.Substring(separatorIndex + 1);
        }

        private static int GetLineNumber(IXmlLineInfo lineInfo)
        {
            return lineInfo != null && lineInfo.HasLineInfo() ? lineInfo.LineNumber : 0;
        }

        private static string BuildReference(string pathRelative, int lineNumber)
        {
            return lineNumber > 0 ? $"{pathRelative}:{lineNumber}" : pathRelative;
        }

        private class XamlCatalogString
        {
            public string Context { get; set; }

            public string Text { get; set; }

            public string PluralText { get; set; }
        }
    }
}
