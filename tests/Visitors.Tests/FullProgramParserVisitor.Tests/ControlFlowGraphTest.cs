using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using AST;
using Utilities;
using Parser;
using Tokenizer;
using Containers;
using Optimizer;

namespace AST.Visitors.Tests.FullProgramParserVisitor.Tests
{
    /// <summary>
    /// End-to-end integration tests for the full pipeline:
    /// Program Text → Tokenizer → Parser → AST → ControlFlowGraphGeneratorVisitor → CFG.
    ///
    /// These tests validate that the CFG is correctly built from full programs,
    /// including proper handling of sequential statements, nested blocks, and unreachable code.
    ///
    /// Key behaviors verified:
    /// - Return statements terminate control flow (no outgoing edges)
    /// - Unreachable code after return forms disjoint subgraphs
    /// - All statements are added as vertices, but edges only connect reachable code
    /// </summary>
    public class ControlFlowGraphIntegrationTest
    {
        private readonly NameAnalysisVisitor _analyzer = new();

        // Helper to build CFG from program text
        private CFG BuildCFG(string programText)
        {
            var ast = Parser.Parser.Parse(programText);
            var visitor = new ControlFlowGraphGeneratorVisitor();
            ast.Accept(visitor, null);
            return (CFG)visitor.GetCFG();
        }

        #region Helper Methods

        /// <summary>
        /// Gets all return statements from the CFG.
        /// </summary>
        private List<ReturnStmt> GetReturnStatements(CFG cfg)
        {
            return cfg.GetVertices().OfType<ReturnStmt>().ToList();
        }

        /// <summary>
        /// Gets all assignment statements from the CFG.
        /// </summary>
        private List<AssignmentStmt> GetAssignmentStatements(CFG cfg)
        {
            return cfg.GetVertices().OfType<AssignmentStmt>().ToList();
        }

        /// <summary>
        /// Gets assignment statement by variable name.
        /// </summary>
        private AssignmentStmt GetAssignmentByName(CFG cfg, string varName)
        {
            return GetAssignmentStatements(cfg)
                .First(a => ((VariableNode)a.Variable).Name == varName);
        }

        /// <summary>
        /// Verifies that a specific edge exists in the CFG.
        /// </summary>
        private void AssertEdgeExists(CFG cfg, Statement from, Statement to)
        {
            Assert.True(cfg.HasEdge(from, to),
                $"Expected edge from {from} to {to}");
        }

        /// <summary>
        /// Verifies that a specific edge does NOT exist in the CFG.
        /// </summary>
        private void AssertNoEdge(CFG cfg, Statement from, Statement to)
        {
            Assert.False(cfg.HasEdge(from, to),
                $"Did not expect edge from {from} to {to}");
        }

        /// <summary>
        /// Computes the set of statements reachable from the CFG's start node via BFS.
        /// </summary>
        private HashSet<Statement> GetReachableStatements(CFG cfg)
        {
            var reachable = new HashSet<Statement>();
            if (cfg.Start == null) return reachable;

            var queue = new Queue<Statement>();
            queue.Enqueue(cfg.Start);
            reachable.Add(cfg.Start);

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

            return reachable;
        }

        /// <summary>
        /// Gets unreachable statements (statements in CFG but not reachable from start).
        /// </summary>
        private HashSet<Statement> GetUnreachableStatements(CFG cfg)
        {
            var all = new HashSet<Statement>(cfg.GetVertices());
            var reachable = GetReachableStatements(cfg);
            all.ExceptWith(reachable);
            return all;
        }

        #endregion

        // -------------------------------------------------
        // 1. Simple Sequential Programs
        // -------------------------------------------------

        [Fact(DisplayName = "Full program: simple assignment and return builds correct CFG")]
        public void CFG_SimpleAssignmentAndReturn_BuildsCorrectGraph()
        {
            string code = @"{
                x := (42)
                return (x)
            }";

            var cfg = BuildCFG(code);

            // Should have 2 vertices: 1 assignment + 1 return
            Assert.Equal(2, cfg.VertexCount());
            // Should have 1 edge: assignment -> return
            Assert.Equal(1, cfg.EdgeCount());

            // Verify the specific edge exists
            var assignment = GetAssignmentStatements(cfg).Single();
            var returnStmt = GetReturnStatements(cfg).Single();
            AssertEdgeExists(cfg, assignment, returnStmt);
        }

        [Fact(DisplayName = "Full program: multiple sequential assignments build linear CFG")]
        public void CFG_MultipleSequentialAssignments_BuildsLinearGraph()
        {
            string code = @"{
                a := (1)
                b := (2)
                c := (3)
                return (a + b + c)
            }";

            var cfg = BuildCFG(code);

            // 3 assignments + 1 return = 4 vertices
            Assert.Equal(4, cfg.VertexCount());
            // Linear flow: a -> b -> c -> return = 3 edges
            Assert.Equal(3, cfg.EdgeCount());

            // Verify linear chain
            var a = GetAssignmentByName(cfg, "a");
            var b = GetAssignmentByName(cfg, "b");
            var c = GetAssignmentByName(cfg, "c");
            var ret = GetReturnStatements(cfg).Single();

            AssertEdgeExists(cfg, a, b);
            AssertEdgeExists(cfg, b, c);
            AssertEdgeExists(cfg, c, ret);
        }

        [Theory(DisplayName = "Full program: various statement counts build correct CFG")]
        [InlineData(@"{
                return (5)
            }", 1, 0)]  // Only return, no edges
        [InlineData(@"{
                x := (1)
                return (x)
            }", 2, 1)]  // 1 assignment -> 1 return
        [InlineData(@"{
                x := (1)
                y := (2)
                return (x)
            }", 3, 2)]  // Linear chain
        public void CFG_VariousStatementCounts_CorrectVerticesAndEdges(string code, int expectedVertices, int expectedEdges)
        {
            var cfg = BuildCFG(code);
            Assert.Equal(expectedVertices, cfg.VertexCount());
            Assert.Equal(expectedEdges, cfg.EdgeCount());
        }

        // -------------------------------------------------
        // 2. Nested Blocks (Block Folding)
        // -------------------------------------------------

        [Fact(DisplayName = "Full program: nested blocks fold correctly into sequential flow")]
        public void CFG_NestedBlocks_FoldCorrectly()
        {
            string code = @"{
                x := (5)
                {
                    y := (x + 10)
                    return (y)
                }
            }";

            var cfg = BuildCFG(code);

            // BlockStmt should be folded: x, y, return = 3 vertices
            Assert.Equal(3, cfg.VertexCount());
            // x -> y -> return = 2 edges
            Assert.Equal(2, cfg.EdgeCount());

            var x = GetAssignmentByName(cfg, "x");
            var y = GetAssignmentByName(cfg, "y");
            var ret = GetReturnStatements(cfg).Single();

            AssertEdgeExists(cfg, x, y);
            AssertEdgeExists(cfg, y, ret);
        }

        [Fact(DisplayName = "Full program: deeply nested blocks maintain correct flow")]
        public void CFG_DeeplyNestedBlocks_MaintainCorrectFlow()
        {
            string code = @"{
                a := (1)
                {
                    b := (2)
                    {
                        c := (3)
                        {
                            d := (4)
                            return (a + b + c + d)
                        }
                    }
                }
            }";

            var cfg = BuildCFG(code);

            // 4 assignments + 1 return = 5 vertices
            Assert.Equal(5, cfg.VertexCount());
            // Linear flow through nested blocks: 4 edges
            Assert.Equal(4, cfg.EdgeCount());

            var a = GetAssignmentByName(cfg, "a");
            var b = GetAssignmentByName(cfg, "b");
            var c = GetAssignmentByName(cfg, "c");
            var d = GetAssignmentByName(cfg, "d");
            var ret = GetReturnStatements(cfg).Single();

            AssertEdgeExists(cfg, a, b);
            AssertEdgeExists(cfg, b, c);
            AssertEdgeExists(cfg, c, d);
            AssertEdgeExists(cfg, d, ret);
        }

        [Fact(DisplayName = "Full program: empty nested blocks don't add vertices")]
        public void CFG_EmptyNestedBlocks_NoExtraVertices()
        {
            string code = @"{
                x := (1)
                {
                    {
                        {
                            y := (2)
                        }
                    }
                }
                return (x)
            }";

            var cfg = BuildCFG(code);

            // Only x, y, return = 3 vertices (empty blocks folded)
            Assert.Equal(3, cfg.VertexCount());
            Assert.Equal(2, cfg.EdgeCount());
        }

        // -------------------------------------------------
        // 3. Return Statements and Unreachable Code (Disjoint Graphs)
        // -------------------------------------------------

        [Fact(DisplayName = "Full program: early return creates disjoint unreachable vertices")]
        public void CFG_EarlyReturn_CreatesDisjointUnreachableVertices()
        {
            string code = @"{
                x := (10)
                return (x)
                y := (20)
                z := (30)
            }";

            var cfg = BuildCFG(code);

            // x, return, y, z = 4 vertices total
            Assert.Equal(4, cfg.VertexCount());

            var x = GetAssignmentByName(cfg, "x");
            var ret = GetReturnStatements(cfg).Single();
            var y = GetAssignmentByName(cfg, "y");
            var z = GetAssignmentByName(cfg, "z");

            // Only x -> return edge exists (1 edge in reachable portion)
            AssertEdgeExists(cfg, x, ret);

            // Return has NO outgoing edges (terminates control flow)
            Assert.Empty(cfg.GetNeighbors(ret));

            // y and z are unreachable - they form their own disjoint subgraph
            // y -> z exists in the unreachable portion
            AssertNoEdge(cfg, ret, y);
            AssertEdgeExists(cfg, y, z);

            // Total edges: x->ret (1) + y->z (1) = 2
            Assert.Equal(2, cfg.EdgeCount());

            // Verify reachability
            var reachable = GetReachableStatements(cfg);
            Assert.Equal(2, reachable.Count);
            Assert.Contains(x, reachable);
            Assert.Contains(ret, reachable);

            var unreachable = GetUnreachableStatements(cfg);
            Assert.Equal(2, unreachable.Count);
            Assert.Contains(y, unreachable);
            Assert.Contains(z, unreachable);
        }

        [Fact(DisplayName = "Full program: return in nested block stops flow, unreachable code is disjoint")]
        public void CFG_ReturnInNestedBlock_CreatesDisjointUnreachableCode()
        {
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

            // a, b, return, c, d = 5 vertices
            Assert.Equal(5, cfg.VertexCount());

            var a = GetAssignmentByName(cfg, "a");
            var b = GetAssignmentByName(cfg, "b");
            var ret = GetReturnStatements(cfg).Single();
            var c = GetAssignmentByName(cfg, "c");
            var d = GetAssignmentByName(cfg, "d");

            // Reachable edges: a -> b -> return
            AssertEdgeExists(cfg, a, b);
            AssertEdgeExists(cfg, b, ret);

            // Return terminates - no edge to c or d
            AssertNoEdge(cfg, ret, c);
            Assert.Empty(cfg.GetNeighbors(ret));

            // c and d are in separate scopes (inner block vs outer block)
            // They are isolated vertices with no edges
            Assert.Empty(cfg.GetNeighbors(c));
            Assert.Empty(cfg.GetNeighbors(d));

            // Total: a->b, b->ret = 2 edges
            Assert.Equal(2, cfg.EdgeCount());

            // Verify reachability
            var reachable = GetReachableStatements(cfg);
            Assert.Equal(3, reachable.Count); // a, b, ret

            var unreachable = GetUnreachableStatements(cfg);
            Assert.Equal(2, unreachable.Count); // c, d
        }

        [Fact(DisplayName = "Full program: multiple returns in sequence - unreachable returns chain together")]
        public void CFG_MultipleReturnsInSequence_UnreachableReturnsChain()
        {
            string code = @"{
                x := (1)
                return (x)
                return (x + 1)
                return (x + 2)
            }";

            var cfg = BuildCFG(code);

            // x, return1, return2, return3 = 4 vertices
            Assert.Equal(4, cfg.VertexCount());

            var x = GetAssignmentByName(cfg, "x");
            var returns = GetReturnStatements(cfg);
            Assert.Equal(3, returns.Count);

            // x -> first return (1 edge)
            // Unreachable returns form a chain: return2 -> return3 (1 edge)
            // Total: 2 edges
            Assert.Equal(2, cfg.EdgeCount());

            // First return has no successors (terminates control flow)
            // Unreachable returns are in the same block so they chain together
            var reachable = GetReachableStatements(cfg);
            Assert.Equal(2, reachable.Count); // x and first return

            var unreachable = GetUnreachableStatements(cfg);
            Assert.Equal(2, unreachable.Count); // two unreachable returns
        }

        // -------------------------------------------------
        // 4. Complex Nested Structures
        // -------------------------------------------------

        [Fact(DisplayName = "Full program: complex nested structure builds correct CFG")]
        public void CFG_ComplexNestedStructure_BuildsCorrectGraph()
        {
            string code = @"{
                init := (0)
                {
                    a := (1)
                    b := (2)
                    {
                        c := (3)
                        d := (4)
                    }
                    e := (5)
                }
                return (100)
            }";

            var cfg = BuildCFG(code);

            // init, a, b, c, d, e, return = 7 vertices
            Assert.Equal(7, cfg.VertexCount());
            // init->a->b->c->d->e->return = 6 edges
            Assert.Equal(6, cfg.EdgeCount());
        }

        [Fact(DisplayName = "Full program: factorial-like computation builds linear CFG")]
        public void CFG_FactorialLikeComputation_BuildsLinearCFG()
        {
            string code = @"{
                n := (5)
                result := (1)
                counter := (1)
                {
                    result := (result * counter)
                    counter := (counter + 1)
                    result := (result * counter)
                    counter := (counter + 1)
                    result := (result * counter)
                    return (result)
                }
            }";

            var cfg = BuildCFG(code);

            // 3 outer + 5 inner + 1 return = 9 vertices
            Assert.Equal(9, cfg.VertexCount());
            // Linear flow = 8 edges
            Assert.Equal(8, cfg.EdgeCount());
        }

        [Fact(DisplayName = "Complex nested: early return in inner block with unreachable code")]
        public void CFG_ComplexNested_EarlyReturnWithUnreachable()
        {
            string code = @"{
                init := (0)
                {
                    a := (1)
                    b := (2)
                    {
                        c := (3)
                        return (c)
                        d := (4)
                    }
                    e := (5)
                }
                final := (100)
            }";

            var cfg = BuildCFG(code);

            var init = GetAssignmentByName(cfg, "init");
            var a = GetAssignmentByName(cfg, "a");
            var b = GetAssignmentByName(cfg, "b");
            var c = GetAssignmentByName(cfg, "c");
            var d = GetAssignmentByName(cfg, "d");
            var e = GetAssignmentByName(cfg, "e");
            var final = GetAssignmentByName(cfg, "final");
            var ret = GetReturnStatements(cfg).Single();

            // Reachable chain: init -> a -> b -> c -> return
            AssertEdgeExists(cfg, init, a);
            AssertEdgeExists(cfg, a, b);
            AssertEdgeExists(cfg, b, c);
            AssertEdgeExists(cfg, c, ret);
            Assert.Empty(cfg.GetNeighbors(ret));

            // d, e, final are all unreachable and isolated (different block scopes)
            Assert.Empty(cfg.GetNeighbors(d));
            Assert.Empty(cfg.GetNeighbors(e));
            Assert.Empty(cfg.GetNeighbors(final));

            // 8 vertices total
            Assert.Equal(8, cfg.VertexCount());
            // 4 edges (reachable chain only)
            Assert.Equal(4, cfg.EdgeCount());

            // Verify reachability
            var reachable = GetReachableStatements(cfg);
            Assert.Equal(5, reachable.Count); // init, a, b, c, ret

            var unreachable = GetUnreachableStatements(cfg);
            Assert.Equal(3, unreachable.Count); // d, e, final
        }

        [Fact(DisplayName = "Complex nested: return in middle block with subsequent code")]
        public void CFG_ComplexNested_ReturnInMiddleBlock()
        {
            string code = @"{
                outer := (1)
                {
                    mid := (2)
                    return (mid)
                    dead := (3)
                }
                after := (5)
            }";

            var cfg = BuildCFG(code);

            var outer = GetAssignmentByName(cfg, "outer");
            var mid = GetAssignmentByName(cfg, "mid");
            var dead = GetAssignmentByName(cfg, "dead");
            var after = GetAssignmentByName(cfg, "after");
            var ret = GetReturnStatements(cfg).Single();

            // Reachable: outer -> mid -> return
            AssertEdgeExists(cfg, outer, mid);
            AssertEdgeExists(cfg, mid, ret);
            Assert.Empty(cfg.GetNeighbors(ret));

            // Unreachable: dead, after (isolated - different scopes)
            Assert.Empty(cfg.GetNeighbors(dead));
            Assert.Empty(cfg.GetNeighbors(after));

            // 5 vertices, 2 edges
            Assert.Equal(5, cfg.VertexCount());
            Assert.Equal(2, cfg.EdgeCount());

            var reachable = GetReachableStatements(cfg);
            Assert.Equal(3, reachable.Count);

            var unreachable = GetUnreachableStatements(cfg);
            Assert.Equal(2, unreachable.Count);
        }

        [Fact(DisplayName = "Complex nested: unreachable code forms chain within same block")]
        public void CFG_ComplexNested_UnreachableChainInSameBlock()
        {
            string code = @"{
                a := (1)
                return (a)
                b := (2)
                c := (3)
                d := (4)
            }";

            var cfg = BuildCFG(code);

            var a = GetAssignmentByName(cfg, "a");
            var b = GetAssignmentByName(cfg, "b");
            var c = GetAssignmentByName(cfg, "c");
            var d = GetAssignmentByName(cfg, "d");
            var ret = GetReturnStatements(cfg).Single();

            // Reachable: a -> return
            AssertEdgeExists(cfg, a, ret);
            Assert.Empty(cfg.GetNeighbors(ret));

            // Unreachable chain in same block: b -> c -> d
            AssertEdgeExists(cfg, b, c);
            AssertEdgeExists(cfg, c, d);
            Assert.Empty(cfg.GetNeighbors(d));

            // 5 vertices, 3 edges (1 reachable + 2 unreachable chain)
            Assert.Equal(5, cfg.VertexCount());
            Assert.Equal(3, cfg.EdgeCount());

            var reachable = GetReachableStatements(cfg);
            Assert.Equal(2, reachable.Count); // a, ret

            var unreachable = GetUnreachableStatements(cfg);
            Assert.Equal(3, unreachable.Count); // b, c, d
        }

        [Fact(DisplayName = "Complex nested: factorial with early return and unreachable continuation")]
        public void CFG_FactorialWithEarlyReturn_UnreachableContinuation()
        {
            string code = @"{
                n := (5)
                result := (1)
                {
                    result := (result * 2)
                    return (result)
                    result := (result * 3)
                    result := (result * 4)
                }
                final := (result)
                return (final)
            }";

            var cfg = BuildCFG(code);

            var n = GetAssignmentByName(cfg, "n");
            var final = GetAssignmentByName(cfg, "final");
            var returns = GetReturnStatements(cfg);
            Assert.Equal(2, returns.Count);

            // Reachable: n -> result(1) -> result(*2) -> return(result)
            var reachable = GetReachableStatements(cfg);
            Assert.Equal(4, reachable.Count);

            // Unreachable: result(*3), result(*4) (chain in inner block), final, return(final) (isolated)
            var unreachable = GetUnreachableStatements(cfg);
            Assert.Equal(4, unreachable.Count);

            // Verify return has no outgoing edges
            foreach (var ret in returns)
            {
                if (reachable.Contains(ret))
                {
                    Assert.Empty(cfg.GetNeighbors(ret));
                }
            }

            // 8 vertices total
            Assert.Equal(8, cfg.VertexCount());
        }

        [Fact(DisplayName = "Complex nested: deeply nested with multiple unreachable segments")]
        public void CFG_DeeplyNested_MultipleUnreachableSegments()
        {
            string code = @"{
                x := (0)
                {
                    y := (1)
                    {
                        z := (2)
                        return (z)
                        deadInner := (22)
                    }
                    deadMid := (11)
                }
                deadOuter := (100)
            }";

            var cfg = BuildCFG(code);

            var x = GetAssignmentByName(cfg, "x");
            var y = GetAssignmentByName(cfg, "y");
            var z = GetAssignmentByName(cfg, "z");
            var deadInner = GetAssignmentByName(cfg, "deadInner");
            var deadMid = GetAssignmentByName(cfg, "deadMid");
            var deadOuter = GetAssignmentByName(cfg, "deadOuter");
            var ret = GetReturnStatements(cfg).Single();

            // Reachable chain: x -> y -> z -> return
            AssertEdgeExists(cfg, x, y);
            AssertEdgeExists(cfg, y, z);
            AssertEdgeExists(cfg, z, ret);

            // All dead* are isolated (different block scopes)
            AssertNoEdge(cfg, ret, deadInner);
            Assert.Empty(cfg.GetNeighbors(deadInner));
            Assert.Empty(cfg.GetNeighbors(deadMid));
            Assert.Empty(cfg.GetNeighbors(deadOuter));

            // 7 vertices, 3 edges
            Assert.Equal(7, cfg.VertexCount());
            Assert.Equal(3, cfg.EdgeCount());

            var reachable = GetReachableStatements(cfg);
            Assert.Equal(4, reachable.Count); // x, y, z, ret

            var unreachable = GetUnreachableStatements(cfg);
            Assert.Equal(3, unreachable.Count); // deadInner, deadMid, deadOuter
        }

        // -------------------------------------------------
        // 5. Edge Cases and Boundary Conditions
        // -------------------------------------------------

        [Fact(DisplayName = "Full program: single return statement")]
        public void CFG_SingleReturnStatement_OneVertex()
        {
            string code = @"{
                return (42)
            }";

            var cfg = BuildCFG(code);

            Assert.Equal(1, cfg.VertexCount());
            Assert.Equal(0, cfg.EdgeCount());
        }

        [Fact(DisplayName = "Full program: empty block creates empty CFG")]
        public void CFG_EmptyBlock_EmptyCFG()
        {
            string code = @"{}";

            var cfg = BuildCFG(code);

            Assert.Equal(0, cfg.VertexCount());
            Assert.Equal(0, cfg.EdgeCount());
        }

        // -------------------------------------------------
        // 6. Real-World Program Patterns
        // -------------------------------------------------

        [Fact(DisplayName = "Full program: variable initialization pattern")]
        public void CFG_VariableInitializationPattern_CorrectStructure()
        {
            string code = @"{
                x := (3)
                y := ((x + 5) * 2)
                z := (y / 2)
                return (z)
            }";

            var cfg = BuildCFG(code);

            Assert.Equal(4, cfg.VertexCount());
            Assert.Equal(3, cfg.EdgeCount());

            var x = GetAssignmentByName(cfg, "x");
            var y = GetAssignmentByName(cfg, "y");
            var z = GetAssignmentByName(cfg, "z");
            var ret = GetReturnStatements(cfg).Single();

            // Verify linear chain
            AssertEdgeExists(cfg, x, y);
            AssertEdgeExists(cfg, y, z);
            AssertEdgeExists(cfg, z, ret);

            // Return has no outgoing edges
            Assert.Empty(cfg.GetNeighbors(ret));
        }

        [Fact(DisplayName = "Full program: scoped computation pattern")]
        public void CFG_ScopedComputationPattern_CorrectFlow()
        {
            string code = @"{
                outer := (10)
                {
                    inner := (outer * 2)
                    {
                        nested := (inner + 5)
                        return (nested)
                    }
                }
            }";

            var cfg = BuildCFG(code);

            // outer, inner, nested, return = 4 vertices
            Assert.Equal(4, cfg.VertexCount());
            // Linear flow = 3 edges
            Assert.Equal(3, cfg.EdgeCount());
        }

        [Fact(DisplayName = "Full program: early exit pattern creates disjoint unreachable code")]
        public void CFG_EarlyExitPattern_CreatesDisjointUnreachableCode()
        {
            string code = @"{
                guard := (0)
                return (guard)
                computation := (guard * 100)
                finalResult := (computation + 50)
                return (finalResult)
            }";

            var cfg = BuildCFG(code);

            // All statements become vertices: guard, return1, computation, finalResult, return2
            Assert.Equal(5, cfg.VertexCount());

            var guard = GetAssignmentByName(cfg, "guard");
            var computation = GetAssignmentByName(cfg, "computation");
            var finalResult = GetAssignmentByName(cfg, "finalResult");
            var returns = GetReturnStatements(cfg);
            Assert.Equal(2, returns.Count);

            // Reachable: guard -> first return (1 edge)
            // Unreachable: computation -> finalResult -> second return (2 edges)
            // Total: 3 edges
            Assert.Equal(3, cfg.EdgeCount());

            // Verify first return has no outgoing edges
            var reachable = GetReachableStatements(cfg);
            Assert.Equal(2, reachable.Count); // guard + first return

            var unreachable = GetUnreachableStatements(cfg);
            Assert.Equal(3, unreachable.Count); // computation, finalResult, second return
        }

        // -------------------------------------------------
        // 7. CFG Properties and Invariants
        // -------------------------------------------------

        [Fact(DisplayName = "CFG property: no statement appears twice as vertex")]
        public void CFG_NoStatementAppearsTwice()
        {
            string code = @"{
                x := (1)
                y := (2)
                z := (3)
                return (x + y + z)
            }";

            var cfg = BuildCFG(code);
            var vertices = cfg.GetVertices().ToList();

            // Verify no duplicates (distinct count equals total count)
            Assert.Equal(vertices.Count, vertices.Distinct().Count());
        }

        [Fact(DisplayName = "CFG property: return statements have no outgoing edges")]
        public void CFG_ReturnStatements_HaveNoOutgoingEdges()
        {
            string code = @"{
                a := (1)
                {
                    b := (2)
                    return (b)
                }
                return (a)
            }";

            var cfg = BuildCFG(code);
            var returns = GetReturnStatements(cfg);
            Assert.Equal(2, returns.Count);

            // ALL return statements should have no outgoing edges
            foreach (var ret in returns)
            {
                Assert.Empty(cfg.GetNeighbors(ret));
            }
        }

        [Fact(DisplayName = "CFG property: edge count is at most vertices - 1 for linear flow without early return")]
        public void CFG_LinearFlowEdgeCountProperty()
        {
            string code = @"{
                a := (1)
                b := (2)
                c := (3)
                d := (4)
                e := (5)
                return (a + b + c + d + e)
            }";

            var cfg = BuildCFG(code);

            // Linear flow: edges = vertices - 1
            Assert.Equal(cfg.VertexCount() - 1, cfg.EdgeCount());
        }

        // -------------------------------------------------
        // 8. Integration with Name Analysis
        // -------------------------------------------------

        [Fact(DisplayName = "Full program: CFG builds correctly even with name analysis errors")]
        public void CFG_BuildsCorrectlyDespiteNameAnalysisErrors()
        {
            string code = @"{
                x := (undeclaredVar)
                y := (x + 1)
                return (y)
            }";

            // CFG generation doesn't require name analysis
            var cfg = BuildCFG(code);

            // CFG should still be built correctly
            Assert.Equal(3, cfg.VertexCount());
            Assert.Equal(2, cfg.EdgeCount());
        }

        [Fact(DisplayName = "Full program: CFG and name analysis can work together")]
        public void CFG_WorksWithNameAnalysis()
        {
            string code = @"{
                x := (10)
                y := (x * 2)
                return (y)
            }";

            var ast = Parser.Parser.Parse(code);

            // Run name analysis first
            var scope = new Tuple<SymbolTable<string, object>, Statement>(
                new SymbolTable<string, object>(), null);
            bool analysisSuccess = ast.Accept(_analyzer, scope);
            Assert.True(analysisSuccess);

            // Then build CFG
            var cfg = BuildCFG(code);
            Assert.Equal(3, cfg.VertexCount());
            Assert.Equal(2, cfg.EdgeCount());
        }

        // -------------------------------------------------
        // 9. Large Program Stress Tests
        // -------------------------------------------------

        [Fact(DisplayName = "Full program: large linear sequence builds correctly")]
        public void CFG_LargeLinearSequence_BuildsCorrectly()
        {
            // Build a program with 10 assignments and 1 return
            string code = @"{
                a := (1)
                b := (2)
                c := (3)
                d := (4)
                e := (5)
                f := (6)
                g := (7)
                h := (8)
                i := (9)
                j := (10)
                return (0)
            }";

            var cfg = BuildCFG(code);

            Assert.Equal(11, cfg.VertexCount()); // 10 assignments + 1 return
            Assert.Equal(10, cfg.EdgeCount()); // Linear chain
        }

        [Fact(DisplayName = "Full program: multiple nested levels with mixed statements")]
        public void CFG_MultipleNestedLevelsWithMixedStatements_BuildsCorrectly()
        {
            string code = @"{
                a := (0)
                b := (1)
                c := (2)
                d := (2)
                e := (1)
                f := (2)
                g := (3)
                h := (2)
                i := (0)
                return (0)
            }";

            var cfg = BuildCFG(code);

            // Count all assignment and return statements
            Assert.Equal(10, cfg.VertexCount());
            Assert.Equal(9, cfg.EdgeCount()); // Linear flow through all
        }

        // -------------------------------------------------
        // 10. Start Node and Reachability Verification
        // -------------------------------------------------

        [Fact(DisplayName = "CFG start node is correctly set to first statement")]
        public void CFG_StartNode_IsFirstStatement()
        {
            string code = @"{
                first := (1)
                second := (2)
                return (first)
            }";

            var cfg = BuildCFG(code);

            Assert.NotNull(cfg.Start);
            Assert.IsType<AssignmentStmt>(cfg.Start);

            // Verify start has the correct variable name
            var startAssignment = (AssignmentStmt)cfg.Start;
            Assert.Equal("first", ((VariableNode)startAssignment.Variable).Name);
        }

        [Fact(DisplayName = "CFG: start node when program begins with return")]
        public void CFG_StartNode_WhenProgramBeginsWithReturn()
        {
            string code = @"{
                return (42)
            }";

            var cfg = BuildCFG(code);

            Assert.NotNull(cfg.Start);
            Assert.IsType<ReturnStmt>(cfg.Start);
        }

        [Fact(DisplayName = "CFG: unreachable code after return has its own start")]
        public void CFG_UnreachableCode_HasOwnLinearFlow()
        {
            string code = @"{
                x := (1)
                return (x)
                y := (2)
                z := (3)
                return (z)
            }";

            var cfg = BuildCFG(code);

            // Verify disjoint components
            var reachable = GetReachableStatements(cfg);
            var unreachable = GetUnreachableStatements(cfg);

            // Reachable: x, return(x) = 2
            Assert.Equal(2, reachable.Count);

            // Unreachable: y, z, return(z) = 3
            Assert.Equal(3, unreachable.Count);

            // Unreachable code still forms a linear chain: y -> z -> return(z)
            var y = GetAssignmentByName(cfg, "y");
            var z = GetAssignmentByName(cfg, "z");
            AssertEdgeExists(cfg, y, z);
            // z has one outgoing edge to the unreachable return
            Assert.Single(cfg.GetNeighbors(z));
        }

        // -------------------------------------------------
        // 11. Edge Verification for Specific Patterns
        // -------------------------------------------------

        [Fact(DisplayName = "CFG: verify no self-loops")]
        public void CFG_NoSelfLoops()
        {
            string code = @"{
                x := (1)
                y := (2)
                z := (3)
                return (x + y + z)
            }";

            var cfg = BuildCFG(code);

            foreach (var vertex in cfg.GetVertices())
            {
                Assert.False(cfg.HasEdge(vertex, vertex),
                    $"Found self-loop on {vertex}");
            }
        }

        [Fact(DisplayName = "CFG: verify all edges go forward in program order")]
        public void CFG_AllEdgesGoForward()
        {
            string code = @"{
                a := (1)
                b := (2)
                c := (3)
                return (a + b + c)
            }";

            var cfg = BuildCFG(code);

            // For a linear CFG without returns in middle, each vertex (except return)
            // should have exactly one outgoing edge
            var assignments = GetAssignmentStatements(cfg);
            foreach (var assignment in assignments)
            {
                var neighbors = cfg.GetNeighbors(assignment);
                Assert.Single(neighbors);
            }

            // Return should have no outgoing edges
            var ret = GetReturnStatements(cfg).Single();
            Assert.Empty(cfg.GetNeighbors(ret));
        }

        // -------------------------------------------------
        // 12. Comprehensive Disjoint Graph Tests
        // -------------------------------------------------

        [Fact(DisplayName = "CFG: verify disjoint components with multiple early returns")]
        public void CFG_DisjointComponents_MultipleEarlyReturns()
        {
            string code = @"{
                a := (1)
                return (a)
                b := (2)
                c := (3)
            }";

            var cfg = BuildCFG(code);

            var a = GetAssignmentByName(cfg, "a");
            var b = GetAssignmentByName(cfg, "b");
            var c = GetAssignmentByName(cfg, "c");
            var ret = GetReturnStatements(cfg).Single();

            // Reachable component: a -> ret
            AssertEdgeExists(cfg, a, ret);
            Assert.Empty(cfg.GetNeighbors(ret));

            // Unreachable component: b -> c
            AssertEdgeExists(cfg, b, c);

            // No edge from ret to b (disjoint)
            AssertNoEdge(cfg, ret, b);

            // Total vertices: 4, Total edges: 2
            Assert.Equal(4, cfg.VertexCount());
            Assert.Equal(2, cfg.EdgeCount());
        }

        [Fact(DisplayName = "CFG: deeply nested return creates correctly structured disjoint graph")]
        public void CFG_DeeplyNestedReturn_CorrectDisjointStructure()
        {
            string code = @"{
                outer := (1)
                {
                    {
                        inner := (2)
                        return (inner)
                        dead := (3)
                    }
                    afterInner := (4)
                }
                afterOuter := (5)
            }";

            var cfg = BuildCFG(code);

            var outer = GetAssignmentByName(cfg, "outer");
            var inner = GetAssignmentByName(cfg, "inner");
            var dead = GetAssignmentByName(cfg, "dead");
            var afterInner = GetAssignmentByName(cfg, "afterInner");
            var afterOuter = GetAssignmentByName(cfg, "afterOuter");
            var ret = GetReturnStatements(cfg).Single();

            // Reachable: outer -> inner -> return
            AssertEdgeExists(cfg, outer, inner);
            AssertEdgeExists(cfg, inner, ret);
            Assert.Empty(cfg.GetNeighbors(ret));

            // No edge from ret to dead
            AssertNoEdge(cfg, ret, dead);

            // Unreachable statements are in separate block scopes, so they're isolated
            // dead is in innermost block, afterInner in middle block, afterOuter in outer block
            Assert.Empty(cfg.GetNeighbors(dead));
            Assert.Empty(cfg.GetNeighbors(afterInner));
            Assert.Empty(cfg.GetNeighbors(afterOuter));

            // Verify counts: 6 vertices, 2 edges (outer->inner, inner->ret)
            Assert.Equal(6, cfg.VertexCount());
            Assert.Equal(2, cfg.EdgeCount());

            // Verify reachability
            var reachable = GetReachableStatements(cfg);
            Assert.Equal(3, reachable.Count); // outer, inner, ret

            var unreachable = GetUnreachableStatements(cfg);
            Assert.Equal(3, unreachable.Count); // dead, afterInner, afterOuter
        }
    }
}
