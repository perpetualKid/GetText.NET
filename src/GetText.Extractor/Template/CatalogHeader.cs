using System;
using System.Text;

namespace GetText.Extractor.Template
{
    public class CatalogHeader
    {
        public string ProjectIdVersion { get; set; } = "PACKAGE VERSION";
        public string ReportMsgidBugsTo { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public DateTime RevisionDate { get; set; }
        public string Translator { get; set; }
        public string TranslatorEmail { get; set; }
        public string LanguageTeam { get; set; }
        public string LanguageTeamEmail { get; set; }
        public string MimeVersion { get; set; } = "1.0";
        public string ContentType { get; set; } = "text/plain; charset=utf-8";
        public string TransferEncoding { get; set; } = "8bit";
        public string PluralForms { get; set; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"\"Project-Id-Version: {ProjectIdVersion}\\n\"");
            if (!string.IsNullOrEmpty(ReportMsgidBugsTo))
            {
                builder.AppendLine($"\"Report-Msgid-Bugs-To: {ReportMsgidBugsTo}\\n\"");
            }
            builder.AppendLine($"\"POT-Creation-Date: {CreationDate.ToRfc822Format()}\\n\"");
            builder.AppendLine($"\"PO-Revision-Date: {DateTime.Now.ToRfc822Format()}\\n\"");
            if (string.IsNullOrEmpty(TranslatorEmail))
            {
                builder.AppendLine($"\"Last-Translator: {Translator}\\n\"");
            }
            else
            {
                builder.AppendLine($"\"Last-Translator: {Translator} <{TranslatorEmail}>\\n\"");
            }
            if (string.IsNullOrEmpty(LanguageTeamEmail))
            {
                builder.AppendLine($"\"Language-Team: {LanguageTeam}\\n\"");
            }
            else
            {
                builder.AppendLine($"\"Language-Team: {LanguageTeam} <{LanguageTeamEmail}>\\n\"");
            }
            builder.AppendLine($"\"MIME-Version: {MimeVersion}\\n\"");
            builder.AppendLine($"\"Content-Type: {ContentType}\\n\"");
            builder.AppendLine($"\"Content-Transfer-Encoding: {TransferEncoding}\\n\"");
            if (!string.IsNullOrEmpty(PluralForms))
            {
                builder.AppendLine($"\"Plural-Forms: {PluralForms}\\n\"");
            }
            builder.AppendLine($"\"X-Generator: GetText.NET Extractor\\n\"");
            return builder.ToString();
        }
    }
}
