using System;
using System.Collections.Generic;
using Xunit;
using AST;
using Tokenizer;
using System.Reflection;

namespace Parser.Tests
{
    public class ParseBlockStmtTests
    {
        // Helper method to invoke the private ParseBlockStmt method using reflection
        private BlockStmt InvokeParseBlockStmt(List<string> lines, SymbolTable<string, object> symbolTable)
        {
            Type parserType = typeof(Parser);
            MethodInfo methodInfo = parserType.GetMethod("ParseBlockStmt", 
                BindingFlags.NonPublic | BindingFlags.Static);
            
            return (BlockStmt)methodInfo.Invoke(null, new object[] { lines, symbolTable });
        }
        
        [Fact]
        public void ParseBlockStmt_EmptyBlock_CreatesNewSymbolTable()
        {
            // Arrange
            var lines = new List<string> { "{", "}" };
            var parentSymbolTable = new SymbolTable<string, object>();
            parentSymbolTable["x"] = 10;
            
            // Act
            var result = InvokeParseBlockStmt(lines, parentSymbolTable);
            
            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.SymbolTable);
            Assert.Same(parentSymbolTable, result.SymbolTable);
            Assert.Empty(result.Statements);
            Assert.Empty(lines); // Both lines should be consumed
        }
        
        [Fact]
        public void ParseBlockStmt_InvalidFirstLine_ThrowsParseException()
        {
            // Arrange - First line is not '{'
            var lines = new List<string> { "invalid", "}" };
            var symbolTable = new SymbolTable<string, object>();
            
            // Act & Assert
            var exception = Assert.Throws<TargetInvocationException>(() => 
                InvokeParseBlockStmt(lines, symbolTable));
            Assert.IsType<ParseException>(exception.InnerException);
            Assert.Contains("must begin with", exception.InnerException.Message, StringComparison.OrdinalIgnoreCase);
        }
        
        [Fact]
        public void ParseBlockStmt_ExtraTokensOnFirstLine_ThrowsParseException()
        {
            // Arrange - '{ extra tokens' instead of just '{'
            var lines = new List<string> { "{ extra", "}" };
            var symbolTable = new SymbolTable<string, object>();
            
            // Act & Assert
            var exception = Assert.Throws<TargetInvocationException>(() => 
                InvokeParseBlockStmt(lines, symbolTable));
            Assert.IsType<ParseException>(exception.InnerException);
            Assert.Contains("Expected 1 token", exception.InnerException.Message, StringComparison.OrdinalIgnoreCase);
        }
        
        [Fact]
        public void ParseBlockStmt_ExtraTokensOnLastLine_ThrowsParseException()
        {
            // Arrange - '} extra tokens' instead of just '}'
            var lines = new List<string> { "{", "} extra" };
            var symbolTable = new SymbolTable<string, object>();
            
            // Act & Assert
            var exception = Assert.Throws<TargetInvocationException>(() => 
                InvokeParseBlockStmt(lines, symbolTable));
            Assert.IsType<ParseException>(exception.InnerException);
        }
        
        [Fact]
        public void ParseBlockStmt_InvalidLastLine_ThrowsParseException()
        {
            // Arrange - Last line is not '}'
            var lines = new List<string> { "{", "invalid" };
            var symbolTable = new SymbolTable<string, object>();
            
            // Act & Assert
            var exception = Assert.Throws<TargetInvocationException>(() => 
                InvokeParseBlockStmt(lines, symbolTable));
            Assert.IsType<ParseException>(exception.InnerException);
            Assert.Contains("Invalid token", exception.InnerException.Message, StringComparison.OrdinalIgnoreCase);
        }
        
        [Fact]
        public void ParseBlockStmt_ConsumesAllLinesOnSuccess()
        {
            // Arrange
            var lines = new List<string> { "{", "x := (42)", "}" };
            var symbolTable = new SymbolTable<string, object>();
            
            // Act
            var result = InvokeParseBlockStmt(lines, symbolTable);
            
            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Statements);
            Assert.Empty(lines); // All lines should be consumed
        }
        
        [Fact]
        public void ParseBlockStmt_MinimalValidProgram_ParsesCorrectly()
        {
            // Arrange - Minimal valid program
            var lines = new List<string> { "{", "}" };
            var symbolTable = new SymbolTable<string, object>();
            
            // Act
            var result = InvokeParseBlockStmt(lines, symbolTable);
            
            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Statements);
            Assert.Empty(lines); // All lines should be consumed
            
            // Verify the Unparse output
            var unparsed = result.Unparse();
            Assert.Equal("{\n}", unparsed);
        }
    }
}