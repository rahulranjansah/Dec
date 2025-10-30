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
    public class ParseException : Exception
    {
        // constructor needs since not static and base because calls parent and requires message to be stored.
        public ParseException(string message) : base(message) { }
    }
    public static class Parser
    {
        public static AST.BlockStmt Parse(string program)
        {
            if (program == null) { throw new ParseException("Program is null"); }

            var lines = program.Split("\n");

            if (lines[0].StartsWith("{") && lines[lines.Length - 1].EndsWith("}"))
            {
                var keyval = new SymbolTable<string, object>();
                return ParseBlockStmt(lines, keyval);
            }
            throw new ArgumentException("Blocks braces are imbalanced");
        }
        // expressions from test it looks like: new[] { "(", "8", "/", "2", ")" }
        // non-recursive but innermost only
        public static AST.ExpressionNode ParseExpression(List<Token> expression)
        {
            // check one case other recursion will do
            if (expression.Count < 3) { throw new ParseException("Too small expression to work with"); }

            if (expression[0]._tkntype == TokenType.LEFT_CURLY && expression[expression.Count - 1]._tkntype == TokenType.RIGHT_CURLY)
            {
                throw new ParseException("mismatching curly braces");
            }

            return ParseExpressionContent(expression.GetRange(1, expression.Count - 1));

            // return ParseExpressionHelper(expression, 0);
        }

        // // helper function to parse the content of the expression
        // private static AST.ExpressionNode ParseExpressionHelper(List<Tokenizer.Token> expression, int i)
        // {
        //     // if (expression[i]._tkntype == TokenType.LEFT_PAREN && expression[i+4]._tkntype == TokenType.RIGHT_PAREN)
        //     if (expression[i]._tkntype == TokenType.LEFT_PAREN && i + 4 < expression.Count && expression[i + 4]._tkntype == TokenType.RIGHT_PAREN)
        //     {
        //         return ParseExpressionContent(expression.GetRange(i + 1, 3));
        //     }

        //     return ParseExpressionHelper(expression.GetRange(i, expression.Count - i - 1), i + 1);
        // }

        public static AST.ExpressionNode ParseExpressionContent(List<Tokenizer.Token> content)
        {
            if (content.Count == 0) { throw new ParseException("No content"); }

            // base case
            if (content.Count == 1) { return HandleSingleToken(content[0]); }
            if (content.Count > 3) //handle x + y + z? more than two, ternary operator?
            {
                var left = HandleSingleToken(content[0]);
                var op = content[1];

                var right = ParseExpressionContent(content.GetRange(2, content.Count - 1));

                // intermediate stack return calls LIFO output
                return CreateBinaryOperatorNode(op._value, left, right);
            }
            throw new ParseException("Invalid expression content");
        }


        public static AST.ExpressionNode HandleSingleToken(Tokenizer.Token token)
        {
            if (token._tkntype == TokenType.INTEGER) { return new LiteralNode(token._value); }
            if (token._tkntype == TokenType.FLOAT) { return new LiteralNode(token._value); }
            if (token._tkntype == TokenType.VARIABLE) { return new VariableNode(token._value); }
            throw new ParseException("Unexpected token may not not float or integer or variable");
        }

        public static AST.ExpressionNode CreateBinaryOperatorNode(string op, AST.ExpressionNode l, AST.ExpressionNode r)
        {
            if (op == TokenConstants.PLUS) { return new AST.PlusNode(l, r); }
            if (op == TokenConstants.MINUS) { return new AST.MinusNode(l, r); }
            if (op == TokenConstants.TIMES) { return new AST.TimesNode(l, r); }
            if (op == TokenConstants.INT_DIV) { return new AST.IntDivNode(l, r); } // This is a float division
            if (op == TokenConstants.FLOAT_DIV) { return new AST.FloatDivNode(l, r); }
            if (op == TokenConstants.MOD) { return new AST.ModulusNode(l, r); }
            if (op == TokenConstants.EXP) { return new AST.ExponentiationNode(l, r); }

            throw new ParseException($"Invalid operator has been used: {op}");
        }

        public static AST.VariableNode ParseVariableNode(string variable)
        {
            if (variable == null) { throw new ParseException("Variable is null"); }
            return new AST.VariableNode(variable);
        }

        // Individual Statements
        public static AST.AssignmentStmt ParseAssignmentStmt(List<Tokenizer.Token> content, SymbolTable<string, object> keyval)
        {
            if (content.Count < 3) throw new ParseException("Assignement statement al least need three tokens");

            // check if the first token is a variable
            if (content[1]._tkntype == TokenType.ASSIGNMENT)
            {
                keyval.Keys.Add(content[0]._value);
                // keyval.Values.Add(null);
                return new AST.AssignmentStmt(ParseVariableNode(content[0]._value), ParseExpression(content.GetRange(2, content.Count - 1)));
            }
            throw new ParseException("Assignement statement must have an assignment operator");
        }

        public static AST.ReturnStmt ParseReturnStatement(List<Tokenizer.Token> content)
        {
            if (content.Count < 2) { throw new ParseException("Return statement must have at least two tokens"); }

            if (content[0]._tkntype == TokenType.RETURN)
            {
                return new AST.ReturnStmt(ParseExpression(content.GetRange(1, content.Count - 1)));
            }
            throw new ParseException("Return statement must start with return keyword");
        }

        public static AST.Statement ParseStatement(List<Tokenizer.Token> content, SymbolTable<string, object> keyval)
        {
            if (content[0]._tkntype == TokenType.RETURN) { return ParseReturnStatement(content); }
            if (content[0]._tkntype == TokenType.ASSIGNMENT)
            {
                return ParseAssignmentStmt(content, keyval);
            }
            throw new ParseException("Invalid statement");
        }

        //     //if right parenthasi, escape recursion by passing back all info in a symbol table, and it will return if list<string> is empty
        // }

        public static void ParseStmtList(List<string> lines, BlockStmt stmt)
        {
            var tokenizer = new TokenizerImpl();

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();

                // End of block
                if (line == "}")
                    return;

                var tokens = tokenizer.Tokenize(line);

                // Handle nested block
                if (tokens[0]._tkntype == TokenType.LEFT_CURLY)
                {
                    // The recursive call handles its own { ... } range
                    var nestedBlock = ParseBlockStmt(lines.Skip(i).ToList(), new SymbolTable<string, object>());

                    // Add nested block to current block's statements
                    stmt.Statements.Add(nestedBlock);

                    // Now skip the lines consumed by that block
                    i += CountLinesInBlock(lines.Skip(i).ToList()) - 1;
                }
                else
                {
                    // Normal (non-block) statement
                    stmt.Statements.Add(ParseStatement(tokens, new SymbolTable<string, object>()));
                }
            }

            // If loop ends without }, it's a syntax error
            throw new ParseException("Block not properly closed with '}'.");
        }


        // public static void ParseStmtListHelper(List<Tokenizer.Token> content, BlockStmt stmt, int i)
        // {

        // }

        // public static AST.BlockStmt ParseBlockStmt(List<string> lines, SymbolTable<string, object> keyval)
        // {
        //     AST.BlockStmt Block = new BlockStmt([]);
        //     var tknzier = new TokenizerImpl();
        //     List<Tokenizer.Token> content = [];
        //     content.AddRange(tknzier.Tokenize(lines[0]));
        //     if (content[0]._tkntype == TokenType.LEFT_CURLY)
        //     {
        //         ParseStmtList(lines.GetRange(1, lines.Count - 1), Block);
        //     }

        //     return Block;
        //     //Somehow check right parens for equality?
        // }

        // add depth like BST to check if is balanced or not like a stack that tracks the numbers.
        public static BlockStmt ParseBlockStmt(List<string> lines, SymbolTable<string, object> table)
        {
            if (!lines[0].Trim().StartsWith("{"))
            {
                throw new ParseException("BLock must start with {");
            }

            //  put the list of statment into blocks
            var stmt = new BlockStmt(new List<Statement>());

            for (int i = 1; i < lines.Count; i++)
            {
                // part 1: working with line trim each
                string line = lines[i].Trim();

                // if hits break from loop out questionable placement
                if (line == "}") break;

                // tokenize each line instead of doing single lines
                var tokens = new TokenizerImpl().Tokenize(line);

                // does that happen early or late?
                //  shouldn't recursion handle itself othercase if we do check for one case
                if (tokens[0]._tkntype == TokenType.LEFT_CURLY)
                {
                    ParseStmtList(lines, stmt);
                    var nestedBlock = ParseBlockStmt(ParseStmtList(lines, stmt), new SymbolTable<string, object>(table));

                    // Blockstmt class has staments getter only do i need to make it to setter to?
                    // Add that nested block to current block
                    stmt.Statements.Add(innerBlock);

                    // 🔹 Skip the lines we just consumed
                    int consumed = CountLinesInBlock(remaining);
                    i += consumed - 1; // -1 because loop will increment again
                }
                else
                {
                    // 🔹 Regular statement
                    var parsedStmt = ParseStatement(tokens, symbols);
                    stmt.Statements.Add(parsedStmt);
                }
            }

            // If you finish the loop without a closing brace, it’s malformed
            throw new ParseException("Block missing closing brace '}'.");
        }

                    stmt.Statements.Add(nestedBlock);

                }

            }

            return stmt;

            //     for (int i = 1; i < lines.Count; i++)
            //     {
            //         string line = lines[i].Trim();

            //         if (line == "}") break; // end of this block

            //         var tokens = new TokenizerImpl().Tokenize(line);

            //         // Handle nested blocks recursively
            //         if (tokens[0]._tkntype == TokenType.LEFT_CURLY)
            //         {
            //             var innerBlockLines = ExtractInnerBlock(lines, ref i);
            //             var nestedBlock = ParseBlockStmt(innerBlockLines, new SymbolTable<string, object>(table));
            //             block.Statements.Add(nestedBlock);
            //         }
            //         else
            //         {
            //             block.Statements.Add(ParseStatement(tokens, table));
            //         }
            //     }

            //     return block;
            // }

        }
    }
}