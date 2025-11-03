/**
* Defines the token structure, token types, and constants used in the tokenizer.
* This file provides the foundational building blocks for tokenization of input
* strings into meaningful symbols (keywords, operators, literals, etc.).
*
* Bugs: None currently known.
*
* @author Rahul, Rick, Zachary
* @date 30th Sept, 2025
*/

using System;

namespace Tokenizer
{
    /// <summary>
    /// Enumerates all possible token types recognized by the tokenizer.
    /// </summary>
    public enum TokenType
    {
        RETURN,       // The keyword "return"
        VARIABLE,     // Identifiers such as variable names
        INTEGER,      // Integer literals (e.g., 42, 0)
        FLOAT,        // Floating-point literals (e.g., 3.14, 0.5)
        ASSIGNMENT,   // Assignment operator ":="
        OPERATOR,     // Arithmetic operators (+, -, *, /, %, **, etc.)
        LEFT_PAREN,   // Opening parenthesis "("
        RIGHT_PAREN,  // Closing parenthesis ")"
        LEFT_CURLY,   // Opening curly brace "{"
        RIGHT_CURLY,   // Closing curly brace "}"
        UNKNOWN        // Random undefined value
    }

    /// <summary>
    /// Stores string constants for all supported operators, symbols, and keywords.
    /// Used to avoid repetition of literal strings across the tokenizer logic.
    /// </summary>
    public static class TokenConstants
    {
        // Arithmetic operators
        public const string PLUS = "+";
        public const string MINUS = "-";
        public const string TIMES = "*";
        public const string INT_DIV = "//";  // Floating-point division
        public const string FLOAT_DIV = "/";     // Integer division
        public const string MOD = "%";         // Modulus operator
        public const string EXP = "**";        // Exponentiation operator

        // Grouping symbols
        public const string LEFT_PAREN = "(";
        public const string RIGHT_PAREN = ")";
        public const string LEFT_CURLY = "{";
        public const string RIGHT_CURLY = "}";

        // Assignment and other operators
        public const string ASSIGMENT = ":=";
        public const string DECIMAL_POINT = ".";

        // Keywords
        public const string RETURN = "return";
    }

    /// <summary>
    /// Represents a single token with its value (string) and type (TokenType).
    /// This class is used throughout tokenization to store parsed symbols.
    /// </summary>
    public class Token
    {
        /// <summary>
        /// The raw string value of the token (e.g., "42", "+", "return").
        /// </summary>
        public string _value { get; set; }

        /// <summary>
        /// The category or type of the token (e.g., INTEGER, OPERATOR).
        /// </summary>
        public TokenType _tkntype { get; set; }

        /// <summary>
        /// Initializes a new Token with the given value and type.
        /// </summary>
        /// <param name="tkn">The string representation of the token.</param>
        /// <param name="type">The token type (from the TokenType enum).</param>
        public Token(string tkn, TokenType type)
        {
            _value = tkn;
            _tkntype = type;
        }

        /// <summary>
        /// Prints token details to the console for debugging purposes.
        /// </summary>
        /// <param name="tkn">The token to print.</param>
        public void ToString(Token tkn)
        {
            // Outputs both value and type of the token
            Console.WriteLine($"This token has the value {_value} and the type of {_tkntype}");
        }

        /// <summary>
        /// Compares two tokens for equality based on their string values.
        /// </summary>
        /// <param name="tkn1">First token.</param>
        /// <param name="tkn2">Second token.</param>
        /// <returns>True if the values match, false otherwise.</returns>
        public bool Equals(Token tkn1, Token tkn2)
        {
            return tkn1._value == tkn2._value;
        }
    }
}
