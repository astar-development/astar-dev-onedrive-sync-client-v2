# AstarOneDrive Sync Client

![AstarOneDrive](src/AstarOneDrive.UI/Assets/astar.png)

[![Build](https://img.shields.io/badge/build-passing-brightgreen)](https://github.com/astar-development/astar-dev-onedrive-sync-client-v2/actions)
[![SonarCloud](https://img.shields.io/badge/sonarcloud-quality%20gate-brightgreen)](https://sonarcloud.io)
[![CodeQL](https://img.shields.io/badge/codeql-enabled-blue)](https://github.com/astar-development/astar-dev-onedrive-sync-client-v2/security/code-scanning)

A cross-platform OneDrive sync client built with AvaloniaUI and a strict Onion Architecture. The UI focuses on multiple layouts, multiple themes, and clear sync status while keeping business logic isolated from presentation.

> [!NOTE]
> This repository targets .NET 10 preview for C# 14 features. Ensure you have the latest .NET 10 SDK installed.

> [!WARNING]
> OneDrive integration is still evolving. Some flows may be stubbed while the UI and architecture are finalized.

## Features

- Cross-platform desktop UI with AvaloniaUI and ReactiveUI
- Onion Architecture: UI -> Application -> Domain, plus Infrastructure
- Multiple layouts (Explorer, Dashboard, Terminal)
- Theme system with multiple presets (Light, Dark, Auto, Colorful, Professional, Hacker, High Contrast)
- Localization-ready resource dictionaries
- Account management, folder tree selection, and sync status views
- Functional-style patterns via AStar.Dev libraries (Result, Option, StrongId)

## Getting Started

### Prerequisites

- .NET SDK 10.0 

### Build

```bash
dotnet restore
dotnet build
```

### Run the UI

```bash
dotnet run --project src/AstarOneDrive.UI
```

### Run tests

```bash
dotnet test
```

## Architecture

The solution follows Onion Architecture, keeping dependencies directional and explicit.

- Domain: core entities and interfaces
- Application: business logic and services
- Infrastructure: external integrations and repositories
- UI: AvaloniaUI presentation layer only

For details, see [docs/implementation-overview.md](docs/implementation-overview.md).

## Project Structure

```
src/
	AstarOneDrive.Domain/
	AstarOneDrive.Application/
	AstarOneDrive.Infrastructure/
	AstarOneDrive.UI/
tests/
	AstarOneDrive.Domain.Tests/
	AstarOneDrive.Application.Tests/
	AstarOneDrive.UI.Tests/
```

## Documentation

- Architecture guide: [docs/implementation-overview.md](docs/implementation-overview.md)
- Implementation plan: [docs/implementation-plan.md](docs/implementation-plan.md)
- Copilot quick reference: [docs/copilot/quick-reference.md](docs/copilot/quick-reference.md)
