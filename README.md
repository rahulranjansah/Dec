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
