/**
* Provides the main implementation of the tokenizer, which converts
* an input string into a list of tokens such as variables, numbers,
* operators, parentheses, and keywords.
*
* Bugs: No major bugs known, but invalid inputs may raise exceptions
*       when not handled in helper methods.
*
* @author Rahul, Rick, Zachary
* @date 30th Sept, 2025
*/

using Tokenizer;
public class TokenizerImpl
{
    /// <summary>
    /// Main entry point for tokenizing a string.
    /// Loops through the input one character at a time and calls helper
    /// methods to recognize and return tokens.
    /// </summary>
    /// <param name="input">The raw program string to tokenize.</param>
    /// <returns>A list of Token objects representing the input string.</returns>
    public List<Token> Tokenize(string input)
    {
        // Holds the list of tokens created from input
        List<Token> TokenList = [];
        int index = 0;

        // Iterate character by character through the string
        while (index < input.Length)
        {
            char currentChar = input[index];

            // Ignore whitespace
            if (char.IsWhiteSpace(currentChar)) { index++; }

            // Numbers → handled by LiteralH
            else if (char.IsDigit(currentChar)) { TokenList.Add(LiteralH(input, ref index)); }

            // Identifiers and keywords → handled by PrimitiveH
            else if (char.IsLetter(currentChar)) { TokenList.Add(PrimitiveH(input, ref index)); }

            // Structures like { } ( ) → handled by StructureH
            else if (new[] {
                TokenConstants.LEFT_CURLY,
                TokenConstants.RIGHT_CURLY,
                TokenConstants.LEFT_PAREN,
                TokenConstants.RIGHT_PAREN
            }.Contains(currentChar.ToString()))
            {
                TokenList.Add(StructureH(currentChar.ToString()));
                index++;
            }

            // Operators (+, -, *, /, :=, etc.) → handled by OperatorsH
            else { TokenList.Add(OperatorsH(input, ref index)); }
        }

        return TokenList;
    }

    #region Helpers for Token Classification

    /// <summary>
    /// Handles keywords and variable names.
    /// If the string matches "return", marks as RETURN.
    /// Otherwise marks as VARIABLE.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <param name="index">Reference index into the string.</param>
    /// <returns>A Token representing the primitive (keyword or variable).</returns>
    /// <exception cref="ArgumentException">Thrown if input is unexpected.</exception>
    private Token PrimitiveH(string input, ref int index)
    {
        string item = "";

        // Collect consecutive letters into a single token
        while (index < input.Length && char.IsLetter(input[index]))
        {
            item = item + input[index];
            index++;
        }

        // Check if it is the reserved keyword "return"
        if (item == TokenConstants.RETURN)
        {
            return new Token(item, TokenType.RETURN);
        }
        else return new Token(item, TokenType.VARIABLE);

        throw new ArgumentException("Invalid character");
    }

    /// <summary>
    /// Handles numeric literals (integers and floats).
    /// Continues reading digits and checks for decimal point.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <param name="index">Reference index into the string.</param>
    /// <returns>A Token of type INTEGER or FLOAT.</returns>
    /// <exception cref="ArgumentException">Thrown if malformed number.</exception>
    private Token LiteralH(string input, ref int index)
    {
        string item = "";

        // Collect digits for integer part
        while ((index < input.Length) && char.IsDigit(input[index]))
        {
            item = item + input[index];
            index++;
        }

        // Check for decimal point indicating a float
        if (index < input.Length && input[index].ToString() == TokenConstants.DECIMAL_POINT)
        {
            item += TokenConstants.DECIMAL_POINT;
            index++;

            // Collect digits for fractional part
            while ((index < input.Length) && char.IsDigit(input[index]))
            {
                item = item + input[index];
                index++;
            }

            return new Token(item, TokenType.FLOAT);
        }

        return new Token(item, TokenType.INTEGER);

        throw new ArgumentException("Invalid character");
    }

    /// <summary>
    /// Handles operators: +, -, *, %, /, **, //, :=
    /// Detects multi-character operators before single-character.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <param name="index">Reference index into the string.</param>
    /// <returns>A Token of type OPERATOR or ASSIGNMENT.</returns>
    /// <exception cref="ArgumentException">Thrown if unknown operator.</exception>
    private Token OperatorsH(string input, ref int index)
    {
        // Handle "**" (exponentiation)
        if (input[index] == '*' &&
            index + 1 < input.Length && input[index + 1] == '*')
        {
            index += 2;
            return new Token(TokenConstants.EXP, TokenType.OPERATOR);
        }

        // Handle "//" (float division)
        if (input[index] == '/' &&
            index + 1 < input.Length && input[index + 1] == '/')
        {
            index += 2;
            return new Token(TokenConstants.INT_DIV, TokenType.OPERATOR);
        }

        // Handle ":=" (assignment)
        if (input[index] == ':' &&
            index + 1 < input.Length && input[index + 1] == '=')
        {
            index += 2;
            return new Token(TokenConstants.ASSIGMENT, TokenType.ASSIGNMENT);
        }

        // Handle single-character operators
        string element = input[index].ToString();
        if (new[] {
                TokenConstants.PLUS,
                TokenConstants.MINUS,
                TokenConstants.TIMES,
                TokenConstants.MOD,
                TokenConstants.FLOAT_DIV
            }.Contains(element))
        {
            index++;
            return new Token(element, TokenType.OPERATOR);
        }

        throw new ArgumentException("Invalid character");
    }

    /// <summary>
    /// Handles structure symbols such as parentheses and curly braces.
    /// </summary>
    /// <param name="letter">The current character as a string.</param>
    /// <returns>A Token with the correct TokenType for the structure.</returns>
    /// <exception cref="ArgumentException">Thrown if unknown structure.</exception>
    private Token StructureH(string letter)
    {
        if (letter == TokenConstants.LEFT_CURLY) { return new Token(letter, TokenType.LEFT_CURLY); }
        else if (letter == TokenConstants.RIGHT_CURLY) { return new Token(letter, TokenType.RIGHT_CURLY); }
        else if (letter == TokenConstants.LEFT_PAREN) { return new Token(letter, TokenType.LEFT_PAREN); }
        else if (letter == TokenConstants.RIGHT_PAREN) { return new Token(letter, TokenType.RIGHT_PAREN); }
        else { throw new ArgumentException("Invalid character"); }
    }

    #endregion
}
