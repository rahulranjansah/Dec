using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Containers;

namespace Containers.Tests
{
    /// <summary>
    /// Test suite for DiGraph.Transpose() method.
    /// Verifies that edge reversal produces correct transposed graphs.
    /// </summary>
    public class TransposeTests
    {
        #region Basic Transpose Tests

        [Fact(DisplayName = "Empty graph transpose produces empty graph")]
        public void EmptyGraph_TransposeProducesEmptyGraph()
        {
            var graph = new DiGraph<int>();

            var transposed = graph.Transpose();

            Assert.Equal(0, transposed.VertexCount());
            Assert.Equal(0, transposed.EdgeCount());
        }

        [Fact(DisplayName = "Single vertex no edges transpose preserves vertex")]
        public void SingleVertex_TransposePreservesVertex()
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);

            var transposed = graph.Transpose();

            Assert.Equal(1, transposed.VertexCount());
            Assert.Equal(0, transposed.EdgeCount());
            Assert.Contains(1, transposed.GetVertices());
        }

        [Fact(DisplayName = "Single edge A→B becomes B→A")]
        public void SingleEdge_TransposeReversesDirection()
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddEdge(1, 2);  // 1 → 2

            var transposed = graph.Transpose();

            // Original: 1 → 2
            // Transposed: 2 → 1
            Assert.True(transposed.HasEdge(2, 1), "Transposed should have edge 2→1");
            Assert.False(transposed.HasEdge(1, 2), "Transposed should NOT have edge 1→2");
        }

        [Fact(DisplayName = "Bidirectional edge remains bidirectional")]
        public void BidirectionalEdge_RemainsBidirectional()
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddEdge(1, 2);  // 1 → 2
            graph.AddEdge(2, 1);  // 2 → 1

            var transposed = graph.Transpose();

            // Bidirectional edges remain bidirectional after transpose
            Assert.True(transposed.HasEdge(1, 2));
            Assert.True(transposed.HasEdge(2, 1));
        }

        #endregion

        #region Chain/Linear Graph Tests

        [Fact(DisplayName = "Linear chain A→B→C becomes C→B→A")]
        public void LinearChain_TransposeReversesAllEdges()
        {
            var graph = new DiGraph<string>();
            graph.AddVertex("A");
            graph.AddVertex("B");
            graph.AddVertex("C");
            graph.AddEdge("A", "B");
            graph.AddEdge("B", "C");

            var transposed = graph.Transpose();

            // Original: A → B → C
            // Transposed: A ← B ← C (i.e., C → B → A)
            Assert.False(transposed.HasEdge("A", "B"));
            Assert.False(transposed.HasEdge("B", "C"));
            Assert.True(transposed.HasEdge("B", "A"), "Should have B→A");
            Assert.True(transposed.HasEdge("C", "B"), "Should have C→B");
        }

        [Fact(DisplayName = "Long chain transpose preserves vertex count and edge count")]
        public void LongChain_TransposePreservesCounts()
        {
            var graph = new DiGraph<int>();
            for (int i = 1; i <= 10; i++)
                graph.AddVertex(i);
            for (int i = 1; i < 10; i++)
                graph.AddEdge(i, i + 1);

            var transposed = graph.Transpose();

            Assert.Equal(10, transposed.VertexCount());
            Assert.Equal(9, transposed.EdgeCount());

            // Verify all edges are reversed
            for (int i = 1; i < 10; i++)
            {
                Assert.False(transposed.HasEdge(i, i + 1), $"Should NOT have {i}→{i + 1}");
                Assert.True(transposed.HasEdge(i + 1, i), $"Should have {i + 1}→{i}");
            }
        }

        #endregion

        #region Cycle Tests

        [Fact(DisplayName = "Simple cycle A→B→C→A remains a cycle")]
        public void SimpleCycle_TransposeRemainsCycle()
        {
            var graph = new DiGraph<string>();
            graph.AddVertex("A");
            graph.AddVertex("B");
            graph.AddVertex("C");
            graph.AddEdge("A", "B");
            graph.AddEdge("B", "C");
            graph.AddEdge("C", "A");

            var transposed = graph.Transpose();

            // Original: A → B → C → A
            // Transposed: A ← B ← C ← A (i.e., A → C → B → A)
            Assert.True(transposed.HasEdge("B", "A"));
            Assert.True(transposed.HasEdge("C", "B"));
            Assert.True(transposed.HasEdge("A", "C"));

            // Verify it's still a cycle (can traverse A → C → B → A)
            var neighborsOfA = transposed.GetNeighbors("A");
            Assert.Contains("C", neighborsOfA);
        }

        [Fact(DisplayName = "Self-loop remains self-loop")]
        public void SelfLoop_RemainsSelfLoop()
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);
            graph.AddEdge(1, 1);  // Self-loop

            var transposed = graph.Transpose();

            Assert.True(transposed.HasEdge(1, 1), "Self-loop should remain after transpose");
        }

        #endregion

        #region Complex Graph Tests

        [Fact(DisplayName = "Star graph: center with outgoing edges becomes center with incoming")]
        public void StarGraphOutgoing_TransposesToIncoming()
        {
            var graph = new DiGraph<int>();
            // Center = 0, spokes = 1, 2, 3, 4
            graph.AddVertex(0);
            for (int i = 1; i <= 4; i++)
            {
                graph.AddVertex(i);
                graph.AddEdge(0, i);  // 0 → i
            }

            var transposed = graph.Transpose();

            // After transpose: all edges point TO center
            for (int i = 1; i <= 4; i++)
            {
                Assert.False(transposed.HasEdge(0, i), $"Should NOT have 0→{i}");
                Assert.True(transposed.HasEdge(i, 0), $"Should have {i}→0");
            }

            // Center should have no outgoing edges in transposed
            Assert.Empty(transposed.GetNeighbors(0));
        }

        [Fact(DisplayName = "Diamond graph transpose reverses all paths")]
        public void DiamondGraph_TransposeReversesAllPaths()
        {
            //     B
            //    ↗ ↘
            //   A   D
            //    ↘ ↗
            //     C
            var graph = new DiGraph<string>();
            graph.AddVertex("A");
            graph.AddVertex("B");
            graph.AddVertex("C");
            graph.AddVertex("D");
            graph.AddEdge("A", "B");
            graph.AddEdge("A", "C");
            graph.AddEdge("B", "D");
            graph.AddEdge("C", "D");

            var transposed = graph.Transpose();

            // Transposed:
            //     B
            //    ↙ ↖
            //   A   D
            //    ↖ ↙
            //     C
            Assert.True(transposed.HasEdge("B", "A"));
            Assert.True(transposed.HasEdge("C", "A"));
            Assert.True(transposed.HasEdge("D", "B"));
            Assert.True(transposed.HasEdge("D", "C"));

            // Original edges should not exist
            Assert.False(transposed.HasEdge("A", "B"));
            Assert.False(transposed.HasEdge("A", "C"));
            Assert.False(transposed.HasEdge("B", "D"));
            Assert.False(transposed.HasEdge("C", "D"));
        }

        [Fact(DisplayName = "Disconnected components transpose correctly")]
        public void DisconnectedComponents_TransposeCorrectly()
        {
            var graph = new DiGraph<int>();
            // Component 1: 1 → 2 → 3
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddVertex(3);
            graph.AddEdge(1, 2);
            graph.AddEdge(2, 3);

            // Component 2: 10 → 20
            graph.AddVertex(10);
            graph.AddVertex(20);
            graph.AddEdge(10, 20);

            var transposed = graph.Transpose();

            // Component 1 transposed: 3 → 2 → 1
            Assert.True(transposed.HasEdge(2, 1));
            Assert.True(transposed.HasEdge(3, 2));

            // Component 2 transposed: 20 → 10
            Assert.True(transposed.HasEdge(20, 10));

            // Components should still be disconnected
            Assert.False(transposed.HasEdge(3, 10));
            Assert.False(transposed.HasEdge(20, 1));
        }

        #endregion

        #region Double Transpose Tests

        [Fact(DisplayName = "Double transpose returns to original graph")]
        public void DoubleTranspose_ReturnsToOriginal()
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddVertex(3);
            graph.AddVertex(4);
            graph.AddEdge(1, 2);
            graph.AddEdge(2, 3);
            graph.AddEdge(3, 4);
            graph.AddEdge(4, 1);  // Cycle
            graph.AddEdge(1, 3);  // Cross edge

            var doubleTransposed = graph.Transpose().Transpose();

            // All original edges should exist
            Assert.True(doubleTransposed.HasEdge(1, 2));
            Assert.True(doubleTransposed.HasEdge(2, 3));
            Assert.True(doubleTransposed.HasEdge(3, 4));
            Assert.True(doubleTransposed.HasEdge(4, 1));
            Assert.True(doubleTransposed.HasEdge(1, 3));

            Assert.Equal(graph.VertexCount(), doubleTransposed.VertexCount());
            Assert.Equal(graph.EdgeCount(), doubleTransposed.EdgeCount());
        }

        #endregion

        #region SCC Preservation Tests

        [Fact(DisplayName = "Transpose preserves SCC membership")]
        public void Transpose_PreservesSCCMembership()
        {
            // Graph with known SCCs: {A,B,C,D} and {E}
            var graph = new DiGraph<string>();
            graph.AddVertex("A");
            graph.AddVertex("B");
            graph.AddVertex("C");
            graph.AddVertex("D");
            graph.AddVertex("E");

            // SCC {A,B,C,D}: cycle
            graph.AddEdge("A", "B");
            graph.AddEdge("B", "C");
            graph.AddEdge("C", "D");
            graph.AddEdge("D", "A");

            // E connected from D but can't get back
            graph.AddEdge("D", "E");

            var transposed = graph.Transpose();

            // In transposed, the cycle {A,B,C,D} should still be a cycle
            // A ← B ← C ← D ← A becomes A → D → C → B → A
            Assert.True(transposed.HasEdge("A", "D"));
            Assert.True(transposed.HasEdge("D", "C"));
            Assert.True(transposed.HasEdge("C", "B"));
            Assert.True(transposed.HasEdge("B", "A"));

            // E → D in transposed (was D → E)
            Assert.True(transposed.HasEdge("E", "D"));
            Assert.False(transposed.HasEdge("D", "E"));
        }

        #endregion

        #region Neighbor Verification Tests

        [Fact(DisplayName = "Transpose correctly updates neighbor lists")]
        public void Transpose_CorrectlyUpdatesNeighborLists()
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddVertex(3);
            graph.AddEdge(1, 2);
            graph.AddEdge(1, 3);

            var transposed = graph.Transpose();

            // In original: 1 has neighbors [2, 3]
            // In transposed: 1 has NO neighbors (edges reversed)
            var neighborsOf1 = transposed.GetNeighbors(1);
            Assert.Empty(neighborsOf1);

            // In transposed: 2 has neighbor [1]
            var neighborsOf2 = transposed.GetNeighbors(2);
            Assert.Single(neighborsOf2);
            Assert.Contains(1, neighborsOf2);

            // In transposed: 3 has neighbor [1]
            var neighborsOf3 = transposed.GetNeighbors(3);
            Assert.Single(neighborsOf3);
            Assert.Contains(1, neighborsOf3);
        }

        #endregion
    }
}

