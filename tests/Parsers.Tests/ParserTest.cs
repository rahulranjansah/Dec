// using System;
// using System.Collections.Generic;
// using Xunit;
// using AST;
// using Parser;

// public class ParserTests
// {
//     [Fact]
//     public void TestSingleAssignment()
//     {
//         string program = "b := 5";
        
//         BlockStmt result = Parser.Parse(program);
        
//         Assert.NotNull(result);
//         Assert.Equal(1, result.Statements.Count);
//         Assert.IsType<AssignmentStmt>(result.Statements[0]);
        
//         var assignStmt = (AssignmentStmt)result.Statements[0];
//         Assert.Equal("b", assignStmt.Variable.Name);
//         Assert.IsType<LiteralNode>(assignStmt.Expression);
        
//         var literal = (LiteralNode)assignStmt.Expression;
//         Assert.Equal(5, literal.Value);
        
//         // Verify unparsing works correctly
//         Assert.Equal(program, assignStmt.Unparse());
//     }
    
//     [Fact]
//     public void TestAssignmentWithExpression()
//     {
//         string program = "a := (2 ** 4)";
        
//         BlockStmt result = Parser.Parse(program);
        
//         Assert.NotNull(result);
//         Assert.Equal(1, result.Statements.Count);
//         Assert.IsType<AssignmentStmt>(result.Statements[0]);
        
//         var assignStmt = (AssignmentStmt)result.Statements[0];
//         Assert.Equal("a", assignStmt.Variable.Name);
//         Assert.IsType<ExponentiationNode>(assignStmt.Expression);
        
//         // Verify unparsing works correctly
//         Assert.Equal(program, assignStmt.Unparse());
//     }
    
//     [Fact]
//     public void TestComplexExpression()
//     {
//         string program = "x := (1 + (2 - (3 * (4 / (5 // 6)))))";
        
//         BlockStmt result = Parser.Parse(program);
        
//         Assert.NotNull(result);
//         Assert.Equal(1, result.Statements.Count);
//         Assert.IsType<AssignmentStmt>(result.Statements[0]);
        
//         var assignStmt = (AssignmentStmt)result.Statements[0];
//         Assert.Equal("x", assignStmt.Variable.Name);
//         Assert.IsType<PlusNode>(assignStmt.Expression);
        
//         // Verify unparsing works correctly
//         Assert.Equal(program, assignStmt.Unparse());
//     }
    
//     [Fact]
//     public void TestReturnStatement()
//     {
//         string program = "return ((a + b) - 2)";
        
//         BlockStmt result = Parser.Parse(program);
        
//         Assert.NotNull(result);
//         Assert.Equal(1, result.Statements.Count);
//         Assert.IsType<ReturnStmt>(result.Statements[0]);
        
//         var returnStmt = (ReturnStmt)result.Statements[0];
//         Assert.IsType<MinusNode>(returnStmt.Expression);
        
//         // Verify unparsing works correctly
//         Assert.Equal(program, returnStmt.Unparse());
//     }
    
//     [Fact]
//     public void TestMultipleStatements()
//     {
//         string program = "a := (5 * 2)" + Environment.NewLine +
//                          "b := (a + 3)" + Environment.NewLine +
//                          "return (a * b)";
        
//         BlockStmt result = Parser.Parse(program);
        
//         Assert.NotNull(result);
//         Assert.Equal(3, result.Statements.Count);
//         Assert.IsType<AssignmentStmt>(result.Statements[0]);
//         Assert.IsType<AssignmentStmt>(result.Statements[1]);
//         Assert.IsType<ReturnStmt>(result.Statements[2]);
        
//         // Verify unparsing works correctly
//         string unparsed = result.Unparse();
//         Assert.Equal(program, unparsed);
//     }
    
//     [Fact]
//     public void TestNestedBlocks()
//     {
//         string program = "a := (5 * 2)" + Environment.NewLine +
//                          "b := (a + 3)" + Environment.NewLine +
//                          "{" + Environment.NewLine +
//                          "    c := (b * 2)" + Environment.NewLine +
//                          "    a := (c - 1)" + Environment.NewLine +
//                          "    {" + Environment.NewLine +
//                          "        b := (a * 3)" + Environment.NewLine +
//                          "    }" + Environment.NewLine +
//                          "}" + Environment.NewLine +
//                          "return (a * b)";
        
//         BlockStmt result = Parser.Parse(program);
        
//         Assert.NotNull(result);
//         Assert.Equal(4, result.Statements.Count);
//         Assert.IsType<AssignmentStmt>(result.Statements[0]);
//         Assert.IsType<AssignmentStmt>(result.Statements[1]);
//         Assert.IsType<BlockStmt>(result.Statements[2]);
//         Assert.IsType<ReturnStmt>(result.Statements[3]);
        
//         var nestedBlock = (BlockStmt)result.Statements[2];
//         Assert.Equal(3, nestedBlock.Statements.Count);
//         Assert.IsType<BlockStmt>(nestedBlock.Statements[2]);
        
//         var innerBlock = (BlockStmt)nestedBlock.Statements[2];
//         Assert.Equal(1, innerBlock.Statements.Count);
        
//         // Verify unparsing works correctly
//         string unparsed = result.Unparse();
//         Assert.Equal(program, unparsed);
//     }
    
//     [Fact]
//     public void TestAllOperators()
//     {
//         string program = "a := (2 + 3)" + Environment.NewLine +
//                          "b := (4 - 1)" + Environment.NewLine +
//                          "c := (5 * 6)" + Environment.NewLine +
//                          "d := (7 / 2)" + Environment.NewLine +
//                          "e := (9 // 4)" + Environment.NewLine +
//                          "f := (10 % 3)" + Environment.NewLine +
//                          "g := (2 ** 3)" + Environment.NewLine +
//                          "return (((((a + b) - c) * d) / e) ** f)";
        
//         BlockStmt result = Parser.Parse(program);
        
//         Assert.NotNull(result);
//         Assert.Equal(8, result.Statements.Count);
        
//         // Verify each operator type
//         Assert.IsType<PlusNode>(((AssignmentStmt)result.Statements[0]).Expression);
//         Assert.IsType<MinusNode>(((AssignmentStmt)result.Statements[1]).Expression);
//         Assert.IsType<TimesNode>(((AssignmentStmt)result.Statements[2]).Expression);
//         Assert.IsType<FloatDivNode>(((AssignmentStmt)result.Statements[3]).Expression);
//         Assert.IsType<IntDivNode>(((AssignmentStmt)result.Statements[4]).Expression);
//         Assert.IsType<ModulusNode>(((AssignmentStmt)result.Statements[5]).Expression);
//         Assert.IsType<ExponentiationNode>(((AssignmentStmt)result.Statements[6]).Expression);
        
//         // Verify return statement has complex expression
//         Assert.IsType<ExponentiationNode>(((ReturnStmt)result.Statements[7]).Expression);
        
//         // Verify unparsing works correctly
//         string unparsed = result.Unparse();
//         Assert.Equal(program, unparsed);
//     }
    
//     [Fact]
//     public void TestFloatLiterals()
//     {
//         string program = "a := 5.25" + Environment.NewLine +
//                         "b := (a * 2.5)" + Environment.NewLine +
//                         "return (b / 0.5)";
        
//         BlockStmt result = Parser.Parse(program);
        
//         Assert.NotNull(result);
//         Assert.Equal(3, result.Statements.Count);
        
//         // Verify first assignment has float literal
//         var firstAssign = (AssignmentStmt)result.Statements[0];
//         Assert.IsType<LiteralNode>(firstAssign.Expression);
//         var literal = (LiteralNode)firstAssign.Expression;
//         Assert.Equal(5.25, literal.Value);
        
//         // Verify unparsing works correctly
//         string unparsed = result.Unparse();
//         Assert.Equal(program, unparsed);
//     }
    
//     [Fact]
//     public void TestInvalidStatementNoParentheses()
//     {
//         string program = "a := 5";
        
//         var exception = Assert.Throws<ParseException>(() => Parser.Parse(program));
//     }
    
//     [Fact]
//     public void TestInvalidTopLevelExpression()
//     {
//         string program = "(1 + 3)";
        
//         var exception = Assert.Throws<ParseException>(() => Parser.Parse(program));
//     }
    
//     [Fact]
//     public void TestInvalidAssignmentNoParentheses()
//     {
//         string program = "a := 1 + 3";
        
//         var exception = Assert.Throws<ParseException>(() => Parser.Parse(program));
//     }
    
//     [Fact]
//     public void TestInvalidVariableName()
//     {
//         string program = "A := (4 - 9)";
        
//         var exception = Assert.Throws<ParseException>(() => Parser.Parse(program));
//     }
    
//     [Fact]
//     public void TestInvalidReturnNoParentheses()
//     {
//         string program = "return 2 + 3";
        
//         var exception = Assert.Throws<ParseException>(() => Parser.Parse(program));
//     }
    
//     [Fact]
//     public void TestInvalidReturnWithParentheses()
//     {
//         string program = "(return 1)";
        
//         var exception = Assert.Throws<ParseException>(() => Parser.Parse(program));
//     }
    
//     [Fact]
//     public void TestInvalidAssignmentWithParentheses()
//     {
//         string program = "(a := 2)";
        
//         var exception = Assert.Throws<ParseException>(() => Parser.Parse(program));
//     }
    
//     [Fact]
//     public void TestUnbalancedOpeningBrace()
//     {
//         string program = "{" + Environment.NewLine +
//                          "a := (5)" + Environment.NewLine;
        
//         var exception = Assert.Throws<ParseException>(() => Parser.Parse(program));
//     }
    
//     [Fact]
//     public void TestUnbalancedClosingBrace()
//     {
//         string program = "a := (5)" + Environment.NewLine +
//                          "}";
        
//         var exception = Assert.Throws<ParseException>(() => Parser.Parse(program));
//     }
    
//     [Fact]
//     public void TestUnbalancedParentheses()
//     {
//         string program = "a := (5 * (2 + 3)";
        
//         var exception = Assert.Throws<ParseException>(() => Parser.Parse(program));
//     }
    
//     [Fact]
//     public void TestInvalidBlockSyntax()
//     {
//         string program = "{ a := (5) }";
        
//         var exception = Assert.Throws<ParseException>(() => Parser.Parse(program));
//     }
    
//     [Fact]
//     public void TestEmptyParentheses()
//     {
//         string program = "a := ()";
        
//         var exception = Assert.Throws<ParseException>(() => Parser.Parse(program));
//     }
    
//     [Fact]
//     public void TestMissingLeftOperand()
//     {
//         string program = "a := (+ 5)";
        
//         var exception = Assert.Throws<ParseException>(() => Parser.Parse(program));
//     }
    
//     [Fact]
//     public void TestMissingRightOperand()
//     {
//         string program = "a := (5 +)";
        
//         var exception = Assert.Throws<ParseException>(() => Parser.Parse(program));
//     }
    
//     [Fact]
//     public void TestInvalidCharacter()
//     {
//         string program = "a := (5 @ 3)";
        
//         var exception = Assert.Throws<ParseException>(() => Parser.Parse(program));
//     }
    
//     [Fact]
//     public void TestMultipleDecimalPoints()
//     {
//         string program = "a := 5.6.7";
        
//         var exception = Assert.Throws<ParseException>(() => Parser.Parse(program));
//     }
    
//     [Fact]
//     public void TestEmptyExpression()
//     {
//         string program = "a := ";
        
//         var exception = Assert.Throws<ParseException>(() => Parser.Parse(program));
//     }
    
//     [Fact]
//     public void TestShadowing()
//     {
//         string program = "a := 5" + Environment.NewLine +
//                          "{" + Environment.NewLine +
//                          "    a := 10" + Environment.NewLine +
//                          "    b := (a * 2)" + Environment.NewLine +
//                          "}" + Environment.NewLine +
//                          "c := (a + 3)";
        
//         BlockStmt result = Parser.Parse(program);
        
//         Assert.NotNull(result);
//         Assert.Equal(3, result.Statements.Count);
//         Assert.IsType<AssignmentStmt>(result.Statements[0]);
//         Assert.IsType<BlockStmt>(result.Statements[1]);
//         Assert.IsType<AssignmentStmt>(result.Statements[2]);
        
//         var nestedBlock = (BlockStmt)result.Statements[1];
//         Assert.Equal(2, nestedBlock.Statements.Count);
        
//         // Verify unparsing works correctly
//         string unparsed = result.Unparse();
//         Assert.Equal(program, unparsed);
        
//         // Verify symbol tables
//         Assert.True(result.SymbolTable.ContainsKey("a"));
//         Assert.True(result.SymbolTable.ContainsKey("c"));
//         Assert.False(result.SymbolTable.ContainsKey("b"));
        
//         Assert.True(nestedBlock.SymbolTable.ContainsKey("a"));
//         Assert.True(nestedBlock.SymbolTable.ContainsKey("b"));
//     }
    
//     [Fact]
//     public void TestEmptyBlock()
//     {
//         string program = "a := 5" + Environment.NewLine +
//                          "{" + Environment.NewLine +
//                          "}" + Environment.NewLine +
//                          "b := 10";
        
//         BlockStmt result = Parser.Parse(program);
        
//         Assert.NotNull(result);
//         Assert.Equal(3, result.Statements.Count);
//         Assert.IsType<AssignmentStmt>(result.Statements[0]);
//         Assert.IsType<BlockStmt>(result.Statements[1]);
//         Assert.IsType<AssignmentStmt>(result.Statements[2]);
        
//         var nestedBlock = (BlockStmt)result.Statements[1];
//         Assert.Equal(0, nestedBlock.Statements.Count);
        
//         // Verify unparsing works correctly
//         string unparsed = result.Unparse();
//         Assert.Equal(program, unparsed);
//     }
    
//     [Fact]
//     public void TestDeepNestedBlocks()
//     {
//         string program = "a := 1" + Environment.NewLine +
//                          "{" + Environment.NewLine +
//                          "    b := 2" + Environment.NewLine +
//                          "    {" + Environment.NewLine +
//                          "        c := 3" + Environment.NewLine +
//                          "        {" + Environment.NewLine +
//                          "            d := 4" + Environment.NewLine +
//                          "            {" + Environment.NewLine +
//                          "                e := 5" + Environment.NewLine +
//                          "            }" + Environment.NewLine +
//                          "        }" + Environment.NewLine +
//                          "    }" + Environment.NewLine +
//                          "}" + Environment.NewLine +
//                          "return ((((a + b) + c) + d) + e)";
        
//         BlockStmt result = Parser.Parse(program);
        
//         Assert.NotNull(result);
//         Assert.Equal(3, result.Statements.Count);
        
//         // Verify unparsing works correctly
//         string unparsed = result.Unparse();
//         Assert.Equal(program, unparsed);
//     }
    
//     [Fact]
//     public void TestStandaloneValue()
//     {
//         string program = "a := 5" + Environment.NewLine +
//                          "b := a" + Environment.NewLine +
//                          "return b";
        
//         BlockStmt result = Parser.Parse(program);
        
//         Assert.NotNull(result);
//         Assert.Equal(3, result.Statements.Count);
        
//         // Verify unparsing works correctly
//         string unparsed = result.Unparse();
//         Assert.Equal(program, unparsed);
//     }
// }