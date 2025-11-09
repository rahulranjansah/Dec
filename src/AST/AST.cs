using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Utilities;
using Containers;

namespace AST
{
    public interface IVisitor<TParam, TResult>
    {
        TResult Visit(PlusNode node, TParam param);
        TResult Visit(MinusNode node, TParam param);
        TResult Visit(TimesNode node, TParam param);
        TResult Visit(FloatDivNode node, TParam param);
        TResult Visit(IntDivNode node, TParam param);
        TResult Visit(ModulusNode node, TParam param);
        TResult Visit(ExponentiationNode node, TParam param);
        TResult Visit(LiteralNode node, TParam param);
        TResult Visit(VariableNode node, TParam param);
        TResult Visit(AssignmentStmt node, TParam param);
        TResult Visit(ReturnStmt node, TParam param);
        TResult Visit(BlockStmt node, TParam param);
    }

    public abstract class Statement
    {
        public abstract string Unparse(int level = 0);
        public abstract TResult Accept<TParam, TResult>(IVisitor<TParam, TResult> visitor, TParam param);
    }


    public class BlockStmt : Statement
    {
        public List<Statement> Statements { get; }
        public Containers.SymbolTable<string, object> SymbolTable { get; }

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

        public override TResult Accept<TParam, TResult>(IVisitor<TParam, TResult> visitor, TParam param)
        {
            return visitor.Visit(this, param);
        }

        public void AddStatement(Statement stmt)
        {
            Statements.Add(stmt);
        }

        public override string Unparse(int level = 0)
        {
            string indent = GeneralUtils.GetIndentation(level);
            string result = indent + "{\n";
            foreach (var lineblock in Statements)
            {
                result += lineblock.Unparse(level + 1) + "\n";
            }
            result += indent + "}";
            return result;
        }
    }

    public class AssignmentStmt : Statement
    {
        public VariableNode Variable { get; }
        public ExpressionNode Expression { get; }

        public AssignmentStmt(VariableNode variable, ExpressionNode expression)
        {
            Variable = variable;
            Expression = expression;
        }

        public override string Unparse(int level = 0)
        {
            string indent = GeneralUtils.GetIndentation(level);
            return $"{indent}{Variable.Unparse()} := {Expression.Unparse()};";
        }

        public override TResult Accept<TParam, TResult>(IVisitor<TParam, TResult> visitor, TParam param)
        {
            return visitor.Visit(this, param);
        }
    }

    public class ReturnStmt : Statement
    {
        public ExpressionNode Expression { get; }

        public ReturnStmt(ExpressionNode expression)
        {
            Expression = expression;
        }

        public override string Unparse(int level = 0)
        {
            string indent = GeneralUtils.GetIndentation(level);
            return $"{indent}return {Expression.Unparse()};";
        }

        public override TResult Accept<TParam, TResult>(IVisitor<TParam, TResult> visitor, TParam param)
        {
            return visitor.Visit(this, param);
        }
    }

    public abstract class ExpressionNode
    {
        public abstract string Unparse(int level = 0);
        public abstract TResult Accept<TParam, TResult>(IVisitor<TParam, TResult> visitor, TParam param);
    }

    public class LiteralNode : ExpressionNode
    {
        public object Value { get; }

        public LiteralNode(object value)
        {
            Value = value;
        }

        public override string Unparse(int level = 0)
        {
            return Value.ToString();
        }

        public override TResult Accept<TParam, TResult>(IVisitor<TParam, TResult> visitor, TParam param)
        {
            return visitor.Visit(this, param);
        }
    }

    public class VariableNode : ExpressionNode
    {
        public string Name { get; set; }

        public VariableNode(string name)
        {
            Name = name;
        }

        public override string Unparse(int level = 0)
        {
            return Name;
        }

        public override TResult Accept<TParam, TResult>(IVisitor<TParam, TResult> visitor, TParam param)
        {
            return visitor.Visit(this, param);
        }
    }

    public abstract class Operator : ExpressionNode
    { }

    public abstract class BinaryOperator : Operator
    {
        public ExpressionNode Left { get; set; }
        public ExpressionNode Right { get; set; }

        public BinaryOperator(ExpressionNode left, ExpressionNode right)
        {
            Left = left;
            Right = right;
        }
    }

    public class PlusNode : BinaryOperator
    {
        public PlusNode(ExpressionNode left, ExpressionNode right)
            : base(left, right) { }

        public override string Unparse(int level = 0)
        {
            return $"({Left.Unparse(level)} + {Right.Unparse(level)})";
        }

        public override TResult Accept<TParam, TResult>(IVisitor<TParam, TResult> visitor, TParam param)
        {
            return visitor.Visit(this, param);
        }
    }

    public class MinusNode : BinaryOperator
    {
        public MinusNode(ExpressionNode left, ExpressionNode right)
            : base(left, right) { }

        public override string Unparse(int level = 0)
        {
            return $"({Left.Unparse(level)} - {Right.Unparse(level)})";
        }

        public override TResult Accept<TParam, TResult>(IVisitor<TParam, TResult> visitor, TParam param)
        {
            return visitor.Visit(this, param);
        }
    }

    public class TimesNode : BinaryOperator
    {
        public TimesNode(ExpressionNode left, ExpressionNode right)
            : base(left, right) { }

        public override string Unparse(int level = 0)
        {
            return $"({Left.Unparse(level)} * {Right.Unparse(level)})";
        }

        public override TResult Accept<TParam, TResult>(IVisitor<TParam, TResult> visitor, TParam param)
        {
            return visitor.Visit(this, param);
        }
    }

    public class FloatDivNode : BinaryOperator
    {
        public FloatDivNode(ExpressionNode left, ExpressionNode right)
            : base(left, right) { }

        public override string Unparse(int level = 0)
        {
            return $"({Left.Unparse(level)} / {Right.Unparse(level)})";
        }

        public override TResult Accept<TParam, TResult>(IVisitor<TParam, TResult> visitor, TParam param)
        {
            return visitor.Visit(this, param);
        }
    }

    public class IntDivNode : BinaryOperator
    {
        public IntDivNode(ExpressionNode left, ExpressionNode right)
            : base(left, right) { }

        public override string Unparse(int level = 0)
        {
            return $"({Left.Unparse(level)} // {Right.Unparse(level)})";
        }

        public override TResult Accept<TParam, TResult>(IVisitor<TParam, TResult> visitor, TParam param)
        {
            return visitor.Visit(this, param);
        }
    }

    public class ModulusNode : BinaryOperator
    {
        public ModulusNode(ExpressionNode left, ExpressionNode right)
            : base(left, right) { }

        public override string Unparse(int level = 0)
        {
            return $"({Left.Unparse(level)} % {Right.Unparse(level)})";
        }

        public override TResult Accept<TParam, TResult>(IVisitor<TParam, TResult> visitor, TParam param)
        {
            return visitor.Visit(this, param);
        }
    }

    public class ExponentiationNode : BinaryOperator
    {
        public ExponentiationNode(ExpressionNode left, ExpressionNode right)
            : base(left, right) { }

        public override string Unparse(int level = 0)
        {
            return $"({Left.Unparse(level)} ** {Right.Unparse(level)})";
        }

        public override TResult Accept<TParam, TResult>(IVisitor<TParam, TResult> visitor, TParam param)
        {
            return visitor.Visit(this, param);
        }
    }
}