using System;
using System.Collections.Generic;
using Xunit;
using AST;
using Tokenizer;
using System.Reflection;
using System.Linq;

namespace Parser.Tests
{
    public class ParseStmtListTests
    {
        // Helper method to invoke the private ParseStmtList method using reflection
        private void InvokeParseStmtList(List<string> lines, BlockStmt blockStmt)
        {
            Type parserType = typeof(Parser);
            MethodInfo methodInfo = parserType.GetMethod("ParseStmtList", 
                BindingFlags.NonPublic | BindingFlags.Static);
            
            methodInfo.Invoke(null, new object[] { lines, blockStmt });
        }
        
        // Helper method to create a BlockStmt with a symbol table
        private BlockStmt CreateBlockStmt(SymbolTable<string, object> parentSymbolTable = null)
        {
            var symbolTable = parentSymbolTable == null ? 
                new SymbolTable<string, object>() : 
                new SymbolTable<string, object>(parentSymbolTable);
                
            return new BlockStmt(symbolTable);
        }
        
        #region Simple Statement Lists (No Nesting)
        
        [Fact]
        public void ParseStmtList_EmptyBlock_ReturnsCorrectly()
        {
            // Arrange
            var lines = new List<string> { "}" };
            var blockStmt = CreateBlockStmt();
            
            // Act
            InvokeParseStmtList(lines, blockStmt);
            
            // Assert
            Assert.Empty(blockStmt.Statements);
            Assert.Single(lines); // '}' should still be in the lines
        }
        
        [Fact]
        public void ParseStmtList_SingleAssignment_ReturnsCorrectly()
        {
            // Arrange
            var lines = new List<string> 
            { 
                "x := (42)",
                "}" 
            };
            var blockStmt = CreateBlockStmt();
            
            // Act
            InvokeParseStmtList(lines, blockStmt);
            
            // Assert
            Assert.Single(blockStmt.Statements);
            Assert.IsType<AssignmentStmt>(blockStmt.Statements.First());
            
            var assignStmt = (AssignmentStmt)blockStmt.Statements.First();
            Assert.Equal("x", assignStmt.Variable.Name);
            Assert.IsType<LiteralNode>(assignStmt.Expression);
            Assert.Equal(42, ((LiteralNode)assignStmt.Expression).Value);
            
            Assert.Single(lines); // '}' should still be in the lines
        }
        
        [Fact]
        public void ParseStmtList_SingleReturn_ReturnsCorrectly()
        {
            // Arrange
            var lines = new List<string> 
            { 
                "return (x)",
                "}" 
            };
            var blockStmt = CreateBlockStmt();
            
            // Act
            InvokeParseStmtList(lines, blockStmt);
            
            // Assert
            Assert.Single(blockStmt.Statements);
            Assert.IsType<ReturnStmt>(blockStmt.Statements.First());
            
            var returnStmt = (ReturnStmt)blockStmt.Statements.First();
            Assert.IsType<VariableNode>(returnStmt.Expression);
            Assert.Equal("x", ((VariableNode)returnStmt.Expression).Name);
            
            Assert.Single(lines); // '}' should still be in the lines
        }
        
        [Fact]
        public void ParseStmtList_MultipleStatements_ReturnsCorrectly()
        {
            // Arrange
            var lines = new List<string> 
            { 
                "x := (5)",
                "y := (10)",
                "z := (x + y)",
                "return (z)",
                "}" 
            };
            var blockStmt = CreateBlockStmt();
            
            // Act
            InvokeParseStmtList(lines, blockStmt);
            
            // Assert
            Assert.Equal(4, blockStmt.Statements.Count);
            
            Assert.IsType<AssignmentStmt>(blockStmt.Statements[0]);
            Assert.IsType<AssignmentStmt>(blockStmt.Statements[1]);
            Assert.IsType<AssignmentStmt>(blockStmt.Statements[2]);
            Assert.IsType<ReturnStmt>(blockStmt.Statements[3]);
            
            // Check that the symbol table has the variables
            Assert.True(blockStmt.SymbolTable.ContainsKey("x"));
            Assert.True(blockStmt.SymbolTable.ContainsKey("y"));
            Assert.True(blockStmt.SymbolTable.ContainsKey("z"));
            
            Assert.Single(lines); // '}' should still be in the lines
        }
        
        [Fact]
        public void ParseStmtList_InvalidStatement_ThrowsParseException()
        {
            // Arrange
            var lines = new List<string> 
            { 
                "invalid statement",
                "}" 
            };
            var blockStmt = CreateBlockStmt();
            
            // Act & Assert
            var exception = Assert.Throws<TargetInvocationException>(() => InvokeParseStmtList(lines, blockStmt));
            Assert.IsType<ParseException>(exception.InnerException);
        }
        
        [Fact]
        public void ParseStmtList_MissingClosingBrace_ThrowsParseException()
        {
            // Arrange
            var lines = new List<string> 
            { 
                "x := (42)"
                // No closing brace
            };
            var blockStmt = CreateBlockStmt();
            
            // Act & Assert
            var exception = Assert.Throws<TargetInvocationException>(() => InvokeParseStmtList(lines, blockStmt));
            Assert.IsType<ParseException>(exception.InnerException);
        }
        
        #endregion
        
        #region Nested Blocks
        
        [Fact]
        public void ParseStmtList_SingleNestedEmptyBlock_ReturnsCorrectly()
        {
            // Arrange
            var lines = new List<string> 
            { 
                "{",
                "}",
                "}" 
            };
            var blockStmt = CreateBlockStmt();
            
            // Act
            InvokeParseStmtList(lines, blockStmt);
            
            // Assert
            Assert.Single(blockStmt.Statements);
            Assert.IsType<BlockStmt>(blockStmt.Statements.First());
            
            var nestedBlock = (BlockStmt)blockStmt.Statements.First();
            Assert.Empty(nestedBlock.Statements);
            
            Assert.Single(lines); // '}' should still be in the lines
        }
        
        [Fact]
        public void ParseStmtList_NestedBlockWithStatements_ReturnsCorrectly()
        {
            // Arrange
            var lines = new List<string> 
            { 
                "{",
                "x := (42)",
                "y := (x + 10)",
                "}",
                "}" 
            };
            var blockStmt = CreateBlockStmt();
            
            // Act
            InvokeParseStmtList(lines, blockStmt);
            
            // Assert
            Assert.Single(blockStmt.Statements);
            Assert.IsType<BlockStmt>(blockStmt.Statements.First());
            
            var nestedBlock = (BlockStmt)blockStmt.Statements.First();
            Assert.Equal(2, nestedBlock.Statements.Count);
            
            Assert.IsType<AssignmentStmt>(nestedBlock.Statements[0]);
            Assert.IsType<AssignmentStmt>(nestedBlock.Statements[1]);
            
            // Check that the variables are in the nested symbol table
            Assert.True(nestedBlock.SymbolTable.ContainsKey("x"));
            Assert.True(nestedBlock.SymbolTable.ContainsKey("y"));
            
            // But not in the parent symbol table
            Assert.False(blockStmt.SymbolTable.ContainsKey("x"));
            Assert.False(blockStmt.SymbolTable.ContainsKey("y"));
            
            Assert.Single(lines); // '}' should still be in the lines
        }
        
        [Fact]
        public void ParseStmtList_MultipleNestedBlocks_ReturnsCorrectly()
        {
            // Arrange
            var lines = new List<string> 
            { 
                "x := (1)",
                "{",
                "y := (2)",
                "}",
                "{",
                "z := (3)",
                "}",
                "}" 
            };
            var blockStmt = CreateBlockStmt();
            
            // Act
            InvokeParseStmtList(lines, blockStmt);
            
            // Assert
            Assert.Equal(3, blockStmt.Statements.Count);
            
            Assert.IsType<AssignmentStmt>(blockStmt.Statements[0]);
            Assert.IsType<BlockStmt>(blockStmt.Statements[1]);
            Assert.IsType<BlockStmt>(blockStmt.Statements[2]);
            
            var firstNestedBlock = (BlockStmt)blockStmt.Statements[1];
            Assert.Single(firstNestedBlock.Statements);
            Assert.IsType<AssignmentStmt>(firstNestedBlock.Statements[0]);
            Assert.Equal("y", ((AssignmentStmt)firstNestedBlock.Statements[0]).Variable.Name);
            
            var secondNestedBlock = (BlockStmt)blockStmt.Statements[2];
            Assert.Single(secondNestedBlock.Statements);
            Assert.IsType<AssignmentStmt>(secondNestedBlock.Statements[0]);
            Assert.Equal("z", ((AssignmentStmt)secondNestedBlock.Statements[0]).Variable.Name);
            
            Assert.Single(lines); // '}' should still be in the lines
        }
        
        [Fact]
        public void ParseStmtList_DeepNesting_ReturnsCorrectly()
        {
            // Arrange
            var lines = new List<string> 
            { 
                "a := (1)",
                "{",
                "b := (2)",
                "{",
                "c := (3)",
                "{",
                "d := (4)",
                "}",
                "}",
                "}",
                "}" 
            };
            var blockStmt = CreateBlockStmt();
            
            // Act
            InvokeParseStmtList(lines, blockStmt);
            
            // Assert
            Assert.Equal(2, blockStmt.Statements.Count);
            
            Assert.IsType<AssignmentStmt>(blockStmt.Statements[0]);
            Assert.IsType<BlockStmt>(blockStmt.Statements[1]);
            
            var level1Block = (BlockStmt)blockStmt.Statements[1];
            Assert.Equal(2, level1Block.Statements.Count);
            Assert.IsType<AssignmentStmt>(level1Block.Statements[0]);
            Assert.IsType<BlockStmt>(level1Block.Statements[1]);
            
            var level2Block = (BlockStmt)level1Block.Statements[1];
            Assert.Equal(2, level2Block.Statements.Count);
            Assert.IsType<AssignmentStmt>(level2Block.Statements[0]);
            Assert.IsType<BlockStmt>(level2Block.Statements[1]);
            
            var level3Block = (BlockStmt)level2Block.Statements[1];
            Assert.Single(level3Block.Statements);
            Assert.IsType<AssignmentStmt>(level3Block.Statements[0]);
            
            // Check proper scoping
            Assert.True(blockStmt.SymbolTable.ContainsKey("a"));
            Assert.False(blockStmt.SymbolTable.ContainsKey("b"));
            Assert.False(blockStmt.SymbolTable.ContainsKey("c"));
            Assert.False(blockStmt.SymbolTable.ContainsKey("d"));
            
            Assert.True(level1Block.SymbolTable.ContainsKey("b"));
            Assert.False(level1Block.SymbolTable.ContainsKey("c"));
            Assert.False(level1Block.SymbolTable.ContainsKey("d"));
            
            Assert.True(level2Block.SymbolTable.ContainsKey("c"));
            Assert.False(level2Block.SymbolTable.ContainsKey("d"));
            
            Assert.True(level3Block.SymbolTable.ContainsKey("d"));
            
            // Check variable lookup through parent scopes
            Assert.True(level3Block.SymbolTable.ContainsKey("c"));
            Assert.True(level3Block.SymbolTable.ContainsKey("b"));
            Assert.True(level3Block.SymbolTable.ContainsKey("a"));
            
            Assert.Single(lines); // '}' should still be in the lines
        }
        
        [Fact]
        public void ParseStmtList_MissingClosingBraceInNestedBlock_ThrowsParseException()
        {
            // Arrange
            var lines = new List<string> 
            { 
                "{",
                "x := (42)",
                // Missing closing brace for nested block
                "}" 
            };
            var blockStmt = CreateBlockStmt();
            
            // Act & Assert
            var exception = Assert.Throws<TargetInvocationException>(() => InvokeParseStmtList(lines, blockStmt));
            Assert.IsType<ParseException>(exception.InnerException);
        }
        
        [Fact]
        public void ParseStmtList_NestedBlockWithReturn_ReturnsCorrectly()
        {
            // Arrange
            var lines = new List<string> 
            { 
                "x := (1)",
                "{",
                "y := (2)",
                "return (x + y)",
                "}",
                "}" 
            };
            var blockStmt = CreateBlockStmt();
            
            // Act
            InvokeParseStmtList(lines, blockStmt);
            
            // Assert
            Assert.Equal(2, blockStmt.Statements.Count);
            
            Assert.IsType<AssignmentStmt>(blockStmt.Statements[0]);
            Assert.IsType<BlockStmt>(blockStmt.Statements[1]);
            
            var nestedBlock = (BlockStmt)blockStmt.Statements[1];
            Assert.Equal(2, nestedBlock.Statements.Count);
            
            Assert.IsType<AssignmentStmt>(nestedBlock.Statements[0]);
            Assert.IsType<ReturnStmt>(nestedBlock.Statements[1]);
            
            var returnStmt = (ReturnStmt)nestedBlock.Statements[1];
            Assert.IsType<PlusNode>(returnStmt.Expression);
            
            Assert.Single(lines); // '}' should still be in the lines
        }
        
        [Fact]
        public void ParseStmtList_VerifyUnparse_ReturnsCorrectString()
        {
            // Arrange
            var lines = new List<string> 
            { 
                "x := (42)",
                "{",
                "y := (x + 10)",
                "}",
                "}" 
            };
            var blockStmt = CreateBlockStmt();
            
            // Act
            InvokeParseStmtList(lines, blockStmt);
            
            // Assert
            var unparseResult = blockStmt.Unparse();
            
            // Check that the unparsed result contains the expected elements
            Assert.Contains("x := 42", unparseResult);
            Assert.Contains("y := (x + 10)", unparseResult);
            Assert.Contains("{", unparseResult);
            Assert.Contains("}", unparseResult);
        }
        
		[Fact]
		public void ParseStmtList_ComplexProgram_ReturnsCorrectly()
		{
			// Arrange - a more complex program with multiple levels and various statements
			var lines = new List<string> 
			{ 
				"a := (10)",
				"b := (20)",
				"{",
				"c := (a + b)",
				"{",
				"d := (c * 2)",
				"return (d)",
				"}",
				"e := (30)",
				"}",
				"f := (40)",
				"}" 
			};
			var blockStmt = CreateBlockStmt();
			
			// Act
			InvokeParseStmtList(lines, blockStmt);
			
			// Assert
			Assert.Equal(4, blockStmt.Statements.Count); // a, b, nested block, f
			
			Assert.IsType<AssignmentStmt>(blockStmt.Statements[0]);
			Assert.IsType<AssignmentStmt>(blockStmt.Statements[1]);
			Assert.IsType<BlockStmt>(blockStmt.Statements[2]);
			Assert.IsType<AssignmentStmt>(blockStmt.Statements[3]);
			
			var nestedBlock = (BlockStmt)blockStmt.Statements[2];
			Assert.Equal(3, nestedBlock.Statements.Count); // c, nested block, e
			
			Assert.IsType<AssignmentStmt>(nestedBlock.Statements[0]);
			Assert.IsType<BlockStmt>(nestedBlock.Statements[1]);
			Assert.IsType<AssignmentStmt>(nestedBlock.Statements[2]);
			
			var innerBlock = (BlockStmt)nestedBlock.Statements[1];
			Assert.Equal(2, innerBlock.Statements.Count); // d, return
			
			Assert.IsType<AssignmentStmt>(innerBlock.Statements[0]);
			Assert.IsType<ReturnStmt>(innerBlock.Statements[1]);
			
			// Check variable definitions in correct scopes
			// Top level scope
			Assert.True(blockStmt.SymbolTable.ContainsKeyLocal("a"));
			Assert.True(blockStmt.SymbolTable.ContainsKeyLocal("b"));
			Assert.True(blockStmt.SymbolTable.ContainsKeyLocal("f"));
			Assert.False(blockStmt.SymbolTable.ContainsKeyLocal("c"));
			Assert.False(blockStmt.SymbolTable.ContainsKeyLocal("d"));
			Assert.False(blockStmt.SymbolTable.ContainsKeyLocal("e"));
			
			// Middle level scope
			Assert.False(nestedBlock.SymbolTable.ContainsKeyLocal("a"));
			Assert.False(nestedBlock.SymbolTable.ContainsKeyLocal("b"));
			Assert.False(nestedBlock.SymbolTable.ContainsKeyLocal("f"));
			Assert.True(nestedBlock.SymbolTable.ContainsKeyLocal("c"));
			Assert.True(nestedBlock.SymbolTable.ContainsKeyLocal("e"));
			Assert.False(nestedBlock.SymbolTable.ContainsKeyLocal("d"));
			
			// Inner level scope
			Assert.True(innerBlock.SymbolTable.ContainsKeyLocal("d"));
			Assert.False(innerBlock.SymbolTable.ContainsKeyLocal("a"));
			Assert.False(innerBlock.SymbolTable.ContainsKeyLocal("b"));
			Assert.False(innerBlock.SymbolTable.ContainsKeyLocal("c"));
			Assert.False(innerBlock.SymbolTable.ContainsKeyLocal("e"));
			Assert.False(innerBlock.SymbolTable.ContainsKeyLocal("f"));
			
			// Check variable accessibility through parent scopes
			// Inner block should have access to all variables defined in parent scopes
			Assert.True(innerBlock.SymbolTable.ContainsKey("a")); // From top level
			Assert.True(innerBlock.SymbolTable.ContainsKey("b")); // From top level
			Assert.True(innerBlock.SymbolTable.ContainsKey("c")); // From middle level
			Assert.True(innerBlock.SymbolTable.ContainsKey("d")); // From inner level
			Assert.True(innerBlock.SymbolTable.ContainsKey("e")); // From middle level
			Assert.True(innerBlock.SymbolTable.ContainsKey("f")); // From top level
			
			// Middle block should have access to top level variables
			Assert.True(nestedBlock.SymbolTable.ContainsKey("a")); 
			Assert.True(nestedBlock.SymbolTable.ContainsKey("b"));
			Assert.True(nestedBlock.SymbolTable.ContainsKey("f"));
			
			Assert.Single(lines); // '}' should still be in the lines
		}
        
        [Fact]
        public void ParseStmtList_AddStatement_AddsToBlockCorrectly()
        {
            // Arrange
            var lines = new List<string> { "}" };
            var blockStmt = CreateBlockStmt();
            var assignStmt = new AssignmentStmt(new VariableNode("x"), new LiteralNode(42));
            
            // Act
            blockStmt.AddStatement(assignStmt);
            InvokeParseStmtList(lines, blockStmt);
            
            // Assert
            Assert.Single(blockStmt.Statements);
            Assert.Same(assignStmt, blockStmt.Statements[0]);
        }

		[Fact]
		public void ParseStmtList_TwoInnerScopes_ReturnsCorrectly()
		{
			// Arrange - a program with one outer scope and two inner scopes
			var lines = new List<string> 
			{ 
				"a := (100)",      // Variable in outer scope
				"{",               // First inner scope
				"b := (a + 5)",
				"c := (200)",
				"}",
				"d := (300)",      // Variable in outer scope, between inner scopes
				"{",               // Second inner scope
				"e := (a + d)",
				"f := (400)",
				"}",
				"g := (500)",      // Variable in outer scope, after all inner scopes
				"}" 
			};
			var blockStmt = CreateBlockStmt();
			
			// Act
			InvokeParseStmtList(lines, blockStmt);
			
			// Assert
			Assert.Equal(5, blockStmt.Statements.Count); // a, first inner block, d, second inner block, g
			
			Assert.IsType<AssignmentStmt>(blockStmt.Statements[0]); // a
			Assert.IsType<BlockStmt>(blockStmt.Statements[1]);      // first inner block
			Assert.IsType<AssignmentStmt>(blockStmt.Statements[2]); // d
			Assert.IsType<BlockStmt>(blockStmt.Statements[3]);      // second inner block
			Assert.IsType<AssignmentStmt>(blockStmt.Statements[4]); // g
			
			// Get the inner blocks
			var firstInnerBlock = (BlockStmt)blockStmt.Statements[1];
			var secondInnerBlock = (BlockStmt)blockStmt.Statements[3];
			
			// Check statements in first inner block
			Assert.Equal(2, firstInnerBlock.Statements.Count);
			Assert.IsType<AssignmentStmt>(firstInnerBlock.Statements[0]); // b
			Assert.IsType<AssignmentStmt>(firstInnerBlock.Statements[1]); // c
			
			// Check statements in second inner block
			Assert.Equal(2, secondInnerBlock.Statements.Count);
			Assert.IsType<AssignmentStmt>(secondInnerBlock.Statements[0]); // e
			Assert.IsType<AssignmentStmt>(secondInnerBlock.Statements[1]); // f
			
			// Check variable definitions in correct scopes using ContainsKeyLocal
			// Outer scope should contain a, d, and g
			Assert.True(blockStmt.SymbolTable.ContainsKeyLocal("a"));
			Assert.True(blockStmt.SymbolTable.ContainsKeyLocal("d"));
			Assert.True(blockStmt.SymbolTable.ContainsKeyLocal("g"));
			Assert.False(blockStmt.SymbolTable.ContainsKeyLocal("b"));
			Assert.False(blockStmt.SymbolTable.ContainsKeyLocal("c"));
			Assert.False(blockStmt.SymbolTable.ContainsKeyLocal("e"));
			Assert.False(blockStmt.SymbolTable.ContainsKeyLocal("f"));
			
			// First inner scope should contain b and c
			Assert.True(firstInnerBlock.SymbolTable.ContainsKeyLocal("b"));
			Assert.True(firstInnerBlock.SymbolTable.ContainsKeyLocal("c"));
			Assert.False(firstInnerBlock.SymbolTable.ContainsKeyLocal("a"));
			Assert.False(firstInnerBlock.SymbolTable.ContainsKeyLocal("d"));
			Assert.False(firstInnerBlock.SymbolTable.ContainsKeyLocal("g"));
			Assert.False(firstInnerBlock.SymbolTable.ContainsKeyLocal("e"));
			Assert.False(firstInnerBlock.SymbolTable.ContainsKeyLocal("f"));
			
			// Second inner scope should contain e and f
			Assert.True(secondInnerBlock.SymbolTable.ContainsKeyLocal("e"));
			Assert.True(secondInnerBlock.SymbolTable.ContainsKeyLocal("f"));
			Assert.False(secondInnerBlock.SymbolTable.ContainsKeyLocal("a"));
			Assert.False(secondInnerBlock.SymbolTable.ContainsKeyLocal("d"));
			Assert.False(secondInnerBlock.SymbolTable.ContainsKeyLocal("g"));
			Assert.False(secondInnerBlock.SymbolTable.ContainsKeyLocal("b"));
			Assert.False(secondInnerBlock.SymbolTable.ContainsKeyLocal("c"));
			
			// Test variable accessibility through parent scopes
			// First inner scope should access to a, d, g, b, c
			Assert.True(firstInnerBlock.SymbolTable.ContainsKey("a"));
			Assert.True(firstInnerBlock.SymbolTable.ContainsKey("d"));
			Assert.True(firstInnerBlock.SymbolTable.ContainsKey("g"));
			Assert.True(firstInnerBlock.SymbolTable.ContainsKey("b"));
            Assert.True(firstInnerBlock.SymbolTable.ContainsKey("c"));
			Assert.False(firstInnerBlock.SymbolTable.ContainsKey("e"));
			Assert.False(firstInnerBlock.SymbolTable.ContainsKey("f"));

			// Second inner scope should access a, d, g, e, f (defined later)
			Assert.True(secondInnerBlock.SymbolTable.ContainsKey("a"));
			Assert.True(secondInnerBlock.SymbolTable.ContainsKey("d"));
			Assert.True(secondInnerBlock.SymbolTable.ContainsKey("g"));
			Assert.False(secondInnerBlock.SymbolTable.ContainsKey("b"));
            Assert.False(secondInnerBlock.SymbolTable.ContainsKey("c"));
			Assert.True(secondInnerBlock.SymbolTable.ContainsKey("e"));
			Assert.True(secondInnerBlock.SymbolTable.ContainsKey("f"));
			
			// Verify the expression in e references both a and d variables
			var eAssign = (AssignmentStmt)secondInnerBlock.Statements[0];
			Assert.IsType<PlusNode>(eAssign.Expression);
			
			var plusNode = (PlusNode)eAssign.Expression;
			Assert.IsType<VariableNode>(((dynamic)plusNode).Left);
			Assert.IsType<VariableNode>(((dynamic)plusNode).Right);
			
			Assert.Equal("a", ((VariableNode)((dynamic)plusNode).Left).Name);
			Assert.Equal("d", ((VariableNode)((dynamic)plusNode).Right).Name);
			
			Assert.Single(lines); // '}' should still be in the lines
		}
        
        #endregion
    }
}