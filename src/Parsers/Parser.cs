/**
 * Parser.cs
 *
 * Implements a recursive-descent parser that transforms tokenized program strings
 * into an abstract syntax tree (AST). It supports nested block parsing, expression
 * evaluation, and statement construction for a basic programming language syntax.
 *
 * Bugs:
 *  - Limited expression precedence handling; operators are treated with equal priority.
 *  - No explicit handling for semicolons or line terminators.
 *
 * @author Rahul,Rick,Zach (Bugs: ChatGpt5)
 * @date <date of completion>
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Formats.Asn1;
using System.Reflection;
using Utilities;
using Containers;
using AST;
using Tokenizer;
using System.Security.Principal;
using System.Runtime.Serialization;

namespace Parser
{
    /// <summary>
    /// Represents an exception that occurs during parsing due to invalid syntax or structure.
    /// </summary>
    public class ParseException : Exception
    {
        /// <summary>
        /// Constructs a new ParseException with a message.
        /// </summary>
        /// <param name="message">Describes the cause of the parsing error.</param>
        public ParseException(string message) : base(message) { }
    }

    /// <summary>
    /// Provides static parsing methods that convert tokenized program strings
    /// into abstract syntax tree representations.
    /// </summary>
    public static class Parser
    {
        #region Entry Point

        /// <summary>
        /// Parses an entire program represented as a string into a BlockStmt node.
        /// </summary>
        /// <param name="Program">The full program source code.</param>
        /// <returns>A root BlockStmt node representing the program structure.</returns>
        /// <exception cref="ParseException">Thrown if the program is structurally invalid.</exception>
        public static AST.BlockStmt Parse(string Program)
        {
            SymbolTable<string, object> blockScope = new SymbolTable<string, object>();
            List<string> lines = new List<string>();

            // Split program text into lines for individual parsing
            foreach (string line in Program.Split('\n'))
            {
                lines.Add(line);
            }

            // Require at least '{' and '}' for a valid block
            // if (lines.Count < 2) { throw new ParseException("Program must have atleas opening '{' and closing '}'."); }

            if (!Program.Contains("{") || !Program.Contains("}"))
            {
                throw new ParseException("Program must have at least opening '{' and closing '}'");
            }
            return ParseBlockStmt(lines, blockScope);
        }

        #endregion

        #region Expression Parsing

        /// <summary>
        /// Parses a simple non-recursive expression surrounded by parentheses.
        /// </summary>
        /// <param name="expression">Tokenized expression list.</param>
        /// <returns>An ExpressionNode representing the expression structure.</returns>
        private static AST.ExpressionNode ParseExpression(List<Token> expression)
        {
            if (expression.Count == 0) { throw new ParseException("Too small expression to work with"); }

            if (expression[0]._tkntype == TokenType.LEFT_CURLY && expression[expression.Count - 1]._tkntype == TokenType.RIGHT_CURLY)
            {
                throw new ParseException("mismatching parenthesis");
            }

            // If expression starts and ends with parentheses, check if they match before removing them
            if (expression.Count >= 2 &&
                expression[0]._tkntype == TokenType.LEFT_PAREN &&
                expression[expression.Count - 1]._tkntype == TokenType.RIGHT_PAREN)
            {
                // Verify these parentheses actually match
                int parenCount = 0;
                bool matching = true;
                for (int i = 0; i < expression.Count; i++)
                {
                    if (expression[i]._tkntype == TokenType.LEFT_PAREN) { parenCount++; }
                    else if (expression[i]._tkntype == TokenType.RIGHT_PAREN) { parenCount--; }

                    // If parenCount reaches 0 before the end, the opening paren doesn't match the closing paren
                    if (parenCount == 0 && i < expression.Count - 1)
                    {
                        matching = false;
                        break;
                    }
                }

                // Only strip if they match
                if (matching && parenCount == 0)
                {
                    return ParseExpressionContent(expression.GetRange(1, expression.Count - 2));
                }
            }

            // Otherwise parse as-is (handles bare literals and variables)
            return ParseExpressionContent(expression);
        }

        /// <summary>
        /// Handles the recursive parsing of an expression's internal content.
        /// </summary>
        /// <param name="content">Tokenized list of expression tokens excluding parentheses.</param>
        /// <returns>An ExpressionNode representing nested or flat expressions.</returns>
        private static AST.ExpressionNode ParseExpressionContent(List<Tokenizer.Token> content)
        {
            if (content.Count == 0) { throw new ParseException("Too small expression to work with"); }
            if (content.Count == 1) { return HandleSingleToken(content[0]); }

            // Strip redundant MATCHING parentheses and recurse
            if (content[0]._tkntype == TokenType.LEFT_PAREN && content[content.Count - 1]._tkntype == TokenType.RIGHT_PAREN)
            {
                // Verify these parentheses actually match
                int parenCount = 0;
                bool matching = true;
                for (int i = 0; i < content.Count; i++)
                {
                    if (content[i]._tkntype == TokenType.LEFT_PAREN) { parenCount++; }
                    else if (content[i]._tkntype == TokenType.RIGHT_PAREN) { parenCount--; }

                    // If parenCount reaches 0 before the end, the opening paren doesn't match the closing paren
                    if (parenCount == 0 && i < content.Count - 1)
                    {
                        matching = false;
                        break;
                    }
                }

                // Only strip if they match
                if (matching && parenCount == 0)
                {
                    return ParseExpressionContent(content.GetRange(1, content.Count - 2));
                }
            }

            // Iterate tokens to detect binary operators and split expression
            for (int i = 0; i < content.Count; i++)
            {
                if (content[i]._tkntype == TokenType.LEFT_PAREN)
                {
                    int parenCount = 1;
                    i++;
                    while (i < content.Count && parenCount > 0)
                    {
                        if (content[i]._tkntype == TokenType.LEFT_PAREN) { parenCount++; }
                        else if (content[i]._tkntype == TokenType.RIGHT_PAREN) { parenCount--; }
                        if (parenCount > 0) { i++; }
                    }
                }

                // Operator found — split left/right recursively
                if (i < content.Count && content[i]._tkntype == TokenType.OPERATOR)
                {
                    // Ensure we have tokens on both sides of the operator
                    if (i > 0 && i < content.Count - 1)
                    {
                        return CreateBinaryOperatorNode(
                            content[i]._value,
                            ParseExpressionContent(content.GetRange(0, i)),
                            ParseExpressionContent(content.GetRange(i + 1, content.Count - i - 1))
                        );
                    }
                }
            }
            throw new ParseException("Not a valid expression syntax");
        }

        /// <summary>
        /// Converts a single token into a Literal or Variable node.
        /// </summary>
        private static AST.ExpressionNode HandleSingleToken(Tokenizer.Token token)
        {
            if (token._tkntype == TokenType.INTEGER) { return new LiteralNode(int.Parse(token._value)); }
            if (token._tkntype == TokenType.FLOAT) { return new LiteralNode(double.Parse(token._value)); }
            if (token._tkntype == TokenType.STRING) { return new LiteralNode(token._value); }
            if (token._tkntype == TokenType.VARIABLE) { return new VariableNode(token._value); }
            throw new ParseException("Unexpected token may not not float or integer or variable");
        }

        /// <summary>
        /// Constructs a BinaryOperatorNode subclass based on operator type.
        /// </summary>
        private static AST.ExpressionNode CreateBinaryOperatorNode(string op, AST.ExpressionNode l, AST.ExpressionNode r)
        {
            if (op == TokenConstants.PLUS) { return new AST.PlusNode(l, r); }
            if (op == TokenConstants.MINUS) { return new AST.MinusNode(l, r); }
            if (op == TokenConstants.TIMES) { return new AST.TimesNode(l, r); }
            if (op == TokenConstants.INT_DIV) { return new AST.IntDivNode(l, r); }
            if (op == TokenConstants.FLOAT_DIV) { return new AST.FloatDivNode(l, r); }
            if (op == TokenConstants.MOD) { return new AST.ModulusNode(l, r); }
            if (op == TokenConstants.EXP) { return new AST.ExponentiationNode(l, r); }

            throw new ParseException($"Invalid operator has been used: {op}");
        }

        #endregion

        #region Variable and Statement Parsing

        /// <summary>
        /// Creates a VariableNode from a string.
        /// </summary>
        private static AST.VariableNode ParseVariableNode(string variable)
        {
            if (variable == null) { throw new ParseException("Variable is null"); }
            return new AST.VariableNode(variable);
        }

        /// <summary>
        /// Parses an assignment statement and registers variables in the symbol table.
        /// </summary>
        private static AST.AssignmentStmt ParseAssignmentStmt(List<Tokenizer.Token> content, SymbolTable<string, object> keyval)
        {
            if (content[0]._tkntype != TokenType.VARIABLE) { throw new ParseException("Invalid variable name"); }
            if (content[1]._tkntype != TokenType.ASSIGNMENT) { throw new ParseException("Expected assignment operator ':=' after variable name"); }

            if (content.Count < 3) throw new ParseException("Assignement statement al least need three tokens");

            // Register variable in the symbol table during parsing
            // But only if it doesn't already exist (to avoid creating unwanted shadow variables)
            if (content[1]._tkntype == TokenType.ASSIGNMENT)
            {
                string varName = content[0]._value;
                if (!keyval.ContainsKey(varName))
                {
                    keyval.Add(varName, null);  // Placeholder value during parsing
                }
                return new AST.AssignmentStmt(
                    ParseVariableNode(varName),
                    ParseExpression(content.GetRange(2, content.Count - 2))
                );
            }
            throw new ParseException("Assignement statement must have an assignment operator");
        }

        /// <summary>
        /// Parses a return statement with an expression.
        /// </summary>
        private static AST.ReturnStmt ParseReturnStatement(List<Tokenizer.Token> content)
        {
            if (content.Count < 2) { throw new ParseException("Missing expression after 'return'"); }

            if (content[0]._tkntype == TokenType.RETURN)
            {
                return new AST.ReturnStmt(ParseExpression(content.GetRange(1, content.Count - 1)));
            }
            throw new ParseException("Missing expression after 'return'");
        }

        /// <summary>
        /// Determines the correct statement type and delegates parsing.
        /// </summary>
        private static AST.Statement ParseStatement(List<Tokenizer.Token> content, SymbolTable<string, object> keyval)
        {
            if (content[0]._tkntype == TokenType.RETURN) { return ParseReturnStatement(content); }
            if (content.Count > 1 && content[1]._tkntype == TokenType.ASSIGNMENT)
            {
                return ParseAssignmentStmt(content, keyval);
            }
            throw new ParseException("Invalid statement");
        }

        #endregion

        #region Block and Statement List Parsing

        /// <summary>
        /// Parses a sequence of statements enclosed within a block.
        /// </summary>
        private static void ParseStmtList(List<string> lines, BlockStmt stmt)
        {
            SymbolTable<string, object> Data = stmt.SymbolTable;
            var tknzier = new TokenizerImpl();
            int i = 0;

            // Process lines until matching '}' is found
            while (i < lines.Count)
            {
                string line = lines[i].Trim();
                var content = tknzier.Tokenize(line);

                // Skip empty lines
                if (content.Count == 0)
                {
                    i++;
                    continue;
                }

                // Handle nested blocks
                if (content[0]._tkntype == TokenType.LEFT_CURLY)
                {
                    // Find the matching closing brace for this nested block
                    int curlyCount = 1;
                    int blockEndLine = i + 1;

                    while (blockEndLine < lines.Count && curlyCount > 0)
                    {
                        var lineTokens = tknzier.Tokenize(lines[blockEndLine]);
                        foreach (var token in lineTokens)
                        {
                            if (token._tkntype == TokenType.LEFT_CURLY) { curlyCount++; }
                            else if (token._tkntype == TokenType.RIGHT_CURLY) { curlyCount--; }
                        }
                        blockEndLine++;
                    }

                    // Check if we found the matching closing brace
                    if (curlyCount > 0)
                    {
                        throw new ParseException("Missing closing '}' for nested block");
                    }

                    // Extract only the lines for this nested block (from i to blockEndLine-1, inclusive)
                    int blockLineCount = blockEndLine - i;
                    var nestedBlockLines = lines.GetRange(i, blockLineCount);
                    var block = ParseBlockStmt(nestedBlockLines, Data);
                    stmt.Statements.Add(block);

                    // Remove the processed nested block lines
                    lines.RemoveRange(i, blockLineCount);
                }
                else if (content[0]._tkntype == TokenType.RIGHT_CURLY)
                {
                    return;
                }
                else
                {
                    // Parse and store single-line statement
                    var onelinerStmt = ParseStatement(content, Data);
                    stmt.Statements.Add(onelinerStmt);
                    lines.RemoveAt(i);
                }
            }

            // If we get here, all lines have been processed without encountering '}'
            // This is normal when called from ParseBlockStmt which has already extracted inner content
            return;
        }

        /// <summary>
        /// Parses a block statement delimited by '{' and '}' into a BlockStmt AST node.
        /// </summary>
        // private static AST.BlockStmt ParseBlockStmt(List<string> lines, SymbolTable<string, object> keyval)
        // {
        //     if (lines.Count == 0) { throw new ParseException("No strings"); }

        //     var tknzier = new TokenizerImpl();
        //     List<Tokenizer.Token> content = [];

        //     content.AddRange(tknzier.Tokenize(lines[0]));
        //     content.AddRange(tknzier.Tokenize(lines[lines.Count - 1]));

        //     // Validate block structure
        //     if (content.Count != 2 ||
        //         content[0]._tkntype != TokenType.LEFT_CURLY ||
        //         content[1]._tkntype != TokenType.RIGHT_CURLY)
        //     {
        //         throw new ParseException("Block must start with '{' and end with '}'");
        //     }

        //     SymbolTable<string, object> blockscope = new SymbolTable<string, object>(keyval);
        //     AST.BlockStmt block = new BlockStmt(blockscope);

        //     // Recursively parse contained statements
        //     ParseStmtList(lines.GetRange(1, lines.Count - 1), block);

        //     return block;
        // }

        // private static AST.BlockStmt ParseBlockStmt(List<string> lines, SymbolTable<string, object> keyval)
        // {
        //     if (lines == null || lines.Count == 0)
        //     {
        //         throw new ParseException("No strings or null lines provided");
        //     }

        //     var tknzier = new TokenizerImpl();
        //     List<Tokenizer.Token> content = new List<Tokenizer.Token>();

        //     // Safely tokenize first line
        //     var firstLine = lines[0];
        //     if (!string.IsNullOrEmpty(firstLine))
        //     {
        //         var firstTokens = tknzier.Tokenize(firstLine);
        //         if (firstTokens != null)
        //         {
        //             content.AddRange(firstTokens);
        //         }
        //     }

        //     // Safely tokenize last line if different from first line
        //     if (lines.Count > 1)
        //     {
        //         var lastLine = lines[lines.Count - 1];
        //         if (!string.IsNullOrEmpty(lastLine) && lastLine != firstLine)
        //         {
        //             var lastTokens = tknzier.Tokenize(lastLine);
        //             if (lastTokens != null)
        //             {
        //                 content.AddRange(lastTokens);
        //             }
        //         }
        //     }

        //     // Validate block structure - check if we have both { and } in the content
        //     bool hasLeft = false;
        //     bool hasRight = false;

        //     foreach (var token in content)
        //     {
        //         if (token?._tkntype == TokenType.LEFT_CURLY)
        //         {
        //             hasLeft = true;
        //         }
        //         else if (token?._tkntype == TokenType.RIGHT_CURLY)
        //         {
        //             hasRight = true;
        //         }
        //     }

        //     if (!hasLeft || !hasRight)
        //     {
        //         throw new ParseException("Block must start with '{' and end with '}'");
        //     }

        //     SymbolTable<string, object> blockscope = new SymbolTable<string, object>(keyval);
        //     AST.BlockStmt block = new BlockStmt(blockscope);

        //     // Only parse statements if there are lines between { and }
        //     if (lines.Count > 2)
        //     {
        //         var innerLines = lines.GetRange(1, lines.Count - 2);
        //         ParseStmtList(innerLines, block);
        //     }

        //     return block;
private static AST.BlockStmt ParseBlockStmt(List<string> lines, SymbolTable<string, object> keyval)
        {
            if (lines == null || lines.Count == 0)
            {
                throw new ParseException("No strings or null lines provided");
            }

            var tknzier = new TokenizerImpl();
            List<Tokenizer.Token> content = new List<Tokenizer.Token>();

            // Safely tokenize first line
            var firstLine = lines[0];
            if (!string.IsNullOrEmpty(firstLine))
            {
                var firstTokens = tknzier.Tokenize(firstLine);
                if (firstTokens != null)
                {
                    content.AddRange(firstTokens);
                }
            }

            // Safely tokenize last line if different from first line
            if (lines.Count > 1)
            {
                var lastLine = lines[lines.Count - 1];
                if (!string.IsNullOrEmpty(lastLine) && lastLine != firstLine)
                {
                    var lastTokens = tknzier.Tokenize(lastLine);
                    if (lastTokens != null)
                    {
                        content.AddRange(lastTokens);
                    }
                }
            }

            // Validate block structure - check if we have both { and } in the content
            bool hasLeft = false;
            bool hasRight = false;

            foreach (var token in content)
            {
                if (token?._tkntype == TokenType.LEFT_CURLY)
                {
                    hasLeft = true;
                }
                else if (token?._tkntype == TokenType.RIGHT_CURLY)
                {
                    hasRight = true;
                }
            }

            if (!hasLeft || !hasRight)
            {
                throw new ParseException("Block must start with '{' and end with '}'");
            }

            // For multi-line blocks, validate that first/last lines contain ONLY braces
            if (lines.Count > 1)
            {
                var firstTokens = tknzier.Tokenize(lines[0]);
                var lastTokens = tknzier.Tokenize(lines[lines.Count - 1]);

                // First line should only have '{'
                if (firstTokens.Count != 1 || firstTokens[0]._tkntype != TokenType.LEFT_CURLY)
                {
                    throw new ParseException("Block must start with '{' and end with '}'");
                }

                // Last line should only have '}'
                if (lastTokens.Count != 1 || lastTokens[0]._tkntype != TokenType.RIGHT_CURLY)
                {
                    throw new ParseException("Block must start with '{' and end with '}'");
                }
            }

            SymbolTable<string, object> blockscope = new SymbolTable<string, object>(keyval);
            AST.BlockStmt block = new BlockStmt(blockscope);

            // Handle single-line blocks (e.g., "{ return (2 ** 3); }")
            if (lines.Count == 1)
            {
                // Tokenize the single line and extract content between { and }
                var tokens = tknzier.Tokenize(lines[0]);
                if (tokens != null && tokens.Count > 2)
                {
                    // Find the positions of { and }
                    int leftBraceIndex = -1;
                    int rightBraceIndex = -1;

                    for (int i = 0; i < tokens.Count; i++)
                    {
                        if (tokens[i]._tkntype == TokenType.LEFT_CURLY && leftBraceIndex == -1)
                        {
                            leftBraceIndex = i;
                        }
                        else if (tokens[i]._tkntype == TokenType.RIGHT_CURLY && rightBraceIndex == -1)
                        {
                            rightBraceIndex = i;
                        }
                    }

                    // Extract tokens between { and }
                    if (leftBraceIndex != -1 && rightBraceIndex != -1 && rightBraceIndex > leftBraceIndex + 1)
                    {
                        var innerTokens = tokens.GetRange(leftBraceIndex + 1, rightBraceIndex - leftBraceIndex - 1);
                        if (innerTokens.Count > 0)
                        {
                            var statement = ParseStatement(innerTokens, blockscope);
                            block.Statements.Add(statement);
                        }
                    }
                }
            }
            // Multi-line blocks
            else if (lines.Count > 2)
            {
                var innerLines = lines.GetRange(1, lines.Count - 2);
                ParseStmtList(innerLines, block);
            }

            return block;


        #endregion
    }

}
}

