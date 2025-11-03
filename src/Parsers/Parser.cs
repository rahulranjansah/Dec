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
        public static AST.BlockStmt Parse(string Program) //Needs to be tokenized
        {
            SymbolTable<string, object> blockScope = new SymbolTable<string, object>();
            List<string> lines = new List<string>();
            foreach (string line in Program.Split('\n'))
            {
                lines.Add(line);
            }
            if (lines.Count < 2) { throw new ParseException("Program must have atleas opening '{' and closing '}'."); }
            return ParseBlockStmt(lines, blockScope);
        }

        // expressions from test it looks like: new[] { "(", "8", "/", "2", ")" }
        // non-recursive but innermost only
        public static AST.ExpressionNode ParseExpression(List<Token> expression)
        {
            // check one case other recursion will do
            if (expression.Count < 3) { throw new ParseException("Too small expression to work with"); }

            if (expression[0]._tkntype == TokenType.LEFT_CURLY && expression[expression.Count - 1]._tkntype == TokenType.RIGHT_CURLY)
            {
                throw new ParseException("mismatching parenthesis");
            }

            return ParseExpressionContent(expression.GetRange(1, expression.Count - 2));
        }

        public static AST.ExpressionNode ParseExpressionContent(List<Tokenizer.Token> content)
        {
            //keep track of a left and right and call cbn on them
            if (content.Count == 0) { throw new ParseException("No content"); }
            if (content.Count == 1) { return HandleSingleToken(content[0]); } //This will handle things like 4

            // we need head recursion call and then do stuff operator handling here
            if (content[0]._tkntype == TokenType.LEFT_PAREN && content[content.Count - 1]._tkntype == TokenType.RIGHT_PAREN)
            {
                return ParseExpressionContent(content.GetRange(1, content.Count - 2));
            }

            for (int i = 0; i < content.Count; i++)
            {
                if (content[i]._tkntype == TokenType.LEFT_PAREN)
                {
                    // nested parenthesis will be handled later in the stack
                    int parenCount = 1;
                    i++;
                    while (i < content.Count && parenCount > 0)
                    {
                        if (content[i]._tkntype == TokenType.LEFT_PAREN) { parenCount++; }
                        else if (content[i]._tkntype == TokenType.RIGHT_PAREN) { parenCount--; }
                        if (parenCount > 0) { i++; }
                    }
                }

                if (content[i]._tkntype == TokenType.OPERATOR)   //if its an operator, left stuff is an expression node, right stuff is an expression node
                {
                    return CreateBinaryOperatorNode(content[i]._value, ParseExpressionContent(content.GetRange(0, i)), ParseExpressionContent(content.GetRange(i + 1, content.Count - i - 1))); //Return binaryopnode
                }
            }
            throw new ParseException("Not a valid expression syntax");
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

        //if right parenthasi, escape recursion by passing back all info in a symbol table, and it will return if list<string> is empty
        // }
        public static void ParseStmtList(List<string> lines, BlockStmt stmt)
        {
            SymbolTable<string, object> Data = new SymbolTable<string, object>();

            //line by line
            var tknzier = new TokenizerImpl();
            int i = 0;
            while (i < lines.Count)
            {
                string line = lines[i].Trim();
                var content = tknzier.Tokenize(line);

                // Skip lines with no tokens (including empty lines)
                if (content.Count == 0)
                {
                    i++;
                    continue;
                }

                if (content[0]._tkntype == TokenType.LEFT_CURLY)
                {
                    // add everything Blockstmt handles it with peeling head recursion
                    var block = ParseBlockStmt(lines.GetRange(i, lines.Count - i), Data);
                    stmt.Statements.Add(block);

                    // eat all the lines outer and recursion will take care of inner
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
                    i += lineBeingEaten;

                }
                else if (content[0]._tkntype == TokenType.RIGHT_CURLY)
                {
                    return;
                }
                else
                {
                    var onelinerStmt = ParseStatement(content, Data);
                    stmt.Statements.Add(onelinerStmt);
                    i++;
                }
            }
        }

        public static AST.BlockStmt ParseBlockStmt(List<string> lines, SymbolTable<string, object> keyval)
        {
            if (lines.Count == 0) { throw new ParseException("No strings"); }

            var tknzier = new TokenizerImpl();
            List<Tokenizer.Token> content = [];

            content.AddRange(tknzier.Tokenize(lines[0]));
            content.AddRange(tknzier.Tokenize(lines[lines.Count - 1]));

            if (content[0]._tkntype != TokenType.LEFT_CURLY || content[1]._tkntype != TokenType.RIGHT_CURLY)
            {
                throw new ParseException("Block must start with '{' and end with '}'");
            }

            SymbolTable<string, object> blockScope = new SymbolTable<string, object>(keyval); //how to use symbol table here?
            AST.BlockStmt Block = new BlockStmt([]);

            ParseStmtList(lines.GetRange(1, lines.Count - 1), Block);

            return Block;

        }
    }
}
