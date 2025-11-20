using Xunit;
using System;
using AST;
using Containers;
using Optimizer;

namespace Optimizer.Tests
{
    /// <summary>
    /// Comprehensive unit test suite for CFG (Control Flow Graph) class.
    /// Tests verify that CFG extends DiGraph correctly and maintains Start property.
    /// </summary>
    public class CFGTests
    {
        #region Constructor Tests

        [Fact(DisplayName = "Default constructor should create empty CFG with null Start")]
        public void Constructor_Default_ShouldCreateEmptyCFGWithNullStart()
        {
            var cfg = new CFG();

            Assert.NotNull(cfg);
            Assert.Null(cfg.Start);
            Assert.Equal(0, cfg.VertexCount());
            Assert.Equal(0, cfg.EdgeCount());
        }

        #endregion

        #region Start Property Tests

        [Fact(DisplayName = "Start property should be settable and gettable")]
        public void Start_ShouldBeSettableAndGettable()
        {
            var cfg = new CFG();
            var stmt = new AssignmentStmt(new VariableNode("x"), new LiteralNode(42));

            cfg.Start = stmt;

            Assert.NotNull(cfg.Start);
            Assert.Equal(stmt, cfg.Start);
        }

        [Fact(DisplayName = "Start property should allow null assignment")]
        public void Start_ShouldAllowNullAssignment()
        {
            var cfg = new CFG();
            var stmt = new AssignmentStmt(new VariableNode("x"), new LiteralNode(42));

            cfg.Start = stmt;
            Assert.NotNull(cfg.Start);

            cfg.Start = null;
            Assert.Null(cfg.Start);
        }

        [Theory(DisplayName = "Start property should work with different statement types")]
        [InlineData(typeof(AssignmentStmt))]
        [InlineData(typeof(ReturnStmt))]
        public void Start_ShouldWorkWithDifferentStatementTypes(Type stmtType)
        {
            var cfg = new CFG();
            Statement stmt;

            if (stmtType == typeof(AssignmentStmt))
            {
                stmt = new AssignmentStmt(new VariableNode("x"), new LiteralNode(42));
            }
            else
            {
                stmt = new ReturnStmt(new LiteralNode(42));
            }

            cfg.Start = stmt;
            Assert.Equal(stmt, cfg.Start);
        }

        #endregion

        #region Inheritance Tests (CFG extends DiGraph)

        [Fact(DisplayName = "CFG should inherit AddVertex from DiGraph")]
        public void CFG_ShouldInheritAddVertex()
        {
            var cfg = new CFG();
            var stmt = new AssignmentStmt(new VariableNode("x"), new LiteralNode(42));

            Assert.True(cfg.AddVertex(stmt));
            Assert.Equal(1, cfg.VertexCount());
            Assert.Contains(stmt, cfg.GetVertices());
        }

        [Fact(DisplayName = "CFG should inherit AddEdge from DiGraph")]
        public void CFG_ShouldInheritAddEdge()
        {
            var cfg = new CFG();
            var stmt1 = new AssignmentStmt(new VariableNode("x"), new LiteralNode(42));
            var stmt2 = new AssignmentStmt(new VariableNode("y"), new LiteralNode(10));

            cfg.AddVertex(stmt1);
            cfg.AddVertex(stmt2);

            Assert.True(cfg.AddEdge(stmt1, stmt2));
            Assert.True(cfg.HasEdge(stmt1, stmt2));
            Assert.Equal(1, cfg.EdgeCount());
        }

        [Fact(DisplayName = "CFG should inherit RemoveVertex from DiGraph")]
        public void CFG_ShouldInheritRemoveVertex()
        {
            var cfg = new CFG();
            var stmt = new AssignmentStmt(new VariableNode("x"), new LiteralNode(42));

            cfg.AddVertex(stmt);
            Assert.True(cfg.RemoveVertex(stmt));
            Assert.Equal(0, cfg.VertexCount());
        }

        [Fact(DisplayName = "CFG should inherit RemoveEdge from DiGraph")]
        public void CFG_ShouldInheritRemoveEdge()
        {
            var cfg = new CFG();
            var stmt1 = new AssignmentStmt(new VariableNode("x"), new LiteralNode(42));
            var stmt2 = new AssignmentStmt(new VariableNode("y"), new LiteralNode(10));

            cfg.AddVertex(stmt1);
            cfg.AddVertex(stmt2);
            cfg.AddEdge(stmt1, stmt2);

            Assert.True(cfg.RemoveEdge(stmt1, stmt2));
            Assert.False(cfg.HasEdge(stmt1, stmt2));
        }

        [Fact(DisplayName = "CFG should inherit HasEdge from DiGraph")]
        public void CFG_ShouldInheritHasEdge()
        {
            var cfg = new CFG();
            var stmt1 = new AssignmentStmt(new VariableNode("x"), new LiteralNode(42));
            var stmt2 = new AssignmentStmt(new VariableNode("y"), new LiteralNode(10));

            cfg.AddVertex(stmt1);
            cfg.AddVertex(stmt2);
            cfg.AddEdge(stmt1, stmt2);

            Assert.True(cfg.HasEdge(stmt1, stmt2));
            Assert.False(cfg.HasEdge(stmt2, stmt1));
        }

        [Fact(DisplayName = "CFG should inherit GetNeighbors from DiGraph")]
        public void CFG_ShouldInheritGetNeighbors()
        {
            var cfg = new CFG();
            var stmt1 = new AssignmentStmt(new VariableNode("x"), new LiteralNode(42));
            var stmt2 = new AssignmentStmt(new VariableNode("y"), new LiteralNode(10));
            var stmt3 = new AssignmentStmt(new VariableNode("z"), new LiteralNode(20));

            cfg.AddVertex(stmt1);
            cfg.AddVertex(stmt2);
            cfg.AddVertex(stmt3);
            cfg.AddEdge(stmt1, stmt2);
            cfg.AddEdge(stmt1, stmt3);

            var neighbors = cfg.GetNeighbors(stmt1);
            Assert.Equal(2, neighbors.Count);
            Assert.Contains(stmt2, neighbors);
            Assert.Contains(stmt3, neighbors);
        }

        [Fact(DisplayName = "CFG should inherit GetVertices from DiGraph")]
        public void CFG_ShouldInheritGetVertices()
        {
            var cfg = new CFG();
            var stmt1 = new AssignmentStmt(new VariableNode("x"), new LiteralNode(42));
            var stmt2 = new AssignmentStmt(new VariableNode("y"), new LiteralNode(10));

            cfg.AddVertex(stmt1);
            cfg.AddVertex(stmt2);

            var vertices = cfg.GetVertices().ToList();
            Assert.Equal(2, vertices.Count);
            Assert.Contains(stmt1, vertices);
            Assert.Contains(stmt2, vertices);
        }

        [Fact(DisplayName = "CFG should inherit VertexCount from DiGraph")]
        public void CFG_ShouldInheritVertexCount()
        {
            var cfg = new CFG();
            var stmt = new AssignmentStmt(new VariableNode("x"), new LiteralNode(42));

            Assert.Equal(0, cfg.VertexCount());
            cfg.AddVertex(stmt);
            Assert.Equal(1, cfg.VertexCount());
        }

        [Fact(DisplayName = "CFG should inherit EdgeCount from DiGraph")]
        public void CFG_ShouldInheritEdgeCount()
        {
            var cfg = new CFG();
            var stmt1 = new AssignmentStmt(new VariableNode("x"), new LiteralNode(42));
            var stmt2 = new AssignmentStmt(new VariableNode("y"), new LiteralNode(10));

            cfg.AddVertex(stmt1);
            cfg.AddVertex(stmt2);
            cfg.AddEdge(stmt1, stmt2);

            Assert.Equal(1, cfg.EdgeCount());
        }

        #endregion

        #region Integration Tests

        [Fact(DisplayName = "CFG should maintain Start property independently of vertices")]
        public void CFG_ShouldMaintainStartIndependently()
        {
            var cfg = new CFG();
            var startStmt = new AssignmentStmt(new VariableNode("x"), new LiteralNode(42));
            var otherStmt = new AssignmentStmt(new VariableNode("y"), new LiteralNode(10));

            cfg.Start = startStmt;
            cfg.AddVertex(otherStmt);

            Assert.Equal(startStmt, cfg.Start);
            Assert.Contains(otherStmt, cfg.GetVertices());
            Assert.DoesNotContain(startStmt, cfg.GetVertices()); // Start is separate
        }

        [Fact(DisplayName = "CFG should allow Start to be a vertex in the graph")]
        public void CFG_ShouldAllowStartToBeVertex()
        {
            var cfg = new CFG();
            var stmt = new AssignmentStmt(new VariableNode("x"), new LiteralNode(42));

            cfg.Start = stmt;
            cfg.AddVertex(stmt);

            Assert.Equal(stmt, cfg.Start);
            Assert.Contains(stmt, cfg.GetVertices());
        }

        [Fact(DisplayName = "CFG should handle complex control flow scenarios")]
        public void CFG_ShouldHandleComplexControlFlow()
        {
            var cfg = new CFG();
            var stmt1 = new AssignmentStmt(new VariableNode("x"), new LiteralNode(1));
            var stmt2 = new AssignmentStmt(new VariableNode("y"), new LiteralNode(2));
            var stmt3 = new ReturnStmt(new LiteralNode(42));

            cfg.Start = stmt1;
            cfg.AddVertex(stmt1);
            cfg.AddVertex(stmt2);
            cfg.AddVertex(stmt3);
            cfg.AddEdge(stmt1, stmt2);
            cfg.AddEdge(stmt2, stmt3);

            Assert.Equal(stmt1, cfg.Start);
            Assert.Equal(3, cfg.VertexCount());
            Assert.Equal(2, cfg.EdgeCount());
            Assert.True(cfg.HasEdge(stmt1, stmt2));
            Assert.True(cfg.HasEdge(stmt2, stmt3));
        }

        #endregion

        #region Polymorphism Tests

        [Fact(DisplayName = "CFG should be usable as DiGraph polymorphically")]
        public void CFG_ShouldBeUsableAsDiGraph()
        {
            CFG cfg = new CFG();
            DiGraph<Statement> graph = cfg; // Polymorphism

            var stmt = new AssignmentStmt(new VariableNode("x"), new LiteralNode(42));
            graph.AddVertex(stmt);

            Assert.Equal(1, graph.VertexCount());
            Assert.Equal(1, cfg.VertexCount());
        }

        #endregion

        #region Start Property Edge Cases

        [Fact(DisplayName = "Start property should work with BlockStmt (though not recommended)")]
        public void Start_ShouldWorkWithBlockStmt()
        {
            var cfg = new CFG();
            var block = new BlockStmt(new List<Statement>());

            cfg.Start = block;

            Assert.Equal(block, cfg.Start);
        }

        #endregion
    }
}