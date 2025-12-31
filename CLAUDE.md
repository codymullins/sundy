# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Sundy is a cross-platform calendar application for managing multiple calendars (Gmail, Outlook, Apple Calendar) with ADHD-friendly features. Built with .NET 10.0, it uses Avalonia for desktop, Blazor/Tauri for web/hybrid, and Uno Platform for mobile.

**Note on UI implementations**: The three UI implementations (Avalonia, Tauri/Blazor, Uno) are experimental. We're evaluating which approach works best for different platforms before consolidating.

## Build Commands

```bash
# Restore and build
dotnet restore
dotnet build

# Run desktop app (Avalonia)
dotnet run --project ui/avalonia/Sundy.Desktop/Sundy.Desktop.csproj

# Run Tauri hybrid app (requires Rust toolchain)
cd ui/tauri/Sundy && ./dev.sh

# Run tests
dotnet test

# Run specific test project
dotnet test tests/Sundy.Core.Tests/Sundy.Test.csproj
```

## Architecture

### CQRS + Mediator Pattern

All business logic flows through the Mediator pattern:
- **Commands**: State-modifying operations (e.g., `CreateEventCommand` → `CreateEventCommandHandler`)
- **Queries**: Read operations (e.g., `GetEventsInRangeQuery` → `GetEventsInRangeQueryHandler`)
- Handlers are colocated with their commands/queries in `core/Sundy.Core/`

### Key Layers

```
UI (Avalonia/Blazor/Uno)
    ↓
ViewModels (CommunityToolkit.Mvvm)
    ↓
Mediator (Commands/Queries)
    ↓
Domain Logic & Handlers
    ↓
Data Stores (Dapper → SQLite)
```

### Project Structure

- `core/Sundy.Core/` - Business logic, CQRS handlers, data stores
  - `Calendars/` - Calendar and event domain
  - `Calendars/Outlook/` - Microsoft Graph integration
  - `Sync/` - Offline-first sync infrastructure
- `ui/avalonia/` - Primary desktop UI (Avalonia)
- `ui/tauri/` - Web/hybrid app (Blazor + Rust)
- `ui/uno/` - Mobile (Uno Platform)
- `tests/` - xUnit tests with Moq and AutoFixture

### Data Store Pattern

Each domain has an interface (`IEventStore`, `ICalendarStore`) with:
- `Dapper*Store` - SQLite implementation for production
- `InMemory*Store` - In-memory implementation for testing

## Coding Conventions

- **Namespaces**: File-scoped (`namespace X;`)
- **Primary constructors**: Preferred for DI
- **Nullable reference types**: Enabled globally
- **Interfaces**: Prefix with `I`
- **Braces**: Always use braces around `if`, `foreach`, `for`, `while`, etc.
- **Avoid `this.`**: Unless necessary for disambiguation

## Key Technologies

- .NET 10.0 (preview language features)
- Avalonia 11.3 with Fluent theme
- Mediator source generator for CQRS
- Dapper for data access
- NodaTime for date/time handling
- Microsoft Graph API for Outlook integration
