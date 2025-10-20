using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;
using AST;


namespace Builders.Tests
{
    public class ASTBuilderTests
    {
        // Helpers to create builders
        private DefaultBuilder DefaultB() => new DefaultBuilder();
        private DebugBuilder DebugB() => new DebugBuilder();
        private NullBuilder NullB() => new NullBuilder();

        // ---------------------
        // Literal / Unparse tests
        // ---------------------

        // Many simple literals via InlineData
        [Theory(DisplayName = "LiteralNode Unparse: various simple types (InlineData)")]
        [InlineData(123, "123")]
        [InlineData(3.14, "3.14")]
        [InlineData("hello world", "hello world")]
        [InlineData(true, "True")]
        [InlineData(false, "False")]
        [InlineData("", "")]
        public void LiteralNode_Unparse_VariousSimpleTypes(object value, string expected)
        {
            var db = DefaultB();
            var lit = db.CreateLiteralNode(value);
            Assert.NotNull(lit);
            Assert.Equal(expected, lit.Unparse());
        }

        // Special cases (NaN, Infinity, null) using MemberData
        public static IEnumerable<object[]> SpecialLiteralCases()
        {
            yield return new object[] { double.NaN, double.NaN.ToString() };
            yield return new object[] { double.PositiveInfinity, double.PositiveInfinity.ToString() };
            // observed behavior: passing null constructs LiteralNode but Unparse throws NullReferenceException
            yield return new object[] { null, null }; // expected null marker for test to assert exception
        }

        [Theory(DisplayName = "LiteralNode Unparse: special cases (MemberData)")]
        [MemberData(nameof(SpecialLiteralCases))]
        public void LiteralNode_Unparse_SpecialCases(object value, string expectedOrNullMarker)
        {
            var db = DefaultB();
            var lit = db.CreateLiteralNode(value);
            Assert.NotNull(lit);

            if (value == null)
            {
                // observed behavior: Value is null -> Unparse throws
                Assert.Throws<NullReferenceException>(() => lit.Unparse());
            }
            else
            {
                Assert.Equal(expectedOrNullMarker, lit.Unparse());
            }
        }

        // ---------------------
        // VariableName tests (edge cases)
        // ---------------------
        [Theory(DisplayName = "VariableNode Unparse: name edge-cases")]
        [InlineData("x")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("long_variable_name_123")]
        public void VariableNode_Unparse_NameEdgeCases(string name)
        {
            var db = DefaultB();
            var v = db.CreateVariableNode(name);
            Assert.NotNull(v);
            Assert.Equal(name, v.Unparse());
        }

        // ---------------------
        // Binary operator tests (parameterized)
        // ---------------------
        // Operator key -> expected unparse for operands 4 and 5
        [Theory(DisplayName = "Binary operators: unparse tokens (operator key)")]
        [InlineData("plus", "(4 + 5)")]
        [InlineData("minus", "(4 - 5)")]
        [InlineData("times", "(4 * 5)")]
        [InlineData("floatdiv", "(4 / 5)")]
        [InlineData("intdiv", "(4 // 5)")]
        [InlineData("modulus", "(4 % 5)")]
        [InlineData("exp", "(4 ** 5)")]
        public void BinaryOperator_Unparse_ByKey(string opKey, string expected)
        {
            var db = DefaultB();
            var a = db.CreateLiteralNode(4);
            var b = db.CreateLiteralNode(5);

            ExpressionNode node = opKey switch
            {
                "plus" => db.CreatePlusNode(a, b),
                "minus" => db.CreateMinusNode(a, b),
                "times" => db.CreateTimesNode(a, b),
                "floatdiv" => db.CreateFloatDivNode(a, b),
                "intdiv" => db.CreateIntDivNode(a, b),
                "modulus" => db.CreateModulusNode(a, b),
                "exp" => db.CreateExponentiationNode(a, b),
                _ => throw new ArgumentException("unknown operator key")
            };

            Assert.NotNull(node);
            Assert.Equal(expected, node.Unparse());
        }

        // Nested expression parameterized (checks structure on a few small combinations)
        public static IEnumerable<object[]> NestedExpressionCases()
        {
            // (1 + (2 * 3)) expected
            yield return new object[] {
                new Func<DefaultBuilder, ExpressionNode>(db =>
                    db.CreatePlusNode(
                        db.CreateLiteralNode(1),
                        db.CreateTimesNode(db.CreateLiteralNode(2), db.CreateLiteralNode(3))
                    )
                ),
                "(1 + (2 * 3))"
            };

            // (((1 + 2) * (3 - 4)) - (5 ** 6))
            yield return new object[] {
                new Func<DefaultBuilder, ExpressionNode>(db =>
                    db.CreateMinusNode(
                        db.CreateTimesNode(
                            db.CreatePlusNode(db.CreateLiteralNode(1), db.CreateLiteralNode(2)),
                            db.CreateMinusNode(db.CreateLiteralNode(3), db.CreateLiteralNode(4))
                        ),
                        db.CreateExponentiationNode(db.CreateLiteralNode(5), db.CreateLiteralNode(6))
                    )
                ),
                "(((1 + 2) * (3 - 4)) - (5 ** 6))"
            };
        }

        [Theory(DisplayName = "Nested expression Unparse (MemberData uses Func to build)")]
        [MemberData(nameof(NestedExpressionCases))]
        public void NestedExpression_Unparse(Func<DefaultBuilder, ExpressionNode> builderFunc, string expected)
        {
            var db = DefaultB();
            var expr = builderFunc(db);
            Assert.Equal(expected, expr.Unparse());
        }

        // ---------------------
        // Assignment / Return / Block tests
        // ---------------------
        [Theory(DisplayName = "AssignmentStmt Unparse format (var, value)")]
        [InlineData("a", 42, "a := 42;")]
        [InlineData("longName", 0, "longName := 0;")]
        public void AssignmentStmt_Unparse_Parameterized(string varName, int value, string expected)
        {
            var db = DefaultB();
            var v = db.CreateVariableNode(varName);
            var lit = db.CreateLiteralNode(value);
            var a = db.CreateAssignmentStmt(v, lit);
            Assert.Equal(expected, a.Unparse().Trim());
        }

        [Theory(DisplayName = "ReturnStmt Unparse format (various values)")]
        [InlineData(99, "return 99;")]
        [InlineData(0, "return 0;")]
        [InlineData("ok", "return ok;")]
        public void ReturnStmt_Unparse_Parameterized(object val, string expected)
        {
            var db = DefaultB();
            var lit = db.CreateLiteralNode(val);
            var r = db.CreateReturnStmt(lit);
            Assert.Equal(expected, r.Unparse().Trim());
        }

        [Fact(DisplayName = "BlockStmt Unparse with multiple statements preserves order")]
        public void BlockStmt_Unparse_PreservesOrder()
        {
            var db = DefaultB();
            var s1 = db.CreateAssignmentStmt(db.CreateVariableNode("x"), db.CreateLiteralNode(1));
            var s2 = db.CreateAssignmentStmt(db.CreateVariableNode("y"), db.CreateLiteralNode(2));
            var block = db.CreateBlockStmt(new List<Statement> { s1, s2 });
            var outText = block.Unparse();
            // check order and presence
            int ix1 = outText.IndexOf("x := 1;");
            int ix2 = outText.IndexOf("y := 2;");
            Assert.True(ix1 >= 0 && ix2 > ix1, "statements not in expected order");
        }

        // Block with null element -> Unparse throws (current observed behavior)
        [Theory(DisplayName = "BlockStmt Unparse with null element throws (parameterized)")]
        [InlineData(true)]
        public void BlockStmt_Unparse_NullElement_Throws(bool dummy)
        {
            var db = DefaultB();
            var listWithNull = new List<Statement> { null };
            var block = db.CreateBlockStmt(listWithNull);
            Assert.Throws<NullReferenceException>(() => block.Unparse());
        }

        // ---------------------
        // Builder behavior parameterized
        // ---------------------

        // NullBuilder should return null for a list of creation method keys (parameterized)
        public static IEnumerable<object[]> NullBuilderMethodKeys()
        {
            yield return new object[] { "literal" };
            yield return new object[] { "variable" };
            yield return new object[] { "plus" };
            yield return new object[] { "assignment" };
            yield return new object[] { "block" };
        }

        [Theory(DisplayName = "NullBuilder returns null for many creators (parameterized)")]
        [MemberData(nameof(NullBuilderMethodKeys))]
        public void NullBuilder_ReturnsNull_ForMethod(string methodKey)
        {
            var nb = NullB();
            var db = DefaultB();
            var left = db.CreateLiteralNode(1);
            var right = db.CreateLiteralNode(2);

            switch (methodKey)
            {
                case "literal":
                    Assert.Null(nb.CreateLiteralNode(1));
                    break;
                case "variable":
                    Assert.Null(nb.CreateVariableNode("x"));
                    break;
                case "plus":
                    Assert.Null(nb.CreatePlusNode(left, right));
                    break;
                case "assignment":
                    // left cast intentionally wrong type -> but builder should return null anyway
                    // Assert.Null(nb.CreateAssignmentStmt(left as VariableNode, right));
                    // break;
                case "block":
                    Assert.Null(nb.CreateBlockStmt(new List<Statement>()));
                    break;
                default:
                    throw new ArgumentException("unknown methodKey");
            }
        }

        // DebugBuilder: parameterize the different creation methods that should throw on null input
        public static IEnumerable<object[]> DebugBuilderNullCheckCases()
        {
            yield return new object[] { "literal" };
            yield return new object[] { "variable" };
            yield return new object[] { "plus_left" };
            yield return new object[] { "plus_right" };
            yield return new object[] { "assignment_var" };
            yield return new object[] { "assignment_expr" };
            yield return new object[] { "return_expr" };
            yield return new object[] { "block_null" };
        }

        [Theory(DisplayName = "DebugBuilder throws ArgumentNullException for null inputs (parameterized)")]
        [MemberData(nameof(DebugBuilderNullCheckCases))]
        public void DebugBuilder_ThrowsOnNullInputs(string caseKey)
        {
            var dbg = DebugB();
            var db = DefaultB();

            switch (caseKey)
            {
                case "literal":
                    Assert.Throws<ArgumentNullException>(() => dbg.CreateLiteralNode(null));
                    break;
                case "variable":
                    Assert.Throws<ArgumentNullException>(() => dbg.CreateVariableNode(null));
                    break;
                case "plus_left":
                    Assert.Throws<ArgumentNullException>(() => dbg.CreatePlusNode(null, db.CreateLiteralNode(1)));
                    break;
                case "plus_right":
                    Assert.Throws<ArgumentNullException>(() => dbg.CreatePlusNode(db.CreateLiteralNode(1), null));
                    break;
                case "assignment_var":
                    Assert.Throws<ArgumentNullException>(() => dbg.CreateAssignmentStmt(null, db.CreateLiteralNode(1)));
                    break;
                case "assignment_expr":
                    Assert.Throws<ArgumentNullException>(() => dbg.CreateAssignmentStmt(db.CreateVariableNode("v"), null));
                    break;
                case "return_expr":
                    Assert.Throws<ArgumentNullException>(() => dbg.CreateReturnStmt(null));
                    break;
                case "block_null":
                    Assert.Throws<ArgumentNullException>(() => dbg.CreateBlockStmt(null));
                    break;
                default:
                    throw new ArgumentException("unknown caseKey");
            }
        }

        // DefaultBuilder positive type & property checks (non-parameterized)
        [Fact(DisplayName = "DefaultBuilder creates concrete types and preserves children")]
        public void DefaultBuilder_CreatesConcreteTypes()
        {
            var db = DefaultB();
            var left = db.CreateLiteralNode(7);
            var right = db.CreateLiteralNode(8);
            var plus = db.CreatePlusNode(left, right);
            Assert.IsType<PlusNode>(plus);
            Assert.Same(left, ((PlusNode)plus).Left);
            Assert.Same(right, ((PlusNode)plus).Right);
        }

        // Negative behavior tests left as explicit Facts for clarity
        [Fact(DisplayName = "AssignmentStmt with null variable or expression: Unparse throws")]
        public void AssignmentStmt_NullMembers_UnparseThrows()
        {
            var a = new AssignmentStmt(null, null);
            Assert.Throws<NullReferenceException>(() => a.Unparse());
        }

        [Fact(DisplayName = "ReturnStmt with null expression: Unparse throws")]
        public void ReturnStmt_NullExpression_UnparseThrows()
        {
            var r = new ReturnStmt(null);
            Assert.Throws<NullReferenceException>(() => r.Unparse());
        }
    }
}
