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

namespace Optimizer.Tests
{
    /// <summary>
    /// Test suite verifying the linear nature of DEC programs by analyzing SCCs in CFGs.
    ///
    /// Key Insight: DEC programs (without loops/cycles) produce linear CFGs where:
    /// - Each statement is its own Strongly Connected Component (SCC)
    /// - Number of SCCs = Number of assignment statements + Number of return statements
    ///
    /// This property holds because in a linear CFG:
    /// - There are no back edges (no loops)
    /// - Control flows strictly from one statement to the next
    /// - Each vertex (statement) can only reach itself, making each a singleton SCC
    ///
    /// Example from Figure 4:
    ///   x := (5)
    ///   y := (10)
    ///   z := (x + y)
    ///   return (z)
    ///
    /// CFG: [x := 5] → [y := 10] → [z := x + y] → [return z]
    /// SCCs: {x := 5}, {y := 10}, {z := x + y}, {return z} = 4 SCCs = 4 statements
    /// </summary>
    public class CFGLinearizationTest
    {
        #region Helper Methods

        /// <summary>
        /// Builds a CFG from program text using the full parsing pipeline.
        /// </summary>
        private CFG BuildCFG(string programText)
        {
            var ast = Parser.Parser.Parse(programText);
            var visitor = new ControlFlowGraphGeneratorVisitor();
            ast.Accept(visitor, null);
            return (CFG)visitor.GetCFG();
        }

        /// <summary>
        /// Counts the total number of statements (assignments + returns) in the CFG.
        /// </summary>
        private int CountStatements(CFG cfg)
        {
            return cfg.GetVertices().Count();
        }

        /// <summary>
        /// Counts only assignment statements in the CFG.
        /// </summary>
        private int CountAssignments(CFG cfg)
        {
            return cfg.GetVertices().OfType<AssignmentStmt>().Count();
        }

        /// <summary>
        /// Counts only return statements in the CFG.
        /// </summary>
        private int CountReturns(CFG cfg)
        {
            return cfg.GetVertices().OfType<ReturnStmt>().Count();
        }

        /// <summary>
        /// Computes the SCCs of the CFG using Kosaraju's algorithm.
        /// </summary>
        private List<List<Statement>> GetSCCs(CFG cfg)
        {
            return cfg.FindStronglyConnectedComponents();
        }

        /// <summary>
        /// Verifies that all SCCs are singletons (size 1), which indicates a linear CFG.
        /// </summary>
        private bool AllSCCsAreSingletons(List<List<Statement>> sccs)
        {
            return sccs.All(scc => scc.Count == 1);
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

        /// <summary>
        /// Finds the SCC containing a specific statement.
        /// </summary>
        private List<Statement> FindSCCContaining(List<List<Statement>> sccs, Statement stmt)
        {
            return sccs.First(scc => scc.Contains(stmt));
        }

        #endregion

        // -------------------------------------------------
        // 1. Figure 4 Example: The Core Test Case
        // -------------------------------------------------

        [Fact(DisplayName = "Figure 4 Example: 4 statements produce 4 SCCs")]
        public void Figure4Example_FourStatements_ProduceFourSCCs()
        {
            // The exact example from Figure 4 in the assignment
            // x := (5), y := (10), z := (x + y), return (z)
            //
            // CFG Structure:
            // [x := 5] → [y := 10] → [z := x+y] → [return z]
            //
            // SCCs: {x:=5}, {y:=10}, {z:=x+y}, {return z} (4 singleton SCCs)
            string code = @"{
                x := (5)
                y := (10)
                z := (x + y)
                return (z)
            }";

            var cfg = BuildCFG(code);
            var sccs = GetSCCs(cfg);

            // Verify statement counts
            Assert.Equal(4, CountStatements(cfg));
            Assert.Equal(3, CountAssignments(cfg));
            Assert.Equal(1, CountReturns(cfg));

            // Core assertion: Number of SCCs equals number of statements
            Assert.Equal(4, sccs.Count);

            // Verify all SCCs are singletons (linear CFG property)
            Assert.True(AllSCCsAreSingletons(sccs),
                "All SCCs should be singletons in a linear CFG");

            // Verify each statement is in its own SCC
            var x = GetAssignmentByName(cfg, "x");
            var y = GetAssignmentByName(cfg, "y");
            var z = GetAssignmentByName(cfg, "z");
            var ret = cfg.GetVertices().OfType<ReturnStmt>().Single();

            var xSCC = FindSCCContaining(sccs, x);
            var ySCC = FindSCCContaining(sccs, y);
            var zSCC = FindSCCContaining(sccs, z);
            var retSCC = FindSCCContaining(sccs, ret);

            Assert.Single(xSCC);
            Assert.Single(ySCC);
            Assert.Single(zSCC);
            Assert.Single(retSCC);

            // Verify SCCs are distinct
            Assert.NotSame(xSCC, ySCC);
            Assert.NotSame(ySCC, zSCC);
            Assert.NotSame(zSCC, retSCC);
        }

        // -------------------------------------------------
        // 2. Simple Linear Programs
        // -------------------------------------------------

        [Fact(DisplayName = "Single return statement: 1 SCC")]
        public void SingleReturn_ProducesOneSCC()
        {
            // CFG Structure: [return 42]
            // SCCs: {return 42} (1 singleton SCC)
            string code = @"{
                return (42)
            }";

            var cfg = BuildCFG(code);
            var sccs = GetSCCs(cfg);

            Assert.Equal(1, CountStatements(cfg));
            Assert.Equal(1, sccs.Count);
            Assert.True(AllSCCsAreSingletons(sccs));
        }

        [Fact(DisplayName = "Single assignment and return: 2 SCCs")]
        public void SingleAssignmentAndReturn_ProducesTwoSCCs()
        {
            // CFG Structure: [x := 42] → [return x]
            // SCCs: {x:=42}, {return x} (2 singleton SCCs)
            string code = @"{
                x := (42)
                return (x)
            }";

            var cfg = BuildCFG(code);
            var sccs = GetSCCs(cfg);

            Assert.Equal(2, CountStatements(cfg));
            Assert.Equal(2, sccs.Count);
            Assert.True(AllSCCsAreSingletons(sccs));
        }

        [Fact(DisplayName = "Linear sequence 3 statements: produces 3 SCCs")]
        public void LinearSequence_ThreeStatements_ProducesThreeSCCs()
        {
            // CFG Structure: [x := 1] → [y := 2] → [return x]
            // SCCs: {x:=1}, {y:=2}, {return x} (3 singleton SCCs)
            string code = @"{
                x := (1)
                y := (2)
                return (x)
            }";

            var cfg = BuildCFG(code);
            var sccs = GetSCCs(cfg);

            Assert.Equal(3, CountStatements(cfg));
            Assert.Equal(3, sccs.Count);
            Assert.True(AllSCCsAreSingletons(sccs));
        }

        [Fact(DisplayName = "Linear sequence 5 statements: produces 5 SCCs")]
        public void LinearSequence_FiveStatements_ProducesFiveSCCs()
        {
            // CFG Structure: [a := 1] → [b := 2] → [c := 3] → [d := 4] → [return a]
            // SCCs: {a:=1}, {b:=2}, {c:=3}, {d:=4}, {return a} (5 singleton SCCs)
            string code = @"{
                a := (1)
                b := (2)
                c := (3)
                d := (4)
                return (a)
            }";

            var cfg = BuildCFG(code);
            var sccs = GetSCCs(cfg);

            Assert.Equal(5, CountStatements(cfg));
            Assert.Equal(5, sccs.Count);
            Assert.True(AllSCCsAreSingletons(sccs));
        }

        // -------------------------------------------------
        // 3. Nested Blocks (Block Folding)
        // -------------------------------------------------

        [Fact(DisplayName = "Nested blocks maintain linear SCC structure")]
        public void NestedBlocks_MaintainLinearSCCStructure()
        {
            // CFG Structure (blocks are folded): [x := 5] → [y := x+10] → [return y]
            // SCCs: {x:=5}, {y:=x+10}, {return y} (3 singleton SCCs)
            string code = @"{
                x := (5)
                {
                    y := (x + 10)
                    return (y)
                }
            }";

            var cfg = BuildCFG(code);
            var sccs = GetSCCs(cfg);

            // Blocks are folded: x, y, return = 3 statements
            Assert.Equal(3, CountStatements(cfg));
            Assert.Equal(3, sccs.Count);
            Assert.True(AllSCCsAreSingletons(sccs));
        }

        [Fact(DisplayName = "Deeply nested blocks: statements equal SCCs")]
        public void DeeplyNestedBlocks_StatementsEqualSCCs()
        {
            // CFG Structure (blocks are folded):
            // [a := 1] → [b := 2] → [c := 3] → [d := 4] → [return a+b+c+d]
            // SCCs: {a:=1}, {b:=2}, {c:=3}, {d:=4}, {return} (5 singleton SCCs)
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
            var sccs = GetSCCs(cfg);

            // 4 assignments + 1 return = 5 statements
            Assert.Equal(5, CountStatements(cfg));
            Assert.Equal(5, sccs.Count);
            Assert.True(AllSCCsAreSingletons(sccs));
        }

        // -------------------------------------------------
        // 4. Programs with Unreachable Code
        // -------------------------------------------------

        [Fact(DisplayName = "Early return with unreachable code: all statements form SCCs")]
        public void EarlyReturn_AllStatementsFormSCCs()
        {
            // CFG Structure (with unreachable code):
            // Reachable:   [x := 10] → [return x]
            // Unreachable: [y := 20]   [z := 30]  (disconnected, no incoming edges)
            // SCCs: {x:=10}, {return x}, {y:=20}, {z:=30} (4 singleton SCCs)
            string code = @"{
                x := (10)
                return (x)
                y := (20)
                z := (30)
            }";

            var cfg = BuildCFG(code);
            var sccs = GetSCCs(cfg);

            // All 4 statements are vertices in the CFG
            Assert.Equal(4, CountStatements(cfg));
            // Each forms its own SCC (even unreachable ones)
            Assert.Equal(4, sccs.Count);
            Assert.True(AllSCCsAreSingletons(sccs));
        }

        [Fact(DisplayName = "Multiple unreachable returns: each is a singleton SCC")]
        public void MultipleUnreachableReturns_EachSingletonSCC()
        {
            // CFG Structure (with unreachable returns):
            // Reachable:   [x := 1] → [return x]
            // Unreachable: [return x+1]   [return x+2]  (disconnected)
            // SCCs: {x:=1}, {return x}, {return x+1}, {return x+2} (4 singleton SCCs)
            string code = @"{
                x := (1)
                return (x)
                return (x + 1)
                return (x + 2)
            }";

            var cfg = BuildCFG(code);
            var sccs = GetSCCs(cfg);

            // 1 assignment + 3 returns = 4 statements
            Assert.Equal(4, CountStatements(cfg));
            Assert.Equal(4, sccs.Count);
            Assert.True(AllSCCsAreSingletons(sccs));
        }

        [Fact(DisplayName = "Complex unreachable structure: SCCs match statement count")]
        public void ComplexUnreachable_SCCsMatchStatementCount()
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
            var sccs = GetSCCs(cfg);

            // All statements are added: init, a, b, c, return, d, e, final = 8
            Assert.Equal(8, CountStatements(cfg));
            Assert.Equal(8, sccs.Count);
            Assert.True(AllSCCsAreSingletons(sccs));
        }

        // -------------------------------------------------
        // 5. Arithmetic Expression Programs
        // -------------------------------------------------

        [Fact(DisplayName = "Program with complex expressions: SCCs count statements only")]
        public void ComplexExpressions_SCCsCountStatementsOnly()
        {
            // CFG Structure (expressions don't create vertices):
            // [x := 3] → [y := (x+5)*2] → [z := y/2] → [result := (x*y)+(z*2)] → [return result]
            // SCCs: {x:=3}, {y:=...}, {z:=...}, {result:=...}, {return} (5 singleton SCCs)
            string code = @"{
                x := (3)
                y := ((x + 5) * 2)
                z := (y / 2)
                result := ((x * y) + (z * 2))
                return (result)
            }";

            var cfg = BuildCFG(code);
            var sccs = GetSCCs(cfg);

            // Expressions don't add CFG vertices, only statements do
            Assert.Equal(5, CountStatements(cfg));
            Assert.Equal(5, sccs.Count);
            Assert.True(AllSCCsAreSingletons(sccs));
        }

        [Fact(DisplayName = "Exponentiation and modulus: linear SCC structure")]
        public void ExponentiationAndModulus_LinearSCCStructure()
        {
            // CFG Structure:
            // [base := 2] → [exp := 10] → [result := base**exp] → [remainder := result%7] → [return remainder]
            // SCCs: {base:=2}, {exp:=10}, {result:=...}, {remainder:=...}, {return} (5 singleton SCCs)
            string code = @"{
                base := (2)
                exp := (10)
                result := (base ** exp)
                remainder := (result % 7)
                return (remainder)
            }";

            var cfg = BuildCFG(code);
            var sccs = GetSCCs(cfg);

            Assert.Equal(5, CountStatements(cfg));
            Assert.Equal(5, sccs.Count);
            Assert.True(AllSCCsAreSingletons(sccs));
        }

        // -------------------------------------------------
        // 6. Large Linear Programs
        // -------------------------------------------------

        [Fact(DisplayName = "10 statements produce 10 SCCs")]
        public void TenStatements_ProduceTenSCCs()
        {
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
                return (a + b + c + d + e + f + g + h + i)
            }";

            var cfg = BuildCFG(code);
            var sccs = GetSCCs(cfg);

            Assert.Equal(10, CountStatements(cfg));
            Assert.Equal(10, sccs.Count);
            Assert.True(AllSCCsAreSingletons(sccs));
        }

        [Fact(DisplayName = "15 statements produce 15 SCCs")]
        public void FifteenStatements_ProduceFifteenSCCs()
        {
            string code = @"{
                aa := (1)
                ab := (2)
                ac := (3)
                ad := (4)
                ae := (5)
                ba := (6)
                bb := (7)
                bc := (8)
                bd := (9)
                be := (10)
                ca := (11)
                cb := (12)
                cc := (13)
                cd := (14)
                return (aa + be)
            }";

            var cfg = BuildCFG(code);
            var sccs = GetSCCs(cfg);

            Assert.Equal(15, CountStatements(cfg));
            Assert.Equal(15, sccs.Count);
            Assert.True(AllSCCsAreSingletons(sccs));
        }

        // -------------------------------------------------
        // 7. Edge Cases
        // -------------------------------------------------

        [Fact(DisplayName = "Empty program: 0 SCCs")]
        public void EmptyProgram_ZeroSCCs()
        {
            string code = @"{}";

            var cfg = BuildCFG(code);
            var sccs = GetSCCs(cfg);

            Assert.Equal(0, CountStatements(cfg));
            Assert.Empty(sccs);
        }

        [Fact(DisplayName = "Variable reassignment: each assignment is a statement")]
        public void VariableReassignment_EachAssignmentIsSCC()
        {
            string code = @"{
                x := (1)
                x := (x + 1)
                x := (x * 2)
                x := (x - 1)
                return (x)
            }";

            var cfg = BuildCFG(code);
            var sccs = GetSCCs(cfg);

            // 4 assignments + 1 return = 5 statements
            Assert.Equal(5, CountStatements(cfg));
            Assert.Equal(5, sccs.Count);
            Assert.True(AllSCCsAreSingletons(sccs));
        }

        // -------------------------------------------------
        // 8. SCC Property Verification
        // -------------------------------------------------

        [Fact(DisplayName = "Linear CFG: all SCCs are singletons")]
        public void LinearCFG_AllSCCsAreSingletons()
        {
            // CFG Structure:
            // [first := 1] → [second := 2] → [third := 3] → [fourth := 4] → [fifth := 5] → [return first+fifth]
            // Linear = no back edges = each vertex is its own SCC
            string code = @"{
                first := (1)
                second := (2)
                third := (3)
                fourth := (4)
                fifth := (5)
                return (first + fifth)
            }";

            var cfg = BuildCFG(code);
            var sccs = GetSCCs(cfg);

            // Primary property: in a linear CFG, every SCC has exactly 1 element
            foreach (var scc in sccs)
            {
                Assert.Single(scc);
            }
        }

        [Fact(DisplayName = "Each statement appears in exactly one SCC")]
        public void EachStatement_InExactlyOneSCC()
        {
            // CFG Structure:
            // [a := 1] → [b := a+1] → [c := b+1] → [d := c+1] → [return d]
            // Each statement appears in exactly one SCC (partition property)
            string code = @"{
                a := (1)
                b := (a + 1)
                c := (b + 1)
                d := (c + 1)
                return (d)
            }";

            var cfg = BuildCFG(code);
            var sccs = GetSCCs(cfg);
            var allStatements = cfg.GetVertices().ToList();

            // Collect all statements from all SCCs
            var statementsInSCCs = sccs.SelectMany(scc => scc).ToList();

            // Every statement should appear exactly once across all SCCs
            Assert.Equal(allStatements.Count, statementsInSCCs.Count);
            foreach (var stmt in allStatements)
            {
                Assert.Single(statementsInSCCs.Where(s => s == stmt));
            }
        }

        // -------------------------------------------------
        // 9. Formula Verification: SCCs = Assignments + Returns
        // -------------------------------------------------

        [Fact(DisplayName = "Formula: 0 assignments + 1 return = 1 SCC")]
        public void Formula_ZeroAssignmentsOneReturn_OneSCC()
        {
            string code = @"{
                return (0)
            }";

            var cfg = BuildCFG(code);
            var sccs = GetSCCs(cfg);

            Assert.Equal(0, CountAssignments(cfg));
            Assert.Equal(1, CountReturns(cfg));
            Assert.Equal(1, sccs.Count);
            Assert.True(AllSCCsAreSingletons(sccs));
        }

        [Fact(DisplayName = "Formula: 1 assignment + 1 return = 2 SCCs")]
        public void Formula_OneAssignmentOneReturn_TwoSCCs()
        {
            string code = @"{
                va := (1)
                return (va)
            }";

            var cfg = BuildCFG(code);
            var sccs = GetSCCs(cfg);

            Assert.Equal(1, CountAssignments(cfg));
            Assert.Equal(1, CountReturns(cfg));
            Assert.Equal(2, sccs.Count);
            Assert.True(AllSCCsAreSingletons(sccs));
        }

        [Fact(DisplayName = "Formula: 3 assignments + 1 return = 4 SCCs (Figure 4 case)")]
        public void Formula_ThreeAssignmentsOneReturn_FourSCCs()
        {
            string code = @"{
                va := (0)
                vb := (1)
                vc := (2)
                return (va)
            }";

            var cfg = BuildCFG(code);
            var sccs = GetSCCs(cfg);

            Assert.Equal(3, CountAssignments(cfg));
            Assert.Equal(1, CountReturns(cfg));
            Assert.Equal(4, sccs.Count);
            Assert.True(AllSCCsAreSingletons(sccs));
        }

        [Fact(DisplayName = "Formula: 5 assignments + 1 return = 6 SCCs")]
        public void Formula_FiveAssignmentsOneReturn_SixSCCs()
        {
            string code = @"{
                va := (0)
                vb := (1)
                vc := (2)
                vd := (3)
                ve := (4)
                return (va)
            }";

            var cfg = BuildCFG(code);
            var sccs = GetSCCs(cfg);

            Assert.Equal(5, CountAssignments(cfg));
            Assert.Equal(1, CountReturns(cfg));
            Assert.Equal(6, sccs.Count);
            Assert.True(AllSCCsAreSingletons(sccs));
        }

        [Fact(DisplayName = "Formula: 1 assignment + 3 returns = 4 SCCs (unreachable returns)")]
        public void Formula_OneAssignmentThreeReturns_FourSCCs()
        {
            string code = @"{
                va := (1)
                return (va)
                return (0)
                return (1)
            }";

            var cfg = BuildCFG(code);
            var sccs = GetSCCs(cfg);

            Assert.Equal(1, CountAssignments(cfg));
            Assert.Equal(3, CountReturns(cfg));
            Assert.Equal(4, sccs.Count);
            Assert.True(AllSCCsAreSingletons(sccs));
        }

        [Fact(DisplayName = "Formula: 10 assignments + 1 return = 11 SCCs")]
        public void Formula_TenAssignmentsOneReturn_ElevenSCCs()
        {
            string code = @"{
                va := (0)
                vb := (1)
                vc := (2)
                vd := (3)
                ve := (4)
                vf := (5)
                vg := (6)
                vh := (7)
                vi := (8)
                vj := (9)
                return (va)
            }";

            var cfg = BuildCFG(code);
            var sccs = GetSCCs(cfg);

            Assert.Equal(10, CountAssignments(cfg));
            Assert.Equal(1, CountReturns(cfg));
            Assert.Equal(11, sccs.Count);
            Assert.True(AllSCCsAreSingletons(sccs));
        }

        // -------------------------------------------------
        // 10. Real-World Pattern Programs
        // -------------------------------------------------

        [Fact(DisplayName = "Factorial-like computation: linear SCCs")]
        public void FactorialLikeComputation_LinearSCCs()
        {
            string code = @"{
                n := (5)
                result := (1)
                result := (result * 1)
                result := (result * 2)
                result := (result * 3)
                result := (result * 4)
                result := (result * 5)
                return (result)
            }";

            var cfg = BuildCFG(code);
            var sccs = GetSCCs(cfg);

            // 7 assignments + 1 return = 8 statements
            Assert.Equal(8, CountStatements(cfg));
            Assert.Equal(8, sccs.Count);
            Assert.True(AllSCCsAreSingletons(sccs));
        }

        [Fact(DisplayName = "Fibonacci-like initialization: linear SCCs")]
        public void FibonacciLikeInit_LinearSCCs()
        {
            string code = @"{
                fibA := (0)
                fibB := (1)
                fibC := (fibA + fibB)
                fibD := (fibB + fibC)
                fibE := (fibC + fibD)
                fibF := (fibD + fibE)
                return (fibF)
            }";

            var cfg = BuildCFG(code);
            var sccs = GetSCCs(cfg);

            // 6 assignments + 1 return = 7 statements
            Assert.Equal(7, CountStatements(cfg));
            Assert.Equal(7, sccs.Count);
            Assert.True(AllSCCsAreSingletons(sccs));
        }

        [Fact(DisplayName = "Swap-like pattern: linear SCCs")]
        public void SwapLikePattern_LinearSCCs()
        {
            string code = @"{
                x := (10)
                y := (20)
                temp := (x)
                x := (y)
                y := (temp)
                return (x + y)
            }";

            var cfg = BuildCFG(code);
            var sccs = GetSCCs(cfg);

            // 5 assignments + 1 return = 6 statements
            Assert.Equal(6, CountStatements(cfg));
            Assert.Equal(6, sccs.Count);
            Assert.True(AllSCCsAreSingletons(sccs));
        }

        // -------------------------------------------------
        // 11. Consistency Checks
        // -------------------------------------------------

        [Fact(DisplayName = "SCC count consistency: recomputing gives same result")]
        public void SCCCountConsistency_RecomputeGivesSameResult()
        {
            string code = @"{
                a := (1)
                b := (2)
                c := (3)
                return (a + b + c)
            }";

            var cfg = BuildCFG(code);

            // Compute SCCs multiple times
            var sccs1 = GetSCCs(cfg);
            var sccs2 = GetSCCs(cfg);
            var sccs3 = GetSCCs(cfg);

            Assert.Equal(sccs1.Count, sccs2.Count);
            Assert.Equal(sccs2.Count, sccs3.Count);
            Assert.Equal(4, sccs1.Count);
        }

        [Fact(DisplayName = "Vertex count equals sum of SCC sizes")]
        public void VertexCount_EqualsSumOfSCCSizes()
        {
            string code = @"{
                x := (1)
                y := (2)
                z := (3)
                w := (4)
                return (x + y + z + w)
            }";

            var cfg = BuildCFG(code);
            var sccs = GetSCCs(cfg);

            int totalSCCSize = sccs.Sum(scc => scc.Count);
            Assert.Equal(cfg.VertexCount(), totalSCCSize);
        }

        // -------------------------------------------------
        // 12. Combined Integration Tests
        // -------------------------------------------------

        [Fact(DisplayName = "Full integration: parse, build CFG, compute SCCs, verify linearity")]
        public void FullIntegration_ParseBuildComputeVerify()
        {
            // Complex program combining multiple features
            string code = @"{
                init := (0)
                {
                    x := (5)
                    y := (10)
                    {
                        z := (x + y)
                        result := (z * 2)
                    }
                }
                final := (result + 1)
                return (final)
            }";

            var cfg = BuildCFG(code);
            var sccs = GetSCCs(cfg);

            // 6 assignments + 1 return = 7 statements
            Assert.Equal(7, CountStatements(cfg));
            Assert.Equal(7, sccs.Count);
            Assert.True(AllSCCsAreSingletons(sccs));

            // Verify linear CFG structure (edges = vertices - 1 for connected graph)
            Assert.Equal(cfg.VertexCount() - 1, cfg.EdgeCount());
        }

        // -------------------------------------------------
        // 13. Reachability and Unreachability Tests
        // -------------------------------------------------

        [Fact(DisplayName = "BFS identifies reachable statements from Start")]
        public void BFS_IdentifiesReachableStatements()
        {
            // CFG Structure:
            // [x := 1] → [y := 2] → [return x+y]
            // All statements reachable from Start (x := 1)
            string code = @"{
                x := (1)
                y := (2)
                return (x + y)
            }";

            var cfg = BuildCFG(code);
            var (reachable, unreachable) = cfg.BreadthFirstSearch();

            // All statements should be reachable in a linear program
            Assert.Equal(3, reachable.Count);
            Assert.Empty(unreachable);

            // Verify specific statements are reachable
            var x = GetAssignmentByName(cfg, "x");
            var y = GetAssignmentByName(cfg, "y");
            var ret = cfg.GetVertices().OfType<ReturnStmt>().Single();

            Assert.Contains(x, reachable);
            Assert.Contains(y, reachable);
            Assert.Contains(ret, reachable);
        }

        [Fact(DisplayName = "BFS correctly identifies unreachable code after return")]
        public void BFS_IdentifiesUnreachableCodeAfterReturn()
        {
            // CFG Structure:
            // Reachable:   [x := 1] → [return x]
            // Unreachable: [y := 2]   [z := 3]  (no incoming edges from reachable)
            string code = @"{
                x := (1)
                return (x)
                y := (2)
                z := (3)
            }";

            var cfg = BuildCFG(code);
            var (reachable, unreachable) = cfg.BreadthFirstSearch();

            // x and first return are reachable
            Assert.Equal(2, reachable.Count);

            // y and z are unreachable
            Assert.Equal(2, unreachable.Count);

            var x = GetAssignmentByName(cfg, "x");
            var y = GetAssignmentByName(cfg, "y");
            var z = GetAssignmentByName(cfg, "z");
            var ret = cfg.GetVertices().OfType<ReturnStmt>().First();

            Assert.Contains(x, reachable);
            Assert.Contains(ret, reachable);
            Assert.Contains(y, unreachable);
            Assert.Contains(z, unreachable);
        }

        [Fact(DisplayName = "Unreachable code forms disconnected SCCs")]
        public void UnreachableCode_FormsDisconnectedSCCs()
        {
            // CFG Structure:
            // Reachable:   [a := 1] → [return a]
            // Unreachable: [b := 2]   [c := 3]  (disconnected components)
            // 4 separate SCCs (no cycles, all singletons)
            string code = @"{
                a := (1)
                return (a)
                b := (2)
                c := (3)
            }";

            var cfg = BuildCFG(code);
            var sccs = GetSCCs(cfg);
            var (reachable, unreachable) = cfg.BreadthFirstSearch();

            // All 4 statements form SCCs
            Assert.Equal(4, sccs.Count);

            // Each unreachable statement is its own SCC
            foreach (var stmt in unreachable)
            {
                var scc = FindSCCContaining(sccs, stmt);
                Assert.Single(scc);
            }

            // Verify unreachable statements are NOT in same SCC as reachable ones
            var a = GetAssignmentByName(cfg, "a");
            var b = GetAssignmentByName(cfg, "b");
            var aSCC = FindSCCContaining(sccs, a);
            var bSCC = FindSCCContaining(sccs, b);
            Assert.NotSame(aSCC, bSCC);
        }

        // -------------------------------------------------
        // 14. Multiple Return Statements (Disconnected Graphs)
        // -------------------------------------------------

        [Fact(DisplayName = "Two returns create two disconnected terminal nodes")]
        public void TwoReturns_CreateDisconnectedTerminals()
        {
            // CFG Structure:
            // Reachable:   [x := 1] → [return x]
            // Unreachable: [return 0]  (disconnected terminal)
            // Two returns form two separate disconnected SCCs
            string code = @"{
                x := (1)
                return (x)
                return (0)
            }";

            var cfg = BuildCFG(code);
            var sccs = GetSCCs(cfg);
            var (reachable, unreachable) = cfg.BreadthFirstSearch();

            // 3 statements total
            Assert.Equal(3, CountStatements(cfg));
            Assert.Equal(3, sccs.Count);

            // First return is reachable, second is not
            var returns = cfg.GetVertices().OfType<ReturnStmt>().ToList();
            Assert.Equal(2, returns.Count);

            // One return should be reachable, one unreachable
            Assert.Single(reachable.OfType<ReturnStmt>());
            Assert.Single(unreachable.OfType<ReturnStmt>());

            // Each return is its own SCC (no connection between them)
            var ret1SCC = FindSCCContaining(sccs, returns[0]);
            var ret2SCC = FindSCCContaining(sccs, returns[1]);
            Assert.NotSame(ret1SCC, ret2SCC);
        }

        [Fact(DisplayName = "Three returns: first reachable, others unreachable")]
        public void ThreeReturns_FirstReachableOthersUnreachable()
        {
            // CFG Structure:
            // Reachable:   [x := 10] → [return x]
            // Unreachable: [return x+1]   [return x+2]  (two disconnected terminals)
            string code = @"{
                x := (10)
                return (x)
                return (x + 1)
                return (x + 2)
            }";

            var cfg = BuildCFG(code);
            var (reachable, unreachable) = cfg.BreadthFirstSearch();

            // x and first return are reachable
            Assert.Equal(2, reachable.Count);

            // 2 returns are unreachable
            Assert.Equal(2, unreachable.Count);
            Assert.Equal(2, unreachable.OfType<ReturnStmt>().Count());
        }

        // -------------------------------------------------
        // 15. CFG Edge Structure Verification
        // -------------------------------------------------

        [Fact(DisplayName = "Linear CFG has correct edge structure: stmt1 → stmt2 → stmt3")]
        public void LinearCFG_HasCorrectEdgeStructure()
        {
            // CFG Structure verified:
            //     [x := 1] → [y := 2] → [return y]
            //
            // Edges: (x, y), (y, return)
            // NO reverse edges: NOT (y, x), NOT (return, y)
            string code = @"{
                x := (1)
                y := (2)
                return (y)
            }";

            var cfg = BuildCFG(code);

            var x = GetAssignmentByName(cfg, "x");
            var y = GetAssignmentByName(cfg, "y");
            var ret = cfg.GetVertices().OfType<ReturnStmt>().Single();

            // Verify edges: x → y → return
            Assert.True(cfg.HasEdge(x, y), "Should have edge x → y");
            Assert.True(cfg.HasEdge(y, ret), "Should have edge y → return");

            // Verify NO reverse edges (linear CFG)
            Assert.False(cfg.HasEdge(y, x), "Should NOT have edge y → x");
            Assert.False(cfg.HasEdge(ret, y), "Should NOT have edge return → y");

            // Start should be first statement
            Assert.Equal(x, cfg.Start);
        }

        [Fact(DisplayName = "Unreachable statements have no incoming edges from reachable")]
        public void UnreachableStatements_NoIncomingEdgesFromReachable()
        {
            string code = @"{
                a := (1)
                return (a)
                b := (2)
            }";

            var cfg = BuildCFG(code);
            var (reachable, unreachable) = cfg.BreadthFirstSearch();

            var b = GetAssignmentByName(cfg, "b");
            Assert.Contains(b, unreachable);

            // No reachable statement should have an edge to b
            foreach (var stmt in reachable)
            {
                Assert.False(cfg.HasEdge(stmt, b),
                    $"Reachable statement should not have edge to unreachable statement b");
            }
        }

        // -------------------------------------------------
        // 16. Statement-SCC Membership Verification
        // -------------------------------------------------

        [Fact(DisplayName = "Each specific statement is in its own distinct SCC")]
        public void EachStatement_InOwnDistinctSCC()
        {
            // CFG Structure:
            // [a := 1] → [b := 2] → [c := a+b] → [return c]
            // Each statement = 1 SCC, all distinct (no shared SCCs)
            string code = @"{
                a := (1)
                b := (2)
                c := (a + b)
                return (c)
            }";

            var cfg = BuildCFG(code);
            var sccs = GetSCCs(cfg);

            var a = GetAssignmentByName(cfg, "a");
            var b = GetAssignmentByName(cfg, "b");
            var c = GetAssignmentByName(cfg, "c");
            var ret = cfg.GetVertices().OfType<ReturnStmt>().Single();

            // Get each statement's SCC
            var aSCC = FindSCCContaining(sccs, a);
            var bSCC = FindSCCContaining(sccs, b);
            var cSCC = FindSCCContaining(sccs, c);
            var retSCC = FindSCCContaining(sccs, ret);

            // Each SCC is singleton
            Assert.Single(aSCC);
            Assert.Single(bSCC);
            Assert.Single(cSCC);
            Assert.Single(retSCC);

            // All SCCs are distinct (no two statements share an SCC)
            var allSCCs = new[] { aSCC, bSCC, cSCC, retSCC };
            for (int i = 0; i < allSCCs.Length; i++)
            {
                for (int j = i + 1; j < allSCCs.Length; j++)
                {
                    Assert.NotSame(allSCCs[i], allSCCs[j]);
                }
            }
        }

        [Fact(DisplayName = "No two statements share an SCC in linear program")]
        public void NoTwoStatements_ShareSCC_InLinearProgram()
        {
            // CFG Structure:
            // [first := 1] → [second := first+1] → [third := second+1] → [fourth := third+1] → [fifth := fourth+1] → [return fifth]
            // 6 statements, 6 SCCs, all singletons (no two share an SCC)
            string code = @"{
                first := (1)
                second := (first + 1)
                third := (second + 1)
                fourth := (third + 1)
                fifth := (fourth + 1)
                return (fifth)
            }";

            var cfg = BuildCFG(code);
            var sccs = GetSCCs(cfg);
            var allStatements = cfg.GetVertices().ToList();

            // Check that no two different statements are in the same SCC
            for (int i = 0; i < allStatements.Count; i++)
            {
                for (int j = i + 1; j < allStatements.Count; j++)
                {
                    var scc1 = FindSCCContaining(sccs, allStatements[i]);
                    var scc2 = FindSCCContaining(sccs, allStatements[j]);
                    Assert.NotSame(scc1, scc2);
                }
            }
        }

        // -------------------------------------------------
        // 17. Linearization Property: No Back Edges
        // -------------------------------------------------

        [Fact(DisplayName = "Linear CFG has no back edges (DAG property)")]
        public void LinearCFG_HasNoBackEdges()
        {
            // CFG Structure (DAG - no cycles):
            // [a := 1] → [b := a] → [c := b] → [d := c] → [return d]
            //
            // NO back edges: ∄ edge (v, u) where u precedes v
            // This is why each vertex is its own SCC
            string code = @"{
                a := (1)
                b := (a)
                c := (b)
                d := (c)
                return (d)
            }";

            var cfg = BuildCFG(code);
            var statements = cfg.GetVertices().ToList();

            // In a linear CFG, for any edge u → v, v should come "after" u
            // This means there should be no way to reach an earlier statement from a later one
            foreach (var stmt in statements)
            {
                var neighbors = cfg.GetNeighbors(stmt);
                foreach (var neighbor in neighbors)
                {
                    // neighbor should not have an edge back to stmt (no cycles)
                    Assert.False(cfg.HasEdge(neighbor, stmt),
                        "Linear CFG should not have back edges");
                }
            }
        }

        [Fact(DisplayName = "Linear CFG edge count equals vertex count minus one (for connected portion)")]
        public void LinearCFG_EdgeCount_ForConnectedPortion()
        {
            string code = @"{
                x := (1)
                y := (2)
                z := (3)
                return (z)
            }";

            var cfg = BuildCFG(code);
            var (reachable, unreachable) = cfg.BreadthFirstSearch();

            // For fully connected linear graph: edges = vertices - 1
            if (unreachable.Count == 0)
            {
                Assert.Equal(cfg.VertexCount() - 1, cfg.EdgeCount());
            }
        }

        // -------------------------------------------------
        // 18. SCCs Partition Property
        // -------------------------------------------------

        [Fact(DisplayName = "SCCs partition all CFG vertices exactly once")]
        public void SCCs_PartitionAllVertices_ExactlyOnce()
        {
            // CFG Structure (with unreachable code):
            // Reachable:   [a := 1] → [b := 2] → [return a]
            // Unreachable: [c := 3]   [return b]  (disconnected)
            //
            // SCCs partition: every vertex in exactly one SCC
            string code = @"{
                a := (1)
                b := (2)
                return (a)
                c := (3)
                return (b)
            }";

            var cfg = BuildCFG(code);
            var sccs = GetSCCs(cfg);
            var allStatements = cfg.GetVertices().ToList();

            // Collect all statements from all SCCs
            var statementsFromSCCs = sccs.SelectMany(scc => scc).ToList();

            // Total count should match
            Assert.Equal(allStatements.Count, statementsFromSCCs.Count);

            // Each statement appears exactly once
            foreach (var stmt in allStatements)
            {
                int occurrences = statementsFromSCCs.Count(s => s == stmt);
                Assert.Equal(1, occurrences);
            }
        }

        // -------------------------------------------------
        // 19. Start Node Verification
        // -------------------------------------------------

        [Fact(DisplayName = "CFG Start is the first statement")]
        public void CFG_Start_IsFirstStatement()
        {
            // CFG Structure:
            // Start → [first := 100] → [second := 200] → [return first]
            // cfg.Start should point to [first := 100]
            string code = @"{
                first := (100)
                second := (200)
                return (first)
            }";

            var cfg = BuildCFG(code);
            var first = GetAssignmentByName(cfg, "first");

            Assert.NotNull(cfg.Start);
            Assert.Equal(first, cfg.Start);
        }

        [Fact(DisplayName = "Start node is always reachable")]
        public void StartNode_IsAlwaysReachable()
        {
            // CFG Structure:
            // Reachable:   [start := 0] → [return start]
            // Unreachable: [dead := 1]
            // Start node is always in reachable set
            string code = @"{
                start := (0)
                return (start)
                dead := (1)
            }";

            var cfg = BuildCFG(code);
            var (reachable, unreachable) = cfg.BreadthFirstSearch();

            Assert.Contains(cfg.Start, reachable);
            Assert.DoesNotContain(cfg.Start, unreachable);
        }
    }
}

