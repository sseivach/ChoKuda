# ChoKuda

ChoKuda is a Windows desktop app for a personal bank of travel places on a map.

The project is intentionally planned before implementation. Product decisions, UX rules, file model, stack, roadmap, quality gate, and milestone status live in [docs/chokuda-planning.md](docs/chokuda-planning.md).

## Current Status

Implemented:

- Milestone 1: App Shell.
- WPF + BlazorWebView shell.
- Left panel, map placeholder, right panel preview, bottom status bar.
- Testable shell state in `ChoKuda.Core`.
- Unit tests for shell state.

Not implemented yet:

- File library.
- Real Leaflet/Stadia map.
- Points, collections, tags.
- Photos/files import.

## Stack

- .NET SDK 10.0.300
- WPF + BlazorWebView/WebView2
- Blazor + CSS
- Bootstrap Icons planned, without Bootstrap CSS/JS
- `System.Text.Json`
- Leaflet + Stadia raster tiles planned
- Leaflet.markercluster planned

## Repository Layout

```text
ChoKuda/
  docs/
    chokuda-planning.md
  src/
    ChoKuda.App/
    ChoKuda.Core/
  tests/
    ChoKuda.Core.Tests/
```

## Commands

Build:

```powershell
dotnet build ChoKuda.slnx
```

Run tests:

```powershell
dotnet test ChoKuda.slnx
```

Run tests with coverage:

```powershell
dotnet test ChoKuda.slnx --collect:"XPlat Code Coverage"
```

Run the app:

```powershell
dotnet run --project src\ChoKuda.App\ChoKuda.App.csproj
```

## Quality Rule

No new implementation milestone starts without an approved plan. Own testable logic must be covered by tests; UI should stay thin and avoid hidden business rules.

