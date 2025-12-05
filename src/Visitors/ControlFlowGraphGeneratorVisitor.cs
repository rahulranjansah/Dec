/**
 * Visitor that generates a Control Flow Graph (CFG) from a given AST.
 * Starting from an initial statement, this visitor walks the AST and
 * records execution order by constructing a directed graph whose
 * edges represent possible flow of control.
 *
 * Bugs: Expression nodes currently return null and do not contribute
 *       to CFG construction. Non-statement nodes are ignored.
 *
 * @author Rahul, Rick, Zach
 * @date 2025-11-20
 */

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using AST;
using Containers;
using Utilities;
using Optimizer;

namespace AST
{
    /// <summary>
    /// Generates a control flow graph (CFG) by visiting AST statement nodes.
    /// Only statements produce vertices and edges; expressions are structurally
    /// visited but currently do not affect the CFG.
    /// </summary>
    public class ControlFlowGraphGeneratorVisitor : IVisitor<Statement, Statement>
    {
        /// <summary>
        /// Internal directed graph storing control-flow edges between statements.
        /// </summary>
        private CFG CFG;

        /// <summary>
        /// Constructs a new CFG generator and initializes the graph
        /// starting at the provided root statement.
        /// </summary>
        /// <param name="start">Entry statement of the program.</param>
        public ControlFlowGraphGeneratorVisitor()
        {
            // Create graph and add initial vertex (must not be a bare block “{”)
            CFG = new CFG();
        }

        #region Binary Operation Visitors (Expressions produce no CFG edges)

        /// <summary>
        /// Visit method for addition node. Expressions do not contribute control flow.
        /// </summary>
        public Statement Visit(PlusNode node, Statement prev)
        {
            return null;
        }

        /// <summary>
        /// Visit method for subtraction node. Does not modify CFG.
        /// </summary>
        public Statement Visit(MinusNode node, Statement prev)
        {
            return null;
        }

        /// <summary>
        /// Visit method for multiplication node. Does not modify CFG.
        /// </summary>
        public Statement Visit(TimesNode node, Statement prev)
        {
            return null;
        }

        /// <summary>
        /// Visit method for float division node.
        /// </summary>
        public Statement Visit(FloatDivNode node, Statement prev)
        {
            return null;
        }

        /// <summary>
        /// Visit method for integer division node.
        /// </summary>
        public Statement Visit(IntDivNode node, Statement prev)
        {
            return null;
        }

        /// <summary>
        /// Visit method for modulus operator node.
        /// </summary>
        public Statement Visit(ModulusNode node, Statement prev)
        {
            return null;
        }

        /// <summary>
        /// Visit method for exponentiation node.
        /// </summary>
        public Statement Visit(ExponentiationNode node, Statement prev)
        {
            return null;
        }

        #endregion

        #region Expression Node Visit Methods (Also no CFG contribution)

        /// <summary>
        /// Visit method for variable references; no control flow added.
        /// </summary>
        public Statement Visit(VariableNode node, Statement prev)
        {
            return null;
        }

        /// <summary>
        /// Visit method for literal values; no control flow added.
        /// </summary>
        public Statement Visit(LiteralNode node, Statement prev)
        {
            return null;
        }

        #endregion

        #region Statement Visitors (The part that actually builds the CFG)

        /// <summary>
        /// Creates a vertex for an assignment statement and links it from previous.
        /// </summary>
        /// <param name="node">Assignment statement</param>
        /// <param name="prev">Statement executed before this one</param>
        /// <returns>The assignment statement (as last executed)</returns>
        public Statement Visit(AssignmentStmt node, Statement prev)
        {
            // Add the new statement as a vertex
            CFG.AddVertex(node);

            if (CFG.Start == null) { CFG.Start = node; }

            // Connect prior statement to this one (linear control flow)
            if (prev != null)
            {
                CFG.AddEdge(prev, node);
            }

            return node;
        }

        /// <summary>
        /// Inserts a return statement into the CFG. Control flow does not continue past returns.
        /// </summary>
        public Statement Visit(ReturnStmt node, Statement prev)
        {
            // Add return as a vertex
            CFG.AddVertex(node);

            if (CFG.Start == null)  { CFG.Start = node; }

            // Link previous statement to this one if valid
            if (prev != null)
            {
                CFG.AddEdge(prev, node);
            }

            // Return acts as a terminator in CFG
            return node;
        }

        /// <summary>
        /// Visits each statement in a block sequentially, creating edges from one
        /// statement to the next. Control flow stops when encountering a return
        /// statement. Unreachable statements are still added as vertices and form
        /// their own disjoint subgraph (no edges from the return to unreachable code).
        /// </summary>
        /// <param name="node">Block of sequential statements</param>
        /// <param name="prev">The statement executed before this block begins</param>
        /// <returns>The final reachable statement encountered in block (the return statement if one was hit)</returns>
        public Statement Visit(BlockStmt node, Statement prev)
        {
            // Begin with prior statement as the entry point for the block
            Statement lastStmt = prev;
            // Track the last reachable statement (to return at the end)
            Statement lastReachable = prev;
            // Track whether we've hit a return (control flow terminator)
            bool hitReturn = prev is ReturnStmt;
            // Track the previous unreachable statement (for building unreachable chain)
            Statement lastUnreachable = null;

            foreach (var stmt in node.Statements)
            {
                if (hitReturn)
                {
                    // After a return, subsequent statements are unreachable
                    // They form their own disjoint subgraph
                    Statement newLast = stmt.Accept(this, lastUnreachable);
                    if (newLast != null)
                    {
                        // If this unreachable statement is a return, it terminates
                        // and creates a disconnected vertex (no edge to next statement)
                        if (newLast is ReturnStmt) { lastUnreachable = null; }
                        else { lastUnreachable = newLast; }
                    }
                }
                else
                {
                    // Normal control flow - visit with the previous statement
                    Statement newLast = stmt.Accept(this, lastStmt);

                    if (newLast != null)
                    {
                        lastStmt = newLast;
                        lastReachable = newLast;
                        // Check if we just hit a return statement
                        if (lastStmt is ReturnStmt)
                        {
                            hitReturn = true;
                        }
                    }
                }
            }

            // Return the last reachable statement (will be the return if one was hit)
            return lastReachable;
        }

        #endregion

        /// <summary>
        /// Retrieves the internally constructed control flow graph.
        /// Intended for testing and CFG inspection.
        /// </summary>
        /// <returns>The directed graph of statements</returns>
        public DiGraph<Statement> GetCFG()
        {
            return CFG;
        }
    }

}
