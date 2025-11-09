using System;
using System.Diagnostics;
using System.Text;
using AST;
using Containers;
using Utilities;

namespace AST
{
    public class NameAnalysisException : Exception
    {
        public NameAnalysisException(string message) : base(message) { }
    }

    //  checks for naming correctness only so true or false
    public class NameAnalysisVisitor :
IVisitor<Tuple<SymbolTable<string, object>, Statement>, bool>
    {

        private bool AnalyzeBinary(BinaryOperator node, Tuple<SymbolTable<string, object>, Statement> context)
        {
            bool leftOk = node.Left.Accept(this, context);
            bool rightOk = node.Right.Accept(this, context);
            return leftOk && rightOk;
        }

        public bool Visit(PlusNode node, Tuple<SymbolTable<string, object>, Statement> items)
        {
            // bool left = node.Left.Accept(this, items);
            // bool right = node.Right.Accept(this, items);
            // return left && right;
            return AnalyzeBinary(node, items);
        }


        public bool Visit(MinusNode node, Tuple<SymbolTable<string,object>, Statement> items)
        {
            // return Visit(node, items);
            return AnalyzeBinary(node, items);
        }
    }

}