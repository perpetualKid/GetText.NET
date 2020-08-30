using System.Collections.Generic;
using System.IO;

namespace GetText.Extractor.Engine.SourceResolver
{
    internal abstract class SourceResolverBase<T>
    {
        protected FileInfo sourcePath;

        public SourceResolverBase(FileInfo sourcePath)
        {
            this.sourcePath = sourcePath;
        }

        public abstract IEnumerable<T> GetInput();
    }
}
