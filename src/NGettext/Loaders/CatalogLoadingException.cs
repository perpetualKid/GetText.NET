using System;

namespace NGettext.Loaders
{
    [Serializable]
    public class CatalogLoadingException : Exception
    {
        public CatalogLoadingException() : base() { }
        public CatalogLoadingException(string message) : base(message) { }
        public CatalogLoadingException(string message, Exception innerException) : base(message, innerException) { }

        protected CatalogLoadingException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }
    }
}
