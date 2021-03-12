using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GetText.Extractor.Template
{
    internal class CatalogTemplate
    {
        internal static string Newline = Environment.NewLine;
        internal static string[] LineEndings = new string[] { "\n\r", "\r\n", "\r", "\n", "\r" };
        private readonly ConcurrentDictionary<string, CatalogEntry> entries = new ConcurrentDictionary<string, CatalogEntry>();

        public string FileName { get; private set; }
        public CatalogHeader Header { get; set; }

        public CatalogTemplate(string fileName)
        {
            FileName = fileName;
            Header = new CatalogHeader();
        }

        public void AddOrUpdateEntry(string context, string messageId, string reference, bool formatString)
        {
            if (messageId == null || string.IsNullOrWhiteSpace(System.Text.RegularExpressions.Regex.Unescape(messageId)))
                return;     // don't care about empty message ids
            if (!entries.TryGetValue(CatalogEntry.BuildKey(context, messageId), out CatalogEntry result))
            {
                result = new CatalogEntry(context, messageId, string.Empty);
                if (!entries.TryAdd(result.Key, result))
                    result = entries[result.Key];
            }
            if (!result.References.Contains(reference))
                result.References.Add(reference);
            result.Comments.Flags = formatString ? result.Comments.Flags | MessageFlags.CSharpFormat : result.Comments.Flags & ~MessageFlags.CSharpFormat;
        }

        public void AddOrUpdateEntry(string context, string messageId, string plural, string reference, bool formatString)
        {
            if (string.IsNullOrEmpty(messageId))
                return;     // don't care about empty message ids
            if (!entries.TryGetValue(CatalogEntry.BuildKey(context, messageId), out CatalogEntry result))
            {
                result = new CatalogEntry(context, messageId, string.Empty);
                if (!entries.TryAdd(result.Key, result))
                    result = entries[result.Key];
            }
            result.PluralMessageId = plural;
            result.References.Add(reference);
            result.Comments.Flags = formatString ? result.Comments.Flags | MessageFlags.CSharpFormat : result.Comments.Flags & ~MessageFlags.CSharpFormat;
        }

        public async Task WriteAsync()
        {
            if (entries.IsEmpty)
                return;

            string backupFile = FileName + ".bak";
            if (File.Exists(backupFile))
            {
                File.Delete(backupFile);
            }
            if (File.Exists(FileName))
            {
                File.Move(FileName, backupFile);
            }
            if (!Directory.Exists(Path.GetDirectoryName(FileName)))
                Directory.CreateDirectory(Path.GetDirectoryName(FileName));
            await Save().ConfigureAwait(false);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(CatalogEntry.Empty);
            builder.Append(Header.ToString());

            foreach (KeyValuePair<string, CatalogEntry> item in entries)
            {
                builder.Append(item.Value.ToString());
            }
            return builder.ToString();
        }

        private async Task Save()
        {
            using (StreamWriter writer = new StreamWriter(FileName))
            {
                await writer.WriteAsync(ToString()).ConfigureAwait(false);
            }
        }
    }
}
