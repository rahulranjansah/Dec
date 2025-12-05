using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Containers;

namespace Containers.Tests
{
    /// <summary>
    /// Test suite for DiGraph.FindStronglyConnectedComponents() using Kosaraju's algorithm.
    /// Tests verify actual SCC membership, not just counts.
    /// </summary>
    public class FindStronglyConnectedComponentsTests
    {
        #region Helper Methods

        /// <summary>
        /// Finds the SCC containing a specific vertex.
        /// </summary>
        private List<T> FindSCCContaining<T>(List<List<T>> sccs, T vertex) where T : notnull
        {
            return sccs.FirstOrDefault(scc => scc.Contains(vertex));
        }

        /// <summary>
        /// Checks if two vertices are in the same SCC.
        /// </summary>
        private bool AreInSameSCC<T>(List<List<T>> sccs, T v1, T v2) where T : notnull
        {
            var scc = FindSCCContaining(sccs, v1);
            return scc != null && scc.Contains(v2);
        }

        /// <summary>
        /// Verifies that a specific set of vertices forms exactly one SCC.
        /// </summary>
        private void AssertSCCContainsExactly<T>(List<List<T>> sccs, params T[] expectedMembers) where T : notnull
        {
            var scc = FindSCCContaining(sccs, expectedMembers[0]);
            Assert.NotNull(scc);
            Assert.Equal(expectedMembers.Length, scc.Count);
            foreach (var member in expectedMembers)
            {
                Assert.Contains(member, scc);
            }
        }

        #endregion

        #region Empty and Single Vertex Tests

        [Fact(DisplayName = "Empty graph produces no SCCs")]
        public void EmptyGraph_ProducesNoSCCs()
        {
            var graph = new DiGraph<int>();

            var sccs = graph.FindStronglyConnectedComponents();

            Assert.Empty(sccs);
        }

        [Fact(DisplayName = "Single vertex is its own SCC")]
        public void SingleVertex_IsOwnSCC()
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);

            var sccs = graph.FindStronglyConnectedComponents();

            Assert.Single(sccs);
            Assert.Single(sccs[0]);
            Assert.Contains(1, sccs[0]);
        }

        [Fact(DisplayName = "Single vertex with self-loop is its own SCC")]
        public void SingleVertexWithSelfLoop_IsOwnSCC()
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);
            graph.AddEdge(1, 1);

            var sccs = graph.FindStronglyConnectedComponents();

            Assert.Single(sccs);
            Assert.Single(sccs[0]);
            Assert.Contains(1, sccs[0]);
        }

        #endregion

        #region Linear Graph Tests (No Cycles = All Singletons)

        [Fact(DisplayName = "Linear chain: each vertex is its own SCC")]
        public void LinearChain_EachVertexIsOwnSCC()
        {
            // Graph Structure:
            //     A → B → C → D
            //
            // No back edges = no cycles = 4 singleton SCCs
            // SCCs: {A}, {B}, {C}, {D}
            var graph = new DiGraph<string>();
            graph.AddVertex("A");
            graph.AddVertex("B");
            graph.AddVertex("C");
            graph.AddVertex("D");
            graph.AddEdge("A", "B");
            graph.AddEdge("B", "C");
            graph.AddEdge("C", "D");

            var sccs = graph.FindStronglyConnectedComponents();

            // Each vertex is its own SCC (no back edges)
            Assert.Equal(4, sccs.Count);
            Assert.All(sccs, scc => Assert.Single(scc));

            // Verify each vertex is in a different SCC
            Assert.False(AreInSameSCC(sccs, "A", "B"));
            Assert.False(AreInSameSCC(sccs, "B", "C"));
            Assert.False(AreInSameSCC(sccs, "C", "D"));
        }

        [Fact(DisplayName = "Two disconnected vertices: two singleton SCCs")]
        public void TwoDisconnectedVertices_TwoSingletonSCCs()
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);
            graph.AddVertex(2);
            // No edges

            var sccs = graph.FindStronglyConnectedComponents();

            Assert.Equal(2, sccs.Count);
            Assert.All(sccs, scc => Assert.Single(scc));
            Assert.False(AreInSameSCC(sccs, 1, 2));
        }

        #endregion

        #region Simple Cycle Tests

        [Fact(DisplayName = "Two-vertex cycle: both in same SCC")]
        public void TwoVertexCycle_BothInSameSCC()
        {
            // Graph Structure:
            //     1 ⟷ 2
            //
            // Bidirectional = cycle = 1 SCC
            // SCCs: {1, 2}
            var graph = new DiGraph<int>();
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddEdge(1, 2);
            graph.AddEdge(2, 1);

            var sccs = graph.FindStronglyConnectedComponents();

            Assert.Single(sccs);  // One SCC containing both
            Assert.Equal(2, sccs[0].Count);
            Assert.Contains(1, sccs[0]);
            Assert.Contains(2, sccs[0]);
        }

        [Fact(DisplayName = "Three-vertex cycle: all in same SCC")]
        public void ThreeVertexCycle_AllInSameSCC()
        {
            // Graph Structure:
            //       A
            //      ↙ ↖
            //     B → C
            //
            // Cycle: A → B → C → A
            // SCCs: {A, B, C}
            var graph = new DiGraph<string>();
            graph.AddVertex("A");
            graph.AddVertex("B");
            graph.AddVertex("C");
            graph.AddEdge("A", "B");
            graph.AddEdge("B", "C");
            graph.AddEdge("C", "A");

            var sccs = graph.FindStronglyConnectedComponents();

            Assert.Single(sccs);
            Assert.Equal(3, sccs[0].Count);
            AssertSCCContainsExactly(sccs, "A", "B", "C");
        }

        [Fact(DisplayName = "Four-vertex cycle: all in same SCC")]
        public void FourVertexCycle_AllInSameSCC()
        {
            // Graph Structure:
            //     1 → 2
            //     ↑   ↓
            //     4 ← 3
            //
            // Cycle: 1 → 2 → 3 → 4 → 1
            // SCCs: {1, 2, 3, 4}
            var graph = new DiGraph<int>();
            for (int i = 1; i <= 4; i++)
                graph.AddVertex(i);
            graph.AddEdge(1, 2);
            graph.AddEdge(2, 3);
            graph.AddEdge(3, 4);
            graph.AddEdge(4, 1);

            var sccs = graph.FindStronglyConnectedComponents();

            Assert.Single(sccs);
            Assert.Equal(4, sccs[0].Count);
            AssertSCCContainsExactly(sccs, 1, 2, 3, 4);
        }

        #endregion

        #region Figure 2 Example: {A,B,C,D}, {E}, {F,G}

        [Fact(DisplayName = "Figure 2: Three SCCs with correct membership")]
        public void Figure2Example_ThreeSCCsWithCorrectMembership()
        {
            // Graph Structure (Figure 2):
            //
            //     A → B
            //     ↑   ↓
            //     D ← C → E        F ⟷ G
            //
            // SCC1: {A,B,C,D} - cycle A→B→C→D→A
            // SCC2: {E} - reachable from D but can't reach back (sink)
            // SCC3: {F,G} - bidirectional (disconnected component)
            //
            // SCCs: {A,B,C,D}, {E}, {F,G}
            var graph = new DiGraph<string>();
            graph.AddVertex("A");
            graph.AddVertex("B");
            graph.AddVertex("C");
            graph.AddVertex("D");
            graph.AddVertex("E");
            graph.AddVertex("F");
            graph.AddVertex("G");

            // Cycle A→B→C→D→A
            graph.AddEdge("A", "B");
            graph.AddEdge("B", "C");
            graph.AddEdge("C", "D");
            graph.AddEdge("D", "A");

            // D→E (E is sink)
            graph.AddEdge("D", "E");

            // F↔G bidirectional
            graph.AddEdge("F", "G");
            graph.AddEdge("G", "F");

            var sccs = graph.FindStronglyConnectedComponents();

            // Should have exactly 3 SCCs
            Assert.Equal(3, sccs.Count);

            // Verify SCC {A,B,C,D}
            Assert.True(AreInSameSCC(sccs, "A", "B"));
            Assert.True(AreInSameSCC(sccs, "A", "C"));
            Assert.True(AreInSameSCC(sccs, "A", "D"));
            AssertSCCContainsExactly(sccs, "A", "B", "C", "D");

            // Verify SCC {E} is singleton
            var sccE = FindSCCContaining(sccs, "E");
            Assert.Single(sccE);

            // Verify SCC {F,G}
            Assert.True(AreInSameSCC(sccs, "F", "G"));
            AssertSCCContainsExactly(sccs, "F", "G");

            // Verify E is NOT in same SCC as D
            Assert.False(AreInSameSCC(sccs, "D", "E"));
        }

        #endregion

        #region Multiple Disconnected Components

        [Fact(DisplayName = "Two disconnected cycles: two separate SCCs")]
        public void TwoDisconnectedCycles_TwoSeparateSCCs()
        {
            // Graph Structure (two disconnected components):
            //
            //     1 → 2           10 ⟷ 20
            //     ↑   ↓
            //     3 ←─┘
            //
            // Component 1: Cycle 1 → 2 → 3 → 1
            // Component 2: Cycle 10 ⟷ 20
            // SCCs: {1,2,3}, {10,20}
            var graph = new DiGraph<int>();

            // Cycle 1: 1 → 2 → 3 → 1
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddVertex(3);
            graph.AddEdge(1, 2);
            graph.AddEdge(2, 3);
            graph.AddEdge(3, 1);

            // Cycle 2: 10 → 20 → 10
            graph.AddVertex(10);
            graph.AddVertex(20);
            graph.AddEdge(10, 20);
            graph.AddEdge(20, 10);

            var sccs = graph.FindStronglyConnectedComponents();

            Assert.Equal(2, sccs.Count);

            // Verify cycle 1 is one SCC
            Assert.True(AreInSameSCC(sccs, 1, 2));
            Assert.True(AreInSameSCC(sccs, 2, 3));
            AssertSCCContainsExactly(sccs, 1, 2, 3);

            // Verify cycle 2 is another SCC
            Assert.True(AreInSameSCC(sccs, 10, 20));
            AssertSCCContainsExactly(sccs, 10, 20);

            // Verify they're not in the same SCC
            Assert.False(AreInSameSCC(sccs, 1, 10));
        }

        [Fact(DisplayName = "Three disconnected components: mix of cycles and singletons")]
        public void ThreeDisconnectedComponents_MixOfCyclesAndSingletons()
        {
            // Graph Structure (three disconnected components):
            //
            //     X → Y
            //     ↑   ↓         P          Q → R
            //     Z ←─┘      (isolated)   (chain)
            //
            // Component 1: Cycle {X,Y,Z}
            // Component 2: Singleton {P}
            // Component 3: Chain Q→R (no cycle = 2 singletons)
            // SCCs: {X,Y,Z}, {P}, {Q}, {R}
            var graph = new DiGraph<string>();

            // Component 1: Cycle X → Y → Z → X
            graph.AddVertex("X");
            graph.AddVertex("Y");
            graph.AddVertex("Z");
            graph.AddEdge("X", "Y");
            graph.AddEdge("Y", "Z");
            graph.AddEdge("Z", "X");

            // Component 2: Single vertex P
            graph.AddVertex("P");

            // Component 3: Chain Q → R (no cycle, two singletons)
            graph.AddVertex("Q");
            graph.AddVertex("R");
            graph.AddEdge("Q", "R");

            var sccs = graph.FindStronglyConnectedComponents();

            // Should have 4 SCCs: {X,Y,Z}, {P}, {Q}, {R}
            Assert.Equal(4, sccs.Count);

            // Verify {X,Y,Z} is one SCC
            AssertSCCContainsExactly(sccs, "X", "Y", "Z");

            // Verify P, Q, R are each singletons
            Assert.Single(FindSCCContaining(sccs, "P"));
            Assert.Single(FindSCCContaining(sccs, "Q"));
            Assert.Single(FindSCCContaining(sccs, "R"));
        }

        #endregion

        #region Source and Sink SCC Tests

        [Fact(DisplayName = "Chain of SCCs: source SCC → middle SCC → sink SCC")]
        public void ChainOfSCCs_SourceMiddleSink()
        {
            // Graph Structure (chain of SCCs):
            //
            //    ┌───┐      ┌───┐      ┌───┐
            //    │1⟷2│ ──→ │3⟷4│ ──→ │5⟷6│
            //    └───┘      └───┘      └───┘
            //    Source     Middle      Sink
            //
            // Inter-SCC edges: 2→3, 4→5 (one-way, no back edges)
            // SCCs: {1,2}, {3,4}, {5,6}
            var graph = new DiGraph<int>();

            // Source SCC: {1,2} cycle
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddEdge(1, 2);
            graph.AddEdge(2, 1);

            // Middle SCC: {3,4} cycle
            graph.AddVertex(3);
            graph.AddVertex(4);
            graph.AddEdge(3, 4);
            graph.AddEdge(4, 3);

            // Sink SCC: {5,6} cycle
            graph.AddVertex(5);
            graph.AddVertex(6);
            graph.AddEdge(5, 6);
            graph.AddEdge(6, 5);

            // Connections between SCCs (one-way)
            graph.AddEdge(2, 3);  // Source → Middle
            graph.AddEdge(4, 5);  // Middle → Sink

            var sccs = graph.FindStronglyConnectedComponents();

            Assert.Equal(3, sccs.Count);

            // Verify each SCC
            AssertSCCContainsExactly(sccs, 1, 2);
            AssertSCCContainsExactly(sccs, 3, 4);
            AssertSCCContainsExactly(sccs, 5, 6);

            // Verify inter-SCC vertices are NOT in same SCC
            Assert.False(AreInSameSCC(sccs, 2, 3));
            Assert.False(AreInSameSCC(sccs, 4, 5));
        }

        [Fact(DisplayName = "Sink SCC: vertices reachable but can't reach back")]
        public void SinkSCC_VerticesReachableButCantReachBack()
        {
            // Graph Structure:
            //
            //       A
            //      ↙ ↖
            //     B → C
            //     ↓   ↓
            //     X   Y
            //
            // Main cycle: A→B→C→A forms SCC {A,B,C}
            // X,Y are sinks: reachable but can't reach back
            // SCCs: {A,B,C}, {X}, {Y}
            var graph = new DiGraph<string>();

            // Main cycle: A → B → C → A
            graph.AddVertex("A");
            graph.AddVertex("B");
            graph.AddVertex("C");
            graph.AddEdge("A", "B");
            graph.AddEdge("B", "C");
            graph.AddEdge("C", "A");

            // Sink vertices: X, Y (reachable from cycle but can't get back)
            graph.AddVertex("X");
            graph.AddVertex("Y");
            graph.AddEdge("B", "X");
            graph.AddEdge("C", "Y");

            var sccs = graph.FindStronglyConnectedComponents();

            // Should have 3 SCCs: {A,B,C}, {X}, {Y}
            Assert.Equal(3, sccs.Count);

            // Main cycle is one SCC
            AssertSCCContainsExactly(sccs, "A", "B", "C");

            // X and Y are singletons
            Assert.Single(FindSCCContaining(sccs, "X"));
            Assert.Single(FindSCCContaining(sccs, "Y"));

            // X and Y are not in the main SCC
            Assert.False(AreInSameSCC(sccs, "A", "X"));
            Assert.False(AreInSameSCC(sccs, "A", "Y"));
        }

        #endregion

        #region Complex Graph Tests

        [Fact(DisplayName = "Nested cycles share SCC")]
        public void NestedCycles_ShareSCC()
        {
            // Graph Structure:
            //
            //     1 → 2
            //     ↑   ↓↘
            //     4 ← 3  (shortcut 2→4)
            //
            // Outer cycle: 1→2→3→4→1
            // Inner shortcut: 2→4 creates additional path
            // All vertices strongly connected via multiple cycles
            // SCCs: {1,2,3,4}
            var graph = new DiGraph<int>();

            // Outer cycle: 1 → 2 → 3 → 4 → 1
            // Inner shortcut: 2 → 4
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddVertex(3);
            graph.AddVertex(4);
            graph.AddEdge(1, 2);
            graph.AddEdge(2, 3);
            graph.AddEdge(3, 4);
            graph.AddEdge(4, 1);
            graph.AddEdge(2, 4);  // Shortcut

            var sccs = graph.FindStronglyConnectedComponents();

            // All vertices should be in one SCC
            Assert.Single(sccs);
            AssertSCCContainsExactly(sccs, 1, 2, 3, 4);
        }

        [Fact(DisplayName = "Figure-8 graph: two overlapping cycles")]
        public void Figure8Graph_TwoOverlappingCycles()
        {
            // Graph Structure (Figure-8 shape):
            //
            //     A           E
            //    ↙ ↖         ↙ ↖
            //   B → C ─────→ D
            //        ↖_______↙
            //
            // Cycle 1: A→B→C→A
            // Cycle 2: C→D→E→C
            // Shared vertex C connects both cycles
            // All vertices mutually reachable via C
            // SCCs: {A,B,C,D,E}
            var graph = new DiGraph<string>();
            graph.AddVertex("A");
            graph.AddVertex("B");
            graph.AddVertex("C");
            graph.AddVertex("D");
            graph.AddVertex("E");

            // Cycle 1
            graph.AddEdge("A", "B");
            graph.AddEdge("B", "C");
            graph.AddEdge("C", "A");

            // Cycle 2
            graph.AddEdge("C", "D");
            graph.AddEdge("D", "E");
            graph.AddEdge("E", "C");

            var sccs = graph.FindStronglyConnectedComponents();

            // All vertices form one SCC (connected through shared vertex C)
            Assert.Single(sccs);
            AssertSCCContainsExactly(sccs, "A", "B", "C", "D", "E");
        }

        [Fact(DisplayName = "Diamond with back edge: all in one SCC")]
        public void DiamondWithBackEdge_AllInOneSCC()
        {
            // Graph Structure (diamond with back edge):
            //
            //       B
            //      ↗ ↘
            //     A   D
            //      ↘ ↗ │
            //       C  │
            //       ↑__│ (back edge D→A)
            //
            // Back edge D→A creates cycle: A→B→D→A and A→C→D→A
            // All vertices mutually reachable
            // SCCs: {A,B,C,D}
            var graph = new DiGraph<string>();
            graph.AddVertex("A");
            graph.AddVertex("B");
            graph.AddVertex("C");
            graph.AddVertex("D");
            graph.AddEdge("A", "B");
            graph.AddEdge("A", "C");
            graph.AddEdge("B", "D");
            graph.AddEdge("C", "D");
            graph.AddEdge("D", "A");  // Back edge creates cycle

            var sccs = graph.FindStronglyConnectedComponents();

            // All form one SCC due to cycle
            Assert.Single(sccs);
            AssertSCCContainsExactly(sccs, "A", "B", "C", "D");
        }

        [Fact(DisplayName = "Diamond without back edge: all singletons")]
        public void DiamondWithoutBackEdge_AllSingletons()
        {
            // Graph Structure (diamond DAG, no cycles):
            //
            //       B
            //      ↗ ↘
            //     A   D
            //      ↘ ↗
            //       C
            //
            // NO back edge = NO cycle = DAG
            // Each vertex is its own SCC
            // SCCs: {A}, {B}, {C}, {D}
            var graph = new DiGraph<string>();
            graph.AddVertex("A");
            graph.AddVertex("B");
            graph.AddVertex("C");
            graph.AddVertex("D");
            graph.AddEdge("A", "B");
            graph.AddEdge("A", "C");
            graph.AddEdge("B", "D");
            graph.AddEdge("C", "D");
            // No back edge

            var sccs = graph.FindStronglyConnectedComponents();

            // Each vertex is its own SCC (no cycles)
            Assert.Equal(4, sccs.Count);
            Assert.All(sccs, scc => Assert.Single(scc));
        }

        #endregion

        #region Partition Property Tests

        [Fact(DisplayName = "SCCs partition all vertices exactly once")]
        public void SCCs_PartitionAllVerticesExactlyOnce()
        {
            // Graph Structure:
            //
            //    ┌─────┐
            //    │1→2→3│ → 6     4⟷5     7→8→9→10
            //    └──↑──┘        (cycle)    (chain)
            //
            // Cycle {1,2,3} + sink {6}
            // Cycle {4,5}
            // Chain {7},{8},{9},{10} (4 singletons)
            // SCCs: {1,2,3}, {4,5}, {6}, {7}, {8}, {9}, {10}
            var graph = new DiGraph<int>();
            for (int i = 1; i <= 10; i++)
                graph.AddVertex(i);

            // Add some edges to create mix of SCCs
            graph.AddEdge(1, 2);
            graph.AddEdge(2, 3);
            graph.AddEdge(3, 1);  // Cycle {1,2,3}
            graph.AddEdge(4, 5);
            graph.AddEdge(5, 4);  // Cycle {4,5}
            graph.AddEdge(3, 6);  // 6 is sink from {1,2,3}
            graph.AddEdge(7, 8);
            graph.AddEdge(8, 9);
            graph.AddEdge(9, 10);  // Chain {7}, {8}, {9}, {10}

            var sccs = graph.FindStronglyConnectedComponents();

            // Collect all vertices from SCCs
            var allVerticesInSCCs = sccs.SelectMany(scc => scc).ToList();

            // Every vertex appears exactly once
            Assert.Equal(10, allVerticesInSCCs.Count);
            for (int i = 1; i <= 10; i++)
            {
                Assert.Single(allVerticesInSCCs.Where(v => v == i));
            }
        }

        [Fact(DisplayName = "Each vertex belongs to exactly one SCC")]
        public void EachVertex_BelongsToExactlyOneSCC()
        {
            // Graph Structure:
            //
            //       A
            //      ↙ ↖
            //     B → C → D → E
            //
            // Cycle {A,B,C} + chain D→E (singletons)
            // SCCs: {A,B,C}, {D}, {E}
            // Partition property: every vertex in exactly ONE SCC
            var graph = new DiGraph<string>();
            var vertices = new[] { "A", "B", "C", "D", "E" };
            foreach (var v in vertices)
                graph.AddVertex(v);

            graph.AddEdge("A", "B");
            graph.AddEdge("B", "C");
            graph.AddEdge("C", "A");  // Cycle
            graph.AddEdge("C", "D");
            graph.AddEdge("D", "E");

            var sccs = graph.FindStronglyConnectedComponents();

            foreach (var vertex in vertices)
            {
                int containingCount = sccs.Count(scc => scc.Contains(vertex));
                Assert.Equal(1, containingCount);
            }
        }

        #endregion

        #region Consistency Tests

        [Fact(DisplayName = "Recomputing SCCs gives consistent results")]
        public void RecomputingSCCs_GivesConsistentResults()
        {
            var graph = new DiGraph<int>();
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddVertex(3);
            graph.AddEdge(1, 2);
            graph.AddEdge(2, 3);
            graph.AddEdge(3, 1);

            var sccs1 = graph.FindStronglyConnectedComponents();
            var sccs2 = graph.FindStronglyConnectedComponents();
            var sccs3 = graph.FindStronglyConnectedComponents();

            Assert.Equal(sccs1.Count, sccs2.Count);
            Assert.Equal(sccs2.Count, sccs3.Count);

            // Same membership
            Assert.True(AreInSameSCC(sccs1, 1, 2));
            Assert.True(AreInSameSCC(sccs2, 1, 2));
            Assert.True(AreInSameSCC(sccs3, 1, 2));
        }

        #endregion

        #region Large Graph Tests

        [Fact(DisplayName = "Large cycle: all vertices in one SCC")]
        public void LargeCycle_AllVerticesInOneSCC()
        {
            // Graph Structure (circular):
            //
            //     0 → 1 → 2 → 3 → ... → 99
            //     ↑________________________│
            //
            // Large cycle: 0→1→2→...→99→0
            // All 100 vertices in ONE SCC
            // SCCs: {0,1,2,...,99}
            var graph = new DiGraph<int>();
            int n = 100;

            for (int i = 0; i < n; i++)
                graph.AddVertex(i);

            for (int i = 0; i < n; i++)
                graph.AddEdge(i, (i + 1) % n);  // Circular

            var sccs = graph.FindStronglyConnectedComponents();

            Assert.Single(sccs);
            Assert.Equal(n, sccs[0].Count);
        }

        [Fact(DisplayName = "Large linear chain: n vertices produce n SCCs")]
        public void LargeLinearChain_NVerticesProduceNSCCs()
        {
            // Graph Structure (linear chain):
            //
            //     0 → 1 → 2 → 3 → ... → 49
            //
            // NO back edges = NO cycles
            // Each vertex is its own SCC
            // SCCs: {0}, {1}, {2}, ..., {49} (50 singletons)
            var graph = new DiGraph<int>();
            int n = 50;

            for (int i = 0; i < n; i++)
                graph.AddVertex(i);

            for (int i = 0; i < n - 1; i++)
                graph.AddEdge(i, i + 1);

            var sccs = graph.FindStronglyConnectedComponents();

            Assert.Equal(n, sccs.Count);
            Assert.All(sccs, scc => Assert.Single(scc));
        }

        #endregion
    }
}

