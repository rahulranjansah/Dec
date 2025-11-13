using System;
using Xunit;
using AST;
using Utilities;
using System.Collections.Generic;
using Containers;
using Visitors;

namespace AST.Tests
{
    /// <summary>
    /// Tests for NameAnalysisVisitor to verify correct variable declaration and usage checking.
    /// </summary>
    public class NameAnalysisVisitorTests
    {
        private readonly NameAnalysisVisitor _visitor;

        public NameAnalysisVisitorTests()
        {
            _visitor = new NameAnalysisVisitor();
        }

        private Tuple<SymbolTable<string, object>, Statement> CreateContext(SymbolTable<string, object> table = null)
        {
            return new Tuple<SymbolTable<string, object>, Statement>(table ?? new SymbolTable<string, object>(null), null);
        }

        // ============================================================================
        // LITERAL AND VARIABLE TESTS
        // ============================================================================

        [Fact]
        public void TestVisit_LiteralNode_AlwaysValid()
        {
            var node = new LiteralNode(5);
            var result = node.Accept(_visitor, CreateContext());
            Assert.True(result);
        }

        [Fact]
        public void TestVisit_VariableNode_DeclaredVariable_ReturnsTrue()
        {
            var table = new SymbolTable<string, object>(null);
            table.Add("x", null);
            var node = new VariableNode("x");

            var result = node.Accept(_visitor, CreateContext(table));
            Assert.True(result);
        }

        [Fact]
        public void TestVisit_VariableNode_UndeclaredVariable_ReturnsFalse()
        {
            var table = new SymbolTable<string, object>(null);
            var node = new VariableNode("y");

            var result = node.Accept(_visitor, CreateContext(table));
            Assert.False(result);
        }

        // ============================================================================
        // ASSIGNMENT TESTS
        // ============================================================================

        [Fact]
        public void TestVisit_AssignmentStmt_AddsVariableToSymbolTable()
        {
            var table = new SymbolTable<string, object>(null);
            var assign = new AssignmentStmt(new VariableNode("a"), new LiteralNode(10));

            var result = assign.Accept(_visitor, CreateContext(table));

            Assert.True(result);
            Assert.True(table.ContainsKey("a"));
        }

        [Fact]
        public void TestVisit_AssignmentStmt_ExpressionWithUndeclaredVar_ReturnsFalse()
        {
            var table = new SymbolTable<string, object>(null);
            var expr = new PlusNode(new VariableNode("x"), new LiteralNode(5));
            var assign = new AssignmentStmt(new VariableNode("result"), expr);

            var result = assign.Accept(_visitor, CreateContext(table));

            Assert.False(result);
            Assert.True(table.ContainsKey("result")); // variable still declared
        }

        [Fact]
        public void TestVisit_AssignmentStmt_ExpressionWithDeclaredVar_ReturnsTrue()
        {
            var table = new SymbolTable<string, object>(null);
            table.Add("x", null);
            var expr = new PlusNode(new VariableNode("x"), new LiteralNode(5));
            var assign = new AssignmentStmt(new VariableNode("result"), expr);

            var result = assign.Accept(_visitor, CreateContext(table));

            Assert.True(result);
            Assert.True(table.ContainsKey("result"));
        }

        // ============================================================================
        // OPERATOR TESTS (Plus, Minus, Times, etc.)
        // ============================================================================

        [Theory]
        [InlineData(typeof(PlusNode))]
        [InlineData(typeof(MinusNode))]
        [InlineData(typeof(TimesNode))]
        [InlineData(typeof(FloatDivNode))]
        [InlineData(typeof(IntDivNode))]
        [InlineData(typeof(ModulusNode))]
        [InlineData(typeof(ExponentiationNode))]
        public void TestVisit_BinaryOperators_DelegatesToChildren(Type nodeType)
        {
            var table = new SymbolTable<string, object>(null);
            table.Add("a", null);
            table.Add("b", null);

            var left = new VariableNode("a");
            var right = new VariableNode("b");

            dynamic node = Activator.CreateInstance(nodeType, left, right);

            var result = node.Accept(_visitor, CreateContext(table));

            Assert.True(result);
        }

        [Fact]
        public void TestVisit_BinaryOperators_WithUndeclaredVariable_ReturnsFalse()
        {
            var table = new SymbolTable<string, object>(null);
            table.Add("a", null);

            var expr = new PlusNode(new VariableNode("a"), new VariableNode("b"));
            var result = expr.Accept(_visitor, CreateContext(table));

            Assert.False(result);
        }

        // ============================================================================
        // RETURN TESTS
        // ============================================================================

        [Fact]
        public void TestVisit_ReturnStmt_DeclaredVariable_ReturnsTrue()
        {
            var table = new SymbolTable<string, object>(null);
            table.Add("x", null);
            var stmt = new ReturnStmt(new VariableNode("x"));

            var result = stmt.Accept(_visitor, CreateContext(table));

            Assert.True(result);
        }

        [Fact]
        public void TestVisit_ReturnStmt_UndeclaredVariable_ReturnsFalse()
        {
            var table = new SymbolTable<string, object>(null);
            var stmt = new ReturnStmt(new VariableNode("z"));

            var result = stmt.Accept(_visitor, CreateContext(table));

            Assert.False(result);
        }

        // ============================================================================
        // BLOCK TESTS
        // ============================================================================

        [Fact]
        public void TestVisit_EmptyBlock_ReturnsTrue()
        {
            var table = new SymbolTable<string, object>(null);
            var block = new BlockStmt(table);

            var result = block.Accept(_visitor, CreateContext(table));
            Assert.True(result);
        }

        [Fact]
        public void TestVisit_BlockWithAssignments_AllDeclared_ReturnsTrue()
        {
            var table = new SymbolTable<string, object>(null);
            var block = new BlockStmt(table);
            block.Statements.Add(new AssignmentStmt(new VariableNode("a"), new LiteralNode(1)));
            block.Statements.Add(new AssignmentStmt(new VariableNode("b"), new PlusNode(new VariableNode("a"), new LiteralNode(2))));

            var result = block.Accept(_visitor, CreateContext(table));

            Assert.True(result);
        }

        [Fact]
        public void TestVisit_BlockWithUndeclaredUse_ReturnsFalse()
        {
            var table = new SymbolTable<string, object>(null);
            var block = new BlockStmt(table);
            block.Statements.Add(new AssignmentStmt(new VariableNode("a"), new VariableNode("b"))); // b undeclared

            var result = block.Accept(_visitor, CreateContext(table));

            Assert.False(result);
        }

        [Fact]
        public void TestVisit_NestedBlock_InheritsParentScope()
        {
            var outerTable = new SymbolTable<string, object>(null);
            var outerBlock = new BlockStmt(outerTable);
            outerBlock.Statements.Add(new AssignmentStmt(new VariableNode("x"), new LiteralNode(1)));

            var innerTable = new SymbolTable<string, object>(outerTable);
            var innerBlock = new BlockStmt(innerTable);
            innerBlock.Statements.Add(new ReturnStmt(new VariableNode("x"))); // from outer scope

            outerBlock.Statements.Add(innerBlock);

            var result = outerBlock.Accept(_visitor, CreateContext(outerTable));

            Assert.True(result);
        }

        [Fact]
        public void TestVisit_NestedBlock_UndeclaredInAllScopes_ReturnsFalse()
        {
            var outerTable = new SymbolTable<string, object>(null);
            var outerBlock = new BlockStmt(outerTable);
            var innerTable = new SymbolTable<string, object>(outerTable);
            var innerBlock = new BlockStmt(innerTable);

            innerBlock.Statements.Add(new ReturnStmt(new VariableNode("missing")));
            outerBlock.Statements.Add(innerBlock);

            var result = outerBlock.Accept(_visitor, CreateContext(outerTable));

            Assert.False(result);
        }
    }
}
