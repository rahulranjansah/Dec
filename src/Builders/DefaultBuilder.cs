/**
 * DefaultBuilder.cs
 *
 * DefaultBuilder constructs AST nodes and prints simple trace messages.
 * Methods are virtual so DebugBuilder and NullBuilder can override them.
 *
 * Bugs: (no known runtime bugs in this file as provided)
 *
 * @author Rahul & Zachary
 * @date 2025-10-09
 */

using System;
using System.Collections.Generic;

namespace AST
{
    #region Builder Classes

    /// <summary>
    /// DefaultBuilder provides factory methods for creating AST nodes.
    /// Each method is virtual so subclasses (for example DebugBuilder or NullBuilder)
    /// can override construction behaviour (for tracing, null-object patterns, etc.).
    /// </summary>
    public class DefaultBuilder
    {
        #region Operator node factories

        /// <summary>
        /// Create a PlusNode representing addition of two expressions.
        /// </summary>
        /// <param name="left">Left operand expression</param>
        /// <param name="right">Right operand expression</param>
        /// <returns>A new PlusNode representing (left + right)</returns>
        public virtual PlusNode CreatePlusNode(ExpressionNode left, ExpressionNode right)
        {
            // Trace: helpful for debugging the builder's activity in simple runs.
            Console.WriteLine("DefaultBuilder: creating PlusNode");

            // Create and return the AST node representing addition.
            return new PlusNode(left, right);
        }

        /// <summary>
        /// Create a MinusNode representing subtraction of two expressions.
        /// </summary>
        /// <param name="left">Left operand expression</param>
        /// <param name="right">Right operand expression</param>
        /// <returns>A new MinusNode representing (left - right)</returns>
        public virtual MinusNode CreateMinusNode(ExpressionNode left, ExpressionNode right)
        {
            // Trace builder action to stdout for visibility during parsing.
            Console.WriteLine("DefaultBuilder: creating MinusNode");

            return new MinusNode(left, right);
        }

        /// <summary>
        /// Create a TimesNode representing multiplication.
        /// </summary>
        /// <param name="left">Left operand</param>
        /// <param name="right">Right operand</param>
        /// <returns>TimesNode representing (left * right)</returns>
        public virtual TimesNode CreateTimesNode(ExpressionNode left, ExpressionNode right)
        {
            // Emit a small trace message; concrete builders may override to suppress or change.
            Console.WriteLine("DefaultBuilder: creating TimesNode");

            return new TimesNode(left, right);
        }

        /// <summary>
        /// Create a FloatDivNode representing floating-point division.
        /// </summary>
        /// <param name="left">Left operand</param>
        /// <param name="right">Right operand</param>
        /// <returns>FloatDivNode representing (left / right)</returns>
        public virtual FloatDivNode CreateFloatDivNode(ExpressionNode left, ExpressionNode right)
        {
            // Trace for diagnostics.
            Console.WriteLine("DefaultBuilder: creating FloatDivNode");

            return new FloatDivNode(left, right);
        }

        /// <summary>
        /// Create an IntDivNode representing integer division (floor/truncated).
        /// </summary>
        /// <param name="left">Left operand</param>
        /// <param name="right">Right operand</param>
        /// <returns>IntDivNode representing (left // right)</returns>
        public virtual IntDivNode CreateIntDivNode(ExpressionNode left, ExpressionNode right)
        {
            // Trace for diagnostics.
            Console.WriteLine("DefaultBuilder: creating IntDivNode");

            return new IntDivNode(left, right);
        }

        /// <summary>
        /// Create a ModulusNode representing the remainder operator.
        /// </summary>
        /// <param name="left">Left operand</param>
        /// <param name="right">Right operand</param>
        /// <returns>ModulusNode representing (left % right)</returns>
        public virtual ModulusNode CreateModulusNode(ExpressionNode left, ExpressionNode right)
        {
            // Trace message emitted to help follow node creation in logs.
            Console.WriteLine("DefaultBuilder: creating ModulusNode");

            return new ModulusNode(left, right);
        }

        /// <summary>
        /// Create an ExponentiationNode representing exponentiation.
        /// </summary>
        /// <param name="left">Base expression</param>
        /// <param name="right">Exponent expression</param>
        /// <returns>ExponentiationNode representing (left ** right)</returns>
        public virtual ExponentiationNode CreateExponentiationNode(ExpressionNode left, ExpressionNode right)
        {
            // Trace creation for debugging and development runs.
            Console.WriteLine("DefaultBuilder: creating ExponentiationNode");

            return new ExponentiationNode(left, right);
        }

        #endregion

        #region Leaf node factories

        /// <summary>
        /// Create a LiteralNode wrapping a runtime value (int, double, string, etc.).
        /// </summary>
        /// <param name="value">The literal value to wrap</param>
        /// <returns>A new LiteralNode</returns>
        public virtual LiteralNode CreateLiteralNode(object value)
        {
            // Trace: creation of a literal in the AST.
            Console.WriteLine("DefaultBuilder: creating LiteralNode");

            return new LiteralNode(value);
        }

        /// <summary>
        /// Create a VariableNode for an identifier.
        /// </summary>
        /// <param name="name">Identifier name</param>
        /// <returns>A new VariableNode</returns>
        public virtual VariableNode CreateVariableNode(string name)
        {
            // Trace: variable node creation for debugging parser/builder interaction.
            Console.WriteLine("DefaultBuilder: creating VariableNode");

            return new VariableNode(name);
        }

        #endregion

        #region Statement factories

        /// <summary>
        /// Create an AssignmentStmt representing "<variable> := <expression>;".
        /// </summary>
        /// <param name="variable">Left-hand side variable</param>
        /// <param name="expression">Right-hand side expression</param>
        /// <returns>AssignmentStmt AST node</returns>
        public virtual AssignmentStmt CreateAssignmentStmt(VariableNode variable, ExpressionNode expression)
        {
            // Trace activity — useful when verifying AST construction from input.
            Console.WriteLine("DefaultBuilder: creating AssignmentStmt");

            return new AssignmentStmt(variable, expression);
        }

        /// <summary>
        /// Create a ReturnStmt that returns the given expression.
        /// </summary>
        /// <param name="expression">Expression being returned</param>
        /// <returns>ReturnStmt AST node</returns>
        public virtual ReturnStmt CreateReturnStmt(ExpressionNode expression)
        {
            // Trace return statement creation for developer visibility.
            Console.WriteLine("DefaultBuilder: creating ReturnStmt");

            return new ReturnStmt(expression);
        }

        /// <summary>
        /// Create a BlockStmt which groups a sequence of statements inside braces.
        /// </summary>
        /// <param name="statements">Ordered list of statements in the block</param>
        /// <returns>BlockStmt AST node</returns>
        public virtual BlockStmt CreateBlockStmt(List<Statement> statements)
        {
            // Trace block creation — blocks often mark scope changes in ASTs.
            Console.WriteLine("DefaultBuilder: creating BlockStmt");

            return new BlockStmt(statements);
        }

        #endregion
    }

    #endregion
}
