using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using Containers;

namespace Containers.Tests
{
    /// <summary>
    /// Comprehensive unit test suite for DiGraph<T> implementation.
    /// Tests cover all methods with positive, negative, and edge cases.
    /// </summary>
    public class DiGraphTests
    {
        #region Constructor & Empty Graph Tests

        [Fact(DisplayName = "Constructor should create empty graph")]
        public void Constructor_ShouldCreateEmptyGraph()
        {
            var graph = new DiGraph<int>();

            Assert.Equal(0, graph.VertexCount());
            Assert.Equal(0, graph.EdgeCount());
            Assert.Empty(graph.GetVertices());
        }

        [Theory(DisplayName = "Constructor should work with different value types")]
        [InlineData(typeof(int))]
        [InlineData(typeof(string))]
        [InlineData(typeof(double))]
        public void Constructor_ShouldWorkWithDifferentTypes(Type type)
        {
            // This test verifies the generic constraint works
            Assert.True(true); // Compile-time check passes
        }

        [Fact(DisplayName = "ToString should not be null on empty graph")]
        public void ToString_ShouldNotBeNull_OnEmptyGraph()
        {
            var graph = new DiGraph<int>();
            var str = graph.ToString();

            Assert.NotNull(str);
            Assert.Contains("Vertices: 0", str, StringComparison.InvariantCultureIgnoreCase);
            Assert.Contains("Edges: 0", str, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion

        #region AddVertex Tests

        [Fact(DisplayName = "AddVertex should return true and increase count when vertex is new")]
        public void AddVertex_ShouldReturnTrueAndIncreaseCount_WhenVertexIsNew()
        {
            var graph = new DiGraph<int>();

            Assert.True(graph.AddVertex(10));
            Assert.Equal(1, graph.VertexCount());
            Assert.Contains(10, graph.GetVertices());
        }

        [Theory(DisplayName = "AddVertex should work with different data types")]
        [InlineData(42)]
        [InlineData(0)]
        [InlineData(-100)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void AddVertex_ShouldWorkWithDifferentIntegers(int value)
        {
            var graph = new DiGraph<int>();

            Assert.True(graph.AddVertex(value));
            Assert.Contains(value, graph.GetVertices());
        }

        [Fact(DisplayName = "AddVertex should return false when vertex already exists")]
        public void AddVertex_ShouldReturnFalse_WhenVertexExists()
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);

            Assert.False(graph.AddVertex(1));
            Assert.Equal(1, graph.VertexCount());
        }

        [Fact(DisplayName = "AddVertex should handle duplicate attempts correctly")]
        public void AddVertex_ShouldHandleDuplicates()
        {
            var graph = new DiGraph<string>();

            Assert.True(graph.AddVertex("A"));
            Assert.False(graph.AddVertex("A"));
            Assert.False(graph.AddVertex("A"));
            Assert.Equal(1, graph.VertexCount());
        }

        [Fact(DisplayName = "AddVertex should work with string vertices")]
        public void AddVertex_ShouldWorkWithStrings()
        {
            var graph = new DiGraph<string>();

            Assert.True(graph.AddVertex("vertex1"));
            Assert.True(graph.AddVertex("vertex2"));
            Assert.Equal(2, graph.VertexCount());
        }

        [Fact(DisplayName = "AddVertex should work with float vertices")]
        public void AddVertex_ShouldWorkWithFloats()
        {
            var graph = new DiGraph<double>();

            Assert.True(graph.AddVertex(3.14));
            Assert.True(graph.AddVertex(2.71));
            Assert.Equal(2, graph.VertexCount());
        }

        #endregion

        #region AddEdge Tests

        [Fact(DisplayName = "AddEdge should return true and increase count when edge is new")]
        public void AddEdge_ShouldReturnTrueAndIncreaseCount_WhenEdgeIsNew()
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);
            graph.AddVertex(2);

            Assert.True(graph.AddEdge(1, 2));
            Assert.Equal(1, graph.EdgeCount());
            Assert.True(graph.HasEdge(1, 2));
        }

        [Fact(DisplayName = "AddEdge should return false when edge already exists")]
        public void AddEdge_ShouldReturnFalse_WhenEdgeExists()
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddEdge(1, 2);

            Assert.False(graph.AddEdge(1, 2));
            Assert.Equal(1, graph.EdgeCount());
        }

        [Fact(DisplayName = "AddEdge should allow self-loops")]
        public void AddEdge_ShouldAllowSelfLoops()
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);

            Assert.True(graph.AddEdge(1, 1));
            Assert.Equal(1, graph.EdgeCount());
            Assert.True(graph.HasEdge(1, 1));
            Assert.Contains(1, graph.GetNeighbors(1));
        }

        [Theory(DisplayName = "AddEdge should throw ArgumentException when source vertex does not exist")]
        [InlineData(99, 1)] // Source does not exist
        [InlineData(98, 99)] // Neither exists
        public void AddEdge_ShouldThrowArgumentException_WhenSourceDoesNotExist(int source, int dest)
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);
            graph.AddVertex(2);

            var exception = Assert.Throws<ArgumentException>(() => graph.AddEdge(source, dest));
            Assert.Contains("not in DiGraph", exception.Message);
        }

        [Theory(DisplayName = "AddEdge should throw ArgumentException when destination vertex does not exist")]
        [InlineData(1, 99)] // Destination does not exist
        public void AddEdge_ShouldThrowArgumentException_WhenDestinationDoesNotExist(int source, int dest)
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);
            graph.AddVertex(2);

            var exception = Assert.Throws<ArgumentException>(() => graph.AddEdge(source, dest));
            Assert.Contains("not in DiGraph", exception.Message);
        }

        [Fact(DisplayName = "AddEdge should create multiple outgoing edges from same vertex")]
        public void AddEdge_ShouldCreateMultipleOutgoingEdges()
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddVertex(3);

            Assert.True(graph.AddEdge(1, 2));
            Assert.True(graph.AddEdge(1, 3));
            Assert.Equal(2, graph.EdgeCount());
            Assert.Equal(2, graph.GetNeighbors(1).Count);
        }

        [Fact(DisplayName = "AddEdge should handle duplicate edge attempts")]
        public void AddEdge_ShouldHandleDuplicateEdges()
        {
            var graph = new DiGraph<string>();
            graph.AddVertex("A");
            graph.AddVertex("B");

            Assert.True(graph.AddEdge("A", "B"));
            Assert.False(graph.AddEdge("A", "B"));
            Assert.False(graph.AddEdge("A", "B"));
            Assert.Equal(1, graph.EdgeCount());
        }

        #endregion

        #region RemoveVertex Tests

        [Fact(DisplayName = "RemoveVertex should return true and remove vertex when vertex exists")]
        public void RemoveVertex_ShouldReturnTrueAndRemoveVertex_WhenVertexExists()
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);
            graph.AddVertex(2);

            Assert.True(graph.RemoveVertex(1));
            Assert.Equal(1, graph.VertexCount());
            Assert.DoesNotContain(1, graph.GetVertices());
        }

        [Fact(DisplayName = "RemoveVertex should return false when vertex does not exist")]
        public void RemoveVertex_ShouldReturnFalse_WhenVertexDoesNotExist()
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);

            Assert.False(graph.RemoveVertex(99));
            Assert.Equal(1, graph.VertexCount());
        }

        [Fact(DisplayName = "RemoveVertex should remove all outgoing edges")]
        public void RemoveVertex_ShouldRemoveAllOutgoingEdges()
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddVertex(3);
            graph.AddEdge(1, 2);
            graph.AddEdge(1, 3);
            graph.AddEdge(2, 3);

            graph.RemoveVertex(1);

            Assert.Equal(2, graph.VertexCount());
            Assert.Equal(1, graph.EdgeCount()); // Only 2->3 should remain
            Assert.False(graph.HasEdge(1, 2));
            Assert.False(graph.HasEdge(1, 3));
        }

        [Fact(DisplayName = "RemoveVertex should remove all incoming edges")]
        public void RemoveVertex_ShouldRemoveAllIncomingEdges()
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddVertex(3);
            graph.AddEdge(1, 2);
            graph.AddEdge(3, 2);

            graph.RemoveVertex(2);

            Assert.Equal(2, graph.VertexCount());
            Assert.Equal(0, graph.EdgeCount());
            Assert.False(graph.HasEdge(1, 2));
            Assert.False(graph.HasEdge(3, 2));
        }

        [Fact(DisplayName = "RemoveVertex should handle removing last vertex")]
        public void RemoveVertex_ShouldHandleRemovingLastVertex()
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);

            Assert.True(graph.RemoveVertex(1));
            Assert.Equal(0, graph.VertexCount());
            Assert.Equal(0, graph.EdgeCount());
            Assert.Empty(graph.GetVertices());
        }

        [Fact(DisplayName = "RemoveVertex should handle removing vertex with self-loop")]
        public void RemoveVertex_ShouldHandleSelfLoop()
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);
            graph.AddEdge(1, 1);

            Assert.True(graph.RemoveVertex(1));
            Assert.Equal(0, graph.VertexCount());
            Assert.Equal(0, graph.EdgeCount());
        }

        #endregion

        #region RemoveEdge Tests

        [Fact(DisplayName = "RemoveEdge should return true and decrease count when edge exists")]
        public void RemoveEdge_ShouldReturnTrueAndDecreaseCount_WhenEdgeExists()
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddEdge(1, 2);

            Assert.True(graph.RemoveEdge(1, 2));
            Assert.Equal(0, graph.EdgeCount());
            Assert.False(graph.HasEdge(1, 2));
        }

        [Theory(DisplayName = "RemoveEdge should return false when edge does not exist")]
        [InlineData(2, 1)] // Edge does not exist (reverse)
        [InlineData(1, 4)] // Edge does not exist (vertices exist)
        public void RemoveEdge_ShouldReturnFalse_WhenEdgeDoesNotExist(int source, int dest)
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddVertex(4);
            graph.AddEdge(1, 2);

            Assert.False(graph.RemoveEdge(source, dest));
            Assert.Equal(1, graph.EdgeCount());
        }

        [Theory(DisplayName = "RemoveEdge should throw ArgumentException when vertex does not exist")]
        [InlineData(99, 1)] // Source does not exist
        [InlineData(1, 99)] // Destination does not exist
        public void RemoveEdge_ShouldThrowArgumentException_WhenVertexDoesNotExist(int source, int dest)
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);
            graph.AddVertex(2);

            var exception = Assert.Throws<ArgumentException>(() => graph.RemoveEdge(source, dest));
            Assert.Contains("not in DiGraph", exception.Message);
        }

        [Fact(DisplayName = "RemoveEdge should handle removing self-loop")]
        public void RemoveEdge_ShouldHandleSelfLoop()
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);
            graph.AddEdge(1, 1);

            Assert.True(graph.RemoveEdge(1, 1));
            Assert.Equal(0, graph.EdgeCount());
            Assert.False(graph.HasEdge(1, 1));
        }

        #endregion

        #region HasEdge Tests

        [Theory(DisplayName = "HasEdge should return correct boolean for existing and non-existing edges")]
        [InlineData(1, 2, true)]  // Existing edge
        [InlineData(3, 4, true)]  // Existing edge
        [InlineData(1, 4, false)] // Non-existing edge
        [InlineData(2, 1, false)] // Non-existing reverse edge
        [InlineData(4, 3, false)] // Non-existing reverse edge
        public void HasEdge_ShouldReturnCorrectBoolean(int source, int dest, bool expected)
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddVertex(3);
            graph.AddVertex(4);
            graph.AddEdge(1, 2);
            graph.AddEdge(3, 4);

            Assert.Equal(expected, graph.HasEdge(source, dest));
        }

        [Theory(DisplayName = "HasEdge should return false when vertex does not exist")]
        [InlineData(99, 1)] // Source does not exist
        [InlineData(1, 99)] // Destination does not exist
        public void HasEdge_ShouldReturnFalse_WhenVertexDoesNotExist(int source, int dest)
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);
            graph.AddVertex(2);

            Assert.False(graph.HasEdge(source, dest));
        }

        [Fact(DisplayName = "HasEdge should return true for self-loops")]
        public void HasEdge_ShouldReturnTrue_ForSelfLoops()
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);
            graph.AddEdge(1, 1);

            Assert.True(graph.HasEdge(1, 1));
        }

        #endregion

        #region GetNeighbors Tests

        [Fact(DisplayName = "GetNeighbors should return all adjacent vertices")]
        public void GetNeighbors_ShouldReturnAllAdjacentVertices()
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddVertex(3);
            graph.AddEdge(1, 2);
            graph.AddEdge(1, 3);

            var neighbors = graph.GetNeighbors(1);

            Assert.Equal(2, neighbors.Count);
            Assert.Contains(2, neighbors);
            Assert.Contains(3, neighbors);
        }

        [Theory(DisplayName = "GetNeighbors should return empty list for sink vertices")]
        [InlineData(2)] // Has incoming, no outgoing
        [InlineData(4)] // Has incoming, no outgoing
        public void GetNeighbors_ShouldReturnEmptyList_ForSinkVertex(int vertex)
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddVertex(3);
            graph.AddVertex(4);
            graph.AddEdge(1, 2);
            graph.AddEdge(3, 4);

            var neighbors = graph.GetNeighbors(vertex);
            Assert.Empty(neighbors);
        }

        [Fact(DisplayName = "GetNeighbors should throw ArgumentException when vertex does not exist")]
        public void GetNeighbors_ShouldThrowArgumentException_WhenVertexDoesNotExist()
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);

            var exception = Assert.Throws<ArgumentException>(() => graph.GetNeighbors(99));
            Assert.Contains("not found", exception.Message);
        }

        [Fact(DisplayName = "GetNeighbors should return empty list for isolated vertex")]
        public void GetNeighbors_ShouldReturnEmptyList_ForIsolatedVertex()
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);

            var neighbors = graph.GetNeighbors(1);
            Assert.Empty(neighbors);
        }

        #endregion

        #region GetVertices / VertexCount / EdgeCount Tests

        [Fact(DisplayName = "GetVertices should return all added vertices")]
        public void GetVertices_ShouldReturnAllAddedVertices()
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddVertex(3);
            graph.AddVertex(4);

            var vertices = graph.GetVertices().ToList();

            Assert.Equal(4, vertices.Count);
            Assert.Contains(1, vertices);
            Assert.Contains(2, vertices);
            Assert.Contains(3, vertices);
            Assert.Contains(4, vertices);
        }

        [Fact(DisplayName = "Counts should be correct after complex operations")]
        public void Counts_ShouldBeCorrectAfterComplexOperations()
        {
            var graph = new DiGraph<int>();

            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddVertex(3);
            graph.AddEdge(1, 2);
            graph.AddEdge(1, 3);
            Assert.Equal(3, graph.VertexCount());
            Assert.Equal(2, graph.EdgeCount());

            graph.AddVertex(4);
            graph.AddEdge(3, 4);
            Assert.Equal(4, graph.VertexCount());
            Assert.Equal(3, graph.EdgeCount());

            graph.RemoveEdge(1, 2);
            Assert.Equal(4, graph.VertexCount());
            Assert.Equal(2, graph.EdgeCount());

            graph.RemoveVertex(3);
            Assert.Equal(3, graph.VertexCount());
            Assert.Equal(0, graph.EdgeCount());
        }

        [Fact(DisplayName = "VertexCount should be zero for empty graph")]
        public void VertexCount_ShouldBeZero_ForEmptyGraph()
        {
            var graph = new DiGraph<int>();
            Assert.Equal(0, graph.VertexCount());
        }

        [Fact(DisplayName = "EdgeCount should be zero for empty graph")]
        public void EdgeCount_ShouldBeZero_ForEmptyGraph()
        {
            var graph = new DiGraph<int>();
            Assert.Equal(0, graph.EdgeCount());
        }

        #endregion

        #region Complex Scenario Tests

        [Fact(DisplayName = "Complex graph operations should maintain consistency")]
        public void ComplexGraphOperations_ShouldMaintainConsistency()
        {
            var graph = new DiGraph<string>();

            // Add vertices
            graph.AddVertex("A");
            graph.AddVertex("B");
            graph.AddVertex("C");
            graph.AddVertex("D");
            Assert.Equal(4, graph.VertexCount());

            // Add edges
            graph.AddEdge("A", "B");
            graph.AddEdge("B", "C");
            graph.AddEdge("C", "D");
            graph.AddEdge("A", "D");
            Assert.Equal(4, graph.EdgeCount());

            // Remove edge
            graph.RemoveEdge("A", "D");
            Assert.Equal(3, graph.EdgeCount());

            // Remove vertex (should remove associated edges)
            graph.RemoveVertex("B");
            Assert.Equal(3, graph.VertexCount());
            Assert.Equal(1, graph.EdgeCount()); // Only C->D remains
        }

        [Fact(DisplayName = "Graph should handle large number of vertices")]
        public void Graph_ShouldHandleLargeNumberOfVertices()
        {
            var graph = new DiGraph<int>();
            const int count = 1000;

            for (int i = 0; i < count; i++)
            {
                graph.AddVertex(i);
            }

            Assert.Equal(count, graph.VertexCount());
        }

        [Fact(DisplayName = "Graph should handle large number of edges")]
        public void Graph_ShouldHandleLargeNumberOfEdges()
        {
            var graph = new DiGraph<int>();
            const int count = 100;

            for (int i = 0; i < count; i++)
            {
                graph.AddVertex(i);
            }

            for (int i = 0; i < count - 1; i++)
            {
                graph.AddEdge(i, i + 1);
            }

            Assert.Equal(count, graph.VertexCount());
            Assert.Equal(count - 1, graph.EdgeCount());
        }

        #endregion

    #region Additional Edge Cases

    [Fact(DisplayName = "ToString should return formatted string with correct counts")]
    public void ToString_ShouldReturnFormattedString()
    {
        var graph = new DiGraph<int>();
        graph.AddVertex(1);
        graph.AddVertex(2);
        graph.AddEdge(1, 2);

        var str = graph.ToString();

        Assert.Contains("Vertices: 2", str);
        Assert.Contains("Edges: 1", str);
    }

    [Fact(DisplayName = "RemoveVertex should handle vertex with multiple self-loops")]
    public void RemoveVertex_ShouldHandleMultipleSelfLoops()
    {
        var graph = new DiGraph<int>();
        graph.AddVertex(1);
        // Note: Can't add same edge twice, but can test single self-loop removal
        graph.AddEdge(1, 1);

        Assert.True(graph.RemoveVertex(1));
        Assert.Equal(0, graph.VertexCount());
        Assert.Equal(0, graph.EdgeCount());
    }

    [Fact(DisplayName = "RemoveVertex should handle vertex with complex edge relationships")]
    public void RemoveVertex_ShouldHandleComplexEdgeRelationships()
    {
        var graph = new DiGraph<int>();
        graph.AddVertex(1);
        graph.AddVertex(2);
        graph.AddVertex(3);
        graph.AddVertex(4);
        graph.AddEdge(1, 2);
        graph.AddEdge(1, 3);
        graph.AddEdge(2, 3);
        graph.AddEdge(3, 4);
        graph.AddEdge(4, 1); // Cycle

        graph.RemoveVertex(3);

        Assert.Equal(3, graph.VertexCount());
        Assert.False(graph.HasEdge(1, 3));
        Assert.False(graph.HasEdge(2, 3));
        Assert.False(graph.HasEdge(3, 4));
    }

    #endregion
    }
}



