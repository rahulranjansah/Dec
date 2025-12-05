using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using AST;
using Containers;
using Optimizer;
using Parser;

namespace Optimizer.Tests
{
    /// <summary>
    /// Integration tests for BFS (Breadth-First Search) on CFG.
    /// Tests follow the pipeline: Parse → CFG Generation → BFS Reachability Analysis.
    /// </summary>
    public class BFSIntegrationTests
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

        /// <summary>
        /// Gets assignment statement by variable name.
        /// </summary>
        private AssignmentStmt GetAssignmentByName(CFG cfg, string varName)
        {
            return cfg.GetVertices()
                .OfType<AssignmentStmt>()
                .First(a => ((VariableNode)a.Variable).Name == varName);
        }

        #endregion

        // -------------------------------------------------
        // 1. Basic BFS Functionality Tests
        // -------------------------------------------------

        [Fact(DisplayName = "BFS: empty CFG returns empty lists")]
        public void BFS_EmptyCFG_ReturnsEmptyLists()
        {
            string code = @"{}";
            var cfg = BuildCFG(code);

            var (reachable, unreachable) = cfg.BreadthFirstSearch();

            Assert.Empty(reachable);
            Assert.Empty(unreachable);
        }

        [Fact(DisplayName = "BFS: single statement is reachable")]
        public void BFS_SingleStatement_IsReachable()
        {
            string code = @"{
                return (42)
            }";

            var cfg = BuildCFG(code);
            var (reachable, unreachable) = cfg.BreadthFirstSearch();

            Assert.Single(reachable);
            Assert.Empty(unreachable);
            Assert.IsType<ReturnStmt>(reachable[0]);
        }

        [Fact(DisplayName = "BFS: linear program - all statements reachable")]
        public void BFS_LinearProgram_AllReachable()
        {
            string code = @"{
                x := (1)
                y := (2)
                z := (3)
                return (x + y + z)
            }";

            var cfg = BuildCFG(code);
            var (reachable, unreachable) = cfg.BreadthFirstSearch();

            Assert.Equal(4, reachable.Count);
            Assert.Empty(unreachable);

            // Verify all statements are present
            var assignments = reachable.OfType<AssignmentStmt>().ToList();
            var returns = reachable.OfType<ReturnStmt>().ToList();
            Assert.Equal(3, assignments.Count);
            Assert.Single(returns);
        }

        // -------------------------------------------------
        // 2. Unreachable Code Detection Tests
        // -------------------------------------------------

        [Fact(DisplayName = "BFS: early return creates unreachable code")]
        public void BFS_EarlyReturn_CreatesUnreachableCode()
        {
            string code = @"{
                x := (1)
                return (x)
                y := (2)
                z := (3)
            }";

            var cfg = BuildCFG(code);
            var (reachable, unreachable) = cfg.BreadthFirstSearch();

            // x and return are reachable
            Assert.Equal(2, reachable.Count);
            // y and z are unreachable
            Assert.Equal(2, unreachable.Count);

            // Verify reachable contains x and return
            var reachableAssignments = reachable.OfType<AssignmentStmt>().ToList();
            Assert.Single(reachableAssignments);
            Assert.Equal("x", ((VariableNode)reachableAssignments[0].Variable).Name);

            // Verify unreachable contains y and z
            var unreachableNames = unreachable
                .OfType<AssignmentStmt>()
                .Select(a => ((VariableNode)a.Variable).Name)
                .ToList();
            Assert.Contains("y", unreachableNames);
            Assert.Contains("z", unreachableNames);
        }

        [Fact(DisplayName = "BFS: multiple returns in sequence - only first reachable")]
        public void BFS_MultipleReturns_OnlyFirstReachable()
        {
            string code = @"{
                x := (1)
                return (x)
                return (x + 1)
                return (x + 2)
            }";

            var cfg = BuildCFG(code);
            var (reachable, unreachable) = cfg.BreadthFirstSearch();

            // x and first return are reachable
            Assert.Equal(2, reachable.Count);
            // Two subsequent returns are unreachable
            Assert.Equal(2, unreachable.Count);

            var reachableReturns = reachable.OfType<ReturnStmt>().ToList();
            Assert.Single(reachableReturns);

            var unreachableReturns = unreachable.OfType<ReturnStmt>().ToList();
            Assert.Equal(2, unreachableReturns.Count);
        }

        // -------------------------------------------------
        // 3. Nested Block Tests
        // -------------------------------------------------

        [Fact(DisplayName = "BFS: nested blocks - all reachable when no early return")]
        public void BFS_NestedBlocks_AllReachable()
        {
            string code = @"{
                a := (1)
                {
                    b := (2)
                    {
                        c := (3)
                    }
                }
                return (a)
            }";

            var cfg = BuildCFG(code);
            var (reachable, unreachable) = cfg.BreadthFirstSearch();

            Assert.Equal(4, reachable.Count);
            Assert.Empty(unreachable);
        }

        [Fact(DisplayName = "BFS: return in nested block creates unreachable code")]
        public void BFS_ReturnInNestedBlock_CreatesUnreachableCode()
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
            var (reachable, unreachable) = cfg.BreadthFirstSearch();

            // a, b, return are reachable
            Assert.Equal(3, reachable.Count);
            // c and d are unreachable
            Assert.Equal(2, unreachable.Count);

            var unreachableNames = unreachable
                .OfType<AssignmentStmt>()
                .Select(a => ((VariableNode)a.Variable).Name)
                .ToList();
            Assert.Contains("c", unreachableNames);
            Assert.Contains("d", unreachableNames);
        }

        // -------------------------------------------------
        // 4. Complex Nested Structure Tests
        // -------------------------------------------------

        [Fact(DisplayName = "BFS: deeply nested with early return")]
        public void BFS_DeeplyNested_WithEarlyReturn()
        {
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
            var (reachable, unreachable) = cfg.BreadthFirstSearch();

            // x, y, z, return are reachable
            Assert.Equal(4, reachable.Count);
            // dead, after, final are unreachable
            Assert.Equal(3, unreachable.Count);

            // Verify specific reachable statements
            var reachableNames = reachable
                .OfType<AssignmentStmt>()
                .Select(a => ((VariableNode)a.Variable).Name)
                .ToList();
            Assert.Contains("x", reachableNames);
            Assert.Contains("y", reachableNames);
            Assert.Contains("z", reachableNames);
        }

        [Fact(DisplayName = "BFS: factorial-like computation - all reachable")]
        public void BFS_FactorialLike_AllReachable()
        {
            string code = @"{
                n := (5)
                result := (1)
                counter := (1)
                {
                    result := (result * counter)
                    counter := (counter + 1)
                    result := (result * counter)
                    return (result)
                }
            }";

            var cfg = BuildCFG(code);
            var (reachable, unreachable) = cfg.BreadthFirstSearch();

            // n, result(1), counter, result*counter, counter+1, result*counter, return = 7
            Assert.Equal(7, reachable.Count);
            Assert.Empty(unreachable);
        }

        [Fact(DisplayName = "BFS: factorial with unreachable continuation")]
        public void BFS_FactorialWithUnreachable_Continuation()
        {
            string code = @"{
                n := (5)
                result := (1)
                {
                    result := (result * 2)
                    return (result)
                    result := (result * 3)
                }
                final := (result)
            }";

            var cfg = BuildCFG(code);
            var (reachable, unreachable) = cfg.BreadthFirstSearch();

            // n, result(1), result(*2), return are reachable = 4
            Assert.Equal(4, reachable.Count);
            // result(*3), final are unreachable = 2
            Assert.Equal(2, unreachable.Count);
        }

        // -------------------------------------------------
        // 5. BFS Order Verification
        // -------------------------------------------------

        [Fact(DisplayName = "BFS: start node is first in reachable list")]
        public void BFS_StartNode_IsFirstInReachableList()
        {
            string code = @"{
                first := (1)
                second := (2)
                third := (3)
                return (first)
            }";

            var cfg = BuildCFG(code);
            var (reachable, unreachable) = cfg.BreadthFirstSearch();

            Assert.Equal(4, reachable.Count);
            Assert.Empty(unreachable);

            // First element should be the start (first assignment)
            Assert.IsType<AssignmentStmt>(reachable[0]);
            var firstStmt = (AssignmentStmt)reachable[0];
            Assert.Equal("first", ((VariableNode)firstStmt.Variable).Name);
        }

        // -------------------------------------------------
        // 6. Edge Cases
        // -------------------------------------------------

        [Fact(DisplayName = "BFS: program starting with return")]
        public void BFS_ProgramStartingWithReturn()
        {
            string code = @"{
                return (42)
                x := (1)
                y := (2)
            }";

            var cfg = BuildCFG(code);
            var (reachable, unreachable) = cfg.BreadthFirstSearch();

            // Only return is reachable
            Assert.Single(reachable);
            Assert.IsType<ReturnStmt>(reachable[0]);

            // x and y are unreachable
            Assert.Equal(2, unreachable.Count);
        }

        [Fact(DisplayName = "BFS: all code after first return is unreachable")]
        public void BFS_AllCodeAfterReturn_IsUnreachable()
        {
            string code = @"{
                a := (1)
                b := (2)
                return (a + b)
                c := (3)
                d := (4)
                e := (5)
                return (c + d + e)
            }";

            var cfg = BuildCFG(code);
            var (reachable, unreachable) = cfg.BreadthFirstSearch();

            // a, b, first return are reachable = 3
            Assert.Equal(3, reachable.Count);
            // c, d, e, second return are unreachable = 4
            Assert.Equal(4, unreachable.Count);
        }

        // -------------------------------------------------
        // 7. Consistency with CFG Structure
        // -------------------------------------------------

        [Fact(DisplayName = "BFS: reachable + unreachable equals total vertices")]
        public void BFS_ReachablePlusUnreachable_EqualsTotalVertices()
        {
            string code = @"{
                a := (1)
                return (a)
                b := (2)
                c := (3)
            }";

            var cfg = BuildCFG(code);
            var (reachable, unreachable) = cfg.BreadthFirstSearch();

            Assert.Equal(cfg.VertexCount(), reachable.Count + unreachable.Count);
        }

        [Fact(DisplayName = "BFS: no overlap between reachable and unreachable")]
        public void BFS_NoOverlap_BetweenReachableAndUnreachable()
        {
            string code = @"{
                x := (1)
                return (x)
                y := (2)
            }";

            var cfg = BuildCFG(code);
            var (reachable, unreachable) = cfg.BreadthFirstSearch();

            // No statement should be in both lists
            var overlap = reachable.Intersect(unreachable).ToList();
            Assert.Empty(overlap);
        }
    }
}

