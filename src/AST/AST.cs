/**
 * A lightweight AST (Abstract Syntax Tree) representation used for unparsing
 * (converting AST back to source-like text). Contains statements and expression
 * node types with simple pretty-printing (Unparse) methods.
 *
 * Bugs: (no known runtime bugs in this file as provided)
 *
 * @author Rahul & Zachary
 * @date 2025-10-09
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Utilities;
using Containers;


namespace AST
{
    public interface IVisitor<TParam, TResult>
    {
        // Expression nodes
        TResult Visit(PlusNode node, TParam param);
        TResult Visit(MinusNode node, TParam param);
        TResult Visit(TimesNode node, TParam param);
        TResult Visit(FloatDivNode node, TParam param);
        TResult Visit(IntDivNode node, TParam param);
        TResult Visit(ModulusNode node, TParam param);
        TResult Visit(ExponentiationNode node, TParam param);
        TResult Visit(LiteralNode node, TParam param);
        TResult Visit(VariableNode node, TParam param);

        // Statement nodes
        TResult Visit(AssignmentStmt node, TParam param);
        TResult Visit(ReturnStmt node, TParam param);
        TResult Visit(BlockStmt node, TParam param);
    }

    #region Statements

    /// <summary>
    /// Base abstract class for all statement nodes in the AST.
    /// Provides a Unparse method that derived classes implement to return a
    /// textual representation of the statement. The optional 'level' parameter
    /// is used for indentation depth when pretty-printing nested blocks.
    /// </summary>
    public abstract class Statement
    {
        /// <summary>
        /// Produce a textual (source-like) representation of this statement.
        /// </summary>
        /// <param name="level">Indentation level (0 = top-level)</param>
        /// <returns>Unparsed string for this statement</returns>
        public abstract string Unparse(int level = 0);
    }

    /// <summary>
    /// Represents a block statement: a sequence of statements enclosed in braces.
    /// Example:
    /// {
    ///     stmt1;
    ///     stmt2;
    /// }
    /// </summary>
    public class BlockStmt : Statement
    {
        /// <summary>
        /// The contained statements in this block. Preserved in insertion order.
        /// </summary>
        public List<Statement> Statements { get; }
        public Containers.SymbolTable<string, object> SymbolTable { get; }

        /// <summary>
        /// Create a new block statement containing the given list of statements.
        /// </summary>
        /// <param name="statements">List of contained statements (may be empty)</param>
        public BlockStmt(List<Statement> statements)
        {
            SymbolTable = new Containers.SymbolTable<string, object>();
            Statements = statements;
        }
        public BlockStmt(Containers.SymbolTable<string, object> symbolTable)
        {
            SymbolTable = symbolTable;
            Statements = new List<Statement>();
        }

        public void AddStatement(Statement stmt)
        {
            Statements.Add(stmt);
        }

        /// <summary>
        /// Unparse the block with correct indentation and newlines.
        /// Uses GeneralUtils.GetIndentation to obtain the indent string for the
        /// provided level. Each inner statement is unparsed at level+1.
        /// </summary>
        /// <param name="level">Indentation level for the opening brace</param>
        /// <returns>Block text including braces and inner statements</returns>
        public override string Unparse(int level = 0)
        {
            // Compute indentation for this block level.
            string indent = GeneralUtils.GetIndentation(level);

            // Start the block with an opening brace and newline.
            string result = indent + "{\n";

            // Unparse each statement in the block with one additional indent level.
            // We append a newline after each unparsed inner statement to keep
            // one statement per line in the resulting string.
            foreach (var lineblock in Statements)
            {
                result += lineblock.Unparse(level + 1) + "\n";
            }

            // Close the block at the original indentation level.
            result += indent + "}";
            return result;
        }
    }

    /// <summary>
    /// Represents an assignment statement of the form: <variable> := <expression>;
    /// </summary>
    public class AssignmentStmt : Statement
    {
        /// <summary>
        /// Left-hand side variable of the assignment.
        /// </summary>
        public VariableNode Variable { get; }

        /// <summary>
        /// Right-hand side expression being assigned to the variable.
        /// </summary>
        public ExpressionNode Expression { get; }

        /// <summary>
        /// Construct an assignment statement.
        /// </summary>
        /// <param name="variable">Variable to assign to (must not be null)</param>
        /// <param name="expression">Expression to assign (must not be null)</param>
        public AssignmentStmt(VariableNode variable, ExpressionNode expression)
        {
            Variable = variable;
            Expression = expression;
        }

        /// <summary>
        /// Unparse the assignment using indentation for the provided level.
        /// The returned string ends with a semicolon to mimic statement syntax.
        /// </summary>
        /// <param name="level">Indentation level</param>
        /// <returns>Unparsed assignment statement</returns>
        public override string Unparse(int level = 0)
        {
            // Obtain the indent for the current level and format the assignment.
            string indent = GeneralUtils.GetIndentation(level);
            return $"{indent}{Variable.Unparse()} := {Expression.Unparse()};";
        }
    }

    /// <summary>
    /// Represents a return statement that returns the value of an expression.
    /// Example: return <expression>;
    /// </summary>
    public class ReturnStmt : Statement
    {
        /// <summary>
        /// Expression whose value is returned by this statement.
        /// </summary>
        public ExpressionNode Expression { get; }

        /// <summary>
        /// Construct a return statement for the given expression.
        /// </summary>
        /// <param name="expression">Expression to return</param>
        public ReturnStmt(ExpressionNode expression)
        {
            Expression = expression;
        }

        /// <summary>
        /// Unparse the return statement with indentation and a trailing semicolon.
        /// </summary>
        /// <param name="level">Indentation level</param>
        /// <returns>Unparsed return statement</returns>
        public override string Unparse(int level = 0)
        {
            // Use GeneralUtils to compute the indentation and format the return.
            string indent = GeneralUtils.GetIndentation(level);
            return $"{indent}return {Expression.Unparse()};";
        }
    }

    #endregion

    #region Expressions

    /// <summary>
    /// Base abstract class for expression nodes. Expressions can be unparsed
    /// to a string representation and may accept an indentation level if needed.
    /// </summary>
    public abstract class ExpressionNode
    {
        /// <summary>
        /// Produce a source-like representation of the expression.
        /// </summary>
        /// <param name="level">Indentation level (optional helper for nested formatting)</param>
        /// <returns>Unparsed expression text</returns>
        public abstract string Unparse(int level = 0);

        public abstract TResult Accept<TParam, TResult>(IVisitor<TParam, TResult> visitor, TParam param);
    }

    /// <summary>
    /// A literal value node (numbers, strings, booleans, etc.). Value is stored
    /// as an object and ToString() is used when unparsing.
    /// </summary>
    public class LiteralNode : ExpressionNode
    {
        /// <summary>
        /// The literal value held by this node.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Create a literal node wrapping the provided value.
        /// </summary>
        /// <param name="value">Literal value (e.g., int, double, string)</param>
        public LiteralNode(object value)
        {
            Value = value;
        }

        /// <summary>
        /// Unparse the literal by delegating to its ToString representation.
        /// Note: if special formatting is required (e.g., quoting strings), handle
        /// that at a higher level or extend this method accordingly.
        /// </summary>
        /// <param name="level">Indentation level (not used)</param>
        /// <returns>String representation of the literal</returns>
        public override string Unparse(int level = 0)
        {
            return Value.ToString();
        }

        public override TResult Accept<TParam, TResult>(IVisitor<TParam, TResult> visitor, TParam param)
        {
            return visitor.Visit(this, param);
        }
    }

    /// <summary>
    /// A variable expression node that represents an identifier.
    /// </summary>
    public class VariableNode : ExpressionNode
    {
        /// <summary>
        /// Name of the variable / identifier.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Construct a variable node with the given name.
        /// </summary>
        /// <param name="name">Identifier name</param>
        public VariableNode(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Unparse the variable by returning its identifier name.
        /// </summary>
        /// <param name="level">Indentation level (not used)</param>
        /// <returns>Variable name</returns>
        public override string Unparse(int level = 0)
        {
            return Name;
        }

        public override TResult Accept<TParam, TResult>(IVisitor<TParam, TResult> visitor, TParam param)
        {
            return visitor.Visit(this, param);
        }
    }

    #endregion

    #region Operators and Binary Operators

    /// <summary>
    /// Abstract base class for operator expressions (unary, binary, etc.).
    /// In this file only binary operators are implemented, but the class
    /// provides a clear extension point.
    /// </summary>
    public abstract class Operator : ExpressionNode
    { }

    /// <summary>
    /// Common base for binary operators that hold a left and right expression.
    /// Provides a constructor to initialize the two operand expressions.
    /// </summary>
    public abstract class BinaryOperator : Operator
    {
        /// <summary>
        /// Left operand of the binary operator.
        /// </summary>
        public ExpressionNode Left { get; set; }

        /// <summary>
        /// Right operand of the binary operator.
        /// </summary>
        public ExpressionNode Right { get; set; }

        /// <summary>
        /// Construct a binary operator with left and right operands.
        /// </summary>
        /// <param name="left">Left operand expression</param>
        /// <param name="right">Right operand expression</param>
        public BinaryOperator(ExpressionNode left, ExpressionNode right)
        {
            Left = left;
            Right = right;
        }
    }

    /// <summary>
    /// Represents addition: (left + right)
    /// </summary>
    public class PlusNode : BinaryOperator
    {
        /// <summary>
        /// Construct a plus node with left and right expressions.
        /// </summary>
        public PlusNode(ExpressionNode left, ExpressionNode right)
            : base(left, right) { }

        /// <summary>
        /// Unparse the addition expression, adding parentheses to preserve
        /// expected grouping when printed back to text.
        /// </summary>
        public override string Unparse(int level = 0)
        {
            return $"({Left.Unparse(level)} + {Right.Unparse(level)})";
        }
        public override TResult Accept<TParam, TResult>(IVisitor<TParam, TResult> visitor, TParam param)
        {
            return visitor.Visit(this, param);
        }
    }

    /// <summary>
    /// Represents subtraction: (left - right)
    /// </summary>
    public class MinusNode : BinaryOperator
    {
        public MinusNode(ExpressionNode left, ExpressionNode right)
            : base(left, right) { }

        /// <summary>
        /// Unparse subtraction, keeping parentheses to make grouping explicit.
        /// </summary>
        public override string Unparse(int level = 0)
        {
            return $"({Left.Unparse(level)} - {Right.Unparse(level)})";
        }
        public override TResult Accept<TParam, TResult>(IVisitor<TParam, TResult> visitor, TParam param)
        {
            return visitor.Visit(this, param);
        }
    }

    /// <summary>
    /// Represents multiplication: (left * right)
    /// </summary>
    public class TimesNode : BinaryOperator
    {
        public TimesNode(ExpressionNode left, ExpressionNode right)
            : base(left, right) { }

        /// <summary>
        /// Unparse multiplication with explicit parentheses.
        /// </summary>
        public override string Unparse(int level = 0)
        {
            return $"({Left.Unparse(level)} * {Right.Unparse(level)})";
        }
        public override TResult Accept<TParam, TResult>(IVisitor<TParam, TResult> visitor, TParam param)
        {
            return visitor.Visit(this, param);
        }
    }

    /// <summary>
    /// Represents floating-point division: (left / right)
    /// </summary>
    public class FloatDivNode : BinaryOperator
    {
        public FloatDivNode(ExpressionNode left, ExpressionNode right)
            : base(left, right) { }

        /// <summary>
        /// Unparse floating division operator.
        /// </summary>
        public override string Unparse(int level = 0)
        {
            return $"({Left.Unparse(level)} / {Right.Unparse(level)})";
        }
    }

    /// <summary>
    /// Represents integer division using a '//' style operator: (left // right)
    /// </summary>
    public class IntDivNode : BinaryOperator
    {
        public IntDivNode(ExpressionNode left, ExpressionNode right)
            : base(left, right) { }

        /// <summary>
        /// Unparse integer-division operator using '//' token to indicate floor
        /// or integer division semantics in many languages.
        /// </summary>
        public override string Unparse(int level = 0)
        {
            return $"({Left.Unparse(level)} // {Right.Unparse(level)})";
        }
        public override TResult Accept<TParam, TResult>(IVisitor<TParam, TResult> visitor, TParam param)
        {
            return visitor.Visit(this, param);
        }
    }

    /// <summary>
    /// Represents modulus (remainder) operator: (left % right)
    /// </summary>
    public class ModulusNode : BinaryOperator
    {
        public ModulusNode(ExpressionNode left, ExpressionNode right)
            : base(left, right) { }

        /// <summary>
        /// Unparse the modulus operator using % token.
        /// </summary>
        public override string Unparse(int level = 0)
        {
            return $"({Left.Unparse(level)} % {Right.Unparse(level)})";
        }

        public override TResult Accept<TParam, TResult>(IVisitor<TParam, TResult> visitor, TParam param)
        {
            return visitor.Visit(this, param);
        }
    }

    /// <summary>
    /// Represents exponentiation operator: (left ** right)
    /// </summary>
    public class ExponentiationNode : BinaryOperator
    {
        public ExponentiationNode(ExpressionNode left, ExpressionNode right)
            : base(left, right) { }

        /// <summary>
        /// Unparse the exponentiation operator using ** token.
        /// </summary>
        public override string Unparse(int level = 0)
        {
            return $"({Left.Unparse(level)} ** {Right.Unparse(level)})";
        }

        public override TResult Accept<TParam, TResult>(IVisitor<TParam, TResult> visitor, TParam param)
        {
            return visitor.Visit(this, param);
        }
    }

    #endregion


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

        public string Visit(MinusNode node, int level)
        {
            string left = node.Left.Accept(this, level);
            string right = node.Right.Accept(this, level);
            return $"({left} - {right})";
        }

        public string Visit(TimesNode node, int level)
        {
            string left = node.Left.Accept(this, level);
            string right = node.Right.Accept(this, level);
            return $"({left} * {right})";
        }

        public string Visit(FloatDivNode node, int level)
        {
            string left = node.Left.Accept(this, level);
            string right = node.Right.Accept(this, level);
            return $"({left} / {right})";
        }

        public string Visit(IntDivNode node, int level)
        {
            string left = node.Left.Accept(this, level);
            string right = node.Right.Accept(this, level);
            return $"({left} // {right})";
        }

        public string Visit(ModulusNode node, int level)
        {
            string left = node.Left.Accept(this, level);
            string right = node.Right.Accept(this, level);
            return $"({left} % {right})";
        }

        public string Visit(ExponentiationNode node, int level)
        {
            string left = node.Left.Accept(this, level);
            string right = node.Right.Accept(this, level);
            return $"({left} ** {right})";
        }

        public string Visit(LiteralNode node, int level)
        {
            return $"{node}";
        }


        #endregion



        #region Statement Node Visit Methods

        // TODO

        #endregion
    }
}


