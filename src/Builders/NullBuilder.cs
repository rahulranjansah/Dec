/**
 * NullBuilder.cs
 *
 * NullBuilder implements a null-object style builder for the AST. Every
 * factory method intentionally returns null instead of constructing AST nodes.
 * This is useful for parser diagnostics and tests where you want to exercise
 * parse actions without allocating node objects or when you want to detect
 * which builder methods the parser invokes without building a full tree.
 *
 * Bugs: (no known runtime bugs in this file as provided)
 *
 * @author Rahul & Zach
 * @date 2025-10-09
 */

using System;
using System.Collections.Generic;

namespace AST
{
    #region Null Builder

    /// <summary>
    /// NullBuilder is a concrete builder that overrides the DefaultBuilder's
    /// factory methods to return null. Use this when you need to run the parser
    /// or builder pipeline without actually allocating AST nodes (for example,
    /// to count invocations, test parse flows, or reproduce a minimal memory
    /// footprint).
    ///
    /// Important: callers that consume the result of these factory methods must
    /// tolerate null values (this builder intentionally violates the usual
    /// non-null contract of DefaultBuilder).
    /// </summary>
    public class NullBuilder : DefaultBuilder
    {
        #region Operator node factories (returning null)

        /// <summary>
        /// Intentionally returns null instead of creating a PlusNode.
        /// </summary>
        /// <param name="left">Left operand (ignored)</param>
        /// <param name="right">Right operand (ignored)</param>
        /// <returns>null</returns>
        public override PlusNode CreatePlusNode(ExpressionNode left, ExpressionNode right)
        {
            // Null-object behavior: no allocation performed.
            return null;
        }

        /// <summary>
        /// Intentionally returns null instead of creating a MinusNode.
        /// </summary>
        /// <param name="left">Left operand (ignored)</param>
        /// <param name="right">Right operand (ignored)</param>
        /// <returns>null</returns>
        public override MinusNode CreateMinusNode(ExpressionNode left, ExpressionNode right)
        {
            return null;
        }

        /// <summary>
        /// Intentionally returns null instead of creating a TimesNode.
        /// </summary>
        /// <param name="left">Left operand (ignored)</param>
        /// <param name="right">Right operand (ignored)</param>
        /// <returns>null</returns>
        public override TimesNode CreateTimesNode(ExpressionNode left, ExpressionNode right)
        {
            return null;
        }

        /// <summary>
        /// Intentionally returns null instead of creating a FloatDivNode.
        /// </summary>
        /// <param name="left">Left operand (ignored)</param>
        /// <param name="right">Right operand (ignored)</param>
        /// <returns>null</returns>
        public override FloatDivNode CreateFloatDivNode(ExpressionNode left, ExpressionNode right)
        {
            return null;
        }

        /// <summary>
        /// Intentionally returns null instead of creating an IntDivNode.
        /// </summary>
        /// <param name="left">Left operand (ignored)</param>
        /// <param name="right">Right operand (ignored)</param>
        /// <returns>null</returns>
        public override IntDivNode CreateIntDivNode(ExpressionNode left, ExpressionNode right)
        {
            return null;
        }

        /// <summary>
        /// Intentionally returns null instead of creating a ModulusNode.
        /// </summary>
        /// <param name="left">Left operand (ignored)</param>
        /// <param name="right">Right operand (ignored)</param>
        /// <returns>null</returns>
        public override ModulusNode CreateModulusNode(ExpressionNode left, ExpressionNode right)
        {
            return null;
        }

        /// <summary>
        /// Intentionally returns null instead of creating an ExponentiationNode.
        /// </summary>
        /// <param name="left">Base expression (ignored)</param>
        /// <param name="right">Exponent expression (ignored)</param>
        /// <returns>null</returns>
        public override ExponentiationNode CreateExponentiationNode(ExpressionNode left, ExpressionNode right)
        {
            return null;
        }

        #endregion

        #region Leaf node factories (returning null)

        /// <summary>
        /// Intentionally returns null instead of creating a LiteralNode.
        /// </summary>
        /// <param name="value">Literal value (ignored)</param>
        /// <returns>null</returns>
        public override LiteralNode CreateLiteralNode(object value)
        {
            return null;
        }

        /// <summary>
        /// Intentionally returns null instead of creating a VariableNode.
        /// </summary>
        /// <param name="name">Variable name (ignored)</param>
        /// <returns>null</returns>
        public override VariableNode CreateVariableNode(string name)
        {
            return null;
        }

        #endregion

        #region Statement factories (returning null)

        /// <summary>
        /// Intentionally returns null instead of creating an AssignmentStmt.
        /// </summary>
        /// <param name="variable">Left-hand variable (ignored)</param>
        /// <param name="expression">Right-hand expression (ignored)</param>
        /// <returns>null</returns>
        public override AssignmentStmt CreateAssignmentStmt(VariableNode variable, ExpressionNode expression)
        {
            return null;
        }

        /// <summary>
        /// Intentionally returns null instead of creating a ReturnStmt.
        /// </summary>
        /// <param name="expression">Expression to return (ignored)</param>
        /// <returns>null</returns>
        public override ReturnStmt CreateReturnStmt(ExpressionNode expression)
        {
            return null;
        }

        /// <summary>
        /// Intentionally returns null instead of creating a BlockStmt.
        /// </summary>
        /// <param name="statements">Statements for the block (ignored)</param>
        /// <returns>null</returns>
        public override BlockStmt CreateBlockStmt(List<Statement> statements)
        {
            return null;
        }

        #endregion
    }

    #endregion
}
