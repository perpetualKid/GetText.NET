namespace GetText.Plural.Ast
{
    /// <summary>
    /// Represents a node in the abstract syntax tree.
    /// </summary>
    public sealed class Token
    {
		public const int MAXCHILDCOUNT = 3;

		/// <summary>
		/// Gets the type of the current token.
		/// </summary>
		public TokenType Type { get; private set; }

		/// <summary>
		/// Gets or sets an optional value associated with this token.
		/// </summary>
		public long Value { get; set; }

        /// <summary>
        /// Gets token children.
        /// </summary>
#pragma warning disable CA1819 // Properties should not return arrays
        public Token[] Children { get;  } = new Token[MAXCHILDCOUNT];
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>
        /// Initializes a new instance of the <see cref="Token"/> class with given type and (optional) value.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        public Token(TokenType type, long value = 0)
		{
			Type = type;
			Value = value;
		}
    }
}
