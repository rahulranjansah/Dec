using System.Net.Mail;
using Containers;
using AST;
using System.Runtime.CompilerServices;

namespace Optimizer
{
    public class CFG : DiGraph<Statement>
    {
        public Statement? Start { get; set; } //Starting point of our digraph

        public CFG() : base()
        {
            this.Start = null;
        }

        public CFG(Statement? start) : base()
        {
            Start = start;
        }

        public (List<Statement> reachable, List<Statement> unreachable) BreadthFirstSearch()
        {
            List<Statement> reachable = new List<Statement>();
            List<Statement> unreachable = new List<Statement>();

            Queue<Statement> queue = new Queue<Statement>();

            Dictionary<Statement, Optimizer.CFG.Color> colors = new Dictionary<Statement, Optimizer.CFG.Color>();
            foreach (Statement statement in _adjacencyList.Keys)
            {
                colors[statement] = Color.WHITE;
            }

            queue.Enqueue(this.Start);
            while (!(queue.Count == 0))
            {
                Statement currStatement = queue.Dequeue();
                foreach (Statement neighbour in _adjacencyList[currStatement])
                {
                    if ( colors[neighbour] == Color.WHITE)
                    {
                        colors[neighbour] = Color.PURPLE;
                        reachable.Add(neighbour);
                        queue.Enqueue(neighbour);
                    }

                }
                colors[currStatement] = Color.BLACK;
            }
            return (reachable, unreachable);
        }

    }
}