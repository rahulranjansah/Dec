/**
 * NameAnalysisVisitor.cs
 *
 * Implements a visitor that performs name analysis over an Abstract Syntax Tree (AST).
 * The purpose of this visitor is to ensure that all variables are properly declared
 * within scope before being referenced, and to register variable names when they are
 * assigned. This ensures semantic correctness before evaluation or code generation.
 *
 * Bugs: Potential ambiguity regarding local vs. global scope checks in nested symbol tables.
 *
 * @author Rahul, Rick, Zachary
 * @date 2025-11-10
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Reflection;
using System.Reflection.Metadata;
using Containers;
using Utilities;

namespace AST
{
    /// <summary>
    /// The NameAnalysisVisitor traverses the AST and ensures that
    /// each variable is declared before being used.
    /// It registers variables in a symbol table during assignment
    /// and validates references in nested scopes.
    /// </summary>
    public class NameAnalysisVisitor : IVisitor<Tuple<SymbolTable<string, object>, Statement>, bool>
    {
        #region Arithmetic Operation Visitors
        /// <summary>
        /// Checks that both operands of a '+' expression are valid variables or literals.
        /// </summary>
        public bool Visit(PlusNode node, Tuple<SymbolTable<string, object>, Statement> param)
        {
            return node.Left.Accept(this, param) && node.Right.Accept(this, param);
        }

        /// <summary>
        /// Checks both operands of a '-' expression for validity.
        /// </summary>
        public bool Visit(MinusNode node, Tuple<SymbolTable<string, object>, Statement> param)
        {
            return node.Left.Accept(this, param) && node.Right.Accept(this, param);
        }

        /// <summary>
        /// Checks both operands of a '*' expression for validity.
        /// </summary>
        public bool Visit(TimesNode node, Tuple<SymbolTable<string, object>, Statement> param)
        {
            return node.Left.Accept(this, param) && node.Right.Accept(this, param);
        }

        /// <summary>
        /// Checks both operands of a floating-point division ('/') for valid names.
        /// </summary>
        public bool Visit(FloatDivNode node, Tuple<SymbolTable<string, object>, Statement> param)
        {
            return node.Left.Accept(this, param) && node.Right.Accept(this, param);
        }

        /// <summary>
        /// Checks both operands of an integer division ('//') for valid names.
        /// </summary>
        public bool Visit(IntDivNode node, Tuple<SymbolTable<string, object>, Statement> param)
        {
            return node.Left.Accept(this, param) && node.Right.Accept(this, param);
        }

        /// <summary>
        /// Checks both operands of a modulus ('%') expression for valid names.
        /// </summary>
        public bool Visit(ModulusNode node, Tuple<SymbolTable<string, object>, Statement> param)
        {
            return node.Left.Accept(this, param) && node.Right.Accept(this, param);
        }

        /// <summary>
        /// Checks both operands of an exponentiation ('^') expression for valid names.
        /// </summary>
        public bool Visit(ExponentiationNode node, Tuple<SymbolTable<string, object>, Statement> param)
        {
            return node.Left.Accept(this, param) && node.Right.Accept(this, param);
        }
        #endregion

        #region Variable and Literal Visitors
        /// <summary>
        /// Validates that a variable has been declared in the current or parent scope.
        /// </summary>
        /// <param name="node">The variable node being visited.</param>
        /// <param name="param">Tuple containing the current symbol table and statement context.</param>
        /// <returns>True if the variable is declared; otherwise false.</returns>
        public bool Visit(VariableNode node, Tuple<SymbolTable<string, object>, Statement> param)
        {
            SymbolTable<string, object> symbolTable = param.Item1;

            string varName = node.Name;

            // Checks if the variable name exists in current or parent scope.
            // (ContainsKeyLocal could be used for more strict scoping rules if needed.)
            if (symbolTable.ContainsKey(varName))
            {
                return true;
            }
            else
            {
                // Reports undeclared variable use — useful for debugging and semantic checks.
                Console.WriteLine($"{varName} is not declared in this scope");
                return false;
            }
        }

        /// <summary>
        /// Literal nodes (numbers, strings, etc.) are always valid in name analysis.
        /// </summary>
        public bool Visit(LiteralNode node, Tuple<SymbolTable<string, object>, Statement> param)
        {
            return true;
        }
        #endregion

        #region Statement Visitors
        /// <summary>
        /// Handles assignment statements by verifying that the expression is valid
        /// and then adding the variable to the current symbol table.
        /// </summary>
        /// <param name="node">Assignment statement node.</param>
        /// <param name="param">Tuple with current symbol table and parent statement context.</param>
        /// <returns>True if the expression is valid; otherwise false.</returns>
        public bool Visit(AssignmentStmt node, Tuple<SymbolTable<string, object>, Statement> param)
        {
            SymbolTable<string, object> symbolTable = param.Item1;

            // Validate right-hand side expression first.
            bool expressionValid = node.Expression.Accept(this, param);

            // Add variable to symbol table only if it doesn't exist locally
            // (This allows re-assignment to existing variables)
            string varName = node.Variable.Name;
            if (!symbolTable.ContainsKeyLocal(varName))
            {
                symbolTable.Add(varName, null);
            }

            return expressionValid;
        }

        /// <summary>
        /// Validates a return statement by checking its return expression.
        /// </summary>
        public bool Visit(ReturnStmt node, Tuple<SymbolTable<string, object>, Statement> param)
        {
            return node.Expression.Accept(this, param);
        }

        /// <summary>
        /// Traverses a block statement, creating a local scope and checking each statement inside.
        /// </summary>
        /// <param name="node">Block statement containing a list of inner statements.</param>
        /// <param name="param">Current symbol table and statement context.</param>
        /// <returns>True if all statements in the block are valid; otherwise false.</returns>
        public bool Visit(BlockStmt node, Tuple<SymbolTable<string, object>, Statement> param)
        {
            bool valid = true;

            // Sequentially check all statements in the block
            foreach (var statment in node.Statements)
            {
                if (!statment.Accept(this, param))
                {
                    // If any statement fails name analysis, mark overall block as invalid.
                    valid = false;
                }
            }

            return valid;
        }
        #endregion
    }
}
