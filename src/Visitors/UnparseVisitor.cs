using System;
using System.Text;
using AST;


namespace AST
{
    /// <summary>
    /// Visitor implementation that unparses the AST back to string representation
    /// Uses the generic visitor pattern with indentation level as parameter and string as result
    /// </summary>
    public class UnparseVisitor : IVisitor<int, string>
    {
        /// <summary>
        /// Unparses the given AST node with the specified indentation level
        /// </summary>
        /// <param name="node">The AST node to unparse</param>
        /// <param name="level">The indentation level</param>
        /// <returns>String representation of the node</returns>
        public string Unparse(ExpressionNode node, int level = 0)
        {
            return node.Accept(this, level);
        }

        /// <summary>
        /// Unparses the given statement with the specified indentation level
        /// </summary>
        /// <param name="stmt">The statement to unparse</param>
        /// <param name="level">The indentation level</param>
        /// <returns>String representation of the statement</returns>
        public string Unparse(Statement stmt, int level = 0)
        {
            return stmt.Accept(this, level);
        }

        #region Expression Node Visit Methods

        public string Visit(PlusNode node, int level)
        {
            string left = node.Left.Accept(this, level);
            string right = node.Right.Accept(this, level);
            return $"({left} + {right})";
        }

        // TODO

        #endregion



        #region Statement Node Visit Methods

        // TODO

        #endregion
    }
}