using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using AST;
using Containers;

namespace AST.Tests
{
    /// <summary>
    /// Comprehensive unit test suite for ControlFlowGraphGeneratorVisitor.
    /// Tests verify CFG generation from AST traversal with all statement types.
    /// </summary>
    public class ControlFlowGraphGeneratorVisitorTests
    {
        #region Helper Methods

        private AssignmentStmt CreateAssignment(string varName, int value)
        {
            return new AssignmentStmt(
                new VariableNode(varName),
                new LiteralNode(value)
            );
        }

        private ReturnStmt CreateReturn(int value)
        {
            return new ReturnStmt(new LiteralNode(value));
        }

        private BlockStmt CreateBlock(params Statement[] statements)
        {
            return new BlockStmt(new List<Statement>(statements));
        }

        #endregion

        #region AssignmentStmt Visit Tests

        [Fact(DisplayName = "Visit AssignmentStmt should add vertex and edge when prev is not null")]
        public void Visit_AssignmentStmt_ShouldAddVertexAndEdge_WhenPrevNotNull()
        {
            var start = CreateAssignment("x", 1);
            var stmt = CreateAssignment("y", 2);
            var visitor = new ControlFlowGraphGeneratorVisitor();
            visitor.Visit(start, null);

            var result = visitor.Visit(stmt, start);
            var cfg = visitor.GetCFG();

            Assert.Equal(stmt, result);
            Assert.Equal(2, cfg.VertexCount());
            Assert.True(cfg.HasEdge(start, stmt));
        }

        [Fact(DisplayName = "Visit AssignmentStmt should add vertex but not edge when prev is null")]
        public void Visit_AssignmentStmt_ShouldAddVertexButNotEdge_WhenPrevNull()
        {
            var start = CreateAssignment("x", 1);
            var stmt = CreateAssignment("y", 2);
            var visitor = new ControlFlowGraphGeneratorVisitor();
            visitor.Visit(start, null);

            var result = visitor.Visit(stmt, null!);
            var cfg = visitor.GetCFG();

            Assert.Equal(stmt, result);
            Assert.Equal(2, cfg.VertexCount());
            // Both vertices exist but no edge between them
            Assert.Equal(0, cfg.EdgeCount());
        }

        [Fact(DisplayName = "Visit AssignmentStmt should return the statement itself")]
        public void Visit_AssignmentStmt_ShouldReturnStatementItself()
        {
            var start = CreateAssignment("x", 1);
            var stmt = CreateAssignment("y", 2);
            var visitor = new ControlFlowGraphGeneratorVisitor();
            visitor.Visit(start, null);

            var result = visitor.Visit(stmt, start);

            Assert.Equal(stmt, result);
            Assert.Same(stmt, result);
        }

        [Fact(DisplayName = "Visit multiple AssignmentStmts should create sequential flow")]
        public void Visit_MultipleAssignmentStmts_ShouldCreateSequentialFlow()
        {
            var stmt1 = CreateAssignment("x", 1);
            var stmt2 = CreateAssignment("y", 2);
            var stmt3 = CreateAssignment("z", 3);
            var visitor = new ControlFlowGraphGeneratorVisitor();
            visitor.Visit(stmt1, null);

            visitor.Visit(stmt2, stmt1);
            visitor.Visit(stmt3, stmt2);
            var cfg = visitor.GetCFG();

            Assert.Equal(3, cfg.VertexCount());
            Assert.Equal(2, cfg.EdgeCount());
            Assert.True(cfg.HasEdge(stmt1, stmt2));
            Assert.True(cfg.HasEdge(stmt2, stmt3));
        }

        #endregion

        #region ReturnStmt Visit Tests

        [Fact(DisplayName = "Visit ReturnStmt should add vertex and edge when prev is not null")]
        public void Visit_ReturnStmt_ShouldAddVertexAndEdge_WhenPrevNotNull()
        {
            var start = CreateAssignment("x", 1);
            var returnStmt = CreateReturn(42);
            var visitor = new ControlFlowGraphGeneratorVisitor();
            visitor.Visit(start, null);

            var result = visitor.Visit(returnStmt, start);
            var cfg = visitor.GetCFG();

            Assert.Equal(returnStmt, result);
            Assert.Equal(2, cfg.VertexCount());
            Assert.True(cfg.HasEdge(start, returnStmt));
        }

        [Fact(DisplayName = "Visit ReturnStmt should add vertex but not edge when prev is null")]
        public void Visit_ReturnStmt_ShouldAddVertexButNotEdge_WhenPrevNull()
        {
            var start = CreateAssignment("x", 1);
            var returnStmt = CreateReturn(42);
            var visitor = new ControlFlowGraphGeneratorVisitor();
            visitor.Visit(start, null);

            var result = visitor.Visit(returnStmt, null!);
            var cfg = visitor.GetCFG();

            Assert.Equal(returnStmt, result);
            Assert.Equal(2, cfg.VertexCount());
        }

        [Fact(DisplayName = "Visit ReturnStmt should return the statement itself")]
        public void Visit_ReturnStmt_ShouldReturnStatementItself()
        {
            var start = CreateAssignment("x", 1);
            var returnStmt = CreateReturn(42);
            var visitor = new ControlFlowGraphGeneratorVisitor();
            visitor.Visit(start, null);

            var result = visitor.Visit(returnStmt, start);

            Assert.Equal(returnStmt, result);
            Assert.Same(returnStmt, result);
        }

        [Fact(DisplayName = "ReturnStmt should have no outgoing edges")]
        public void ReturnStmt_ShouldHaveNoOutgoingEdges()
        {
            var start = CreateAssignment("x", 1);
            var returnStmt = CreateReturn(42);
            var visitor = new ControlFlowGraphGeneratorVisitor();
            visitor.Visit(start, null);

            visitor.Visit(returnStmt, start);
            var cfg = visitor.GetCFG();

            var neighbors = cfg.GetNeighbors(returnStmt);
            Assert.Empty(neighbors);
        }

        #endregion

        #region Expression Node Visit Tests

        [Theory(DisplayName = "Expression nodes should return null and not add vertices")]
        [InlineData(typeof(PlusNode))]
        [InlineData(typeof(MinusNode))]
        [InlineData(typeof(TimesNode))]
        [InlineData(typeof(FloatDivNode))]
        [InlineData(typeof(IntDivNode))]
        [InlineData(typeof(ModulusNode))]
        [InlineData(typeof(ExponentiationNode))]
        public void Visit_ExpressionNodes_ShouldReturnNull(Type nodeType)
        {
            var start = CreateAssignment("x", 1);
            var left = new LiteralNode(1);
            var right = new LiteralNode(2);
            var node = (ExpressionNode)Activator.CreateInstance(nodeType, left, right)!;
            var visitor = new ControlFlowGraphGeneratorVisitor();
            visitor.Visit(start, null);

            var result = node.Accept(visitor, start);
            var cfg = visitor.GetCFG();

            Assert.Null(result);
            Assert.Equal(1, cfg.VertexCount()); // Only start
        }

        [Fact(DisplayName = "Visit LiteralNode should return null")]
        public void Visit_LiteralNode_ShouldReturnNull()
        {
            var start = CreateAssignment("x", 1);
            var literal = new LiteralNode(42);
            var visitor = new ControlFlowGraphGeneratorVisitor();
            visitor.Visit(start, null);

            var result = visitor.Visit(literal, start);
            var cfg = visitor.GetCFG();

            Assert.Null(result);
            Assert.Equal(1, cfg.VertexCount());
        }

        [Fact(DisplayName = "Visit VariableNode should return null")]
        public void Visit_VariableNode_ShouldReturnNull()
        {
            var start = CreateAssignment("x", 1);
            var variable = new VariableNode("y");
            var visitor = new ControlFlowGraphGeneratorVisitor();
            visitor.Visit(start, null);

            var result = visitor.Visit(variable, start);
            var cfg = visitor.GetCFG();

            Assert.Null(result);
            Assert.Equal(1, cfg.VertexCount());
        }

        #endregion

        #region BlockStmt Visit Tests

        [Fact(DisplayName = "Visit empty BlockStmt should return prev statement")]
        public void Visit_EmptyBlockStmt_ShouldReturnPrevStatement()
        {
            var start = CreateAssignment("x", 1);
            var block = CreateBlock();
            var visitor = new ControlFlowGraphGeneratorVisitor();
            visitor.Visit(start, null);

            var result = visitor.Visit(block, start);
            var cfg = visitor.GetCFG();

            Assert.Equal(start, result);
            Assert.Equal(1, cfg.VertexCount());
        }

        [Fact(DisplayName = "Visit BlockStmt with single statement should add vertex and edge")]
        public void Visit_BlockStmtWithSingleStatement_ShouldAddVertexAndEdge()
        {
            var start = CreateAssignment("x", 1);
            var innerStmt = CreateAssignment("y", 2);
            var block = CreateBlock(innerStmt);
            var visitor = new ControlFlowGraphGeneratorVisitor();
            visitor.Visit(start, null);

            var result = visitor.Visit(block, start);
            var cfg = visitor.GetCFG();

            Assert.Equal(innerStmt, result);
            Assert.Equal(2, cfg.VertexCount());
            Assert.True(cfg.HasEdge(start, innerStmt));
        }

        [Fact(DisplayName = "Visit BlockStmt with multiple statements should create sequential flow")]
        public void Visit_BlockStmtWithMultipleStatements_ShouldCreateSequentialFlow()
        {
            var start = CreateAssignment("x", 1);
            var stmt1 = CreateAssignment("a", 10);
            var stmt2 = CreateAssignment("b", 20);
            var stmt3 = CreateAssignment("c", 30);
            var block = CreateBlock(stmt1, stmt2, stmt3);
            var visitor = new ControlFlowGraphGeneratorVisitor();
            visitor.Visit(start, null);

            var result = visitor.Visit(block, start);
            var cfg = visitor.GetCFG();

            Assert.Equal(stmt3, result);
            Assert.Equal(4, cfg.VertexCount());
            Assert.True(cfg.HasEdge(start, stmt1));
            Assert.True(cfg.HasEdge(stmt1, stmt2));
            Assert.True(cfg.HasEdge(stmt2, stmt3));
        }

        [Fact(DisplayName = "Visit BlockStmt with return should stop flow - unreachable code is disjoint")]
        public void Visit_BlockStmtWithReturn_ShouldStopFlowAfterReturn()
        {
            var start = CreateAssignment("x", 1);
            var stmt1 = CreateAssignment("a", 10);
            var returnStmt = CreateReturn(42);
            var stmt2 = CreateAssignment("b", 20); // Unreachable
            var block = CreateBlock(stmt1, returnStmt, stmt2);
            var visitor = new ControlFlowGraphGeneratorVisitor();
            visitor.Visit(start, null);

            var result = visitor.Visit(block, start);
            var cfg = visitor.GetCFG();

            // Block returns the return statement (last reachable statement)
            Assert.Equal(returnStmt, result);
            Assert.True(cfg.HasEdge(start, stmt1));
            Assert.True(cfg.HasEdge(stmt1, returnStmt));
            // stmt2 should be added as vertex but NO edge from return (disjoint)
            Assert.Contains(stmt2, cfg.GetVertices());
            Assert.False(cfg.HasEdge(returnStmt, stmt2));
            // Return has no outgoing edges
            Assert.Empty(cfg.GetNeighbors(returnStmt));
        }

        [Fact(DisplayName = "Visit nested BlockStmt should create correct flow")]
        public void Visit_NestedBlockStmt_ShouldCreateCorrectFlow()
        {
            var start = CreateAssignment("x", 1);
            var stmt1 = CreateAssignment("a", 10);
            var stmt2 = CreateAssignment("b", 20);
            var innerBlock = CreateBlock(stmt2);
            var outerBlock = CreateBlock(stmt1, innerBlock);
            var visitor = new ControlFlowGraphGeneratorVisitor();
            visitor.Visit(start, null);

            var result = visitor.Visit(outerBlock, start);
            var cfg = visitor.GetCFG();

            Assert.Equal(stmt2, result);
            Assert.Equal(3, cfg.VertexCount());
            Assert.True(cfg.HasEdge(start, stmt1));
            Assert.True(cfg.HasEdge(stmt1, stmt2));
        }

        [Fact(DisplayName = "Visit deeply nested BlockStmt should create correct flow")]
        public void Visit_DeeplyNestedBlockStmt_ShouldCreateCorrectFlow()
        {
            var start = CreateAssignment("x", 1);
            var stmt1 = CreateAssignment("a", 10);
            var stmt2 = CreateAssignment("b", 20);
            var stmt3 = CreateAssignment("c", 30);

            var innerMost = CreateBlock(stmt3);
            var middle = CreateBlock(stmt2, innerMost);
            var outer = CreateBlock(stmt1, middle);
            var visitor = new ControlFlowGraphGeneratorVisitor();
            visitor.Visit(start, null);

            var result = visitor.Visit(outer, start);
            var cfg = visitor.GetCFG();

            Assert.Equal(stmt3, result);
            Assert.Equal(4, cfg.VertexCount());
            Assert.True(cfg.HasEdge(start, stmt1));
            Assert.True(cfg.HasEdge(stmt1, stmt2));
            Assert.True(cfg.HasEdge(stmt2, stmt3));
        }

        [Fact(DisplayName = "Visit BlockStmt with null prev should handle correctly")]
        public void Visit_BlockStmtWithNullPrev_ShouldHandleCorrectly()
        {
            var start = CreateAssignment("x", 1);
            var stmt = CreateAssignment("y", 2);
            var block = CreateBlock(stmt);
            var visitor = new ControlFlowGraphGeneratorVisitor();
            visitor.Visit(start, null);

            var result = visitor.Visit(block, null!);
            var cfg = visitor.GetCFG();

            Assert.Equal(stmt, result);
            Assert.Equal(2, cfg.VertexCount());
            // No edge from null to stmt
        }

        #endregion

        #region Complex Flow Tests

        [Fact(DisplayName = "Visit complex nested structure should build correct CFG")]
        public void Visit_ComplexNestedStructure_ShouldBuildCorrectCFG()
        {
            var start = CreateAssignment("init", 0);
            var stmt1 = CreateAssignment("a", 1);
            var stmt2 = CreateAssignment("b", 2);
            var stmt3 = CreateAssignment("c", 3);
            var stmt4 = CreateAssignment("d", 4);
            var returnStmt = CreateReturn(100);

            var innerBlock = CreateBlock(stmt3, stmt4);
            var outerBlock = CreateBlock(stmt1, stmt2, innerBlock, returnStmt);
            var visitor = new ControlFlowGraphGeneratorVisitor();
            visitor.Visit(start, null);

            var result = visitor.Visit(outerBlock, start);
            var cfg = visitor.GetCFG();

            Assert.Equal(returnStmt, result);
            Assert.Equal(6, cfg.VertexCount());
            Assert.True(cfg.HasEdge(start, stmt1));
            Assert.True(cfg.HasEdge(stmt1, stmt2));
            Assert.True(cfg.HasEdge(stmt2, stmt3));
            Assert.True(cfg.HasEdge(stmt3, stmt4));
            Assert.True(cfg.HasEdge(stmt4, returnStmt));
        }

        [Fact(DisplayName = "Visit mixed statements and blocks should maintain correct order")]
        public void Visit_MixedStatementsAndBlocks_ShouldMaintainCorrectOrder()
        {
            var start = CreateAssignment("start", 0);
            var stmt1 = CreateAssignment("before", 1);
            var blockStmt1 = CreateAssignment("in_block_1", 2);
            var blockStmt2 = CreateAssignment("in_block_2", 3);
            var stmt2 = CreateAssignment("after", 4);

            var block = CreateBlock(blockStmt1, blockStmt2);
            var visitor = new ControlFlowGraphGeneratorVisitor();
            visitor.Visit(start, null);

            visitor.Visit(stmt1, start);
            visitor.Visit(block, stmt1);
            visitor.Visit(stmt2, blockStmt2);

            var cfg = visitor.GetCFG();

            Assert.Equal(5, cfg.VertexCount());
            Assert.True(cfg.HasEdge(start, stmt1));
            Assert.True(cfg.HasEdge(stmt1, blockStmt1));
            Assert.True(cfg.HasEdge(blockStmt1, blockStmt2));
            Assert.True(cfg.HasEdge(blockStmt2, stmt2));
        }

        [Fact(DisplayName = "Visit large sequence should handle correctly")]
        public void Visit_LargeSequence_ShouldHandleCorrectly()
        {
            var statements = new List<AssignmentStmt>();
            for (int i = 0; i < 100; i++)
            {
                statements.Add(CreateAssignment($"var{i}", i));
            }

            var visitor = new ControlFlowGraphGeneratorVisitor();
            visitor.Visit(statements[0], null);

            for (int i = 1; i < 100; i++)
            {
                visitor.Visit(statements[i], statements[i - 1]);
            }

            var cfg = visitor.GetCFG();

            Assert.Equal(100, cfg.VertexCount());
            Assert.Equal(99, cfg.EdgeCount());
        }

        #endregion

        #region Edge Cases

        [Fact(DisplayName = "Visit same statement twice should not create duplicate vertex")]
        public void Visit_SameStatementTwice_ShouldNotCreateDuplicateVertex()
        {
            var start = CreateAssignment("x", 1);
            var stmt = CreateAssignment("y", 2);
            var visitor = new ControlFlowGraphGeneratorVisitor();
            visitor.Visit(start, null);

            visitor.Visit(stmt, start);
            visitor.Visit(stmt, start);

            var cfg = visitor.GetCFG();
            Assert.Equal(2, cfg.VertexCount());
        }

        [Fact(DisplayName = "Visit block with only return should add only return")]
        public void Visit_BlockWithOnlyReturn_ShouldAddOnlyReturn()
        {
            var start = CreateAssignment("x", 1);
            var returnStmt = CreateReturn(42);
            var block = CreateBlock(returnStmt);
            var visitor = new ControlFlowGraphGeneratorVisitor();
            visitor.Visit(start, null);

            var result = visitor.Visit(block, start);
            var cfg = visitor.GetCFG();

            Assert.Equal(returnStmt, result);
            Assert.Equal(2, cfg.VertexCount());
            Assert.True(cfg.HasEdge(start, returnStmt));
        }

        [Fact(DisplayName = "GetCFG should return non-null graph")]
        public void GetCFG_ShouldReturnNonNullGraph()
        {
            var start = CreateAssignment("x", 1);
            var visitor = new ControlFlowGraphGeneratorVisitor();
            visitor.Visit(start, null);

            var cfg = visitor.GetCFG();

            Assert.NotNull(cfg);
        }

        [Fact(DisplayName = "All vertices should be reachable from start")]
        public void AllVertices_ShouldBeReachableFromStart()
        {
            var start = CreateAssignment("x", 1);
            var stmt1 = CreateAssignment("a", 10);
            var stmt2 = CreateAssignment("b", 20);
            var stmt3 = CreateAssignment("c", 30);
            var visitor = new ControlFlowGraphGeneratorVisitor();
            visitor.Visit(start, null);

            visitor.Visit(stmt1, start);
            visitor.Visit(stmt2, stmt1);
            visitor.Visit(stmt3, stmt2);
            var cfg = visitor.GetCFG();

            var reachable = new HashSet<Statement>();
            var queue = new Queue<Statement>();
            queue.Enqueue(start);
            reachable.Add(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var neighbor in cfg.GetNeighbors(current))
                {
                    if (!reachable.Contains(neighbor))
                    {
                        reachable.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            Assert.Equal(cfg.VertexCount(), reachable.Count);
        }

        #endregion
    }
}



