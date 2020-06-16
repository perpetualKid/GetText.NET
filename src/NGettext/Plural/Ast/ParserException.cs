using System;

namespace NGettext.Plural.Ast
{
    [Serializable]
    public class ParserException : Exception
    {
        public ParserException() : base() { }
        public ParserException(string message) : base(message) { }
        public ParserException(string message, Exception innerException) : base(message, innerException) { }

        protected ParserException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext) : base(serializationInfo, streamingContext) { }
    }
}
