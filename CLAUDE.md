# CLAUDE.md - Guidelines for Claude & AI Coding Assistants

## Workspace Information
- **Project**: DemoPulse (CS2 Match & Voice Manager)
- **Framework**: .NET 8.0 WPF + WebView2
- **Primary Language**: C# 12 & Modern JavaScript (ES6+)

## Commands
- **Build**: `dotnet build`
- **Test Run**: `dotnet run`
- **Release Package**: `dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o .\dist`

## Code Style & Architecture
- Maintain crisp separation: C# handles native Windows APIs (Steam launcher, File system, CS2 CFG generation); JS inside `ui/index.html` handles UI rendering and interactive data visualization.
- Always check IPC message prefixes (`OPEN_DEMO:`, `LAUNCH_CS2`, etc.) when adding C# <-> Web message handlers.
