# Compiler Project

A compiler implementation in C# for a simple programming language featuring arithmetic expressions, variable assignments, block statements with lexical scoping, and return statements.

## Overview

This project implements a complete compiler pipeline with the following stages:

1. **Tokenization** - Converts source code into a stream of tokens
2. **Parsing** - Builds an Abstract Syntax Tree (AST) from tokens
3. **Name Analysis** - Performs semantic analysis on variable names
4. **Evaluation** - Executes the program by traversing the AST

## Features

### Language Support

- **Arithmetic Operators**: `+`, `-`, `*`, `/` (float division), `//` (integer division), `%` (modulus), `**` (exponentiation)
- **Variables**: Identifier-based variable storage and retrieval
- **Assignment Statements**: Variable assignment using `:=` operator
- **Return Statements**: Return values from blocks
- **Block Statements**: Lexically scoped code blocks with hierarchical symbol tables
- **Parenthesized Expressions**: Grouping for precedence control

### Project Components

#### **AST** (`src/AST/`)
Abstract Syntax Tree node definitions and visitor pattern implementation:
- Expression nodes: `PlusNode`, `MinusNode`, `TimesNode`, `FloatDivNode`, `IntDivNode`, `ModulusNode`, `ExponentiationNode`
- Literal and variable nodes: `LiteralNode`, `VariableNode`
- Statement nodes: `AssignmentStmt`, `ReturnStmt`, `BlockStmt`
- Visitor interface: `IVisitor<TParam, TResult>`

#### **Builders** (`src/Builders/`)
Factory pattern implementations for AST node creation:
- `DefaultBuilder` - Standard AST node construction
- `DebugBuilder` - Enhanced builder with null-checking and validation
- `NullBuilder` - Returns null for all operations (useful for testing)

#### **Containers** (`src/Containers/`)
Custom data structures:
- `DLL<T>` - Doubly-linked list implementation
- `SymbolTable<TKey, TValue>` - Hierarchical symbol table with parent scope support
- `BinaryTree<T>` - Binary tree with digit tree specialization

#### **Parsers** (`src/Parsers/`)
Recursive descent parser that converts token streams to AST:
- Expression parsing with operator precedence
- Statement parsing (assignments, returns, blocks)
- Block statement parsing with scope management
- Comprehensive error reporting with `ParseException`

#### **Tokenizer** (`src/Tokenizer/`)
Lexical analyzer that breaks source code into tokens:
- Token types: `RETURN`, `VARIABLE`, `INTEGER`, `FLOAT`, `ASSIGNMENT`, `OPERATOR`, `LEFT_PAREN`, `RIGHT_PAREN`, `LEFT_CURLY`, `RIGHT_CURLY`
- Supports multi-character operators (`//`, `**`, `:=`)
- Whitespace handling and string tokenization

#### **Visitors** (`src/Visitors/`)
AST traversal implementations using the Visitor pattern:
- `UnparseVisitor` - Converts AST back to source code string
- `EvaluateVisitor` - Interprets and executes the AST
- `NameAnalysisVisitor` - (In development) Semantic analysis

#### **Utilities** (`src/Utilities/`)
Helper functions for general tasks:
- String manipulation (camelCase conversion, occurrence counting)
- Array operations (duplicates, unique items)
- Validation (password strength, variable names, operators)
- Indentation utilities for code formatting

## Architecture

### Design Patterns

- **Visitor Pattern**: Used for AST traversal (unparsing, evaluation, name analysis)
- **Builder Pattern**: Flexible AST node creation with multiple implementations
- **Composite Pattern**: Hierarchical AST structure with expression and statement nodes
- **Chain of Responsibility**: Symbol table parent chain for variable resolution

### Symbol Table Hierarchy

The symbol table implementation supports lexical scoping:
- Each `BlockStmt` has its own symbol table
- Symbol tables reference parent scopes
- Variable lookup searches current scope, then parent scopes recursively
- Local operations (`ContainsKeyLocal`, `TryGetValueLocal`) work only on current scope

## Testing

Comprehensive test coverage using xUnit:

- **AST.Tests** - AST node functionality
- **Builders.Tests** - Builder pattern implementations (331 tests)
- **Containers.Tests** - Data structure correctness (DLL, SymbolTable, BinaryTree)
- **Parsers.Tests** - Parser functionality including expressions, statements, and blocks
- **Tokenizer.Tests** - Tokenization accuracy
- **Utilities.Tests** - Utility function validation

Run all tests:

```bash
dotnet test
```

## Example Usage

```csharp
using Parser;

// Parse a simple program
string program = @"
{
    x := 5;
    y := (x + 3);
    return (y * 2);
}";

BlockStmt ast = Parser.Parse(program);

// Unparse to verify
string unparsed = ast.Unparse();
Console.WriteLine(unparsed);

// Evaluate the program
var evaluator = new EvaluateVisitor();
object result = evaluator.Evaluate(ast);
Console.WriteLine($"Result: {result}"); // Output: Result: 16
```

## Helpful Commands

Example commands for managing the .NET solution and projects.

Build and Clean the Solution

```bash

dotnet clean
rm -rf **/bin **/obj

dotnet restore
dotnet build -v n
```

Create a New Project and Add it to the Solution
```bash
dotnet new classlib -n Parsers
dotnet sln Compiler.sln add Parsers/Parsers.csproj
```
Create a New Test Project and Add it to the Solution
```bash
dotnet new xunit -n Parsers.Tests
dotnet sln ../src/Compiler.sln add Parsers.Tests/Parsers.Tests.csproj
dotnet add Parsers.Tests/Parsers.Tests.csproj reference ../src/Parsers/Parsers.csproj
```
Track Changes to the Solution File
```bash
dotnet sln Compiler.sln list
dotnet sln Compiler.sln remove src/Utilities/Containers/Containers.csproj
```


## Requirements

- .NET 9.0 SDK or later
- C# 10.0 or later

## License

This project is an educational compiler implementation.
