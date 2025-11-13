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
                throw new ParseException("Program must have at least opening '{' and closing '}'.");
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
            // Ensure minimum valid structure (e.g., "( 8 / 2 )")
            if (expression.Count < 3) { throw new ParseException("Too small expression to work with"); }

            if (expression[0]._tkntype == TokenType.LEFT_CURLY && expression[expression.Count - 1]._tkntype == TokenType.RIGHT_CURLY)
            {
                throw new ParseException("mismatching parenthesis");
            }

            // Remove outer parentheses and evaluate the content
            return ParseExpressionContent(expression.GetRange(1, expression.Count - 2));
        }

        /// <summary>
        /// Handles the recursive parsing of an expression's internal content.
        /// </summary>
        /// <param name="content">Tokenized list of expression tokens excluding parentheses.</param>
        /// <returns>An ExpressionNode representing nested or flat expressions.</returns>
        private static AST.ExpressionNode ParseExpressionContent(List<Tokenizer.Token> content)
        {
            if (content.Count == 0) { throw new ParseException("No content"); }
            if (content.Count == 1) { return HandleSingleToken(content[0]); }

            // Strip redundant parentheses and recurse
            if (content[0]._tkntype == TokenType.LEFT_PAREN && content[content.Count - 1]._tkntype == TokenType.RIGHT_PAREN)
            {
                return ParseExpressionContent(content.GetRange(1, content.Count - 2));
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

            // Register variable in the symbol table
            if (content[1]._tkntype == TokenType.ASSIGNMENT)
            {
                keyval.Keys.Add(content[0]._value);
                return new AST.AssignmentStmt(
                    ParseVariableNode(content[0]._value),
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
                    var block = ParseBlockStmt(lines.GetRange(i, lines.Count - i), Data);
                    stmt.Statements.Add(block);

                    // Skip over nested block lines
                    int curlyCount = 1;
                    int lineBeingEaten = i + 1;
                    while (lineBeingEaten < lines.Count && curlyCount > 0)
                    {
                        foreach (var token in tknzier.Tokenize(lines[lineBeingEaten]))
                        {
                            if (token._tkntype == TokenType.LEFT_CURLY) { curlyCount++; }
                            else if (token._tkntype == TokenType.RIGHT_CURLY) { curlyCount--; }
                        }
                        lineBeingEaten++;
                    }
                    lines.RemoveRange(i, lineBeingEaten - i);
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

            throw new ParseException("Missing closing '}' in block.");
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
private static AST.BlockStmt ParseBlockStmt(List<string> lines, SymbolTable<string, object> parent)
{
    var tokenizer = new TokenizerImpl();
    var tokens = new List<Token>();

    // Flatten lines → token stream
    foreach (string line in lines)
        tokens.AddRange(tokenizer.Tokenize(line));

    if (tokens.Count == 0)
        throw new ParseException("Empty block");

    // Find opening "{"
    int start = tokens.FindIndex(t => t._tkntype == TokenType.LEFT_CURLY);
    if (start == -1)
        throw new ParseException("Missing '{'");

    // Find matching "}"
    int depth = 0;
    int end = -1;

    for (int i = start; i < tokens.Count; i++)
    {
        if (tokens[i]._tkntype == TokenType.LEFT_CURLY)
            depth++;

        else if (tokens[i]._tkntype == TokenType.RIGHT_CURLY)
            depth--;

        if (depth == 0)
        {
            end = i;
            break;
        }
    }

    if (end == -1)
        throw new ParseException("Missing '}'");

    // Extract inner token slice
    var innerTokens = tokens.GetRange(start + 1, end - start - 1);

    // Convert inner tokens to lines again (simple version: join into one line)
    List<string> innerLines = new List<string>();
    if (innerTokens.Count > 0)
    {
        string line = string.Join(" ", innerTokens.Select(t => t._value));
        innerLines.Add(line);
    }

    // Create block scope
    var scope = new SymbolTable<string, object>(parent);
    var block = new BlockStmt(scope);

    // Use existing statement list parser
    ParseStmtList(innerLines, block);

    return block;


        #endregion
    }

}
}

