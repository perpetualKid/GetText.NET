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
            builder.Append($"\"Project-Id-Version: {ProjectIdVersion}\\n\"{CatalogTemplate.Newline}");
            if (!string.IsNullOrEmpty(ReportMsgidBugsTo))
            {
                builder.Append($"\"Report-Msgid-Bugs-To: {ReportMsgidBugsTo}\\n\"");
            }
            builder.Append($"\"POT-Creation-Date: {CreationDate.ToRfc822Format()}\\n\"{CatalogTemplate.Newline}");
            builder.Append($"\"PO-Revision-Date: {DateTime.Now.ToRfc822Format()}\\n\"{CatalogTemplate.Newline}");
            if (string.IsNullOrEmpty(TranslatorEmail))
            {
                builder.Append($"\"Last-Translator: {Translator}\\n\"{CatalogTemplate.Newline}");
            }
            else
            {
                builder.Append($"\"Last-Translator: {Translator} <{TranslatorEmail}>\\n\"{CatalogTemplate.Newline}");
            }
            if (string.IsNullOrEmpty(LanguageTeamEmail))
            {
                builder.Append($"\"Language-Team: {LanguageTeam}\\n\"{CatalogTemplate.Newline}");
            }
            else
            {
                builder.Append($"\"Language-Team: {LanguageTeam} <{LanguageTeamEmail}>\\n\"{CatalogTemplate.Newline}");
            }
            builder.Append($"\"MIME-Version: {MimeVersion}\\n\"{CatalogTemplate.Newline}");
            builder.Append($"\"Content-Type: {ContentType}\\n\"{CatalogTemplate.Newline}");
            builder.Append($"\"Content-Transfer-Encoding: {TransferEncoding}\\n\"{CatalogTemplate.Newline}");
            if (!string.IsNullOrEmpty(PluralForms))
            {
                builder.Append($"\"Plural-Forms: {PluralForms}\\n\"{CatalogTemplate.Newline}");
            }
            builder.Append($"\"X-Generator: GetText.NET Extractor\\n\"{CatalogTemplate.Newline}");
            builder.Append(CatalogTemplate.Newline);
            return builder.ToString();
        }
    }
}
