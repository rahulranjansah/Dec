using System;
using System.Collections.Generic;
using Xunit;
using AST;
using Tokenizer;
using System.Reflection;

namespace Parser.Tests
{
    public class ParseExpressionTests
    {
        private List<Token> CreateTokens(string[] values, TokenType[] types)
        {
            var list = new List<Token>();
            for (int i = 0; i < values.Length; i++)
                list.Add(new Token(values[i], types[i]));
            return list;
        }

        private ExpressionNode InvokeParseExpression(List<Token> tokens)
        {
            Type parserType = typeof(global::Parser.Parser);
            MethodInfo m = parserType.GetMethod("ParseExpression",
                BindingFlags.NonPublic | BindingFlags.Static);
            return (ExpressionNode)m.Invoke(null, new object[] { tokens });
        }

        [Fact]
        public void ParseExpression_SimpleAddition_ReturnsPlusNode()
        {
            var tokens = CreateTokens(
                new[] { "(", "5", "+", "3", ")" },
                new[] { TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.OPERATOR, TokenType.INTEGER, TokenType.RIGHT_PAREN });

            var result = InvokeParseExpression(tokens);

            Assert.NotNull(result);
            var plus = Assert.IsType<PlusNode>(result);
            Assert.Equal("5", ((LiteralNode)plus.Left).Value);
            Assert.Equal("3", ((LiteralNode)plus.Right).Value);
        }

        [Fact]
        public void ParseExpression_SimpleSubtraction_ReturnsMinusNode()
        {
            var tokens = CreateTokens(
                new[] { "(", "10", "-", "7", ")" },
                new[] { TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.OPERATOR, TokenType.INTEGER, TokenType.RIGHT_PAREN });

            var result = InvokeParseExpression(tokens);

            var minus = Assert.IsType<MinusNode>(result);
            Assert.Equal("10", ((LiteralNode)minus.Left).Value);
            Assert.Equal("7", ((LiteralNode)minus.Right).Value);
        }

        [Fact]
        public void ParseExpression_SimpleMultiplication_ReturnsTimesNode()
        {
            var tokens = CreateTokens(
                new[] { "(", "4", "*", "6", ")" },
                new[] { TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.OPERATOR, TokenType.INTEGER, TokenType.RIGHT_PAREN });

            var result = InvokeParseExpression(tokens);

            var times = Assert.IsType<TimesNode>(result);
            Assert.Equal("4", ((LiteralNode)times.Left).Value);
            Assert.Equal("6", ((LiteralNode)times.Right).Value);
        }

        [Fact]
        public void ParseExpression_SimpleDivision_ReturnsFloatDivNode()
        {
            var tokens = CreateTokens(
                new[] { "(", "8", "/", "2", ")" },
                new[] { TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.OPERATOR, TokenType.INTEGER, TokenType.RIGHT_PAREN });

            var result = InvokeParseExpression(tokens);

            var div = Assert.IsType<FloatDivNode>(result);
            Assert.Equal("8", ((LiteralNode)div.Left).Value);
            Assert.Equal("2", ((LiteralNode)div.Right).Value);
        }

        [Fact]
        public void ParseExpression_SimpleModulo_ReturnsModulusNode()
        {
            var tokens = CreateTokens(
                new[] { "(", "10", "%", "3", ")" },
                new[] { TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.OPERATOR, TokenType.INTEGER, TokenType.RIGHT_PAREN });

            var result = InvokeParseExpression(tokens);

            var mod = Assert.IsType<ModulusNode>(result);
            Assert.Equal("10", ((LiteralNode)mod.Left).Value);
            Assert.Equal("3", ((LiteralNode)mod.Right).Value);
        }

        [Fact]
        public void ParseExpression_SimpleExponentiation_ReturnsExponentiationNode()
        {
            var tokens = CreateTokens(
                new[] { "(", "2", "**", "3", ")" },
                new[] { TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.OPERATOR, TokenType.INTEGER, TokenType.RIGHT_PAREN });

            var result = InvokeParseExpression(tokens);

            var exp = Assert.IsType<ExponentiationNode>(result);
            Assert.Equal("2", ((LiteralNode)exp.Left).Value);
            Assert.Equal("3", ((LiteralNode)exp.Right).Value);
        }

        [Fact]
        public void ParseExpression_WithVariables_ReturnsPlusNode()
        {
            var tokens = CreateTokens(
                new[] { "(", "x", "+", "y", ")" },
                new[] { TokenType.LEFT_PAREN, TokenType.VARIABLE, TokenType.OPERATOR, TokenType.VARIABLE, TokenType.RIGHT_PAREN });

            var result = InvokeParseExpression(tokens);

            var plus = Assert.IsType<PlusNode>(result);
            Assert.Equal("x", ((VariableNode)plus.Left).Name);
            Assert.Equal("y", ((VariableNode)plus.Right).Name);
        }

        [Fact]
        public void ParseExpression_NestedLeftSide_ReturnsCorrectTree()
        {
            // ((5 + 3) * 2)
            var tokens = CreateTokens(
                new[] { "(", "(", "5", "+", "3", ")", "*", "2", ")" },
                new[] { TokenType.LEFT_PAREN, TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.OPERATOR,
                        TokenType.INTEGER, TokenType.RIGHT_PAREN, TokenType.OPERATOR, TokenType.INTEGER, TokenType.RIGHT_PAREN });

            var result = InvokeParseExpression(tokens);

            var times = Assert.IsType<TimesNode>(result);
            var left = Assert.IsType<PlusNode>(times.Left);
            var right = Assert.IsType<LiteralNode>(times.Right);

            Assert.Equal("5", ((LiteralNode)left.Left).Value);
            Assert.Equal("3", ((LiteralNode)left.Right).Value);
            Assert.Equal("2", right.Value);
        }

        [Fact]
        public void ParseExpression_InvalidOperator_ThrowsParseException()
        {
            var tokens = CreateTokens(
                new[] { "(", "5", "$", "3", ")" },
                new[] { TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.OPERATOR, TokenType.INTEGER, TokenType.RIGHT_PAREN });

            var ex = Assert.Throws<TargetInvocationException>(() => InvokeParseExpression(tokens));
            Assert.IsType<ParseException>(ex.InnerException);
            Assert.Contains("Invalid operator", ex.InnerException.Message);
        }

        [Fact]
        public void ParseExpression_MismatchedParenthesis_ThrowsParseException()
        {
            var tokens = CreateTokens(
                new[] { "{", "2", "+", "3", "}" },
                new[] { TokenType.LEFT_CURLY, TokenType.INTEGER, TokenType.OPERATOR, TokenType.INTEGER, TokenType.RIGHT_CURLY });

            var ex = Assert.Throws<TargetInvocationException>(() => InvokeParseExpression(tokens));
            Assert.IsType<ParseException>(ex.InnerException);
            Assert.Contains("mismatching", ex.InnerException.Message);
        }

        [Fact]
        public void ParseExpression_EmptyParentheses_ThrowsParseException()
        {
            var tokens = CreateTokens(
                new[] { "(", ")" },
                new[] { TokenType.LEFT_PAREN, TokenType.RIGHT_PAREN });

            var ex = Assert.Throws<TargetInvocationException>(() => InvokeParseExpression(tokens));
            Assert.IsType<ParseException>(ex.InnerException);
        }

        [Fact]
        public void ParseExpression_SingleIntegerLiteral_ReturnsLiteralNode()
        {
            var tokens = CreateTokens(
                new[] { "(", "42", ")" },
                new[] { TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.RIGHT_PAREN });

            var result = InvokeParseExpression(tokens);

            var lit = Assert.IsType<LiteralNode>(result);
            Assert.Equal("42", lit.Value);
        }

        [Fact]
        public void ParseExpression_SingleVariable_ReturnsVariableNode()
        {
            var tokens = CreateTokens(
                new[] { "(", "x", ")" },
                new[] { TokenType.LEFT_PAREN, TokenType.VARIABLE, TokenType.RIGHT_PAREN });

            var result = InvokeParseExpression(tokens);

            var varNode = Assert.IsType<VariableNode>(result);
            Assert.Equal("x", varNode.Name);
        }

        [Fact]
        public void ParseExpression_NestedSingleValue_ReturnsSameLiteral()
        {
            var tokens = CreateTokens(
                new[] { "(", "(", "42", ")", ")" },
                new[] { TokenType.LEFT_PAREN, TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.RIGHT_PAREN, TokenType.RIGHT_PAREN });

            var result = InvokeParseExpression(tokens);

            var lit = Assert.IsType<LiteralNode>(result);
            Assert.Equal("42", lit.Value);
        }

        [Fact]
        public void ParseExpression_InvalidStructure_ThrowsParseException()
        {
            var tokens = CreateTokens(
                new[] { "(", "42", "junk", ")" },
                new[] { TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.VARIABLE, TokenType.RIGHT_PAREN });

            var ex = Assert.Throws<TargetInvocationException>(() => InvokeParseExpression(tokens));
            Assert.IsType<ParseException>(ex.InnerException);
        }
    }
}
