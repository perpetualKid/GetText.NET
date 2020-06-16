using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace NGettext.Plural.Ast
{
    /// <summary>
    /// Plural rule formula parser.
    /// Ported from the I18n component from Zend Framework (https://github.com/zendframework/zf2).
    /// </summary>
    public sealed class AstTokenParser
    {
        private readonly Dictionary<TokenType, TokenDefinition> tokenDefinitions = new Dictionary<TokenType, TokenDefinition>();

        private string input;

        private int position;

        private Token currentToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="AstTokenParser"/> class with default token definitions.
        /// </summary>
        public AstTokenParser()
        {
            // Ternary operators
            this.RegisterTokenDefinition(TokenType.TernaryIf, 20)
                .SetLeftDenotationGetter((self, left) =>
                {
                    self.Children[0] = left;
                    self.Children[1] = this.ParseNextExpression();
                    this.AdvancePosition(TokenType.TernaryElse);
                    self.Children[2] = this.ParseNextExpression();
                    return self;
                });
            this.RegisterTokenDefinition(TokenType.TernaryElse);

            // Boolean operators
            this.RegisterLeftInfixTokenDefinition(TokenType.Or, 30);
            this.RegisterLeftInfixTokenDefinition(TokenType.And, 40);

            // Equal operators
            this.RegisterLeftInfixTokenDefinition(TokenType.Equals, 50);
            this.RegisterLeftInfixTokenDefinition(TokenType.NotEquals, 50);

            // Compare operators
            this.RegisterLeftInfixTokenDefinition(TokenType.GreaterThan, 50);
            this.RegisterLeftInfixTokenDefinition(TokenType.LessThan, 50);
            this.RegisterLeftInfixTokenDefinition(TokenType.GreaterOrEquals, 50);
            this.RegisterLeftInfixTokenDefinition(TokenType.LessOrEquals, 50);

            // Add operators
            this.RegisterLeftInfixTokenDefinition(TokenType.Minus, 60);
            this.RegisterLeftInfixTokenDefinition(TokenType.Plus, 60);

            // Multiply operators
            this.RegisterLeftInfixTokenDefinition(TokenType.Multiply, 70);
            this.RegisterLeftInfixTokenDefinition(TokenType.Divide, 70);
            this.RegisterLeftInfixTokenDefinition(TokenType.Modulo, 70);

            // Not operator
            this.RegisterPrefixTokenDefinition(TokenType.Not, 80);

            // Literals
            this.RegisterTokenDefinition(TokenType.N)
                .SetNullDenotationGetter((self) =>
                {
                    return self;
                });
            this.RegisterTokenDefinition(TokenType.Number)
                .SetNullDenotationGetter((self) =>
                {
                    return self;
                });

            // Parentheses
            this.RegisterTokenDefinition(TokenType.LeftParenthesis)
                .SetNullDenotationGetter((self) =>
                {
                    var expression = this.ParseNextExpression();
                    this.AdvancePosition(TokenType.RightParenthesis);
                    return expression;
                });
            this.RegisterTokenDefinition(TokenType.RightParenthesis);

            // EOF
            this.RegisterTokenDefinition(TokenType.EOF);
        }

        private TokenDefinition RegisterTokenDefinition(TokenType tokenType, int leftBindingPower = 0)
        {
            if (tokenDefinitions.TryGetValue(tokenType, out TokenDefinition definition))
            {
                definition.LeftBindingPower = Math.Max(definition.LeftBindingPower, leftBindingPower);
            }
            else
            {
                definition = new TokenDefinition(tokenType, leftBindingPower);
                tokenDefinitions[tokenType] = definition;
            }

            return definition;
        }

        private TokenDefinition RegisterLeftInfixTokenDefinition(TokenType tokenType, int leftBindingPower)
        {
            return this.RegisterTokenDefinition(tokenType, leftBindingPower)
                .SetLeftDenotationGetter((self, left) =>
                {
                    self.Children[0] = left;
                    self.Children[1] = this.ParseNextExpression(leftBindingPower);
                    return self;
                });
        }

        private TokenDefinition RegisterRightInfixTokenDefinition(TokenType tokenType, int leftBindingPower)
        {
            return this.RegisterTokenDefinition(tokenType, leftBindingPower)
                .SetLeftDenotationGetter((self, left) =>
                {
                    self.Children[0] = left;
                    self.Children[1] = this.ParseNextExpression(leftBindingPower - 1);
                    return self;
                });
        }

        private TokenDefinition RegisterPrefixTokenDefinition(TokenType tokenType, int leftBindingPower)
        {
            return this.RegisterTokenDefinition(tokenType, leftBindingPower)
                .SetNullDenotationGetter((self) =>
                {
                    self.Children[0] = this.ParseNextExpression(leftBindingPower);
                    self.Children[1] = null;
                    return self;
                });
        }

        private TokenDefinition GetDefinition(TokenType tokenType)
        {
            if (!tokenDefinitions.TryGetValue(tokenType, out TokenDefinition tokenDefinition))
            {
                throw new ParserException($"Can not find token definition for \"{tokenType}\" token type.");
            }
            return tokenDefinition;
        }

        /// <summary>
        /// Parses the input string that contains a plural rule formula and generates an abstract syntax tree.
        /// </summary>
        /// <param name="input">Input string.</param>
        /// <returns>Root node of the abstract syntax tree.</returns>
        public Token Parse(string input)
        {
            this.input = input + "\0";
            position = 0;
            currentToken = this.GetNextToken();

            return this.ParseNextExpression();
        }

        private Token ParseNextExpression(int rightBindingPower = 0)
        {
            Token token = currentToken;
            currentToken = this.GetNextToken();
            Token left = this.GetDefinition(token.Type).GetNullDenotation(token);

            while (rightBindingPower < this.GetDefinition(currentToken.Type).LeftBindingPower)
            {
                token = currentToken;
                currentToken = this.GetNextToken();
                left = this.GetDefinition(token.Type).GetLeftDenotation(token, left);
            }

            return left;
        }

        private void AdvancePosition()
        {
            currentToken = this.GetNextToken();
        }

        private void AdvancePosition(TokenType expectedTokenType)
        {
            if (currentToken.Type != expectedTokenType)
            {
                throw new ParserException($"Expected token \"{expectedTokenType}\" but received \"{currentToken.Type}\"");
            }
            this.AdvancePosition();
        }

        private Token GetNextToken()
        {
            while (input[position] == ' ' || input[position] == '\t')
            {
                position++;
            }

            char character = input[position++];
            TokenType tokenType = TokenType.None;
            long value = 0L;

            switch (character)
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    StringBuilder sb = new StringBuilder();
                    sb.Append(character);
                    while (char.IsNumber(input[position]))
                    {
                        sb.Append(input[position++]);
                    }
                    tokenType = TokenType.Number;
                    value = long.Parse(sb.ToString(), CultureInfo.InvariantCulture);
                    break;

                case '=':
                case '&':
                case '|':
                    if (input[position] == character)
                    {
                        position++;
                        switch (character)
                        {
                            case '=': tokenType = TokenType.Equals; break;
                            case '&': tokenType = TokenType.And; break;
                            case '|': tokenType = TokenType.Or; break;
                        }
                    }
                    else
                    {
                        throw new ParserException($"Found invalid character \"{input[position]}\" after character \"{character}\" in input stream.");
                    }
                    break;

                case '!':
                    if (input[position] == '=')
                    {
                        position++;
                        tokenType = TokenType.NotEquals;
                    }
                    else
                    {
                        tokenType = TokenType.Not;
                    }
                    break;

                case '<':
                    if (input[position] == '=')
                    {
                        position++;
                        tokenType = TokenType.LessOrEquals;
                    }
                    else
                    {
                        tokenType = TokenType.LessThan;
                    }
                    break;

                case '>':
                    if (input[position] == '=')
                    {
                        position++;
                        tokenType = TokenType.GreaterOrEquals;
                    }
                    else
                    {
                        tokenType = TokenType.GreaterThan;
                    }
                    break;

                case '*': tokenType = TokenType.Multiply; break;
                case '/': tokenType = TokenType.Divide; break;
                case '%': tokenType = TokenType.Modulo; break;
                case '+': tokenType = TokenType.Plus; break;
                case '-': tokenType = TokenType.Minus; break;
                case 'n': tokenType = TokenType.N; break;
                case '?': tokenType = TokenType.TernaryIf; break;
                case ':': tokenType = TokenType.TernaryElse; break;
                case '(': tokenType = TokenType.LeftParenthesis; break;
                case ')': tokenType = TokenType.RightParenthesis; break;

                case ';':
                case '\n':
                case '\0':
                    tokenType = TokenType.EOF;
                    position--;
                    break;

                default:
                    throw new ParserException($"Found invalid character \"{character}\" in input stream at position {position}.");
            }

            return new Token(tokenType, value);
        }
    }
}
