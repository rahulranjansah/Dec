using System;
using Xunit;
using AST;
using Containers;

namespace AST.Tests
{
    /// <summary>
    /// Tests for the EvaluateVisitor class, which interprets AST programs.
    /// Covers arithmetic, variables, assignments, scoping, and errors.
    /// </summary>
    public class EvaluateVisitorTests
    {
        private EvaluateVisitor CreateVisitor() => new EvaluateVisitor();

        private SymbolTable<string, object> CreateGlobalScope() =>
            new SymbolTable<string, object>();

        // -------------------------------
        // LITERAL AND VARIABLE TESTS
        // -------------------------------

        [Fact(DisplayName = "LiteralNode returns its raw value")]
        public void LiteralNode_ReturnsValue()
        {
            var visitor = CreateVisitor();
            var litInt = new LiteralNode(42);
            var litFloat = new LiteralNode(3.14);

            Assert.Equal(42, litInt.Accept(visitor, null));
            Assert.Equal(3.14, litFloat.Accept(visitor, null));
        }

        [Fact(DisplayName = "VariableNode retrieves stored value from symbol table")]
        public void VariableNode_ReturnsValueFromSymbolTable()
        {
            var visitor = CreateVisitor();
            var table = CreateGlobalScope();
            table.Add("x", 10);

            var variable = new VariableNode("x");
            var result = variable.Accept(visitor, table);

            Assert.Equal(10, result);
        }

        [Fact(DisplayName = "VariableNode throws EvaluationException when undefined")]
        public void VariableNode_ThrowsOnUndefined()
        {
            var visitor = CreateVisitor();
            var table = CreateGlobalScope();

            var variable = new VariableNode("y");

            Assert.Throws<EvaluationException>(() => variable.Accept(visitor, table));
        }

        // -------------------------------
        // ARITHMETIC TESTS
        // -------------------------------

        [Theory(DisplayName = "PlusNode correctly adds integers and floats")]
        [InlineData(5, 3, 8)]
        [InlineData(2.5, 1.5, 4.0)]
        [InlineData(2, 1.5, 3.5)]
        public void PlusNode_AddsValues(object left, object right, double expected)
        {
            var visitor = CreateVisitor();
            var node = new PlusNode(new LiteralNode(left), new LiteralNode(right));
            var result = node.Accept(visitor, null);

            Assert.Equal(expected, Convert.ToDouble(result));
        }

        [Theory(DisplayName = "MinusNode correctly subtracts integers and floats")]
        [InlineData(10, 4, 6)]
        [InlineData(5.5, 2.5, 3.0)]
        [InlineData(10, 2.5, 7.5)]
        public void MinusNode_SubtractsValues(object left, object right, double expected)
        {
            var visitor = CreateVisitor();
            var node = new MinusNode(new LiteralNode(left), new LiteralNode(right));
            var result = node.Accept(visitor, null);

            Assert.Equal(expected, Convert.ToDouble(result));
        }

        [Theory(DisplayName = "TimesNode multiplies correctly for ints and floats")]
        [InlineData(2, 3, 6)]
        [InlineData(2.5, 2, 5.0)]
        [InlineData(1.5, 2.0, 3.0)]
        public void TimesNode_MultipliesCorrectly(object left, object right, double expected)
        {
            var visitor = CreateVisitor();
            var node = new TimesNode(new LiteralNode(left), new LiteralNode(right));
            var result = node.Accept(visitor, null);

            Assert.Equal(expected, Convert.ToDouble(result));
        }

        [Fact(DisplayName = "FloatDivNode divides correctly")]
        public void FloatDivNode_DividesCorrectly()
        {
            var visitor = CreateVisitor();
            var node = new FloatDivNode(new LiteralNode(10.0), new LiteralNode(2.0));

            var result = node.Accept(visitor, null);
            Assert.Equal(5.0, Convert.ToDouble(result));
        }

        [Fact(DisplayName = "FloatDivNode throws on division by zero")]
        public void FloatDivNode_ThrowsOnZero()
        {
            var visitor = CreateVisitor();
            var node = new FloatDivNode(new LiteralNode(10), new LiteralNode(0.0));

            Assert.Throws<EvaluationException>(() => node.Accept(visitor, null));
        }

        [Fact(DisplayName = "IntDivNode performs integer division correctly")]
        public void IntDivNode_IntegerDivision()
        {
            var visitor = CreateVisitor();
            var node = new IntDivNode(new LiteralNode(9), new LiteralNode(2));

            var result = node.Accept(visitor, null);
            Assert.Equal(4, result);
        }

        [Fact(DisplayName = "IntDivNode throws on divide by zero")]
        public void IntDivNode_ThrowsOnZero()
        {
            var visitor = CreateVisitor();
            var node = new IntDivNode(new LiteralNode(5), new LiteralNode(0));

            Assert.Throws<EvaluationException>(() => node.Accept(visitor, null));
        }

        [Theory(DisplayName = "ModulusNode works for integers and floats")]
        [InlineData(10, 3, 1)]
        [InlineData(10.0, 3.0, 1.0)]
        public void ModulusNode_WorksCorrectly(object left, object right, double expected)
        {
            var visitor = CreateVisitor();
            var node = new ModulusNode(new LiteralNode(left), new LiteralNode(right));

            var result = node.Accept(visitor, null);
            Assert.Equal(expected, Convert.ToDouble(result));
        }

        [Fact(DisplayName = "ExponentiationNode computes powers correctly")]
        public void ExponentiationNode_PowersCorrectly()
        {
            var visitor = CreateVisitor();
            var node = new ExponentiationNode(new LiteralNode(2), new LiteralNode(3));
            var result = node.Accept(visitor, null);

            Assert.Equal(8.0, Convert.ToDouble(result));
        }

        // -------------------------------
        // ASSIGNMENT AND BLOCK TESTS
        // -------------------------------

        [Fact(DisplayName = "AssignmentStmt defines new variable")]
        public void AssignmentStmt_DefinesVariable()
        {
            var visitor = CreateVisitor();
            var table = CreateGlobalScope();

            var assign = new AssignmentStmt(new VariableNode("x"), new LiteralNode(42));
            assign.Accept(visitor, table);

            Assert.True(table.ContainsKey("x"));
            Assert.Equal(42, table["x"]);
        }

        [Fact(DisplayName = "AssignmentStmt updates existing variable")]
        public void AssignmentStmt_UpdatesVariable()
        {
            var visitor = CreateVisitor();
            var table = CreateGlobalScope();
            table.Add("x", 10);

            var assign = new AssignmentStmt(new VariableNode("x"), new LiteralNode(20));
            assign.Accept(visitor, table);

            Assert.Equal(20, table["x"]);
        }

        [Fact(DisplayName = "BlockStmt executes statements sequentially and respects scope")]
        public void BlockStmt_SequentialExecution()
        {
            var visitor = CreateVisitor();
            var block = new BlockStmt(new SymbolTable<string, object>(null));

            block.AddStatement(new AssignmentStmt(new VariableNode("a"), new LiteralNode(5)));
            block.AddStatement(new AssignmentStmt(new VariableNode("b"),
                new PlusNode(new VariableNode("a"), new LiteralNode(3))));

            block.Accept(visitor, block.SymbolTable);

            Assert.Equal(5, block.SymbolTable["a"]);
            Assert.Equal(8.0, Convert.ToDouble(block.SymbolTable["b"]));
        }

        [Fact(DisplayName = "ReturnStmt evaluates and returns its expression")]
        public void ReturnStmt_ReturnsValue()
        {
            var visitor = CreateVisitor();
            var table = CreateGlobalScope();

            var returnStmt = new ReturnStmt(new LiteralNode(99));
            var result = returnStmt.Accept(visitor, table);

            Assert.Equal(99, result);
        }
    }
}
