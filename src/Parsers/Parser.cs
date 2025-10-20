using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Formats.Asn1;
using System.Reflection;
using Utilities;
using AST;
using Tokenizer;
using System.Security.Principal;

namespace Parser
{
    public class ParseException : Exception
    {
        // constructor needs since not static and base because calls parent and requires message to be stored.
        public ParseException(string message) : base(message) { }
    }
    public static class Parser
    {
        public static AST.BlockStmt Parse(string Program)
        {
            return null;
            if (Program == null) { throw new ParseException("Program is null"); }
            if (Program[0]._tkntype == TokenType.LEFT_CURLY && Program[Program.Count - 1]._tkntype == TokenType.RIGHT.CURLY)
            {
                return AST.BlockStmt()
            }
        }
        // expressions from test it looks like: new[] { "(", "8", "/", "2", ")" }
        // non-recursive but innermost only
        public static AST.ExpressionNode ParseExpression(List<Token> expression)
        {
            return ParseExpressionHelper(expression, 0);
        }

        // helper function to parse the content of the expression
        private static AST.ExpressionNode ParseExpressionHelper(List<Tokenizer.Token> expression, int i)
        {
            // if (expression[i]._tkntype == TokenType.LEFT_PAREN && expression[i+4]._tkntype == TokenType.RIGHT_PAREN)
            if (expression[i]._tkntype == TokenType.LEFT_PAREN && i + 4 < expression.Count && expression[i + 4]._tkntype == TokenType.RIGHT_PAREN)
            {
                return ParseExpressionContent(expression.GetRange(i + 1, 3));
            }

            return ParseExpressionHelper(expression.GetRange(i, expression.Count - i - 1), i + 1);
        }

        public static AST.ExpressionNode ParseExpressionContent(List<Tokenizer.Token> content)
        {
            if (content.Count == 0) { throw new ParseException("No content"); }
            if (content.Count == 1) { return HandleSingleToken(content[0]); }
            if (content.Count == 3) //handle x + y + z?
            {
                var left = HandleSingleToken(content[0]);

                // Tokenizer.Token type elements
                var op = content[1];
                var right = HandleSingleToken(content[2]);
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

        // Blocks
        public static void ParseStmtList(List<string> lines, BlockStmt stmt)
        {
            //line by line
            //check for left parenthasi in first posifition

            //if it is, feed line by line to parse statment untill a parethasi
            //if left parenthasi, call parseblockstatment

            //if right parenthasi, escape recursion by passing back all info in a symbol table, and it will return if list<string> is empty
        }

        // public static void ParseStmtListHelper(List<Tokenizer.Token> content, BlockStmt stmt, int i)
        // {

        // }

        public static AST.BlockStmt ParseBlockStmt(List<string> lines, SymbolTable<string, object> keyval)
        {
            AST.BlockStmt Block = new BlockStmt([]);
            var tknzier = new TokenizerImpl();
            List<Tokenizer.Token> content = [];
            content.AddRange(tknzier.Tokenize(lines[0]));
            if (content[0]._tkntype == TokenType.LEFT_CURLY)
            {
                ParseStmtList(lines.GetRange(1, lines.Count - 1), Block);
            }

            return Block;
            //Somehow check right parens for equality?
        }
    }
}