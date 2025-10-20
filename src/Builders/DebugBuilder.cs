/**
 * DebugBuilder.cs
 *
 * DebugBuilder is a diagnostic builder that extends DefaultBuilder.
 * It validates inputs to each factory method (guarding against nulls)
 * and then delegates to the base DefaultBuilder to perform the actual
 * node creation. This is useful during parser development to catch
 * invalid AST construction early and provide clearer stack traces.
 *
 * Bugs: (no known runtime bugs in this file as provided)
 *
 * @author Rahul & Zach.
 * @date 2025-10-09
 */

using System;
using System.Collections.Generic;

namespace AST
{
    #region Debug Builder

    /// <summary>
    /// NullBuilder that does not create any objects; useful for assessing parsing problems
    /// while avoiding object creation.
    ///
    /// Note: despite the "NullBuilder" remark in the original docstring, this concrete
    /// DebugBuilder performs input validation and delegates to the base builder.
    /// The intent is to "write clean builders"—perform checks here and call the base
    /// implementation which actually constructs the nodes.
    /// </summary>
    public class DebugBuilder : DefaultBuilder
    {
        #region Operator node factories with validation

        /// <summary>
        /// Create a PlusNode after validating that operands are not null.
        /// </summary>
        /// <param name="left">Left operand expression (must not be null)</param>
        /// <param name="right">Right operand expression (must not be null)</param>
        /// <returns>A PlusNode created by the base builder</returns>
        /// <exception cref="ArgumentNullException">Thrown when left or right is null</exception>
        public override PlusNode CreatePlusNode(ExpressionNode left, ExpressionNode right)
        {
            // Validate inputs early — this helps the parser surface issues close to their cause.
            if (left == null)
                throw new ArgumentNullException("left");
            if (right == null)
                throw new ArgumentNullException("right");

            // Delegate the actual node creation to the base implementation.
            return base.CreatePlusNode(left, right);
        }

        /// <summary>
        /// Create a MinusNode after validating operands.
        /// </summary>
        /// <param name="left">Left operand expression (must not be null)</param>
        /// <param name="right">Right operand expression (must not be null)</param>
        /// <returns>A MinusNode created by the base builder</returns>
        /// <exception cref="ArgumentNullException">Thrown when left or right is null</exception>
        public override MinusNode CreateMinusNode(ExpressionNode left, ExpressionNode right)
        {
            if (left == null)
                throw new ArgumentNullException("left");
            if (right == null)
                throw new ArgumentNullException("right");

            return base.CreateMinusNode(left, right);
        }

        /// <summary>
        /// Create a TimesNode after validating operands.
        /// </summary>
        /// <param name="left">Left operand expression (must not be null)</param>
        /// <param name="right">Right operand expression (must not be null)</param>
        /// <returns>A TimesNode created by the base builder</returns>
        /// <exception cref="ArgumentNullException">Thrown when left or right is null</exception>
        public override TimesNode CreateTimesNode(ExpressionNode left, ExpressionNode right)
        {
            if (left == null)
                throw new ArgumentNullException("left");
            if (right == null)
                throw new ArgumentNullException("right");

            return base.CreateTimesNode(left, right);
        }

        /// <summary>
        /// Create a FloatDivNode after validating operands.
        /// </summary>
        /// <param name="left">Left operand expression (must not be null)</param>
        /// <param name="right">Right operand expression (must not be null)</param>
        /// <returns>A FloatDivNode created by the base builder</returns>
        /// <exception cref="ArgumentNullException">Thrown when left or right is null</exception>
        public override FloatDivNode CreateFloatDivNode(ExpressionNode left, ExpressionNode right)
        {
            if (left == null)
                throw new ArgumentNullException("left");
            if (right == null)
                throw new ArgumentNullException("right");

            return base.CreateFloatDivNode(left, right);
        }

        /// <summary>
        /// Create an IntDivNode after validating operands.
        /// </summary>
        /// <param name="left">Left operand expression (must not be null)</param>
        /// <param name="right">Right operand expression (must not be null)</param>
        /// <returns>An IntDivNode created by the base builder</returns>
        /// <exception cref="ArgumentNullException">Thrown when left or right is null</exception>
        public override IntDivNode CreateIntDivNode(ExpressionNode left, ExpressionNode right)
        {
            if (left == null)
                throw new ArgumentNullException("left");
            if (right == null)
                throw new ArgumentNullException("right");

            return base.CreateIntDivNode(left, right);
        }

        /// <summary>
        /// Create a ModulusNode after validating operands.
        /// </summary>
        /// <param name="left">Left operand expression (must not be null)</param>
        /// <param name="right">Right operand expression (must not be null)</param>
        /// <returns>A ModulusNode created by the base builder</returns>
        /// <exception cref="ArgumentNullException">Thrown when left or right is null</exception>
        public override ModulusNode CreateModulusNode(ExpressionNode left, ExpressionNode right)
        {
            if (left == null)
                throw new ArgumentNullException("left");
            if (right == null)
                throw new ArgumentNullException("right");

            return base.CreateModulusNode(left, right);
        }

        /// <summary>
        /// Create an ExponentiationNode after validating operands.
        /// </summary>
        /// <param name="left">Base expression (must not be null)</param>
        /// <param name="right">Exponent expression (must not be null)</param>
        /// <returns>An ExponentiationNode created by the base builder</returns>
        /// <exception cref="ArgumentNullException">Thrown when left or right is null</exception>
        public override ExponentiationNode CreateExponentiationNode(ExpressionNode left, ExpressionNode right)
        {
            if (left == null)
                throw new ArgumentNullException("left");
            if (right == null)
                throw new ArgumentNullException("right");

            return base.CreateExponentiationNode(left, right);
        }

        #endregion

        #region Leaf node factories with validation

        /// <summary>
        /// Create a LiteralNode after validating the value is not null.
        /// </summary>
        /// <param name="value">Literal value to wrap (must not be null)</param>
        /// <returns>A LiteralNode created by the base builder</returns>
        /// <exception cref="ArgumentNullException">Thrown when value is null</exception>
        public override LiteralNode CreateLiteralNode(object value)
        {
            // Defensive check: a null literal likely indicates a bug earlier in the pipeline.
            if (value == null)
            {
                throw new ArgumentNullException("value null");
            }
            return base.CreateLiteralNode(value);
        }

        /// <summary>
        /// Create a VariableNode after validating the name.
        /// </summary>
        /// <param name="name">Identifier name (must not be null)</param>
        /// <returns>A VariableNode created by the base builder</returns>
        /// <exception cref="ArgumentNullException">Thrown when name is null</exception>
        public override VariableNode CreateVariableNode(string name)
        {
            // Guard: variable names should always be non-null strings.
            if (name == null) { throw new ArgumentNullException("name error"); }
            return base.CreateVariableNode(name);
        }

        #endregion

        #region Statement factories with validation

        /// <summary>
        /// Create an AssignmentStmt after validating both sides are present.
        /// </summary>
        /// <param name="variable">Left-hand side variable (must not be null)</param>
        /// <param name="expression">Right-hand side expression (must not be null)</param>
        /// <returns>An AssignmentStmt created by the base builder</returns>
        /// <exception cref="ArgumentNullException">Thrown when variable or expression is null</exception>
        public override AssignmentStmt CreateAssignmentStmt(VariableNode variable, ExpressionNode expression)
        {
            // Provide a helpful message to quickly identify which argument was null.
            if (variable == null || expression == null) { throw new ArgumentNullException("either your variable or expression is null"); }
            return base.CreateAssignmentStmt(variable, expression);
        }

        /// <summary>
        /// Create a ReturnStmt after validating the expression is present.
        /// </summary>
        /// <param name="expression">Expression to return (must not be null)</param>
        /// <returns>A ReturnStmt created by the base builder</returns>
        /// <exception cref="ArgumentNullException">Thrown when expression is null</exception>
        public override ReturnStmt CreateReturnStmt(ExpressionNode expression)
        {
            if (expression == null) { throw new ArgumentNullException("expression errors"); }
            return base.CreateReturnStmt(expression);
        }

        /// <summary>
        /// Create a BlockStmt after validating the provided statement list is not null.
        /// An empty list is acceptable — it represents an empty block.
        /// </summary>
        /// <param name="statements">Ordered list of statements for the block (must not be null)</param>
        /// <returns>A BlockStmt created by the base builder</returns>
        /// <exception cref="ArgumentNullException">Thrown when statements is null</exception>
        public override BlockStmt CreateBlockStmt(List<Statement> statements)
        {
            // Validate that the caller provided a list container for the block's statements.
            if (statements == null)
                throw new ArgumentNullException("Statements not found");

            return base.CreateBlockStmt(statements);
        }

        #endregion
    }

    #endregion
}
