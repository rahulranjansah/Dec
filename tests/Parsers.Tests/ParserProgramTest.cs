using System;
using System.Collections.Generic;
using Xunit;
using AST;
using Tokenizer;
using System.Reflection;

namespace Parser.Tests
{
    public class ParseProgramTests
    {
        // Helper method to access protected Left property in BinaryOperator
        private ExpressionNode GetLeft(BinaryOperator op)
        {
            PropertyInfo propInfo = typeof(BinaryOperator).GetProperty("Left", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            return (ExpressionNode)propInfo.GetValue(op);
        }
        
        // Helper method to access protected Right property in BinaryOperator
        private ExpressionNode GetRight(BinaryOperator op)
        {
            PropertyInfo propInfo = typeof(BinaryOperator).GetProperty("Right", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            return (ExpressionNode)propInfo.GetValue(op);
        }
        
        [Fact]
        public void Parse_EmptyProgram_ReturnsEmptyBlockStmt()
        {
            // Arrange
            string program = "{\n}";
            
            // Act
            var result = Parser.Parse(program);
            
            // Assert
            Assert.NotNull(result);
            Assert.IsType<BlockStmt>(result);
            Assert.Empty(result.Statements);
            Assert.NotNull(result.SymbolTable);
            
            // Verify unparsing
            var unparsed = result.Unparse();
            Assert.Contains("{", unparsed);
            Assert.Contains("}", unparsed);
        }
        
        [Fact]
        public void Parse_SingleAssignment_ReturnsCorrectAST()
        {
            // Arrange
            string program = "{\n  x := (42)\n}";
            
            // Act
            var result = Parser.Parse(program);
            
            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Statements);
            Assert.IsType<AssignmentStmt>(result.Statements[0]);
            
            var assign = (AssignmentStmt)result.Statements[0];
            Assert.Equal("x", assign.Variable.Name);
            Assert.IsType<LiteralNode>(assign.Expression);
            Assert.Equal(42, ((LiteralNode)assign.Expression).Value);
            
            Assert.True(result.SymbolTable.ContainsKey("x"));
            
            // Verify unparsing
            var unparsed = result.Unparse();
            Assert.Contains("x := 42", unparsed);
        }
        
        [Fact]
        public void Parse_SingleReturn_ReturnsCorrectAST()
        {
            // Arrange
            string program = "{\n  return (42)\n}";
            
            // Act
            var result = Parser.Parse(program);
            
            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Statements);
            Assert.IsType<ReturnStmt>(result.Statements[0]);
            
            var returnStmt = (ReturnStmt)result.Statements[0];
            Assert.IsType<LiteralNode>(returnStmt.Expression);
            Assert.Equal(42, ((LiteralNode)returnStmt.Expression).Value);
            
            // Verify unparsing
            var unparsed = result.Unparse();
            Assert.Contains("return 42", unparsed);
        }
        
        [Fact]
        public void Parse_SimpleExpressions_ReturnsCorrectAST()
        {
            // Arrange
            string program = @"{
  a := (5)
  b := (10)
  c := (a + b)
  d := (c * 2)
  return (d)
}";
            
            // Act
            var result = Parser.Parse(program);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Statements.Count);
            
            // Check that all variables were added to the symbol table
            Assert.True(result.SymbolTable.ContainsKey("a"));
            Assert.True(result.SymbolTable.ContainsKey("b"));
            Assert.True(result.SymbolTable.ContainsKey("c"));
            Assert.True(result.SymbolTable.ContainsKey("d"));
            
            // Check the last assignment expression
            var dAssign = (AssignmentStmt)result.Statements[3];
            Assert.IsType<TimesNode>(dAssign.Expression);
            var timesNode = (TimesNode)dAssign.Expression;
            
            Assert.IsType<VariableNode>(timesNode.Left);
            Assert.IsType<LiteralNode>(timesNode.Right);
            
            var leftVar = (VariableNode)timesNode.Left;
            var rightLiteral = (LiteralNode)timesNode.Right;
            
            Assert.Equal("c", leftVar.Name);
            Assert.Equal(2, rightLiteral.Value);
            
            // Check the return statement
            var returnStmt = (ReturnStmt)result.Statements[4];
            Assert.IsType<VariableNode>(returnStmt.Expression);
            Assert.Equal("d", ((VariableNode)returnStmt.Expression).Name);
            
            // Verify unparsing
            var unparsed = result.Unparse();
            Assert.Contains("a := 5", unparsed);
            Assert.Contains("b := 10", unparsed);
            Assert.Contains("c := (a + b)", unparsed);
            Assert.Contains("d := (c * 2)", unparsed);
            Assert.Contains("return d", unparsed);
        }
        
        [Fact]
        public void Parse_NestedBlock_ReturnsCorrectAST()
        {
            // Arrange
            string program = @"{
  x := (10)
  {
    y := (x + 5)
    z := (y * 2)
  }
  return (x)
}";
            
            // Act
            var result = Parser.Parse(program);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Statements.Count);
            
            Assert.IsType<AssignmentStmt>(result.Statements[0]);
            Assert.IsType<BlockStmt>(result.Statements[1]);
            Assert.IsType<ReturnStmt>(result.Statements[2]);
            
            var nestedBlock = (BlockStmt)result.Statements[1];
            Assert.Equal(2, nestedBlock.Statements.Count);
            Assert.IsType<AssignmentStmt>(nestedBlock.Statements[0]);
            Assert.IsType<AssignmentStmt>(nestedBlock.Statements[1]);
            
            // Check variable scoping
            Assert.True(result.SymbolTable.ContainsKey("x"));
            Assert.False(result.SymbolTable.ContainsKey("y"));
            Assert.False(result.SymbolTable.ContainsKey("z"));
            
            Assert.True(nestedBlock.SymbolTable.ContainsKey("y"));
            Assert.True(nestedBlock.SymbolTable.ContainsKey("z"));
            Assert.True(nestedBlock.SymbolTable.ContainsKey("x")); // Can access parent variable
            
            // Verify unparsing
            var unparsed = result.Unparse();
            Assert.Contains("x := 10", unparsed);
            Assert.Contains("y := (x + 5)", unparsed);
            Assert.Contains("z := (y * 2)", unparsed);
            Assert.Contains("return x", unparsed);
            
            // Verify nested braces in the unparsed output
            int openBraceCount = unparsed.Count(c => c == '{');
            int closeBraceCount = unparsed.Count(c => c == '}');
            Assert.Equal(2, openBraceCount);
            Assert.Equal(2, closeBraceCount);
        }
        
        [Fact]
        public void Parse_AllOperators_ReturnsCorrectAST()
        {
            // Arrange - A program testing all supported operators
            string program = @"{
  a := (5 + 3)
  b := (10 - 2)
  c := (4 * 6)
  d := (8 / 2)
  e := (9 // 2)
  f := (10 % 3)
  g := (2 ** 3)
  return (a)
}";
            
            // Act
            var result = Parser.Parse(program);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(8, result.Statements.Count);
            
            Assert.IsType<AssignmentStmt>(result.Statements[0]);
            Assert.IsType<AssignmentStmt>(result.Statements[1]);
            Assert.IsType<AssignmentStmt>(result.Statements[2]);
            Assert.IsType<AssignmentStmt>(result.Statements[3]);
            Assert.IsType<AssignmentStmt>(result.Statements[4]);
            Assert.IsType<AssignmentStmt>(result.Statements[5]);
            Assert.IsType<AssignmentStmt>(result.Statements[6]);
            Assert.IsType<ReturnStmt>(result.Statements[7]);
            
            // Check expressions in assignments
            Assert.IsType<PlusNode>(((AssignmentStmt)result.Statements[0]).Expression);
            Assert.IsType<MinusNode>(((AssignmentStmt)result.Statements[1]).Expression);
            Assert.IsType<TimesNode>(((AssignmentStmt)result.Statements[2]).Expression);
            Assert.IsType<FloatDivNode>(((AssignmentStmt)result.Statements[3]).Expression);
            Assert.IsType<IntDivNode>(((AssignmentStmt)result.Statements[4]).Expression);
            Assert.IsType<ModulusNode>(((AssignmentStmt)result.Statements[5]).Expression);
            Assert.IsType<ExponentiationNode>(((AssignmentStmt)result.Statements[6]).Expression);
            
            // Verify unparsing
            var unparsed = result.Unparse();
            Assert.Contains("a := (5 + 3)", unparsed);
            Assert.Contains("b := (10 - 2)", unparsed);
            Assert.Contains("c := (4 * 6)", unparsed);
            Assert.Contains("d := (8 / 2)", unparsed);
            Assert.Contains("e := (9 // 2)", unparsed);
            Assert.Contains("f := (10 % 3)", unparsed);
            Assert.Contains("g := (2 ** 3)", unparsed);
            Assert.Contains("return a", unparsed);
        }
        
        [Fact]
        public void Parse_ComplexNestedExpressions_ReturnsCorrectAST()
        {
            // Arrange - Program with complex nested expressions
            string program = @"{
  a := (5)
  b := (10)
  c := ((a + b) * (b - a))
  d := (((c + 1) ** 2) // 10)
  return (d)
}";
            
            // Act
            var result = Parser.Parse(program);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Statements.Count);
            
            // Check the complex expression for c
            var cAssign = (AssignmentStmt)result.Statements[2];
            Assert.IsType<TimesNode>(cAssign.Expression);
            var timesNode = (TimesNode)cAssign.Expression;
            
            Assert.IsType<PlusNode>(timesNode.Left);
            Assert.IsType<MinusNode>(timesNode.Right);
            
            // Check the even more complex expression for d
            var dAssign = (AssignmentStmt)result.Statements[3];
            Assert.IsType<IntDivNode>(dAssign.Expression);
            var intDivNode = (IntDivNode)dAssign.Expression;
            
            Assert.IsType<ExponentiationNode>(intDivNode.Left);
            Assert.IsType<LiteralNode>(intDivNode.Right);
            
            var expNode = (ExponentiationNode)intDivNode.Left;
            Assert.IsType<PlusNode>(expNode.Left);
            Assert.IsType<LiteralNode>(expNode.Right);
            
            // Verify unparsing
            var unparsed = result.Unparse();
            Assert.Contains("a := 5", unparsed);
            Assert.Contains("b := 10", unparsed);
            Assert.Contains("c := ((a + b) * (b - a))", unparsed);
            Assert.Contains("d := (((c + 1) ** 2) // 10)", unparsed);
            Assert.Contains("return d", unparsed);
        }
        
        [Fact]
        public void Parse_MultipleNestedBlocks_ReturnsCorrectAST()
        {
            // Arrange - Program with multiple levels of nested blocks
            string program = @"{
  a := (1)
  {
    b := (2)
    {
      c := (3)
      {
        d := (4)
      }
    }
  }
  return (a)
}";
            
            // Act
            var result = Parser.Parse(program);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Statements.Count);
            
            Assert.IsType<AssignmentStmt>(result.Statements[0]);
            Assert.IsType<BlockStmt>(result.Statements[1]);
            Assert.IsType<ReturnStmt>(result.Statements[2]);
            
            var level1Block = (BlockStmt)result.Statements[1];
            Assert.Equal(2, level1Block.Statements.Count);
            Assert.IsType<AssignmentStmt>(level1Block.Statements[0]);
            Assert.IsType<BlockStmt>(level1Block.Statements[1]);
            
            var level2Block = (BlockStmt)level1Block.Statements[1];
            Assert.Equal(2, level2Block.Statements.Count);
            Assert.IsType<AssignmentStmt>(level2Block.Statements[0]);
            Assert.IsType<BlockStmt>(level2Block.Statements[1]);
            
            var level3Block = (BlockStmt)level2Block.Statements[1];
            Assert.Single(level3Block.Statements);
            Assert.IsType<AssignmentStmt>(level3Block.Statements[0]);
            
            // Verify variable access at different levels
            Assert.True(level3Block.SymbolTable.ContainsKey("a"));
            Assert.True(level3Block.SymbolTable.ContainsKey("b"));
            Assert.True(level3Block.SymbolTable.ContainsKey("c"));
            
            // Verify unparsing
            var unparsed = result.Unparse();
            Assert.Contains("a := 1", unparsed);
            Assert.Contains("b := 2", unparsed);
            Assert.Contains("c := 3", unparsed);
            Assert.Contains("d := 4", unparsed);
            Assert.Contains("return a", unparsed);
            
            // Count braces to verify nesting levels
            int openBraceCount = unparsed.Count(c => c == '{');
            int closeBraceCount = unparsed.Count(c => c == '}');
            Assert.Equal(4, openBraceCount);  // One for main block, three for nested
            Assert.Equal(4, closeBraceCount);
            
            // Check indentation levels
            string[] lines = unparsed.Split('\n');
            var dLine = Array.Find(lines, line => line.Contains("d := 4"));
            Assert.NotNull(dLine);
            Assert.StartsWith("      ", dLine);  // Should have multiple indents
        }
        
        [Fact]
        public void Parse_ParallelBlocks_ReturnsCorrectAST()
        {
            // Arrange - Program with two blocks at the same level
            string program = @"{
  a := (1)
  {
    b := (2)
  }
  {
    c := (3)
  }
  return (a)
}";
            
            // Act
            var result = Parser.Parse(program);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.Statements.Count);
            
            Assert.IsType<AssignmentStmt>(result.Statements[0]);
            Assert.IsType<BlockStmt>(result.Statements[1]);
            Assert.IsType<BlockStmt>(result.Statements[2]);
            Assert.IsType<ReturnStmt>(result.Statements[3]);
            
            var firstBlock = (BlockStmt)result.Statements[1];
            var secondBlock = (BlockStmt)result.Statements[2];
            
            Assert.Single(firstBlock.Statements);
            Assert.Single(secondBlock.Statements);
            
            Assert.True(firstBlock.SymbolTable.ContainsKey("b"));
            Assert.True(secondBlock.SymbolTable.ContainsKey("c"));
            
            // Variables should not be accessible across parallel blocks
            Assert.False(firstBlock.SymbolTable.ContainsKey("c"));
            Assert.False(secondBlock.SymbolTable.ContainsKey("b"));
            
            // Verify unparsing
            var unparsed = result.Unparse();
            Assert.Contains("a := 1", unparsed);
            Assert.Contains("b := 2", unparsed);
            Assert.Contains("c := 3", unparsed);
            Assert.Contains("return a", unparsed);
            
            int openBraceCount = unparsed.Count(c => c == '{');
            int closeBraceCount = unparsed.Count(c => c == '}');
            Assert.Equal(3, openBraceCount);  // One for main block, two for inner blocks
            Assert.Equal(3, closeBraceCount);
        }
        
        [Fact]
        public void Parse_MissingBraces_ThrowsParseException()
        {
            // Arrange - Program missing opening brace
            string program = "\n  x := (42)\n}";
            
            // Act & Assert
            var exception = Assert.Throws<ParseException>(() => Parser.Parse(program));
            Assert.Contains("must start with", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
        
        [Fact]
        public void Parse_InvalidSyntax_ThrowsParseException()
        {
            // Arrange - Program with syntax error
            string program = "{\n  x = 42\n}";  // Using = instead of :=
            
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => Parser.Parse(program));
			Assert.Contains("Invalid character", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
        
        [Fact]
        public void Parse_Unparse_ParseAgain_StillWorks()
        {
            // Arrange - A simple program
            string program = "{\n  x := (42)\n  return (x)\n}";
            
            // Act
            var ast = Parser.Parse(program);
            var unparsed = ast.Unparse();
            
            // Assert
            Assert.Contains("x := 42", unparsed);
            Assert.Contains("return x", unparsed);
        }
        
        [Fact]
        public void Parse_UnparseAndVerifyStatementTypes()
        {
            // Arrange - Different statement types in a program
            string program = @"{
  a := (1)
  {
    b := (2)
  }
  return (a)
}";
            
            // Act
            var result = Parser.Parse(program);
            
            // Assert - Check each statement and its unparsed result
            Assert.Equal(3, result.Statements.Count);
            
            // Check assignment statement
            var assignStmt = (AssignmentStmt)result.Statements[0];
            var assignUnparsed = assignStmt.Unparse();
            Assert.Contains("a := 1", assignUnparsed);
            
            // Check block statement
            var blockStmt = (BlockStmt)result.Statements[1];
            var blockUnparsed = blockStmt.Unparse();
            Assert.Contains("{", blockUnparsed);
            Assert.Contains("b := 2", blockUnparsed);
            Assert.Contains("}", blockUnparsed);
            
            // Check return statement
            var returnStmt = (ReturnStmt)result.Statements[2];
            var returnUnparsed = returnStmt.Unparse();
            Assert.Contains("return a", returnUnparsed);
        }
    }
}