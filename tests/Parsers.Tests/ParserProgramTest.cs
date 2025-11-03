using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using AST;
using Tokenizer;

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
            string program = "{\n}";
            var result = Parser.Parse(program);

            Assert.NotNull(result);
            Assert.IsType<BlockStmt>(result);
            Assert.Empty(result.Statements);
            Assert.NotNull(result.SymbolTable);

            string unparsed = result.Unparse();
            Assert.Contains("{", unparsed);
            Assert.Contains("}", unparsed);
        }

        [Fact]
        public void Parse_SingleAssignment_ReturnsCorrectAST()
        {
            string program = "{\n  x := (42)\n}";
            var result = Parser.Parse(program);

            Assert.NotNull(result);
            Assert.Single(result.Statements);
            Assert.IsType<AssignmentStmt>(result.Statements[0]);

            var assignStmt = (AssignmentStmt)result.Statements[0];
            Assert.Equal("x", assignStmt.Variable.Name);
            Assert.IsType<LiteralNode>(assignStmt.Expression);
            Assert.Equal(42, ((LiteralNode)assignStmt.Expression).Value);

            Assert.True(result.SymbolTable.ContainsKey("x"));

            string unparsed = result.Unparse();
            Assert.Contains("x := 42", unparsed);
        }

        [Fact]
        public void Parse_SingleReturn_ReturnsCorrectAST()
        {
            string program = "{\n  return (42)\n}";
            var result = Parser.Parse(program);

            Assert.NotNull(result);
            Assert.Single(result.Statements);
            Assert.IsType<ReturnStmt>(result.Statements[0]);

            var returnStmt = (ReturnStmt)result.Statements[0];
            Assert.IsType<LiteralNode>(returnStmt.Expression);
            Assert.Equal(42, ((LiteralNode)returnStmt.Expression).Value);

            string unparsed = result.Unparse();
            Assert.Contains("return 42", unparsed);
        }

        [Fact]
        public void Parse_SimpleExpressions_ReturnsCorrectAST()
        {
            string program = @"{
  a := (5)
  b := (10)
  c := (a + b)
  d := (c * 2)
  return (d)
}";
            var result = Parser.Parse(program);

            Assert.NotNull(result);
            Assert.Equal(5, result.Statements.Count);

            Assert.True(result.SymbolTable.ContainsKey("a"));
            Assert.True(result.SymbolTable.ContainsKey("b"));
            Assert.True(result.SymbolTable.ContainsKey("c"));
            Assert.True(result.SymbolTable.ContainsKey("d"));

            var dAssign = (AssignmentStmt)result.Statements[3];
            var timesNode = Assert.IsType<TimesNode>(dAssign.Expression);
            Assert.IsType<VariableNode>(timesNode.Left);
            Assert.IsType<LiteralNode>(timesNode.Right);

            var leftVar = (VariableNode)timesNode.Left;
            var rightLiteral = (LiteralNode)timesNode.Right;
            Assert.Equal("c", leftVar.Name);
            Assert.Equal(2, rightLiteral.Value);

            var returnStmt = (ReturnStmt)result.Statements[4];
            Assert.IsType<VariableNode>(returnStmt.Expression);
            Assert.Equal("d", ((VariableNode)returnStmt.Expression).Name);

            string unparsed = result.Unparse();
            Assert.Contains("a := 5", unparsed);
            Assert.Contains("b := 10", unparsed);
            Assert.Contains("c := (a + b)", unparsed);
            Assert.Contains("d := (c * 2)", unparsed);
            Assert.Contains("return d", unparsed);
        }

        [Fact]
        public void Parse_NestedBlock_ReturnsCorrectAST()
        {
            string program = @"{
  x := (10)
  {
    y := (x + 5)
    z := (y * 2)
  }
  return (x)
}";
            var result = Parser.Parse(program);

            Assert.NotNull(result);
            Assert.Equal(3, result.Statements.Count);
            Assert.IsType<AssignmentStmt>(result.Statements[0]);
            Assert.IsType<BlockStmt>(result.Statements[1]);
            Assert.IsType<ReturnStmt>(result.Statements[2]);

            var nestedBlock = (BlockStmt)result.Statements[1];
            Assert.Equal(2, nestedBlock.Statements.Count);
            Assert.IsType<AssignmentStmt>(nestedBlock.Statements[0]);
            Assert.IsType<AssignmentStmt>(nestedBlock.Statements[1]);

            Assert.True(result.SymbolTable.ContainsKey("x"));
            Assert.False(result.SymbolTable.ContainsKey("y"));
            Assert.False(result.SymbolTable.ContainsKey("z"));

            Assert.True(nestedBlock.SymbolTable.ContainsKey("y"));
            Assert.True(nestedBlock.SymbolTable.ContainsKey("z"));
            Assert.True(nestedBlock.SymbolTable.ContainsKey("x"));

            string unparsed = result.Unparse();
            Assert.Contains("x := 10", unparsed);
            Assert.Contains("y := (x + 5)", unparsed);
            Assert.Contains("z := (y * 2)", unparsed);
            Assert.Contains("return x", unparsed);

            int openBraceCount = unparsed.Count(c => c == '{');
            int closeBraceCount = unparsed.Count(c => c == '}');
            Assert.Equal(2, openBraceCount);
            Assert.Equal(2, closeBraceCount);
        }

        [Fact]
        public void Parse_AllOperators_ReturnsCorrectAST()
        {
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
            var result = Parser.Parse(program);

            Assert.NotNull(result);
            Assert.Equal(8, result.Statements.Count);

            Assert.IsType<PlusNode>(((AssignmentStmt)result.Statements[0]).Expression);
            Assert.IsType<MinusNode>(((AssignmentStmt)result.Statements[1]).Expression);
            Assert.IsType<TimesNode>(((AssignmentStmt)result.Statements[2]).Expression);
            Assert.IsType<FloatDivNode>(((AssignmentStmt)result.Statements[3]).Expression);
            Assert.IsType<IntDivNode>(((AssignmentStmt)result.Statements[4]).Expression);
            Assert.IsType<ModulusNode>(((AssignmentStmt)result.Statements[5]).Expression);
            Assert.IsType<ExponentiationNode>(((AssignmentStmt)result.Statements[6]).Expression);
            Assert.IsType<ReturnStmt>(result.Statements[7]);

            string unparsed = result.Unparse();
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
            string program = @"{
  a := (5)
  b := (10)
  c := ((a + b) * (b - a))
  d := (((c + 1) ** 2) // 10)
  return (d)
}";
            var result = Parser.Parse(program);

            Assert.NotNull(result);
            Assert.Equal(5, result.Statements.Count);

            var cAssign = (AssignmentStmt)result.Statements[2];
            var timesNode = Assert.IsType<TimesNode>(cAssign.Expression);
            Assert.IsType<PlusNode>(timesNode.Left);
            Assert.IsType<MinusNode>(timesNode.Right);

            var dAssign = (AssignmentStmt)result.Statements[3];
            var intDivNode = Assert.IsType<IntDivNode>(dAssign.Expression);
            Assert.IsType<ExponentiationNode>(intDivNode.Left);
            Assert.IsType<LiteralNode>(intDivNode.Right);

            string unparsed = result.Unparse();
            Assert.Contains("c := ((a + b) * (b - a))", unparsed);
            Assert.Contains("d := (((c + 1) ** 2) // 10)", unparsed);
            Assert.Contains("return d", unparsed);
        }

        [Fact]
        public void Parse_MultipleNestedBlocks_ReturnsCorrectAST()
        {
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
            var result = Parser.Parse(program);

            Assert.NotNull(result);
            Assert.Equal(3, result.Statements.Count);
            Assert.IsType<AssignmentStmt>(result.Statements[0]);
            Assert.IsType<BlockStmt>(result.Statements[1]);
            Assert.IsType<ReturnStmt>(result.Statements[2]);

            var level1 = (BlockStmt)result.Statements[1];
            var level2 = (BlockStmt)level1.Statements[1];
            var level3 = (BlockStmt)level2.Statements[1];

            Assert.True(level3.SymbolTable.ContainsKey("a"));
            Assert.True(level3.SymbolTable.ContainsKey("b"));
            Assert.True(level3.SymbolTable.ContainsKey("c"));

            string unparsed = result.Unparse();
            Assert.Contains("a := 1", unparsed);
            Assert.Contains("b := 2", unparsed);
            Assert.Contains("c := 3", unparsed);
            Assert.Contains("d := 4", unparsed);
            Assert.Contains("return a", unparsed);

            int openBraces = unparsed.Count(c => c == '{');
            int closeBraces = unparsed.Count(c => c == '}');
            Assert.Equal(4, openBraces);
            Assert.Equal(4, closeBraces);

            string[] lines = unparsed.Split('\n');
            var dLine = Array.Find(lines, l => l.Contains("d := 4"));
            Assert.NotNull(dLine);
            Assert.StartsWith("      ", dLine);
        }

        [Fact]
        public void Parse_ParallelBlocks_ReturnsCorrectAST()
        {
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
            var result = Parser.Parse(program);

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
            Assert.False(firstBlock.SymbolTable.ContainsKey("c"));
            Assert.False(secondBlock.SymbolTable.ContainsKey("b"));

            string unparsed = result.Unparse();
            Assert.Contains("a := 1", unparsed);
            Assert.Contains("b := 2", unparsed);
            Assert.Contains("c := 3", unparsed);
            Assert.Contains("return a", unparsed);

            int openBraces = unparsed.Count(c => c == '{');
            int closeBraces = unparsed.Count(c => c == '}');
            Assert.Equal(3, openBraces);
            Assert.Equal(3, closeBraces);
        }

        [Fact]
        public void Parse_MissingBraces_ThrowsParseException()
        {
            string program = "\n  x := (42)\n}";
            var exception = Assert.Throws<ParseException>(() => Parser.Parse(program));
            Assert.Contains("must start with", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Parse_InvalidSyntax_ThrowsParseException()
        {
            string program = "{\n  x = 42\n}";
            var exception = Assert.Throws<ArgumentException>(() => Parser.Parse(program));
            Assert.Contains("Invalid character", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Parse_Unparse_ParseAgain_StillWorks()
        {
            string program = "{\n  x := (42)\n  return (x)\n}";
            var ast = Parser.Parse(program);
            string unparsed = ast.Unparse();
            Assert.Contains("x := 42", unparsed);
            Assert.Contains("return x", unparsed);
        }

        [Fact]
        public void Parse_UnparseAndVerifyStatementTypes()
        {
            string program = @"{
  a := (1)
  {
    b := (2)
  }
  return (a)
}";
            var result = Parser.Parse(program);
            Assert.Equal(3, result.Statements.Count);

            var assignStmt = (AssignmentStmt)result.Statements[0];
            string assignUnparsed = assignStmt.Unparse();
            Assert.Contains("a := 1", assignUnparsed);

            var blockStmt = (BlockStmt)result.Statements[1];
            string blockUnparsed = blockStmt.Unparse();
            Assert.Contains("{", blockUnparsed);
            Assert.Contains("b := 2", blockUnparsed);
            Assert.Contains("}", blockUnparsed);

            var returnStmt = (ReturnStmt)result.Statements[2];
            string returnUnparsed = returnStmt.Unparse();
            Assert.Contains("return a", returnUnparsed);
        }
    }
}
