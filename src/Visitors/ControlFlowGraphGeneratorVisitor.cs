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
        private DiGraph<Statement>? CFG;

        /// <summary>
        /// Constructs a new CFG generator and initializes the graph
        /// starting at the provided root statement.
        /// </summary>
        /// <param name="start">Entry statement of the program.</param>
        public ControlFlowGraphGeneratorVisitor()
        {
            // Create graph and add initial vertex (must not be a bare block “{”)
            CFG = new DiGraph<Statement>();
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
        /// statement. Unreachable statements are still added as vertices but receive no edges.
        /// </summary>
        /// <param name="node">Block of sequential statements</param>
        /// <param name="prev">The statement executed before this block begins</param>
        /// <returns>The final reachable statement encountered in block</returns>
        public Statement Visit(BlockStmt node, Statement prev)
        {
            // Begin with prior statement as the entry point for the block
            Statement lastStmt = prev;

            foreach (var stmt in node.Statements)
            {
                // If a return has occurred earlier, subsequent statements are unreachable
                if (lastStmt is ReturnStmt)
                {
                    // Still represent unreachable statements in the graph
                    if (stmt is AssignmentStmt || stmt is ReturnStmt)
                    {
                        CFG.AddVertex(stmt);
                    }
                    continue;
                }

                // Visit the next statement in sequence and update last reachable statement
                Statement newLast = stmt.Accept(this, lastStmt);

                if (newLast != null)
                {
                    lastStmt = newLast;
                }
            }

            // The last meaningful statement in the block
            return lastStmt;
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
