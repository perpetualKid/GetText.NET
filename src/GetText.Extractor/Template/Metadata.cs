using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetText.Extractor.Template
{
    public class Metadata
    {
        private List<string> autoComments;
        private List<string> references;

        public List<string> AutoComments => autoComments ?? (autoComments = new List<string>());

        public List<string> References => references ?? (references = new List<string>());

        public MessageFlags Flags { get; set; }

        //other metadata (translator comments, previous string) is discarded as we deal with templates only

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            if (null != autoComments)
                foreach (string comment in AutoComments)
                {
                    builder.Append($"#. {comment}");
                    builder.Append(Catalog.Newline);
                }
            if (null != references)
                foreach (string reference in References)
                {
                    builder.Append($"#: {reference}");
                    builder.Append(Catalog.Newline);
                }
            if (Flags != MessageFlags.None)
            {
                builder.Append($"#");
                if ((Flags & MessageFlags.Fuzzy) == MessageFlags.Fuzzy)
                    builder.Append(", fuzzy");
                if ((Flags & MessageFlags.CSharpFormat) == MessageFlags.CSharpFormat)
                    builder.Append(", csharp-format");
                builder.Append(Catalog.Newline);
            }
            return builder.ToString();
        }
    }
}
