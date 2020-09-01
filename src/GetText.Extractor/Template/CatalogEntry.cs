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
        private List<string> pluralMessages;

        public string Context { get; }
        public string MessageId { get; }
        public string Message { get; set; } = string.Empty;
        public string PluralMessageId { get; set; }
        public List<string> PluralMessages => pluralMessages ?? (pluralMessages = new List<string>());
        public CommentData Comments => comments ?? (comments = new CommentData());
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

        public CatalogEntry(string messageId, string message) : this(messageId)
        {
            Message = message;
        }

        public CatalogEntry(string context, string messageId, string message) : this(messageId, message)
        {
            Context = context;
        }

        public string Key => BuildKey(Context, MessageId);

        public static string BuildKey(string context, string messageId) => $"{context?.Trim()}|{messageId}";

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
                if (pluralMessages?.Count > 1)
                {
                    for (int i = 0; i < PluralMessages.Count; i++)
                    {
                        FormatMessageStringAndAppend(builder, $"msgstr[{i}]", EnsureCorrectEndings(MessageId, PluralMessages[i]));
                    }
                }
                else
                {
                    for (int i = 0; i < 2; i++)
                    {
                        FormatMessageStringAndAppend(builder, $"msgstr[{i}]", string.Empty);
                    }
                }
            }
            else
            {
                FormatMessageStringAndAppend(builder, "msgstr", EnsureCorrectEndings(MessageId, Message));
            }
            if (!string.IsNullOrEmpty(MessageId))
                builder.Append(CatalogTemplate.Newline);
            return builder.ToString();
        }

        private static void FormatMessageStringAndAppend(StringBuilder builder, string prefix, string message)
        {
            //            string escapedMessage = StringEscaping.ToGetTextFormat(message);

            //format to 80 cols
            //first the simple case: does it fit one one line, with the prefix, and contain no newlines?
            if (prefix.Length + message.Length < 77 && !message.Contains("\\n"))
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
                    builder.Append($"\"{message.Substring(lastBreakAt, possibleBreak - lastBreakAt)}\"");
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
            string remainder = message.Substring(lastBreakAt);
            if (remainder.Length > 0)
            {
                builder.Append($"\"{remainder}\"");
                builder.Append(CatalogTemplate.Newline);
            }
            return;
        }

        // Ensures that the end lines of text are the same as in the reference string.
        private static string EnsureCorrectEndings(string reference, string text)
        {
            if (text.Length == 0)
                return string.Empty;

            int numEndings = 0;
            for (int i = text.Length - 1; i >= 0 && text[i] == '\n'; i--)
                numEndings++;
            StringBuilder builder = new StringBuilder(text, 0, text.Length - numEndings, text.Length + reference.Length - numEndings);
            for (int i = reference.Length - 1; i >= 0 && reference[i] == '\n'; i--)
            {
                builder.Append('\n');
            }
            return builder.ToString();
        }

    }
}
