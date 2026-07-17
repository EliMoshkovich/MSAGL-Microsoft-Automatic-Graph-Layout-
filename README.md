# SQL Server Schema Visualizer (MSAGL demo)

A small C# Windows Forms application (project name **Glee**) that draws the
schema of a SQL Server database as an interactive graph, using Microsoft's
**MSAGL** (Microsoft Automatic Graph Layout) library. It was written in 2017
against the **AdventureWorks2014** sample database.

## What it does

- Connects to a SQL Server instance and reads key/constraint metadata from
  `INFORMATION_SCHEMA.KEY_COLUMN_USAGE` (see `SQL_Server/SQL_Manager.cs`).
- Builds one node per table (labelled with the table's key columns), grouped
  into a sub-graph per table, all nested under an "Adventure 2014" root
  sub-graph.
- Adds a directed edge from a table's foreign-key column to every table whose
  primary key matches that column, so the drawing approximates the FK → PK
  relationships of the database.
- Renders the result in MSAGL's `GViewer` control: automatic layout,
  mouse-wheel zoom, node dragging, and hover highlighting/tooltips for nodes
  and edges (`Form1.cs`).

Key classification is heuristic: a column is treated as a primary key when its
constraint name starts with `PK`, otherwise as a foreign key.

## Requirements

- Windows with .NET Framework 4.5.2 (the project is a classic
  WinForms/.NET Framework project — it does not build or run on
  macOS/Linux or on modern .NET without porting).
- Visual Studio (or MSBuild) to build `Glee.csproj`.
- A reachable SQL Server instance with the
  [AdventureWorks2014](https://learn.microsoft.com/sql/samples/adventureworks-install-configure)
  sample database (any database works, but the FK → PK matching assumes the
  `PK`/`FK` constraint-naming convention).

## Running

1. Edit `CONNECTION_STRING` in `SQL_Server/SQL_Manager.cs` so that
   `Data Source` points to your SQL Server instance.
2. Open `Glee.csproj` in Visual Studio, build, and run. The graph is created
   on startup.

## Project layout

| Path | Description |
| --- | --- |
| `Form1.cs`, `Form1.Designer.cs` | Main form; graph construction and viewer interaction. Based on MSAGL's `WindowsApplicationSample`. |
| `SQL_Server/` | Database access: `SQL_Manager` (connection + metadata query), `ObjectTable` / `CSTable` (table/key models). |
| `lib/` | Microsoft MSAGL 3.0 assemblies referenced by the project (`Microsoft.Msagl`, `Microsoft.Msagl.Drawing`, `Microsoft.Msagl.GraphViewerGdi`). |

## Attribution

Graph layout and rendering are provided by
[Microsoft Automatic Graph Layout (MSAGL)](https://github.com/microsoft/automatic-graph-layout),
Copyright (c) Microsoft Corporation, released under the
[MIT License](https://github.com/microsoft/automatic-graph-layout/blob/master/LICENSE).
The binaries in `lib/` are redistributed from that project under that license —
the full license text is included in [`lib/MSAGL-LICENSE.txt`](lib/MSAGL-LICENSE.txt).
The UI code in `Form1.cs` started from MSAGL's `WindowsApplicationSample` demo. This
repository is not affiliated with or endorsed by Microsoft.
