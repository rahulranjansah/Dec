using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using AST;
using Containers;
using Utilities;

namespace AST
{
    /// <summary>
    /// Exception thrown when an evaluation error occurs
    /// </summary>
    public class EvaluationException : Exception
    {
        public EvaluationException(string message) : base(message){ }
    }

    /// <summary>
    /// Visitor that evaluates an AST, executing the program and returning the final value
    /// Uses symbol tables to store variable values during execution
    /// </summary>
    public class EvaluateVisitor : IVisitor<SymbolTable<string, object>, object>
    {
        // Flag to indicate if a return statement has been encountered
        private bool _returnEncountered;

        // Value from the return statement
        private object _returnValue;

        /// <summary>
        /// Initializes a new instance of the EvaluateVisitor class
        /// </summary>
        public EvaluateVisitor()
        {
            _returnEncountered = false;
            _returnValue = null;
        }

        /// <summary>
        /// Evaluates the given AST and returns the result
        /// </summary>
        /// <param name="ast">The AST to evaluate</param>
        /// <returns>The result of the evaluation (typically from a return statement)</returns>
        public object Evaluate(Statement ast)
        {
            _returnEncountered = false;
            _returnValue = null;

            // Execute the AST with a null initial scope
            // (the BlockStmt will use its own symbol table)
            ast.Accept(this, null);

            return _returnValue;
        }

        public object Visit(PlusNode node, SymbolTable<string, object> symbolTable)
        {
            object left = node.Left.Accept(this, symbolTable);
            object right = node.Right.Accept(this, symbolTable);

            if (left is int l && right is int r) { return l + r; }
            return Convert.ToDouble(left) + Convert.ToDouble(right);
        }

        public object Visit(MinusNode node, SymbolTable<string, object> symbolTable)
        {
            object left = node.Left.Accept(this, symbolTable);
            object right = node.Right.Accept(this, symbolTable);

            if (left is int l && right is int r) { return l - r; }
            return Convert.ToDouble(left) - Convert.ToDouble(right);
        }

        public object Visit(TimesNode node, SymbolTable<string, object> symbolTable)
        {
            object left = node.Left.Accept(this, symbolTable);
            object right = node.Right.Accept(this, symbolTable);

            if (left is int l && right is int r) { return l * r; }
            return Convert.ToDouble(left) * Convert.ToDouble(right);
        }

        public object Visit(FloatDivNode node, SymbolTable<string, object> symbolTable)
        {
            object left = node.Left.Accept(this, symbolTable);
            object right = node.Right.Accept(this, symbolTable);
            double r = Convert.ToDouble(right);
            if (r == 0.0)
                throw new EvaluationException("Division by zero");

            double l = Convert.ToDouble(left);
            return l / r;
        }

        public object Visit(IntDivNode node, SymbolTable<string, object> symbolTable)
        {
            object left = node.Left.Accept(this, symbolTable);
            object right = node.Right.Accept(this, symbolTable);
            int r = Convert.ToInt32(right);
            if (r == 0)
                throw new EvaluationException("Division by zero");

            int l = Convert.ToInt32(left);
            return l / r;
        }

        public object Visit(ModulusNode node, SymbolTable<string, object> symbolTable)
        {
            object left = node.Left.Accept(this, symbolTable);
            object right = node.Right.Accept(this, symbolTable);

            if (left is int l && right is int r)
            {
                if (r == 0) { throw new EvaluationException("Division by zero"); }
                return l % r;
            }
            if (Convert.ToDouble(right) == 0.0) { throw new EvaluationException("Cannot divide by float zero"); }

            return Convert.ToDouble(left) % Convert.ToDouble(right);
        }

        public object Visit(ExponentiationNode node, SymbolTable<string, object> symbolTable)
        {
            object left = node.Left.Accept(this, symbolTable);
            object right = node.Right.Accept(this, symbolTable);

            if (left is int l && right is int r)
            {
                return Math.Pow(l, r);
            }

            return Math.Pow(Convert.ToDouble(left), Convert.ToDouble(right));

        }


        #region Expression Node Visit Methods

        public object Visit(VariableNode node, SymbolTable<string, object> symbolTable)
        {
            if (symbolTable.TryGetValue(node.Name, out object value)) { return value; }

            throw new EvaluationException($"Undefined variable '{node.Name}'");
        }


        public object Visit(LiteralNode node, SymbolTable<string, object> symbolTable)
        {
            return node.Value;
        }

        #endregion

        #region Statement Node Visit Methods

        public object Visit(AssignmentStmt node, SymbolTable<string, object> symbolTable)
        {
            // Evaluate the expression and get the value
            object value = node.Expression.Accept(this, symbolTable);
            string name = node.Variable.Name;

            // Check if variable exists in current or parent scopes
            if (symbolTable.ContainsKey(name))
            {
                // Variable exists somewhere in the scope chain - update it
                symbolTable[name] = value;
            }
            else
            {
                // Variable doesn't exist anywhere - create new one
                symbolTable.Add(name, value);
            }

            return _returnValue;
        }

        public object Visit(ReturnStmt node, SymbolTable<string, object> symbolTable)
        {
            _returnValue = node.Expression.Accept(this, symbolTable);
            _returnEncountered = true;
            return _returnValue;
        }

        public object Visit(BlockStmt node, SymbolTable<string, object> symbolTable)
        {
            // Use this block's symbol table, which is already linked to its parent
            SymbolTable<string, object> currentScope = node.SymbolTable;

            foreach (var stmt in node.Statements)
            {
                stmt.Accept(this, currentScope);

                // If a return statement was encountered, stop processing and return
                if (_returnEncountered)
                {
                    return _returnValue;
                }
            }
            return _returnValue;
        }

        #endregion
    }
}