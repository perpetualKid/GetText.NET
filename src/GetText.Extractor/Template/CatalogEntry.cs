using System;
using System.Collections.Generic;
using System.Text;

namespace GetText.Extractor.Template
{
    /// <summary>
    /// https://www.gnu.org/software/gettext/manual/gettext.html#PO-Files
    /// </summary>
    internal class CatalogEntry
    {
        private CommentData comments;

        public string Context { get; }
        public string MessageId { get; }
        public string PluralMessageId { get; set; }
        public CommentData Comments => comments ??= new CommentData();
        public List<string> References => Comments.References;

        public static CatalogEntry Empty { get; } = new CatalogEntry();

        private CatalogEntry()
        {
            MessageId = string.Empty;
        }

        public CatalogEntry(string messageId)
        {
            if (string.IsNullOrEmpty(messageId))
                throw new ArgumentOutOfRangeException(nameof(messageId));

            MessageId = messageId;
        }

        public CatalogEntry(string context, string messageId) : this(messageId)
        {
            Context = context;
        }

        public string Key => BuildKey(Context, MessageId);

        public static string BuildKey(string context, string messageId)
        {
            return $"{context?.Trim()}|{messageId}";
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            if (null != comments)
                builder.Append(Comments.ToString());
            if (null != Context)    //empty context is different from null
            {
                FormatMessageStringAndAppend(builder, "msgctxt", Context);
            }
            FormatMessageStringAndAppend(builder, "msgid", MessageId);

            if (!string.IsNullOrEmpty(PluralMessageId))
            {
                FormatMessageStringAndAppend(builder, "msgid_plural", PluralMessageId);
                FormatMessageStringAndAppend(builder, $"msgstr[{0}]", string.Empty);
                FormatMessageStringAndAppend(builder, $"msgstr[{1}]", string.Empty);
            }
            else
            {
                FormatMessageStringAndAppend(builder, "msgstr", string.Empty);
            }
            if (!string.IsNullOrEmpty(MessageId))
                builder.Append(CatalogTemplate.Newline);
            return builder.ToString();
        }

        private static void FormatMessageStringAndAppend(StringBuilder builder, string prefix, string message)
        {
            message = message.Replace("\"", "\\\"");

            //format to 80 cols
            //first the simple case: does it fit one one line, with the prefix, and contain no newlines?
            if (prefix.Length + message.Length < 77 && !message.Contains("\\n", StringComparison.Ordinal))
            {
                builder.Append($"{prefix} \"{message}\"");
                builder.Append(CatalogTemplate.Newline);
                return;
            }
            //not the simple case.

            // first line is typically: prefix ""
            builder.Append($"{prefix} \"\"");
            builder.Append(CatalogTemplate.Newline);

            //followed by 80-col width break on spaces
            int possibleBreak = -1;
            int currLineLen = 0;
            int lastBreakAt = 0;
            bool forceBreak = false;

            int pos = 0;
            while (pos < message.Length)
            {
                char c = message[pos];

                //handle escapes			
                if (c == '\\' && pos + 1 < message.Length)
                {
                    pos++;
                    currLineLen++;

                    char c2 = message[pos];
                    if (c2 == 'n')
                    {
                        possibleBreak = pos + 1;
                        forceBreak = true;
                    }
                    else if (c2 == 't')
                    {
                        possibleBreak = pos + 1;
                    }
                }

                if (c == ' ')
                {
                    possibleBreak = pos + 1;
                }
                if (forceBreak || (currLineLen >= 77 && possibleBreak != -1))
                {
                    builder.Append($"\"{message[lastBreakAt..possibleBreak]}\"");
                    builder.Append(CatalogTemplate.Newline);

                    //reset state for new line
                    currLineLen = 0;
                    lastBreakAt = possibleBreak;
                    possibleBreak = -1;
                    forceBreak = false;
                }
                pos++;
                currLineLen++;
            }
            string remainder = message[lastBreakAt..];
            if (remainder.Length > 0)
            {
                builder.Append($"\"{remainder}\"");
                builder.Append(CatalogTemplate.Newline);
            }
            return;
        }
    }
}
