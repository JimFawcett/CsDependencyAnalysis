# CsDependencyAnalysis

https://JimFawcett.github.io/CsDependencyAnalysis.html

Static type-based analysis of file dependencies for all C# files in a specified directory tree.

## Overview

The analyzer runs two passes over a directory tree of C# source files:

1. **Type Analysis** — parses files to build a type table of all defined namespaces, classes, structs, interfaces, enums, and delegates.
2. **Dependency Analysis** — parses files again to find type usages (declarations, parameters, inheritance) and maps each file to the files it depends on.

A dependency graph is then built from the dependency table and analyzed for **strong components** (cycles) using Tarjan's algorithm.

## Packages

| Package | Description |
|---|---|
| `DemoExecutive` | Entry point; orchestrates type analysis, dependency analysis, graph construction, and strong component detection |
| `Parser` | Rule-based parser pipeline: applies rules to semi-expressions and fires actions |
| `SemiExp` | Semi-expression collector; groups tokens into meaningful statement units |
| `Toker` | Tokenizer; breaks source text into tokens |
| `TypeTable` | Stores type definitions found during Pass 1 |
| `DependencyTable` | Stores per-file dependency lists built during Pass 2 |
| `Element` | `Elem` struct holding type category, name, file, and namespace |
| `FileMgr` | Directory tree navigator; collects files matching a pattern |
| `CsGraph` | Generic directed graph with strong-component (Tarjan) analysis |
| `Display` | Formatted console output helpers |

## Usage

```
dotnet run --project DemoExecutive/DemoExecutive.csproj <path> <pattern>
```

Example (analyze the project itself):

```
dotnet run --project DemoExecutive/DemoExecutive.csproj ../../../ *.cs
```

## Build

Requires .NET 10.0 SDK.

```
dotnet build DemoExecutive/DemoExecutive.csproj
```
