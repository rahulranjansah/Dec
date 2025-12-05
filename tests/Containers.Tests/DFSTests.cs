using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using AST;
using Containers;
using Optimizer;
using Parser;

namespace Containers.Tests
{
    /// <summary>
    /// Tests for DFS (Depth-First Search) on DiGraph.
    ///
    /// Key Properties of DFS in DiGraph:
    /// - Visits ALL vertices (including unreachable ones in CFG context)
    /// - Returns a stack where each vertex appears exactly once
    /// - Used internally for Kosaraju's SCC algorithm
    ///
    /// Note: DFS does NOT use CFG.Start - it iterates through all vertices.
    /// For reachability analysis, use BFS which respects Start.
    /// </summary>
    public class DFSTests
    {
        #region Helper Methods

        /// <summary>
        /// Builds a CFG from program text using the full pipeline.
        /// </summary>
        private CFG BuildCFG(string programText)
        {
            var ast = Parser.Parser.Parse(programText);
            var visitor = new ControlFlowGraphGeneratorVisitor();
            ast.Accept(visitor, null);
            return (CFG)visitor.GetCFG();
        }

        #endregion

        // -------------------------------------------------
        // 1. Empty Graph
        // -------------------------------------------------

        [Fact(DisplayName = "DFS: empty CFG returns empty stack")]
        public void DFS_EmptyCFG_ReturnsEmptyStack()
        {
            string code = @"{}";
            var cfg = BuildCFG(code);

            var result = cfg.DepthFirstSearch();

            Assert.Empty(result);
        }

        // -------------------------------------------------
        // 2. All Vertices Visited (Including Unreachable)
        // -------------------------------------------------

        [Fact(DisplayName = "DFS: visits all vertices including unreachable")]
        public void DFS_VisitsAllVertices_IncludingUnreachable()
        {
            // CFG Structure:
            // Reachable:   [x := 1] → [return x]
            // Unreachable: [y := 2]   [z := 3]  (no edges from reachable)
            //
            // DFS visits ALL vertices by iterating GetVertices()
            string code = @"{
                x := (1)
                return (x)
                y := (2)
                z := (3)
            }";

            var cfg = BuildCFG(code);
            var result = cfg.DepthFirstSearch();

            // DFS visits ALL 4 vertices (unlike BFS which only visits reachable)
            Assert.Equal(4, result.Count);
        }

        [Fact(DisplayName = "DFS: single statement returns stack with one element")]
        public void DFS_SingleStatement_ReturnsStackWithOneElement()
        {
            string code = @"{
                return (42)
            }";

            var cfg = BuildCFG(code);
            var result = cfg.DepthFirstSearch();

            Assert.Single(result);
        }

        [Fact(DisplayName = "DFS: linear program - all vertices in stack")]
        public void DFS_LinearProgram_AllVerticesInStack()
        {
            // CFG: [x := 1] → [y := 2] → [z := 3] → [return]
            string code = @"{
                x := (1)
                y := (2)
                z := (3)
                return (x + y + z)
            }";

            var cfg = BuildCFG(code);
            var result = cfg.DepthFirstSearch();

            Assert.Equal(4, result.Count);
        }

        // -------------------------------------------------
        // 3. Every Vertex Appears Exactly Once
        // -------------------------------------------------

        [Fact(DisplayName = "DFS: every vertex appears exactly once in result")]
        public void DFS_EveryVertex_AppearsExactlyOnce()
        {
            // CFG with both reachable and unreachable vertices:
            // Reachable:   [a := 1] → [b := 2] → [return]
            // Unreachable: [c := 3]   [d := 4]
            string code = @"{
                a := (1)
                b := (2)
                return (a + b)
                c := (3)
                d := (4)
            }";

            var cfg = BuildCFG(code);
            var result = cfg.DepthFirstSearch();

            var vertices = cfg.GetVertices().ToList();
            var stackContents = new List<Statement>();
            while (result.Count > 0)
            {
                stackContents.Add(result.Pop());
            }

            // Same count
            Assert.Equal(vertices.Count, stackContents.Count);

            // Each vertex appears exactly once (partition property)
            foreach (var vertex in vertices)
            {
                Assert.Single(stackContents.Where(s => s == vertex));
            }
        }

        // -------------------------------------------------
        // 4. Nested Blocks
        // -------------------------------------------------

        [Fact(DisplayName = "DFS: nested blocks - all vertices visited")]
        public void DFS_NestedBlocks_AllVerticesVisited()
        {
            // CFG (blocks folded): [outer := 1] → [inner := 2] → [deep := 3] → [return]
            string code = @"{
                outer := (1)
                {
                    inner := (2)
                    {
                        deep := (3)
                    }
                }
                return (outer)
            }";

            var cfg = BuildCFG(code);
            var result = cfg.DepthFirstSearch();

            Assert.Equal(4, result.Count);
        }

        [Fact(DisplayName = "DFS: nested with unreachable - all vertices visited")]
        public void DFS_NestedWithUnreachable_AllVerticesVisited()
        {
            // CFG:
            // Reachable:   [a := 1] → [b := 2] → [return b]
            // Unreachable: [c := 3]   [d := 4]
            string code = @"{
                a := (1)
                {
                    b := (2)
                    return (b)
                    c := (3)
                }
                d := (4)
            }";

            var cfg = BuildCFG(code);
            var result = cfg.DepthFirstSearch();

            // DFS visits ALL 5 vertices
            Assert.Equal(5, result.Count);
        }

        // -------------------------------------------------
        // 5. Comparison with BFS
        // -------------------------------------------------

        [Fact(DisplayName = "DFS visits all; BFS only visits reachable")]
        public void DFS_VisitsAll_BFS_OnlyReachable()
        {
            // CFG:
            // Reachable:   [a := 1] → [return a]
            // Unreachable: [b := 2]   [c := 3]
            string code = @"{
                a := (1)
                return (a)
                b := (2)
                c := (3)
            }";

            var cfg = BuildCFG(code);

            var dfsResult = cfg.DepthFirstSearch();
            var (bfsReachable, bfsUnreachable) = cfg.BreadthFirstSearch();

            // DFS visits ALL vertices
            Assert.Equal(4, dfsResult.Count);

            // BFS only visits reachable from Start
            Assert.Equal(2, bfsReachable.Count);
            Assert.Equal(2, bfsUnreachable.Count);

            // DFS count = BFS reachable + BFS unreachable
            Assert.Equal(dfsResult.Count, bfsReachable.Count + bfsUnreachable.Count);
        }

        [Fact(DisplayName = "DFS and BFS: same count for fully connected graph")]
        public void DFS_BFS_SameCount_FullyConnected()
        {
            // CFG: [x := 1] → [y := 2] → [z := 3] → [return]
            // All vertices reachable
            string code = @"{
                x := (1)
                y := (2)
                z := (3)
                return (x + y + z)
            }";

            var cfg = BuildCFG(code);

            var dfsResult = cfg.DepthFirstSearch();
            var (bfsReachable, bfsUnreachable) = cfg.BreadthFirstSearch();

            // For fully connected graph, DFS and BFS visit same vertices
            Assert.Equal(bfsReachable.Count, dfsResult.Count);
            Assert.Empty(bfsUnreachable);
        }

        // -------------------------------------------------
        // 6. Multiple Disconnected Components
        // -------------------------------------------------

        [Fact(DisplayName = "DFS: multiple disconnected components all visited")]
        public void DFS_MultipleDisconnectedComponents_AllVisited()
        {
            // CFG:
            // Component 1: [a := 1] → [return a]
            // Component 2: [b := 2]
            // Component 3: [c := 3]   [d := 4]
            string code = @"{
                a := (1)
                return (a)
                b := (2)
                c := (3)
                d := (4)
            }";

            var cfg = BuildCFG(code);
            var result = cfg.DepthFirstSearch();

            // All 5 vertices visited across all components
            Assert.Equal(5, result.Count);
        }

        // -------------------------------------------------
        // 7. Edge Cases
        // -------------------------------------------------

        [Fact(DisplayName = "DFS: program starting with return")]
        public void DFS_ProgramStartingWithReturn()
        {
            // CFG:
            // Reachable:   [return 42]
            // Unreachable: [x := 1]   [y := 2]
            string code = @"{
                return (42)
                x := (1)
                y := (2)
            }";

            var cfg = BuildCFG(code);
            var result = cfg.DepthFirstSearch();

            // All 3 vertices visited
            Assert.Equal(3, result.Count);
        }

        [Fact(DisplayName = "DFS: deeply nested with unreachable code")]
        public void DFS_DeeplyNested_UnreachableCode()
        {
            // CFG:
            // Reachable:   [x := 0] → [y := 1] → [z := 2] → [return z]
            // Unreachable: [dead := 99]   [after := 11]   [final := 100]
            string code = @"{
                x := (0)
                {
                    y := (1)
                    {
                        z := (2)
                        return (z)
                        dead := (99)
                    }
                    after := (11)
                }
                final := (100)
            }";

            var cfg = BuildCFG(code);
            var result = cfg.DepthFirstSearch();

            // Count: x, y, z, return, dead, after, final = 7 vertices
            Assert.Equal(7, result.Count);
        }
    }
}
