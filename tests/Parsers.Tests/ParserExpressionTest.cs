using System;
using System.Collections.Generic;
using Xunit;
using AST;
using Tokenizer;
using System.Linq;
using System.Reflection;

namespace Parser.Tests
{
    public class ParseExpressionTests
    {
        // Helper method to create a token list for testing ParseExpression
        private List<Token> CreateTokens(string[] tokenValues, TokenType[] tokenTypes)
        {
            var tokens = new List<Token>();
            for (int i = 0; i < tokenValues.Length; i++)
            {
                tokens.Add(new Token(tokenValues[i], tokenTypes[i]));
            }
            return tokens;
        }
        
        // Helper method to invoke the private ParseExpression method using reflection
        private ExpressionNode InvokeParseExpression(List<Token> tokens)
        {
            Type parserType = typeof(Parser);
            MethodInfo methodInfo = parserType.GetMethod("ParseExpression", 
                BindingFlags.NonPublic | BindingFlags.Static);
            
            return (ExpressionNode)methodInfo.Invoke(null, new object[] { tokens });
        }

        [Fact]
        public void ParseExpression_SimpleAddition_ReturnsPlusNode()
        {
            // Arrange
            var tokens = CreateTokens(
                new[] { "(", "5", "+", "3", ")" },
                new[] { TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.OPERATOR, TokenType.INTEGER, TokenType.RIGHT_PAREN }
            );

            // Act
            var result = InvokeParseExpression(tokens);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PlusNode>(result);
            
            var plusNode = (PlusNode)result;
            var leftExpr = plusNode.Left;
            var rightExpr = plusNode.Right;
            
            Assert.IsType<LiteralNode>(leftExpr);
            Assert.IsType<LiteralNode>(rightExpr);
            
            var leftLiteral = (LiteralNode)leftExpr;
            var rightLiteral = (LiteralNode)rightExpr;
            
            Assert.Equal(5, leftLiteral.Value);
            Assert.Equal(3, rightLiteral.Value);
        }

        [Fact]
        public void ParseExpression_SimpleSubtraction_ReturnsMinusNode()
        {
            // Arrange
            var tokens = CreateTokens(
                new[] { "(", "10", "-", "7", ")" },
                new[] { TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.OPERATOR, TokenType.INTEGER, TokenType.RIGHT_PAREN }
            );

            // Act
            var result = InvokeParseExpression(tokens);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<MinusNode>(result);
            
            var minusNode = (MinusNode)result;
            var leftExpr = minusNode.Left;
            var rightExpr = minusNode.Right;
            
            Assert.IsType<LiteralNode>(leftExpr);
            Assert.IsType<LiteralNode>(rightExpr);
            
            var leftLiteral = (LiteralNode)leftExpr;
            var rightLiteral = (LiteralNode)rightExpr;
            
            Assert.Equal(10, leftLiteral.Value);
            Assert.Equal(7, rightLiteral.Value);
        }

        [Fact]
        public void ParseExpression_SimpleMultiplication_ReturnsTimesNode()
        {
            // Arrange
            var tokens = CreateTokens(
                new[] { "(", "4", "*", "6", ")" },
                new[] { TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.OPERATOR, TokenType.INTEGER, TokenType.RIGHT_PAREN }
            );

            // Act
            var result = InvokeParseExpression(tokens);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<TimesNode>(result);
            
            var timesNode = (TimesNode)result;
            var leftExpr = timesNode.Left;
            var rightExpr = timesNode.Right;
            
            Assert.IsType<LiteralNode>(leftExpr);
            Assert.IsType<LiteralNode>(rightExpr);
            
            var leftLiteral = (LiteralNode)leftExpr;
            var rightLiteral = (LiteralNode)rightExpr;
            
            Assert.Equal(4, leftLiteral.Value);
            Assert.Equal(6, rightLiteral.Value);
        }

        [Fact]
        public void ParseExpression_SimpleDivision_ReturnsFloatDivNode()
        {
            // Arrange
            var tokens = CreateTokens(
                new[] { "(", "8", "/", "2", ")" },
                new[] { TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.OPERATOR, TokenType.INTEGER, TokenType.RIGHT_PAREN }
            );

            // Act
            var result = InvokeParseExpression(tokens);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<FloatDivNode>(result);
            
            var divNode = (FloatDivNode)result;
            var leftExpr = divNode.Left;
            var rightExpr = divNode.Right;
            
            Assert.IsType<LiteralNode>(leftExpr);
            Assert.IsType<LiteralNode>(rightExpr);
            
            var leftLiteral = (LiteralNode)leftExpr;
            var rightLiteral = (LiteralNode)rightExpr;
            
            Assert.Equal(8, leftLiteral.Value);
            Assert.Equal(2, rightLiteral.Value);
        }

        [Fact]
        public void ParseExpression_SimpleIntegerDivision_ReturnsIntDivNode()
        {
            // Arrange
            var tokens = CreateTokens(
                new[] { "(", "9", "//", "2", ")" },
                new[] { TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.OPERATOR, TokenType.INTEGER, TokenType.RIGHT_PAREN }
            );

            // Act
            var result = InvokeParseExpression(tokens);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<IntDivNode>(result);
            
            var intDivNode = (IntDivNode)result;
            var leftExpr = intDivNode.Left;
            var rightExpr = intDivNode.Right;
            
            Assert.IsType<LiteralNode>(leftExpr);
            Assert.IsType<LiteralNode>(rightExpr);
            
            var leftLiteral = (LiteralNode)leftExpr;
            var rightLiteral = (LiteralNode)rightExpr;
            
            Assert.Equal(9, leftLiteral.Value);
            Assert.Equal(2, rightLiteral.Value);
        }

        [Fact]
        public void ParseExpression_SimpleModulo_ReturnsModulusNode()
        {
            // Arrange
            var tokens = CreateTokens(
                new[] { "(", "10", "%", "3", ")" },
                new[] { TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.OPERATOR, TokenType.INTEGER, TokenType.RIGHT_PAREN }
            );

            // Act
            var result = InvokeParseExpression(tokens);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ModulusNode>(result);
            
            var modNode = (ModulusNode)result;
            var leftExpr = modNode.Left;
            var rightExpr = modNode.Right;
            
            Assert.IsType<LiteralNode>(leftExpr);
            Assert.IsType<LiteralNode>(rightExpr);
            
            var leftLiteral = (LiteralNode)leftExpr;
            var rightLiteral = (LiteralNode)rightExpr;
            
            Assert.Equal(10, leftLiteral.Value);
            Assert.Equal(3, rightLiteral.Value);
        }

        [Fact]
        public void ParseExpression_SimplePower_ReturnsExponentiationNode()
        {
            // Arrange
            var tokens = CreateTokens(
                new[] { "(", "2", "**", "3", ")" },
                new[] { TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.OPERATOR, TokenType.INTEGER, TokenType.RIGHT_PAREN }
            );

            // Act
            var result = InvokeParseExpression(tokens);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ExponentiationNode>(result);
            
            var expNode = (ExponentiationNode)result;
            var leftExpr = expNode.Left;
            var rightExpr = expNode.Right;
            
            Assert.IsType<LiteralNode>(leftExpr);
            Assert.IsType<LiteralNode>(rightExpr);
            
            var leftLiteral = (LiteralNode)leftExpr;
            var rightLiteral = (LiteralNode)rightExpr;
            
            Assert.Equal(2, leftLiteral.Value);
            Assert.Equal(3, rightLiteral.Value);
        }

        [Fact]
        public void ParseExpression_WithVariables_ReturnsCorrectNode()
        {
            // Arrange
            var tokens = CreateTokens(
                new[] { "(", "x", "+", "y", ")" },
                new[] { TokenType.LEFT_PAREN, TokenType.VARIABLE, TokenType.OPERATOR, TokenType.VARIABLE, TokenType.RIGHT_PAREN }
            );

            // Act
            var result = InvokeParseExpression(tokens);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<PlusNode>(result);
            
            var plusNode = (PlusNode)result;
            var leftExpr = plusNode.Left;
            var rightExpr = plusNode.Right;
            
            Assert.IsType<VariableNode>(leftExpr);
            Assert.IsType<VariableNode>(rightExpr);
            
            var leftVar = (VariableNode)leftExpr;
            var rightVar = (VariableNode)rightExpr;
            
            Assert.Equal("x", leftVar.Name);
            Assert.Equal("y", rightVar.Name);
        }

        [Fact]
        public void ParseExpression_WithFloatLiterals_ReturnsCorrectNode()
        {
            // Arrange
            var tokens = CreateTokens(
                new[] { "(", "3.14", "*", "2.5", ")" },
                new[] { TokenType.LEFT_PAREN, TokenType.FLOAT, TokenType.OPERATOR, TokenType.FLOAT, TokenType.RIGHT_PAREN }
            );

            // Act
            var result = InvokeParseExpression(tokens);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<TimesNode>(result);
            
            var timesNode = (TimesNode)result;
            var leftExpr = timesNode.Left;
            var rightExpr = timesNode.Right;
            
            Assert.IsType<LiteralNode>(leftExpr);
            Assert.IsType<LiteralNode>(rightExpr);
            
            var leftLiteral = (LiteralNode)leftExpr;
            var rightLiteral = (LiteralNode)rightExpr;
            
            Assert.Equal(3.14, leftLiteral.Value);
            Assert.Equal(2.5, rightLiteral.Value);
        }

        [Fact]
        public void ParseExpression_NestedLeftSide_ReturnsCorrectTree()
        {
            // Arrange - create expression: ((5 + 3) * 2)
            var tokens = CreateTokens(
                new[] { "(", "(", "5", "+", "3", ")", "*", "2", ")" },
                new[] { 
                    TokenType.LEFT_PAREN, 
                    TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.OPERATOR, TokenType.INTEGER, TokenType.RIGHT_PAREN, 
                    TokenType.OPERATOR, 
                    TokenType.INTEGER, 
                    TokenType.RIGHT_PAREN 
                }
            );

            // Act
            var result = InvokeParseExpression(tokens);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<TimesNode>(result);
            
            var timesNode = (TimesNode)result;
            var leftExpr = timesNode.Left;
            var rightExpr = timesNode.Right;
            
            Assert.IsType<PlusNode>(leftExpr);
            Assert.IsType<LiteralNode>(rightExpr);
            
            var plusNode = (PlusNode)leftExpr;
            var rightLiteral = (LiteralNode)rightExpr;
            
            var plusLeftExpr = plusNode.Left;
            var plusRightExpr = plusNode.Right;
            
            Assert.IsType<LiteralNode>(plusLeftExpr);
            Assert.IsType<LiteralNode>(plusRightExpr);
            
            var plusLeftLiteral = (LiteralNode)plusLeftExpr;
            var plusRightLiteral = (LiteralNode)plusRightExpr;
            
            Assert.Equal(5, plusLeftLiteral.Value);
            Assert.Equal(3, plusRightLiteral.Value);
            Assert.Equal(2, rightLiteral.Value);
        }

        [Fact]
        public void ParseExpression_NestedRightSide_ReturnsCorrectTree()
        {
            // Arrange - create expression: (2 * (5 + 3))
            var tokens = CreateTokens(
                new[] { "(", "2", "*", "(", "5", "+", "3", ")", ")" },
                new[] { 
                    TokenType.LEFT_PAREN, 
                    TokenType.INTEGER, 
                    TokenType.OPERATOR, 
                    TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.OPERATOR, TokenType.INTEGER, TokenType.RIGHT_PAREN, 
                    TokenType.RIGHT_PAREN 
                }
            );

            // Act
            var result = InvokeParseExpression(tokens);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<TimesNode>(result);
            
            var timesNode = (TimesNode)result;
            var leftExpr = timesNode.Left;
            var rightExpr = timesNode.Right;
            
            Assert.IsType<LiteralNode>(leftExpr);
            Assert.IsType<PlusNode>(rightExpr);
            
            var leftLiteral = (LiteralNode)leftExpr;
            var plusNode = (PlusNode)rightExpr;
            
            Assert.Equal(2, leftLiteral.Value);
            
            var plusLeftExpr = plusNode.Left;
            var plusRightExpr = plusNode.Right;
            
            Assert.IsType<LiteralNode>(plusLeftExpr);
            Assert.IsType<LiteralNode>(plusRightExpr);
            
            var plusLeftLiteral = (LiteralNode)plusLeftExpr;
            var plusRightLiteral = (LiteralNode)plusRightExpr;
            
            Assert.Equal(5, plusLeftLiteral.Value);
            Assert.Equal(3, plusRightLiteral.Value);
        }

        [Fact]
        public void ParseExpression_DeeplyNested_ReturnsCorrectTree()
        {
            // Arrange - create expression: ((2 + 3) * (7 - 4))
            var tokens = CreateTokens(
                new[] { "(", "(", "2", "+", "3", ")", "*", "(", "7", "-", "4", ")", ")" },
                new[] { 
                    TokenType.LEFT_PAREN, 
                    TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.OPERATOR, TokenType.INTEGER, TokenType.RIGHT_PAREN,
                    TokenType.OPERATOR,
                    TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.OPERATOR, TokenType.INTEGER, TokenType.RIGHT_PAREN,
                    TokenType.RIGHT_PAREN 
                }
            );

            // Act
            var result = InvokeParseExpression(tokens);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<TimesNode>(result);
            
            var timesNode = (TimesNode)result;
            var leftExpr = timesNode.Left;
            var rightExpr = timesNode.Right;
            
            Assert.IsType<PlusNode>(leftExpr);
            Assert.IsType<MinusNode>(rightExpr);
            
            var plusNode = (PlusNode)leftExpr;
            var minusNode = (MinusNode)rightExpr;
            
            var plusLeftExpr = plusNode.Left;
            var plusRightExpr = plusNode.Right;
            var minusLeftExpr = minusNode.Left;
            var minusRightExpr = minusNode.Right;
            
            Assert.IsType<LiteralNode>(plusLeftExpr);
            Assert.IsType<LiteralNode>(plusRightExpr);
            Assert.IsType<LiteralNode>(minusLeftExpr);
            Assert.IsType<LiteralNode>(minusRightExpr);
            
            var plusLeftLiteral = (LiteralNode)plusLeftExpr;
            var plusRightLiteral = (LiteralNode)plusRightExpr;
            var minusLeftLiteral = (LiteralNode)minusLeftExpr;
            var minusRightLiteral = (LiteralNode)minusRightExpr;
            
            Assert.Equal(2, plusLeftLiteral.Value);
            Assert.Equal(3, plusRightLiteral.Value);
            Assert.Equal(7, minusLeftLiteral.Value);
            Assert.Equal(4, minusRightLiteral.Value);
        }

        [Fact]
        public void ParseExpression_MixedTypes_ReturnsCorrectTree()
        {
            // Arrange - create expression: (x * (5.5 + y))
            var tokens = CreateTokens(
                new[] { "(", "x", "*", "(", "5.5", "+", "y", ")", ")" },
                new[] { 
                    TokenType.LEFT_PAREN, 
                    TokenType.VARIABLE, 
                    TokenType.OPERATOR, 
                    TokenType.LEFT_PAREN, TokenType.FLOAT, TokenType.OPERATOR, TokenType.VARIABLE, TokenType.RIGHT_PAREN, 
                    TokenType.RIGHT_PAREN 
                }
            );

            // Act
            var result = InvokeParseExpression(tokens);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<TimesNode>(result);
            
            var timesNode = (TimesNode)result;
            var leftExpr = timesNode.Left;
            var rightExpr = timesNode.Right;
            
            Assert.IsType<VariableNode>(leftExpr);
            Assert.IsType<PlusNode>(rightExpr);
            
            var leftVar = (VariableNode)leftExpr;
            var plusNode = (PlusNode)rightExpr;
            
            Assert.Equal("x", leftVar.Name);
            
            var plusLeftExpr = plusNode.Left;
            var plusRightExpr = plusNode.Right;
            
            Assert.IsType<LiteralNode>(plusLeftExpr);
            Assert.IsType<VariableNode>(plusRightExpr);
            
            var plusLeftLiteral = (LiteralNode)plusLeftExpr;
            var plusRightVar = (VariableNode)plusRightExpr;
            
            Assert.Equal(5.5, plusLeftLiteral.Value);
            Assert.Equal("y", plusRightVar.Name);
        }

        [Fact]
        public void ParseExpression_MissingLeftParen_ThrowsParseException()
        {
            // Arrange
            var tokens = CreateTokens(
                new[] { "5", "+", "3", ")" },
                new[] { TokenType.INTEGER, TokenType.OPERATOR, TokenType.INTEGER, TokenType.RIGHT_PAREN }
            );

            // Act & Assert
            var exception = Assert.Throws<TargetInvocationException>(() => InvokeParseExpression(tokens));
            Assert.IsType<ParseException>(exception.InnerException);
            Assert.Contains("must begin with a (", exception.InnerException.Message);
        }

        [Fact]
        public void ParseExpression_MissingRightParen_ThrowsParseException()
        {
            // Arrange
            var tokens = CreateTokens(
                new[] { "(", "5", "+", "3" },
                new[] { TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.OPERATOR, TokenType.INTEGER }
            );

            // Act & Assert
            var exception = Assert.Throws<TargetInvocationException>(() => InvokeParseExpression(tokens));
            Assert.IsType<ParseException>(exception.InnerException);
            // The error message will depend on your implementation
        }

        [Fact]
        public void ParseExpression_InvalidOperator_ThrowsParseException()
        {
            // Arrange
            var tokens = CreateTokens(
                new[] { "(", "5", "$", "3", ")" },
                new[] { TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.UNKNOWN, TokenType.INTEGER, TokenType.RIGHT_PAREN }
            );

            // Act & Assert
            var exception = Assert.Throws<TargetInvocationException>(() => InvokeParseExpression(tokens));
            Assert.IsType<ParseException>(exception.InnerException);
            Assert.Contains("Invalid operator", exception.InnerException.Message);
        }

        [Fact]
        public void ParseExpression_MissingInternalClosingParen_ThrowsParseException()
        {
            // Arrange - create expression: ((2 + 3 * (7 - 4))
            // Missing closing parenthesis after "3"
            var tokens = CreateTokens(
                new[] { "(", "(", "2", "+", "3", "*", "(", "7", "-", "4", ")", ")" },
                new[] { 
                    TokenType.LEFT_PAREN, 
                    TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.OPERATOR, TokenType.INTEGER,
                    TokenType.OPERATOR,
                    TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.OPERATOR, TokenType.INTEGER, TokenType.RIGHT_PAREN,
                    TokenType.RIGHT_PAREN 
                }
            );

            // Act & Assert
            var exception = Assert.Throws<TargetInvocationException>(() => InvokeParseExpression(tokens));
            Assert.IsType<ParseException>(exception.InnerException);
            // The error message will depend on the implementation, but we should get some kind of parse exception
        }

        [Fact]
        public void ParseExpression_MissingInternalOpeningParen_ThrowsParseException()
        {
            // Arrange - create expression: ((2 + 3) * 7 - 4))
            // Missing opening parenthesis before "7"
            var tokens = CreateTokens(
                new[] { "(", "(", "2", "+", "3", ")", "*", "7", "-", "4", ")", ")" },
                new[] { 
                    TokenType.LEFT_PAREN, 
                    TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.OPERATOR, TokenType.INTEGER, TokenType.RIGHT_PAREN,
                    TokenType.OPERATOR,
                    TokenType.INTEGER, TokenType.OPERATOR, TokenType.INTEGER, TokenType.RIGHT_PAREN,
                    TokenType.RIGHT_PAREN 
                }
            );

            // Act & Assert
            var exception = Assert.Throws<TargetInvocationException>(() => InvokeParseExpression(tokens));
            Assert.IsType<ParseException>(exception.InnerException);
            // The error message will depend on the implementation
        }

        [Fact]
        public void ParseExpression_MissingBothParens_ThrowsParseException()
        {
            // Arrange - create expression: (2 + 3 * 7 - 4)
            // Missing both parentheses for nested expressions
            var tokens = CreateTokens(
                new[] { "(", "2", "+", "3", "*", "7", "-", "4", ")" },
                new[] { 
                    TokenType.LEFT_PAREN, 
                    TokenType.INTEGER, TokenType.OPERATOR, TokenType.INTEGER, 
                    TokenType.OPERATOR,
                    TokenType.INTEGER, TokenType.OPERATOR, TokenType.INTEGER,
                    TokenType.RIGHT_PAREN 
                }
            );

            // Act & Assert
            var exception = Assert.Throws<TargetInvocationException>(() => InvokeParseExpression(tokens));
            Assert.IsType<ParseException>(exception.InnerException);
            // The error message will depend on the implementation
        }

        [Fact]
        public void ParseExpression_NestedMissingClosingParen_ThrowsParseException()
        {
            // Arrange - create expression: (x * (5.5 + y)
            // Missing the outermost closing parenthesis
            var tokens = CreateTokens(
                new[] { "(", "x", "*", "(", "5.5", "+", "y", ")" },
                new[] { 
                    TokenType.LEFT_PAREN, 
                    TokenType.VARIABLE, 
                    TokenType.OPERATOR, 
                    TokenType.LEFT_PAREN, TokenType.FLOAT, TokenType.OPERATOR, TokenType.VARIABLE, TokenType.RIGHT_PAREN
                }
            );

            // Act & Assert
            var exception = Assert.Throws<TargetInvocationException>(() => InvokeParseExpression(tokens));
            Assert.IsType<ParseException>(exception.InnerException);
            Assert.Contains("Missing )", exception.InnerException.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ParseExpression_NestedMissingOpeningParen_ThrowsParseException()
        {
            // Arrange - create expression: (x * 5.5 + y))
            // Missing the inner opening parenthesis
            var tokens = CreateTokens(
                new[] { "(", "x", "*", "5.5", "+", "y", ")", ")" },
                new[] { 
                    TokenType.LEFT_PAREN, 
                    TokenType.VARIABLE, 
                    TokenType.OPERATOR, 
                    TokenType.FLOAT, TokenType.OPERATOR, TokenType.VARIABLE, TokenType.RIGHT_PAREN,
                    TokenType.RIGHT_PAREN
                }
            );

            // Act & Assert
            var exception = Assert.Throws<TargetInvocationException>(() => InvokeParseExpression(tokens));
            Assert.IsType<ParseException>(exception.InnerException);
            // The error message will depend on the implementation
        }

        [Fact]
        public void ParseExpression_DeeplyNestedBalancedParens_ReturnsCorrectTree()
        {
            // Arrange - create expression: ((((2 + 3) * 4) - 5))
            // This is a valid expression with multiple levels of nesting
            var tokens = CreateTokens(
                new[] { "(", "(", "(", "(", "2", "+", "3", ")", "*", "4", ")", "-", "5", ")", ")" },
                new[] { 
                    TokenType.LEFT_PAREN, TokenType.LEFT_PAREN, TokenType.LEFT_PAREN, TokenType.LEFT_PAREN,
                    TokenType.INTEGER, TokenType.OPERATOR, TokenType.INTEGER, TokenType.RIGHT_PAREN,
                    TokenType.OPERATOR, TokenType.INTEGER, TokenType.RIGHT_PAREN,
                    TokenType.OPERATOR, TokenType.INTEGER, TokenType.RIGHT_PAREN,
                    TokenType.RIGHT_PAREN
                }
            );

            // Act
            var result = InvokeParseExpression(tokens);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<MinusNode>(result);
            
            var minusNode = (MinusNode)result;
            var leftExpr = minusNode.Left;
            var rightExpr = minusNode.Right;
            
            Assert.IsType<TimesNode>(leftExpr);
            Assert.IsType<LiteralNode>(rightExpr);
            
            var timesNode = (TimesNode)leftExpr;
            var rightLiteral = (LiteralNode)rightExpr;
            
            var timesLeftExpr = timesNode.Left;
            var timesRightExpr = timesNode.Right;
            
            Assert.IsType<PlusNode>(timesLeftExpr);
            Assert.IsType<LiteralNode>(timesRightExpr);
            
            var plusNode = (PlusNode)timesLeftExpr;
            var timesRightLiteral = (LiteralNode)timesRightExpr;
            
            var plusLeftExpr = plusNode.Left;
            var plusRightExpr = plusNode.Right;
            
            Assert.IsType<LiteralNode>(plusLeftExpr);
            Assert.IsType<LiteralNode>(plusRightExpr);
            
            var plusLeftLiteral = (LiteralNode)plusLeftExpr;
            var plusRightLiteral = (LiteralNode)plusRightExpr;
            
            Assert.Equal(2, plusLeftLiteral.Value);
            Assert.Equal(3, plusRightLiteral.Value);
            Assert.Equal(4, timesRightLiteral.Value);
            Assert.Equal(5, rightLiteral.Value);
        }

        [Fact]
        public void ParseExpression_MismatchedParenTypes_ThrowsParseException()
        {
            // Arrange - create an expression with a block ending delimiter '}' instead of a closing parenthesis ')'
            // This tests handling of parenthesis type mismatches
            var tokens = CreateTokens(
                new[] { "(", "2", "+", "3", "}" },
                new[] { 
                    TokenType.LEFT_PAREN, 
                    TokenType.INTEGER, TokenType.OPERATOR, TokenType.INTEGER, 
                    TokenType.RIGHT_BRACE 
                }
            );

            // Act & Assert
            var exception = Assert.Throws<TargetInvocationException>(() => InvokeParseExpression(tokens));
            Assert.IsType<ParseException>(exception.InnerException);
            Assert.Contains("must end with a )", exception.InnerException.Message, StringComparison.OrdinalIgnoreCase);
        }

        #region Single tokenValues

        [Fact]
        public void ParseExpression_SingleIntegerLiteral_ReturnsLiteralNode()
        {
            // Arrange - create expression: (42)
            var tokens = CreateTokens(
                new[] { "(", "42", ")" },
                new[] { TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.RIGHT_PAREN }
            );

            // Act
            var result = InvokeParseExpression(tokens);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<LiteralNode>(result);
            
            var literalNode = (LiteralNode)result;
            Assert.Equal(42, literalNode.Value);
        }
        
        [Fact]
        public void ParseExpression_SingleFloatLiteral_ReturnsLiteralNode()
        {
            // Arrange - create expression: (3.14)
            var tokens = CreateTokens(
                new[] { "(", "3.14", ")" },
                new[] { TokenType.LEFT_PAREN, TokenType.FLOAT, TokenType.RIGHT_PAREN }
            );

            // Act
            var result = InvokeParseExpression(tokens);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<LiteralNode>(result);
            
            var literalNode = (LiteralNode)result;
            Assert.Equal(3.14, literalNode.Value);
        }
        
        [Fact]
        public void ParseExpression_SingleVariable_ReturnsVariableNode()
        {
            // Arrange - create expression: (x)
            var tokens = CreateTokens(
                new[] { "(", "x", ")" },
                new[] { TokenType.LEFT_PAREN, TokenType.VARIABLE, TokenType.RIGHT_PAREN }
            );

            // Act
            var result = InvokeParseExpression(tokens);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<VariableNode>(result);
            
            var variableNode = (VariableNode)result;
            Assert.Equal("x", variableNode.Name);
        }
        
        [Fact]
        public void ParseExpression_NestedSingleValue_ReturnsCorrectNode()
        {
            // Arrange - create expression: ((42))
            var tokens = CreateTokens(
                new[] { "(", "(", "42", ")", ")" },
                new[] { TokenType.LEFT_PAREN, TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.RIGHT_PAREN, TokenType.RIGHT_PAREN }
            );

            // Act
            var result = InvokeParseExpression(tokens);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<LiteralNode>(result);
            
            var literalNode = (LiteralNode)result;
            Assert.Equal(42, literalNode.Value);
        }
        
        [Fact]
        public void ParseExpression_DeeplyNestedSingleValue_ReturnsCorrectNode()
        {
            // Arrange - create expression: ((((x))))
            var tokens = CreateTokens(
                new[] { "(", "(", "(", "(", "x", ")", ")", ")", ")" },
                new[] { 
                    TokenType.LEFT_PAREN, TokenType.LEFT_PAREN, TokenType.LEFT_PAREN, TokenType.LEFT_PAREN, 
                    TokenType.VARIABLE, 
                    TokenType.RIGHT_PAREN, TokenType.RIGHT_PAREN, TokenType.RIGHT_PAREN, TokenType.RIGHT_PAREN 
                }
            );

            // Act
            var result = InvokeParseExpression(tokens);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<VariableNode>(result);
            
            var variableNode = (VariableNode)result;
            Assert.Equal("x", variableNode.Name);
        }
        
        [Fact]
        public void ParseExpression_EmptyParentheses_ThrowsParseException()
        {
            // Arrange - create expression: ()
            var tokens = CreateTokens(
                new[] { "(", ")" },
                new[] { TokenType.LEFT_PAREN, TokenType.RIGHT_PAREN }
            );

            // Act & Assert
            var exception = Assert.Throws<TargetInvocationException>(() => InvokeParseExpression(tokens));
            Assert.IsType<ParseException>(exception.InnerException);
            // The error message will depend on the implementation
        }
        
        [Fact]
        public void ParseExpression_ZeroValue_ReturnsLiteralNode()
        {
            // Arrange - create expression: (0)
            var tokens = CreateTokens(
                new[] { "(", "0", ")" },
                new[] { TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.RIGHT_PAREN }
            );

            // Act
            var result = InvokeParseExpression(tokens);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<LiteralNode>(result);
            
            var literalNode = (LiteralNode)result;
            Assert.Equal(0, literalNode.Value);
        }
        
        [Fact]
        public void ParseExpression_NegativeValue_ReturnsCorrectTree()
        {
            // Arrange - create expression: (0 - 5)
            // Note: This is how negative values would be expressed in this language
            var tokens = CreateTokens(
                new[] { "(", "0", "-", "5", ")" },
                new[] { TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.OPERATOR, TokenType.INTEGER, TokenType.RIGHT_PAREN }
            );

            // Act
            var result = InvokeParseExpression(tokens);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<MinusNode>(result);
            
            // Additional verification could be done here to check the left and right children
        }
        
        [Fact]
        public void ParseExpression_SingleValueFollowedByJunk_ThrowsParseException()
        {
            // Arrange - create expression: (42 junk)
            var tokens = CreateTokens(
                new[] { "(", "42", "junk", ")" },
                new[] { TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.VARIABLE, TokenType.RIGHT_PAREN }
            );

            // Act & Assert
            var exception = Assert.Throws<TargetInvocationException>(() => InvokeParseExpression(tokens));
            Assert.IsType<ParseException>(exception.InnerException);
            // The error message will depend on the implementation
        }
        
        [Fact]
        public void ParseExpression_MissingValueBetweenParens_ThrowsParseException()
        {
            // Arrange - create expression: (( ))
            var tokens = CreateTokens(
                new[] { "(", "(", ")", ")" },
                new[] { TokenType.LEFT_PAREN, TokenType.LEFT_PAREN, TokenType.RIGHT_PAREN, TokenType.RIGHT_PAREN }
            );

            // Act & Assert
            var exception = Assert.Throws<TargetInvocationException>(() => InvokeParseExpression(tokens));
            Assert.IsType<ParseException>(exception.InnerException);
            // The error message will depend on the implementation
        }

        #endregion
    }
}