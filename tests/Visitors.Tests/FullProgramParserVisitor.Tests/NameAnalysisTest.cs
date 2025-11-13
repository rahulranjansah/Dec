using System;
using Xunit;
using AST;
using Utilities;
using Containers;
using Parser;
using Tokenizer;

namespace AST.Visitors.Tests.FullProgramParserVisitor.Tests
{
    /// <summary>
    /// Full integration tests for the parsing, name analysis, and evaluation pipeline.
    /// These tests verify that raw program text can be tokenized, parsed,
    /// semantically validated, and evaluated end-to-end.
    /// </summary>
    public class FullProgramIntegrationTests
    {
        private readonly NameAnalysisVisitor _analyzer = new();
        private readonly EvaluateVisitor _evaluator = new();

        private Tuple<SymbolTable<string, object>, Statement> Scope() =>
            new Tuple<SymbolTable<string, object>, Statement>(new SymbolTable<string, object>(), null);

        // -------------------------------------------------
        // 1. Happy-path programs (well-formed, valid scope)
        // -------------------------------------------------

        [Fact(DisplayName = "Full program: simple arithmetic and return")]
        public void FullProgram_SimpleArithmetic_ReturnsCorrectValue()
        {
            string code = @"{
                x := (3)
                y := ((x + 5) * 2)
                return (y)
            }";

            var ast = Parser.Parser.Parse(code);
            bool analyzed = ast.Accept(_analyzer, Scope());
            Assert.True(analyzed);

            var result = _evaluator.Evaluate(ast);
            Assert.Equal(16, Convert.ToInt32(result));
        }

        [Fact(DisplayName = "Full program: nested blocks and variable shadowing")]
        public void FullProgram_NestedBlocks_ShadowingResolvesCorrectly()
        {
            string code = @"{
                x := (5)
                {
                    x := (x + 10)
                    return (x)
                }
            }";

            var ast = Parser.Parser.Parse(code);
            bool analyzed = ast.Accept(_analyzer, Scope());
            Assert.True(analyzed);

            var result = _evaluator.Evaluate(ast);
            Assert.Equal(15, Convert.ToInt32(result));
        }

        [Fact(DisplayName = "Full program: chained arithmetic expressions")]
        public void FullProgram_ChainedArithmetic_EvaluatesCorrectly()
        {
            string code = @"{
                a := (2 + 3) * (4 - 1)
                return a ** 2
            }";

            var ast = Parser.Parser.Parse(code);
            Assert.True(ast.Accept(_analyzer, Scope()));
            Assert.Equal(225, Convert.ToInt32(_evaluator.Evaluate(ast)));
        }

        // -------------------------------------------------
        // 2. Semantic failure cases (NameAnalysis errors)
        // -------------------------------------------------

        [Fact(DisplayName = "Full program: undeclared variable in return fails analysis")]
        public void FullProgram_UndeclaredVariable_FailsAnalysis()
        {
            string code = @"{
                x := (3)
                return (y)
            }";

            var ast = Parser.Parser.Parse(code);
            bool analyzed = ast.Accept(_analyzer, Scope());
            Assert.False(analyzed);
        }

        [Fact(DisplayName = "Full program: undeclared variable in expression fails analysis")]
        public void FullProgram_ExpressionUsesUndeclaredVariable_Fails()
        {
            string code = @"{
                result := (a + 1)
                return (result)
            }";

            var ast = Parser.Parser.Parse(code);
            Assert.False(ast.Accept(_analyzer, Scope()));
        }

        // -------------------------------------------------
        // 3. Runtime error handling (EvaluateVisitor errors)
        // -------------------------------------------------

        [Fact(DisplayName = "Full program: division by zero throws EvaluationException")]
        public void FullProgram_DivideByZero_ThrowsEvaluationException()
        {
            string code = @"{
                x := (10)
                y := (x / 0)
                return (y)
            }";

            var ast = Parser.Parser.Parse(code);
            Assert.True(ast.Accept(_analyzer, Scope()));

            Assert.Throws<EvaluationException>(() => _evaluator.Evaluate(ast));
        }

        [Fact(DisplayName = "Full program: integer division and modulus evaluate correctly")]
        public void FullProgram_IntDivAndModulus_ReturnExpectedValues()
        {
            string code = @"{
                a := (10)
                b := (a // 3)
                c := (a % 3)
                return (b + c)
            }";

            var ast = Parser.Parser.Parse(code);
            Assert.True(ast.Accept(_analyzer, Scope()));

            var result = _evaluator.Evaluate(ast);
            Assert.Equal(4, Convert.ToInt32(result)); // 10 // 3 = 3, 10 % 3 = 1, so 3 + 1 = 4
        }

        // -------------------------------------------------
        // 4. Literal and mixed-type operations
        // -------------------------------------------------

        [Fact(DisplayName = "Full program: handles mixed integer and float arithmetic")]
        public void FullProgram_MixedNumericTypes_ReturnsDouble()
        {
            string code = @"{
                a := (2)
                b := (3.5)
                return (a * b)
            }";

            var ast = Parser.Parser.Parse(code);
            Assert.True(ast.Accept(_analyzer, Scope()));

            var result = _evaluator.Evaluate(ast);
            Assert.Equal(7.0, Convert.ToDouble(result));
        }

        [Fact(DisplayName = "Full program: literal values propagate correctly through analysis and evaluation")]
        public void FullProgram_LiteralPropagation_Works()
        {
            string code = @"{
                msg := (""hello"")
                num := (42)
                return (num)
            }";

            var ast = Parser.Parser.Parse(code);
            Assert.True(ast.Accept(_analyzer, Scope()));

            var result = _evaluator.Evaluate(ast);
            Assert.Equal(42, Convert.ToInt32(result));
        }
    }
}
