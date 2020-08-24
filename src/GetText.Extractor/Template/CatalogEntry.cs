using System;
using System.Collections.Generic;
using System.Text;

namespace GetText.Extractor.Template
{
    /// <summary>
    /// https://www.gnu.org/software/gettext/manual/gettext.html#PO-Files
    /// </summary>
    public class CatalogEntry
    {
        private Metadata metaData;
        private List<string> pluralMessages;

        public string Context { get; set; }
        public string MessageId { get; }
        public string Message { get; set; } = string.Empty;
        public string PluralMessageId { get; set; }
        public List<string> PluralMessages => pluralMessages ?? (pluralMessages = new List<string>());
        public Metadata MetaData => metaData ?? (metaData = new Metadata());

        internal bool HasPlural => pluralMessages?.Count > 0;

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

        public CatalogEntry(string messageId, string message): this(messageId)
        { 
            Message = message; 
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            if (null != metaData)
                builder.Append(MetaData.ToString());
            if (!string.IsNullOrEmpty(Context))
            {
                FormatMessageStringAndAppend(builder, "msgctxt", Context);
            }
            FormatMessageStringAndAppend(builder, "msgid", MessageId);

            if (HasPlural)
            {
                FormatMessageStringAndAppend(builder, "msgid_plural", MessageId);
                for(int i = 0; i < PluralMessages.Count; i++)
                {
                    FormatMessageStringAndAppend(builder, $"msgstr[{i}]", EnsureCorrectEndings(MessageId, PluralMessages[i]));
                }
            }
            else
            {
                FormatMessageStringAndAppend(builder, "msgstr", EnsureCorrectEndings(MessageId, Message));
            }
            builder.Append(Catalog.Newline);
            return builder.ToString();
        }

        private static void FormatMessageStringAndAppend(StringBuilder builder, string prefix, string message)
        {
            string escapedMessage = StringEscaping.ToGetTextFormat(message);
            
            //format to 80 cols
            //first the simple case: does it fit one one line, with the prefix, and contain no newlines?
            if (prefix.Length + escapedMessage.Length < 77 && !escapedMessage.Contains("\\n"))
            {
                builder.Append($"{prefix} \"{escapedMessage}\"");
                builder.Append(Catalog.Newline);
                return;
            }
            //not the simple case.

            // first line is typically: prefix ""
            builder.Append($"{prefix} \"\"");
            builder.Append(Catalog.Newline);

            //followed by 80-col width break on spaces
            int possibleBreak = -1;
            int currLineLen = 0;
            int lastBreakAt = 0;
            bool forceBreak = false;

            int pos = 0;
            while (pos < escapedMessage.Length)
            {
                char c = escapedMessage[pos];

                //handle escapes			
                if (c == '\\' && pos + 1 < escapedMessage.Length)
                {
                    pos++;
                    currLineLen++;

                    char c2 = escapedMessage[pos];
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
                    builder.Append("\"");
                    builder.Append(escapedMessage.Substring(lastBreakAt, possibleBreak - lastBreakAt));
                    builder.Append("\"");
                    builder.Append(Catalog.Newline);

                    //reset state for new line
                    currLineLen = 0;
                    lastBreakAt = possibleBreak;
                    possibleBreak = -1;
                    forceBreak = false;
                }
                pos++;
                currLineLen++;
            }
            string remainder = escapedMessage.Substring(lastBreakAt);
            if (remainder.Length > 0)
            {
                builder.Append("\"");
                builder.Append(remainder);
                builder.Append("\"");
                builder.Append(Catalog.Newline);
            }
            return;
        }

        // Ensures that the end lines of text are the same as in the reference string.
        private static string EnsureCorrectEndings(string reference, string text)
        {
            if (text.Length == 0)
                return "";

            int numEndings = 0;
            for (int i = text.Length - 1; i >= 0 && text[i] == '\n'; i--, numEndings++)
                ;
            StringBuilder builder = new StringBuilder(text, 0, text.Length - numEndings, text.Length + reference.Length - numEndings);
            for (int i = reference.Length - 1; i >= 0 && reference[i] == '\n'; i--)
            {
                builder.Append('\n');
            }
            return builder.ToString();
        }

    }
}
