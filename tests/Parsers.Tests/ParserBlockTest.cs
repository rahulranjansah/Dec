using System;
using System.Collections.Generic;
using Xunit;
using AST;
using Tokenizer;
using Containers;
using System.Reflection;
using System.Linq;

namespace Parser.Tests
{
    public class ParseStmtListTests
    {
        // Helper: invoke Parser.ParseStmtList() privately via reflection
        private void InvokeParseStmtList(List<string> lines, BlockStmt blockStmt)
        {
            Type parserType = typeof(Parser);
            MethodInfo methodInfo = parserType.GetMethod("ParseStmtList",
                BindingFlags.NonPublic | BindingFlags.Static);

            if (methodInfo == null)
                throw new MissingMethodException("ParseStmtList not found in Parser class.");

            methodInfo.Invoke(null, new object[] { lines, blockStmt });
        }

        // Helper: build BlockStmt with optional parent scope
        private BlockStmt CreateBlockStmt(SymbolTable<string, object> parentScope = null)
        {
            var scope = parentScope == null
                ? new SymbolTable<string, object>()
                : new SymbolTable<string, object>(parentScope);

            return new BlockStmt(scope);
        }

        // --------------------------------------------------------------
        // Simple Flat Blocks
        // --------------------------------------------------------------

        [Fact]
        public void ParseStmtList_EmptyBlock_ReturnsCorrectly()
        {
            var lines = new List<string> { "}" };
            var block = CreateBlockStmt();

            InvokeParseStmtList(lines, block);

            Assert.Empty(block.Statements);
            Assert.Single(lines); // '}' should remain
        }

        [Fact]
        public void ParseStmtList_SingleAssignment_ReturnsCorrectly()
        {
            var lines = new List<string> { "x := (42)", "}" };
            var block = CreateBlockStmt();

            InvokeParseStmtList(lines, block);

            Assert.Single(block.Statements);
            var assign = Assert.IsType<AssignmentStmt>(block.Statements.First());

            Assert.Equal("x", assign.Variable.Name);
            var literal = Assert.IsType<LiteralNode>(assign.Expression);
            Assert.Equal(42, literal.Value);
            Assert.True(block.SymbolTable.ContainsKey("x"));

            Assert.Single(lines); // '}' should remain
        }

        [Fact]
        public void ParseStmtList_SingleReturn_ReturnsCorrectly()
        {
            var lines = new List<string> { "return (x)", "}" };
            var block = CreateBlockStmt();

            InvokeParseStmtList(lines, block);

            Assert.Single(block.Statements);
            var ret = Assert.IsType<ReturnStmt>(block.Statements.First());
            var varNode = Assert.IsType<VariableNode>(ret.Expression);
            Assert.Equal("x", varNode.Name);

            Assert.Single(lines);
        }

        [Fact]
        public void ParseStmtList_MultipleStatements_ReturnsCorrectly()
        {
            var lines = new List<string>
            {
                "x := (5)",
                "y := (10)",
                "z := (x + y)",
                "return (z)",
                "}"
            };
            var block = CreateBlockStmt();

            InvokeParseStmtList(lines, block);

            Assert.Equal(4, block.Statements.Count);
            Assert.True(block.SymbolTable.ContainsKey("x"));
            Assert.True(block.SymbolTable.ContainsKey("y"));
            Assert.True(block.SymbolTable.ContainsKey("z"));
            Assert.Single(lines);
        }

        [Fact]
        public void ParseStmtList_InvalidStatement_ThrowsParseException()
        {
            var lines = new List<string> { "invalid statement", "}" };
            var block = CreateBlockStmt();

            var ex = Assert.Throws<TargetInvocationException>(() =>
                InvokeParseStmtList(lines, block));
            Assert.IsType<ParseException>(ex.InnerException);
        }

        [Fact]
        public void ParseStmtList_MissingClosingBrace_ThrowsParseException()
        {
            var lines = new List<string> { "x := (42)" }; // missing '}'
            var block = CreateBlockStmt();

            var ex = Assert.Throws<TargetInvocationException>(() =>
                InvokeParseStmtList(lines, block));
            Assert.IsType<ParseException>(ex.InnerException);
        }

        // --------------------------------------------------------------
        // Nested Block Variants
        // --------------------------------------------------------------

        [Fact]
        public void ParseStmtList_SingleNestedEmptyBlock_ReturnsCorrectly()
        {
            var lines = new List<string> { "{", "}", "}" };
            var block = CreateBlockStmt();

            InvokeParseStmtList(lines, block);

            Assert.Single(block.Statements);
            var inner = Assert.IsType<BlockStmt>(block.Statements.First());
            Assert.Empty(inner.Statements);
            Assert.Single(lines);
        }

        [Fact]
        public void ParseStmtList_NestedBlockWithStatements_ReturnsCorrectly()
        {
            var lines = new List<string> { "{", "x := (42)", "y := (x + 10)", "}", "}" };
            var block = CreateBlockStmt();

            InvokeParseStmtList(lines, block);

            var nested = Assert.IsType<BlockStmt>(block.Statements.First());
            Assert.Equal(2, nested.Statements.Count);
            Assert.True(nested.SymbolTable.ContainsKey("x"));
            Assert.True(nested.SymbolTable.ContainsKey("y"));
            Assert.False(block.SymbolTable.ContainsKey("x"));
            Assert.Single(lines);
        }

        [Fact]
        public void ParseStmtList_MultipleNestedBlocks_ReturnsCorrectly()
        {
            var lines = new List<string>
            {
                "x := (1)", "{", "y := (2)", "}", "{", "z := (3)", "}", "}"
            };
            var block = CreateBlockStmt();

            InvokeParseStmtList(lines, block);

            Assert.Equal(3, block.Statements.Count);
            Assert.Single(lines);
        }

        [Fact]
        public void ParseStmtList_DeepNesting_ReturnsCorrectly()
        {
            var lines = new List<string>
            {
                "a := (1)", "{", "b := (2)", "{", "c := (3)", "{", "d := (4)", "}", "}", "}", "}"
            };
            var block = CreateBlockStmt();

            InvokeParseStmtList(lines, block);

            var level1 = (BlockStmt)block.Statements[1];
            var level2 = (BlockStmt)level1.Statements[1];
            var level3 = (BlockStmt)level2.Statements[1];

            Assert.True(level3.SymbolTable.ContainsKey("d"));
            Assert.True(level3.SymbolTable.ContainsKey("a"));
            Assert.True(level3.SymbolTable.ContainsKey("b"));
            Assert.True(level3.SymbolTable.ContainsKey("c"));
        }

        [Fact]
        public void ParseStmtList_MissingClosingBraceInNestedBlock_ThrowsParseException()
        {
            var lines = new List<string> { "{", "x := (42)", "}" }; // missing one '}'
            var block = CreateBlockStmt();

            var ex = Assert.Throws<TargetInvocationException>(() =>
                InvokeParseStmtList(lines, block));
            Assert.IsType<ParseException>(ex.InnerException);
        }

        [Fact]
        public void ParseStmtList_NestedBlockWithReturn_ReturnsCorrectly()
        {
            var lines = new List<string>
            {
                "x := (1)", "{", "y := (2)", "return (x + y)", "}", "}"
            };
            var block = CreateBlockStmt();

            InvokeParseStmtList(lines, block);

            var inner = (BlockStmt)block.Statements[1];
            var ret = (ReturnStmt)inner.Statements[1];
            Assert.IsType<PlusNode>(ret.Expression);
        }

        [Fact]
        public void ParseStmtList_VerifyUnparse_ReturnsCorrectString()
        {
            var lines = new List<string> { "x := (42)", "{", "y := (x + 10)", "}", "}" };
            var block = CreateBlockStmt();

            InvokeParseStmtList(lines, block);
            var result = block.Unparse();

            Assert.Contains("x := 42", result);
            Assert.Contains("y := (x + 10)", result);
            Assert.Contains("{", result);
            Assert.Contains("}", result);
        }

        // --------------------------------------------------------------
        // Multi-level & Complex Scoping Tests
        // --------------------------------------------------------------

        [Fact]
        public void ParseStmtList_ComplexProgram_ReturnsCorrectly()
        {
            var lines = new List<string>
            {
                "a := (10)", "b := (20)", "{", "c := (a + b)", "{", "d := (c * 2)", "return (d)",
                "}", "e := (30)", "}", "f := (40)", "}"
            };

            var block = CreateBlockStmt();
            InvokeParseStmtList(lines, block);

            var mid = (BlockStmt)block.Statements[2];
            var inner = (BlockStmt)mid.Statements[1];

            Assert.True(inner.SymbolTable.ContainsKey("a"));
            Assert.True(inner.SymbolTable.ContainsKey("c"));
            Assert.True(inner.SymbolTable.ContainsKey("d"));
        }

        [Fact]
        public void ParseStmtList_AddStatement_AddsToBlockCorrectly()
        {
            var lines = new List<string> { "}" };
            var block = CreateBlockStmt();
            var stmt = new AssignmentStmt(new VariableNode("x"), new LiteralNode(42));

            block.AddStatement(stmt);
            InvokeParseStmtList(lines, block);

            Assert.Single(block.Statements);
            Assert.Same(stmt, block.Statements[0]);
        }

        [Fact]
        public void ParseStmtList_TwoInnerScopes_ReturnsCorrectly()
        {
            var lines = new List<string>
            {
                "a := (100)", "{", "b := (a + 5)", "c := (200)", "}", "d := (300)",
                "{", "e := (a + d)", "f := (400)", "}", "g := (500)", "}"
            };

            var block = CreateBlockStmt();
            InvokeParseStmtList(lines, block);

            var first = (BlockStmt)block.Statements[1];
            var second = (BlockStmt)block.Statements[3];

            Assert.True(first.SymbolTable.ContainsKey("b"));
            Assert.True(second.SymbolTable.ContainsKey("e"));
            Assert.True(second.SymbolTable.ContainsKey("f"));
        }
    }
}
