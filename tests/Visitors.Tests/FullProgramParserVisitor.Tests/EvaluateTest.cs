using System;
using Xunit;
using AST;
using Utilities;
using Parser;
using Tokenizer;
using Containers;

namespace AST.Visitors.Tests.FullProgramParserVisitor.Tests
{
    /// <summary>
    /// End-to-end integration tests for the full pipeline:
    /// Program Text → Tokenizer → Parser → AST → NameAnalysisVisitor → EvaluateVisitor → Result.
    ///
    /// These tests validate not only arithmetic correctness,
    /// but also scoping, shadowing, and runtime exception handling.
    /// </summary>
    public class EvaluateIntegrationTest
    {
        private readonly NameAnalysisVisitor _analyzer = new();
        private readonly EvaluateVisitor _evaluator = new();

        private Tuple<SymbolTable<string, object>, Statement> Scope() =>
            new Tuple<SymbolTable<string, object>, Statement>(new SymbolTable<string, object>(), null);

        // -----------------------------------------------------
        // 1. Arithmetic and Expression Evaluation (Full Program)
        // -----------------------------------------------------

        [Theory(DisplayName = "Full program arithmetic expressions evaluate correctly")]
        [InlineData(@"{ return (3 + 5); }", 8)]
        [InlineData(@"{ return (10 - 4); }", 6)]
        [InlineData(@"{ return (2 * 5); }", 10)]
        [InlineData(@"{ return (9 / 3); }", 3.0)]
        [InlineData(@"{ return (9 // 2); }", 4)]
        [InlineData(@"{ return (10 % 3); }", 1)]
        [InlineData(@"{ return (2 ** 3); }", 8)]
        public void Evaluate_ArithmeticPrograms_ReturnsExpected(string program, object expected)
        {
            var ast = Parser.Parser.Parse(program);
            Assert.True(ast.Accept(_analyzer, Scope()));

            var result = _evaluator.Evaluate(ast);
            Assert.Equal(Convert.ToDouble(expected), Convert.ToDouble(result));
        }

        // -----------------------------------------------------
        // 2. Variable Declaration, Assignment, and Return
        // -----------------------------------------------------

        [Fact(DisplayName = "Full program variable assignment and return works correctly")]
        public void Evaluate_AssignmentAndReturn_ProducesCorrectResult()
        {
            string code = @"{
                x := 42;
                return x;
            }";

            var ast = Parser.Parser.Parse(code);
            Assert.True(ast.Accept(_analyzer, Scope()));

            var result = _evaluator.Evaluate(ast);
            Assert.Equal(42, Convert.ToInt32(result));
        }

        [Fact(DisplayName = "Full program evaluates sequential statements and expressions correctly")]
        public void Evaluate_SequentialStatements_EvaluateCorrectly()
        {
            string code = @"{
                a := 3;
                b := (a + 7);
                c := (b * 2);
                return c;
            }";

            var ast = Parser.Parser.Parse(code);
            Assert.True(ast.Accept(_analyzer, Scope()));

            var result = _evaluator.Evaluate(ast);
            Assert.Equal(20, Convert.ToInt32(result));
        }

        // -----------------------------------------------------
        // 3. Block and Nested Scope Behavior
        // -----------------------------------------------------

        [Fact(DisplayName = "Full program nested blocks handle shadowing correctly")]
        public void Evaluate_NestedBlocks_ShadowingWorks()
        {
            string code = @"{
                x := 10;
                {
                    x := (x + 5);
                    return x;
                }
            }";

            var ast = Parser.Parser.Parse(code);
            Assert.True(ast.Accept(_analyzer, Scope()));

            var result = _evaluator.Evaluate(ast);
            Assert.Equal(15, Convert.ToInt32(result));
        }

        [Fact(DisplayName = "Full program nested blocks preserve outer scope after inner return")]
        public void Evaluate_NestedBlocks_PreserveOuterScope()
        {
            string code = @"{
                x := 4;
                {
                    y := (x * 2);
                    return y;
                }
                return x;  // unreachable in current semantics but safe to test
            }";

            var ast = Parser.Parser.Parse(code);
            Assert.True(ast.Accept(_analyzer, Scope()));

            var result = _evaluator.Evaluate(ast);
            Assert.Equal(8, Convert.ToInt32(result));
        }

        // -----------------------------------------------------
        // 4. Error & Edge Case Handling
        // -----------------------------------------------------

        [Theory(DisplayName = "Full program: divide or modulus by zero throws EvaluationException")]
        [InlineData(@"{ return (5 / 0); }")]
        [InlineData(@"{ return (5 // 0); }")]
        [InlineData(@"{ return (5 % 0); }")]
        public void Evaluate_DivideOrModulusByZero_Throws(string code)
        {
            var ast = Parser.Parser.Parse(code);
            Assert.True(ast.Accept(_analyzer, Scope()));

            Assert.Throws<EvaluationException>(() => _evaluator.Evaluate(ast));
        }

        [Fact(DisplayName = "Full program: accessing undeclared variable fails name analysis")]
        public void Evaluate_UndeclaredVariable_FailsAnalysis()
        {
            string code = @"{
                return ghost;
            }";

            var ast = Parser.Parser.Parse(code);
            bool result = ast.Accept(_analyzer, Scope());
            Assert.False(result);
        }

        // -----------------------------------------------------
        // 5. Complex & Mixed Expressions
        // -----------------------------------------------------

        [Fact(DisplayName = "Full program complex nested arithmetic computes correctly")]
        public void Evaluate_ComplexNestedExpression_ReturnsCorrectValue()
        {
            string code = @"{
                result := ((3 + 2) * (4 - 1)) ** 2;
                return result;
            }";

            var ast = Parser.Parser.Parse(code);
            Assert.True(ast.Accept(_analyzer, Scope()));

            var result = _evaluator.Evaluate(ast);
            Assert.Equal(225, Convert.ToInt32(result));
        }

        [Fact(DisplayName = "Full program handles mixed int and float arithmetic properly")]
        public void Evaluate_MixedIntFloatExpression_ReturnsDouble()
        {
            string code = @"{
                a := 2;
                b := 3.5;
                return (a * b);
            }";

            var ast = Parser.Parser.Parse(code);
            Assert.True(ast.Accept(_analyzer, Scope()));

            var result = _evaluator.Evaluate(ast);
            Assert.Equal(7.0, Convert.ToDouble(result));
        }

        // -----------------------------------------------------
        // 6. Literal Handling
        // -----------------------------------------------------

        [Theory(DisplayName = "Full program literal returns correct value for all types")]
        [InlineData(@"{ return 42; }", 42)]
        [InlineData(@"{ return 3.14; }", 3.14)]
        [InlineData(@"{ return ""hello""; }", "hello")]
        public void Evaluate_LiteralPrograms_ReturnExpected(string code, object expected)
        {
            var ast = Parser.Parser.Parse(code);
            Assert.True(ast.Accept(_analyzer, Scope()));

            var result = _evaluator.Evaluate(ast);
            Assert.Equal(expected, result);
        }

            // -----------------------------------------------------
        // 7. Complex Multi-Block Program and Runtime Exceptions
        // -----------------------------------------------------

        [Fact(DisplayName = "Full program complex factorial-like computation executes correctly")]
        public void Evaluate_ComplexProgram_ComputesFactorialCorrectly()
        {
            // Simulates iterative multiplication through nested blocks.
            // Equivalent to: result = 1*2*3*4*5 = 120
            string program = @"{
                n := (5)
                result := (1)
                counter := (1)
                {
                    result := (result * counter)
                    counter := (counter + 1)

                    {
                        temp := (counter)
                    }

                    result := (result * counter)
                    counter := (counter + 1)
                    result := (result * counter)
                    counter := (counter + 1)
                    result := (result * counter)
                    counter := (counter + 1)
                    result := (result * counter)
                    counter := (counter + 1)

                    return (result)
                }
            }";

            var ast = Parser.Parser.Parse(program);
            Assert.True(ast.Accept(_analyzer, Scope()));

            var result = _evaluator.Evaluate(ast);
            Assert.Equal(120, Convert.ToInt32(result)); // 5! = 120
        }

        [Fact(DisplayName = "Full program division by zero raises EvaluationException")]
        public void Evaluate_DivisionByZero_ThrowsEvaluationException()
        {
            string program = @"{
                return (10 / 0)
            }";

            var ast = Parser.Parser.Parse(program);
            Assert.True(ast.Accept(_analyzer, Scope()));

            var exception = Assert.Throws<EvaluationException>(() => _evaluator.Evaluate(ast));
            Assert.Equal("Division by zero", exception.Message);
        }

    }
}
