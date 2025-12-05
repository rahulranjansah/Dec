/**
 * Control Flow Graph (CFG) implementation for DEC programs.
 * Extends DiGraph<Statement> to represent program control flow.
 *
 * Key Features:
 * - Start property: Entry point for the CFG (first statement)
 * - BFS: Finds reachable/unreachable statements from Start
 * - Inherits DFS, Transpose, SCC from DiGraph
 *
 * CFG Structure for linear DEC programs:
 *   [stmt1] → [stmt2] → [stmt3] → [return]
 *
 * Example with unreachable code:
 *   Reachable:   [x := 1] → [return x]
 *   Unreachable: [y := 2]  -> [z := 3]  (no edges from reachable)
 *
 * @author Rahul, Rick, Zach
 */

using System.Net.Mail;
using Containers;
using AST;
using System.Runtime.CompilerServices;

namespace Optimizer
{
    /// <summary>
    /// Control Flow Graph for DEC programs.
    /// Vertices are Statements, edges represent control flow.
    /// </summary>
    public class CFG : DiGraph<Statement>
    {
        /// <summary>
        /// Entry point of the CFG (first statement in the program).
        /// BFS uses this to determine reachability.
        /// </summary>
        public Statement? Start { get; set; }

        /// <summary>
        /// Creates an empty CFG with no start node.
        /// </summary>
        public CFG() : base()
        {
            this.Start = null;
        }

        /// <summary>
        /// Creates a CFG with a specified start node.
        /// </summary>
        /// <param name="start">The entry point statement</param>
        public CFG(Statement? start) : base()
        {
            Start = start;
        }

        /// <summary>
        /// Performs Breadth-First Search from Start to find reachable and unreachable statements.
        ///
        /// Algorithm:
        /// 1. Initialize all vertices as WHITE (unvisited)
        /// 2. Start from cfg.Start, mark as PURPLE (in queue), add to reachable
        /// 3. Process queue: for each WHITE neighbor, mark PURPLE, add to reachable
        /// 4. After BFS, any WHITE vertices are unreachable (dead code)
        ///
        /// Key Difference from DFS:
        /// - BFS uses Start as entry point → only finds reachable vertices
        /// - DFS iterates ALL vertices → visits everything including unreachable
        ///
        /// Use Case: Dead code elimination - unreachable statements can be removed.
        /// </summary>
        /// <returns>Tuple of (reachable statements, unreachable statements)</returns>
        public (List<Statement> reachable, List<Statement> unreachable) BreadthFirstSearch()
        {
            List<Statement> reachable = new List<Statement>();
            List<Statement> unreachable = new List<Statement>();

            Queue<Statement> queue = new Queue<Statement>();

            // Initialize all vertices as WHITE (unvisited)
            Dictionary<Statement, Optimizer.CFG.Color> colors = new Dictionary<Statement, Optimizer.CFG.Color>();
            foreach (Statement statement in _adjacencyList.Keys)
            {
                colors[statement] = Color.WHITE;
            }

            // Guard: if Start is null or not in graph, return empty results
            if (this.Start == null || !_adjacencyList.Keys.Contains(this.Start)) { return (reachable, unreachable); }

            // Begin BFS from Start node
            queue.Enqueue(this.Start);
            reachable.Add(this.Start);
            colors[this.Start] = Color.PURPLE;  // PURPLE = in queue / discovered

            // Process queue until empty
            while (!(queue.Count == 0))
            {
                Statement currStatement = queue.Dequeue();

                // Visit all WHITE neighbors
                foreach (Statement neighbour in _adjacencyList[currStatement])
                {
                    if ( colors[neighbour] == Color.WHITE)
                    {
                        colors[neighbour] = Color.PURPLE;
                        reachable.Add(neighbour);
                        queue.Enqueue(neighbour);
                    }

                }
                colors[currStatement] = Color.BLACK;  // BLACK = fully processed
            }

            // Any vertex still WHITE is unreachable (dead code)
            foreach (Statement statement in _adjacencyList.Keys)
            {
                if (colors[statement] == Color.WHITE)
                {
                    unreachable.Add(statement);
                }
            }
            return (reachable, unreachable);
        }

    }
}