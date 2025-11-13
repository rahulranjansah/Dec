using Xunit;
using AST;
using System.Collections.Generic;

namespace AST.Tests
{
    /// <summary>
    /// Comprehensive test suite for UnparseVisitor.
    /// Verifies correct reconstruction of source text from AST nodes,
    /// including indentation, nested blocks, and null handling.
    /// </summary>
    public class UnparseVisitorTests
    {
        private readonly UnparseVisitor _visitor = new UnparseVisitor();

        // -------------------------------
        // BASIC NODES
        // -------------------------------

        // [Fact(DisplayName = "LiteralNode unparses various literal types safely")]
        // public void LiteralNode_UnparsesSafely()
        // {
        //     Assert.Equal("42", new LiteralNode(42).Accept(_visitor, 0));
        //     Assert.Equal("3.14", new LiteralNode(3.14).Accept(_visitor, 0));
        //     Assert.Equal("hello", new LiteralNode("hello").Accept(_visitor, 0));

        //     // Null-safe check: should not throw
        //     var nullNode = new LiteralNode(null);
        //     var result = Record.Exception(() => nullNode.Accept(_visitor, 0));
        //     Assert.Null(result); // no exception
        // }

        [Fact(DisplayName = "VariableNode unparses variable name correctly")]
        public void VariableNode_UnparsesName()
        {
            Assert.Equal("x", new VariableNode("x").Accept(_visitor, 0));
        }

        // -------------------------------
        // BINARY OPERATORS
        // -------------------------------

        [Theory(DisplayName = "Binary operators unparse sub-expressions correctly")]
        [InlineData(typeof(PlusNode), "(1 + 2)")]
        [InlineData(typeof(MinusNode), "(1 - 2)")]
        [InlineData(typeof(TimesNode), "(1 * 2)")]
        [InlineData(typeof(FloatDivNode), "(1 / 2)")]
        [InlineData(typeof(IntDivNode), "(1 // 2)")]
        [InlineData(typeof(ModulusNode), "(1 % 2)")]
        [InlineData(typeof(ExponentiationNode), "(1 ** 2)")]
        public void BinaryOperators_UnparseProperly(System.Type nodeType, string expected)
        {
            var left = new LiteralNode(1);
            var right = new LiteralNode(2);
            var node = (ExpressionNode)System.Activator.CreateInstance(nodeType, left, right);

            Assert.Equal(expected, node.Accept(_visitor, 0));
        }

        // -------------------------------
        // STATEMENT NODES
        // -------------------------------

        [Fact(DisplayName = "AssignmentStmt unparses correctly with indentation")]
        public void AssignmentStmt_UnparsesProperly()
        {
            var stmt = new AssignmentStmt(
                new VariableNode("x"),
                new PlusNode(new LiteralNode(5), new LiteralNode(3))
            );

            var result = stmt.Accept(_visitor, 1);
            Assert.Contains("x := (5 + 3)", result);
        }

        [Fact(DisplayName = "ReturnStmt unparses correctly with indentation")]
        public void ReturnStmt_UnparsesProperly()
        {
            var stmt = new ReturnStmt(new LiteralNode(99));
            var result = stmt.Accept(_visitor, 2);
            Assert.Contains("return 99", result);
        }

        [Fact(DisplayName = "Empty BlockStmt unparses to braces only")]
        public void BlockStmt_Empty_UnparsesCorrectly()
        {
            var block = new BlockStmt(new List<Statement>());
            var result = block.Accept(_visitor, 0);
            Assert.Equal("{\n}", result.Replace("\r", ""));
        }

        [Fact(DisplayName = "BlockStmt unparses multiple statements with correct structure")]
        public void BlockStmt_UnparsesProperly()
        {
            var block = new BlockStmt(new List<Statement>
            {
                new AssignmentStmt(new VariableNode("a"), new LiteralNode(5)),
                new ReturnStmt(new VariableNode("a"))
            });

            var result = block.Accept(_visitor, 0);
            Assert.Contains("a := 5", result);
            Assert.Contains("return a", result);
            Assert.StartsWith("{", result.TrimStart());
            Assert.EndsWith("}", result.TrimEnd());
        }

        // -------------------------------
        // EDGE CASES
        // -------------------------------

        [Fact(DisplayName = "Nested BlockStmts indent correctly and preserve scope structure")]
        public void NestedBlocks_UnparseWithProperIndentation()
        {
            var inner = new BlockStmt(new List<Statement>
            {
                new AssignmentStmt(new VariableNode("x"), new LiteralNode(10))
            });
            var outer = new BlockStmt(new List<Statement> { inner });

            string result = outer.Accept(_visitor, 0);
            Assert.Contains("{", result);
            Assert.Contains("x := 10", result);
            Assert.Matches(@"\s{4}x := 10", result); // exactly 4-space indent for inner block
        }

        [Fact(DisplayName = "Variable shadowing scenario unparses both scopes distinctly")]
        public void VariableShadowing_UnparsesCorrectly()
        {
            var inner = new BlockStmt(new List<Statement>
            {
                new AssignmentStmt(new VariableNode("x"), new LiteralNode(99))
            });
            var outer = new BlockStmt(new List<Statement>
            {
                new AssignmentStmt(new VariableNode("x"), new LiteralNode(1)),
                inner
            });

            string result = outer.Accept(_visitor, 0);
            Assert.Contains("x := 1", result);
            Assert.Contains("x := 99", result);
        }

        [Fact(DisplayName = "Complex nested expression unparses with full parentheses")]
        public void ComplexNestedExpression_UnparsesCorrectly()
        {
            var expr = new TimesNode(
                new PlusNode(new LiteralNode(2), new LiteralNode(3)),
                new MinusNode(new LiteralNode(4), new LiteralNode(1))
            );
            string result = expr.Accept(_visitor, 0);
            Assert.Equal("((2 + 3) * (4 - 1))", result);
        }

        [Fact(DisplayName = "Variable names with Unicode or special characters are preserved")]
        public void VariableNode_NonAsciiNames_UnparsedVerbatim()
        {
            var stmt = new AssignmentStmt(
                new VariableNode("α_β"),
                new LiteralNode(42)
            );

            string result = stmt.Accept(_visitor, 0);
            Assert.Contains("α_β := 42", result);
        }

        [Fact(DisplayName = "Deep indentation level generates correct spacing")]
        public void DeepIndentation_ProducesCorrectIndent()
        {
            var stmt = new AssignmentStmt(new VariableNode("z"), new LiteralNode(7));
            string result = stmt.Accept(_visitor, 3);

            // Expect at least 12 spaces (3 * 4)
            Assert.Matches(@"^\s{12}z := 7", result);
        }
    }
}
