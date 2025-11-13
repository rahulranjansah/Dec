using System;
using System.Collections.Generic;
using Xunit;
using AST;
using Tokenizer;
using Containers;
using System.Reflection;

namespace Parser.Tests
{
    public class StatementParsingTests
    {
        // Helper method to create a token list for testing
        private List<Token> CreateTokens(string[] tokenValues, TokenType[] tokenTypes)
        {
            var tokens = new List<Token>();
            for (int i = 0; i < tokenValues.Length; i++)
            {
                tokens.Add(new Token(tokenValues[i], tokenTypes[i]));
            }
            return tokens;
        }

        // Helper method to invoke the private ParseReturnStatement method using reflection
        private ReturnStmt InvokeParseReturnStatement(List<Token> tokens)
        {
            Type parserType = typeof(Parser);
            MethodInfo methodInfo = parserType.GetMethod("ParseReturnStatement",
                BindingFlags.NonPublic | BindingFlags.Static);

            return (ReturnStmt)methodInfo.Invoke(null, new object[] { tokens });
        }

        // Helper method to invoke the private ParseAssignmentStmt method using reflection
        private AssignmentStmt InvokeParseAssignmentStmt(List<Token> tokens, SymbolTable<string, object> symbolTable)
        {
            Type parserType = typeof(Parser);
            MethodInfo methodInfo = parserType.GetMethod("ParseAssignmentStmt",
                BindingFlags.NonPublic | BindingFlags.Static);

            return (AssignmentStmt)methodInfo.Invoke(null, new object[] { tokens, symbolTable });
        }

        #region ReturnStatement Tests

        [Fact]
        public void ParseReturnStatement_SimpleIntegerLiteral_ReturnsCorrectNode()
        {
            // Arrange - Note: All expressions must be surrounded by parentheses
            var tokens = CreateTokens(
                new[] { "return", "(", "42", ")" },
                new[] { TokenType.RETURN, TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.RIGHT_PAREN }
            );

            // Act
            var result = InvokeParseReturnStatement(tokens);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Expression);
            Assert.IsType<LiteralNode>(result.Expression);

            var literalNode = (LiteralNode)result.Expression;
            Assert.Equal(42, literalNode.Value);
        }

        [Fact]
        public void ParseReturnStatement_SimpleFloatLiteral_ReturnsCorrectNode()
        {
            // Arrange - Note: All expressions must be surrounded by parentheses
            var tokens = CreateTokens(
                new[] { "return", "(", "3.14", ")" },
                new[] { TokenType.RETURN, TokenType.LEFT_PAREN, TokenType.FLOAT, TokenType.RIGHT_PAREN }
            );

            // Act
            var result = InvokeParseReturnStatement(tokens);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Expression);
            Assert.IsType<LiteralNode>(result.Expression);

            var literalNode = (LiteralNode)result.Expression;
            // Assert.Equal(3.14, literalNode.Value, 4);
            // Assert.Equal(Math.Round(3.14, 4), Math.Round((double)literalNode.Value, 4));
            Assert.Equal(3.14, (double)literalNode.Value, precision: 3);



        }

        [Fact]
        public void ParseReturnStatement_SimpleVariable_ReturnsCorrectNode()
        {
            // Arrange - Note: All expressions must be surrounded by parentheses
            var tokens = CreateTokens(
                new[] { "return", "(", "x", ")" },
                new[] { TokenType.RETURN, TokenType.LEFT_PAREN, TokenType.VARIABLE, TokenType.RIGHT_PAREN }
            );

            // Act
            var result = InvokeParseReturnStatement(tokens);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Expression);
            Assert.IsType<VariableNode>(result.Expression);

            var variableNode = (VariableNode)result.Expression;
            Assert.Equal("x", variableNode.Name);
        }

        [Fact]
        public void ParseReturnStatement_BinaryOperation_ReturnsCorrectNode()
        {
            // Arrange - Note: Binary operations must be in the format (x + y)
            var tokens = CreateTokens(
                new[] { "return", "(", "x", "+", "42", ")" },
                new[] { TokenType.RETURN, TokenType.LEFT_PAREN, TokenType.VARIABLE, TokenType.OPERATOR, TokenType.INTEGER, TokenType.RIGHT_PAREN }
            );

            // Act
            var result = InvokeParseReturnStatement(tokens);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Expression);
            Assert.IsType<PlusNode>(result.Expression);
        }

        [Fact]
        public void ParseReturnStatement_NestedExpression_ReturnsCorrectNode()
        {
            // Arrange - Note: Nested operations format: (x + (y * z))
            var tokens = CreateTokens(
                new[] { "return", "(", "x", "+", "(", "y", "*", "z", ")", ")" },
                new[] {
                    TokenType.RETURN,
                    TokenType.LEFT_PAREN, TokenType.VARIABLE, TokenType.OPERATOR,
                    TokenType.LEFT_PAREN, TokenType.VARIABLE, TokenType.OPERATOR, TokenType.VARIABLE, TokenType.RIGHT_PAREN,
                    TokenType.RIGHT_PAREN
                }
            );

            // Act
            var result = InvokeParseReturnStatement(tokens);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Expression);
            Assert.IsType<PlusNode>(result.Expression);
        }

        [Fact]
        public void ParseReturnStatement_MissingExpression_ThrowsParseException()
        {
            // Arrange
            var tokens = CreateTokens(
                new[] { "return" },
                new[] { TokenType.RETURN }
            );

            // Act & Assert
            var exception = Assert.Throws<TargetInvocationException>(() => InvokeParseReturnStatement(tokens));
            Assert.IsType<ParseException>(exception.InnerException);
            Assert.Contains("missing expression", exception.InnerException.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ParseReturnStatement_MissingOpeningParenthesis_ThrowsParseException()
        {
            // Arrange - Missing opening parenthesis
            var tokens = CreateTokens(
                new[] { "return", "42", ")" },
                new[] { TokenType.RETURN, TokenType.INTEGER, TokenType.RIGHT_PAREN }
            );

            // Act & Assert
            var exception = Assert.Throws<TargetInvocationException>(() => InvokeParseReturnStatement(tokens));
            Assert.IsType<ParseException>(exception.InnerException);
        }

        [Fact]
        public void ParseReturnStatement_MissingClosingParenthesis_ThrowsParseException()
        {
            // Arrange - Missing closing parenthesis
            var tokens = CreateTokens(
                new[] { "return", "(", "42" },
                new[] { TokenType.RETURN, TokenType.LEFT_PAREN, TokenType.INTEGER }
            );

            // Act & Assert
            var exception = Assert.Throws<TargetInvocationException>(() => InvokeParseReturnStatement(tokens));
            Assert.IsType<ParseException>(exception.InnerException);
        }

        #endregion

        #region AssignmentStatement Tests

        [Fact]
        public void ParseAssignmentStmt_SimpleIntegerLiteral_ReturnsCorrectNode()
        {
            // Arrange - Note: All expressions must be surrounded by parentheses
            var tokens = CreateTokens(
                new[] { "x", ":=", "(", "42", ")" },
                new[] { TokenType.VARIABLE, TokenType.ASSIGNMENT, TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.RIGHT_PAREN }
            );
            var symbolTable = new SymbolTable<string, object>();

            // Act
            var result = InvokeParseAssignmentStmt(tokens, symbolTable);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Variable);
            Assert.NotNull(result.Expression);

            Assert.IsType<VariableNode>(result.Variable);
            Assert.IsType<LiteralNode>(result.Expression);

            var variableNode = (VariableNode)result.Variable;
            var literalNode = (LiteralNode)result.Expression;

            Assert.Equal("x", variableNode.Name);
            Assert.Equal(42, literalNode.Value);

            // Verify that variable was added to symbol table
            Assert.True(symbolTable.ContainsKey("x"));
        }

        [Fact]
        public void ParseAssignmentStmt_SimpleVariableExpression_ReturnsCorrectNode()
        {
            // Arrange - Note: Even a single variable must be surrounded by parentheses
            var tokens = CreateTokens(
                new[] { "result", ":=", "(", "value", ")" },
                new[] { TokenType.VARIABLE, TokenType.ASSIGNMENT, TokenType.LEFT_PAREN, TokenType.VARIABLE, TokenType.RIGHT_PAREN }
            );
            var symbolTable = new SymbolTable<string, object>();

            // Act
            var result = InvokeParseAssignmentStmt(tokens, symbolTable);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Variable);
            Assert.NotNull(result.Expression);

            Assert.IsType<VariableNode>(result.Variable);
            Assert.IsType<VariableNode>(result.Expression);

            var variableNode = (VariableNode)result.Variable;
            var expressionNode = (VariableNode)result.Expression;

            Assert.Equal("result", variableNode.Name);
            Assert.Equal("value", expressionNode.Name);

            // Verify that variable was added to symbol table
            Assert.True(symbolTable.ContainsKey("result"));
        }

        [Fact]
        public void ParseAssignmentStmt_BinaryOperation_ReturnsCorrectNode()
        {
            // Arrange - Note: Binary operations must be in the format (a + b)
            var tokens = CreateTokens(
                new[] { "result", ":=", "(", "a", "+", "b", ")" },
                new[] { TokenType.VARIABLE, TokenType.ASSIGNMENT, TokenType.LEFT_PAREN, TokenType.VARIABLE, TokenType.OPERATOR, TokenType.VARIABLE, TokenType.RIGHT_PAREN }
            );
            var symbolTable = new SymbolTable<string, object>();

            // Act
            var result = InvokeParseAssignmentStmt(tokens, symbolTable);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Variable);
            Assert.NotNull(result.Expression);

            Assert.IsType<VariableNode>(result.Variable);
            Assert.IsType<PlusNode>(result.Expression);

            var variableNode = (VariableNode)result.Variable;
            Assert.Equal("result", variableNode.Name);

            // Verify that variable was added to symbol table
            Assert.True(symbolTable.ContainsKey("result"));
        }

        [Fact]
        public void ParseAssignmentStmt_NestedExpression_ReturnsCorrectNode()
        {
            // Arrange - Note: Nested operations format: (a + (b * c))
            var tokens = CreateTokens(
                new[] { "result", ":=", "(", "a", "+", "(", "b", "*", "c", ")", ")" },
                new[] {
                    TokenType.VARIABLE, TokenType.ASSIGNMENT,
                    TokenType.LEFT_PAREN, TokenType.VARIABLE, TokenType.OPERATOR,
                    TokenType.LEFT_PAREN, TokenType.VARIABLE, TokenType.OPERATOR, TokenType.VARIABLE, TokenType.RIGHT_PAREN,
                    TokenType.RIGHT_PAREN
                }
            );
            var symbolTable = new SymbolTable<string, object>();

            // Act
            var result = InvokeParseAssignmentStmt(tokens, symbolTable);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<VariableNode>(result.Variable);
            Assert.IsType<PlusNode>(result.Expression);

            // Verify that variable was added to symbol table
            Assert.True(symbolTable.ContainsKey("result"));
        }

        [Fact]
        public void ParseAssignmentStmt_InvalidVariableName_ThrowsParseException()
        {
            // Arrange - Using a number as a variable name
            var tokens = CreateTokens(
                new[] { "123", ":=", "(", "42", ")" },
                new[] { TokenType.INTEGER, TokenType.ASSIGNMENT, TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.RIGHT_PAREN }
            );
            var symbolTable = new SymbolTable<string, object>();

            // Act & Assert
            var exception = Assert.Throws<TargetInvocationException>(() => InvokeParseAssignmentStmt(tokens, symbolTable));
            Assert.IsType<ParseException>(exception.InnerException);
            Assert.Contains("Invalid variable name", exception.InnerException.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ParseAssignmentStmt_MissingAssignmentOperator_ThrowsParseException()
        {
            // Arrange - Using = instead of :=
            var tokens = CreateTokens(
                new[] { "x", "=", "(", "42", ")" },
                new[] { TokenType.VARIABLE, TokenType.UNKNOWN, TokenType.LEFT_PAREN, TokenType.INTEGER, TokenType.RIGHT_PAREN }
            );
            var symbolTable = new SymbolTable<string, object>();

            // Act & Assert
            var exception = Assert.Throws<TargetInvocationException>(() => InvokeParseAssignmentStmt(tokens, symbolTable));
            Assert.IsType<ParseException>(exception.InnerException);
            Assert.Contains("Expected assignment operator", exception.InnerException.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ParseAssignmentStmt_MissingOpeningParenthesis_ThrowsParseException()
        {
            // Arrange - Missing opening parenthesis
            var tokens = CreateTokens(
                new[] { "x", ":=", "42", ")" },
                new[] { TokenType.VARIABLE, TokenType.ASSIGNMENT, TokenType.INTEGER, TokenType.RIGHT_PAREN }
            );
            var symbolTable = new SymbolTable<string, object>();

            // Act & Assert
            var exception = Assert.Throws<TargetInvocationException>(() => InvokeParseAssignmentStmt(tokens, symbolTable));
            Assert.IsType<ParseException>(exception.InnerException);
        }

        [Fact]
        public void ParseAssignmentStmt_MissingClosingParenthesis_ThrowsParseException()
        {
            // Arrange - Missing closing parenthesis
            var tokens = CreateTokens(
                new[] { "x", ":=", "(", "42" },
                new[] { TokenType.VARIABLE, TokenType.ASSIGNMENT, TokenType.LEFT_PAREN, TokenType.INTEGER }
            );
            var symbolTable = new SymbolTable<string, object>();

            // Act & Assert
            var exception = Assert.Throws<TargetInvocationException>(() => InvokeParseAssignmentStmt(tokens, symbolTable));
            Assert.IsType<ParseException>(exception.InnerException);
        }

        [Fact]
        public void ParseAssignmentStmt_MissingExpression_ThrowsParseException()
        {
            // Arrange - Missing right-hand side expression
            var tokens = CreateTokens(
                new[] { "x", ":=" },
                new[] { TokenType.VARIABLE, TokenType.ASSIGNMENT }
            );
            var symbolTable = new SymbolTable<string, object>();

            // Act & Assert
            var exception = Assert.Throws<TargetInvocationException>(() => InvokeParseAssignmentStmt(tokens, symbolTable));
            Assert.IsType<ParseException>(exception.InnerException);
        }

        [Fact]
        public void ParseAssignmentStmt_NestedScopes_VariablesFoundCorrectly()
        {
            // Arrange
            var globalScope = new SymbolTable<string, object>();
            globalScope.Add("global", "global_value");

            var functionScope = new SymbolTable<string, object>(globalScope);
            functionScope.Add("function", "function_value");

            var blockScope = new SymbolTable<string, object>(functionScope);

            var tokensUsingGlobalVar = CreateTokens(
                new[] { "result", ":=", "(", "global", "+", "5", ")" },
                new[] { TokenType.VARIABLE, TokenType.ASSIGNMENT, TokenType.LEFT_PAREN, TokenType.VARIABLE, TokenType.OPERATOR, TokenType.INTEGER, TokenType.RIGHT_PAREN }
            );

            // Act
            var result = InvokeParseAssignmentStmt(tokensUsingGlobalVar, blockScope);

            // Assert
            Assert.NotNull(result);
            Assert.True(blockScope.ContainsKey("result"));

            // Verify we can look up variables from any parent scope
            Assert.True(blockScope.TryGetValue("global", out _));
            Assert.True(blockScope.TryGetValue("function", out _));

            // But local lookups won't find parent variables
            Assert.False(blockScope.TryGetValueLocal("global", out _));
            Assert.False(blockScope.TryGetValueLocal("function", out _));
        }

        [Fact]
        public void ParseAssignmentStmt_ComplexExpression_AllOperators_WorksCorrectly()
        {
            // Test all supported operators
            var operators = new[] { "+", "-", "*", "/", "//", "%", "**" };
            var operatorTypes = new[] {
                typeof(PlusNode), typeof(MinusNode), typeof(TimesNode),
                typeof(FloatDivNode), typeof(IntDivNode), typeof(ModulusNode),
                typeof(ExponentiationNode)
            };

            for (int i = 0; i < operators.Length; i++)
            {
                // Arrange
                var tokens = CreateTokens(
                    new[] { "result", ":=", "(", "a", operators[i], "b", ")" },
                    new[] { TokenType.VARIABLE, TokenType.ASSIGNMENT, TokenType.LEFT_PAREN, TokenType.VARIABLE, TokenType.OPERATOR, TokenType.VARIABLE, TokenType.RIGHT_PAREN }
                );
                var symbolTable = new SymbolTable<string, object>();

                // Act
                var result = InvokeParseAssignmentStmt(tokens, symbolTable);

                // Assert
                Assert.NotNull(result);
                Assert.IsType<VariableNode>(result.Variable);
                Assert.IsType(operatorTypes[i], result.Expression);
            }
        }

        #endregion
    }
}