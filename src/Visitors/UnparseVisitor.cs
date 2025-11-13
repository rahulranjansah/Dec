using System;
using System.Text;
using AST;
using Utilities;

namespace AST
{

    public class UnparseVisitor : IVisitor<int, string>
    {
        public string Unparse(ExpressionNode node, int level = 0)
        {
            return node.Accept(this, level);
        }

        public string Unparse(Statement stmt, int level = 0)
        {
            return stmt.Accept(this, level);
        }

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
            return node.Value.ToString();
        }

        public string Visit(VariableNode node, int level)
        {
            return node.Name;
        }
        public string Visit(AssignmentStmt node, int level)
        {
            string indent = GeneralUtils.GetIndentation(level);
            return $"{indent}{node.Variable.Unparse()} := {node.Expression.Unparse()}";
        }
        public string Visit(ReturnStmt node, int level)
        {
            string indent = GeneralUtils.GetIndentation(level);
            return $"{indent}return {node.Expression.Unparse()}";
        }

        public string Visit(BlockStmt node, int level)
        {
            string indent = GeneralUtils.GetIndentation(level);
            string result = indent + "{\n";
            foreach (var stmt in node.Statements)
            {
                result += stmt.Accept(this, level + 1) + "\n";
            }
            result += indent + "}";
            return result;
        }

    }

}

