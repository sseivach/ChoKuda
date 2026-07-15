# ChoKuda

ChoKuda is a Windows desktop app for a personal bank of travel places on a map.

The project is intentionally planned before implementation. Product decisions, UX rules, file model, stack, roadmap, quality gate, and milestone status live in [docs/chokuda-planning.md](docs/chokuda-planning.md).

Manual acceptance is tracked in [docs/MANUAL_TESTING.md](docs/MANUAL_TESTING.md). Accepted MVP constraints are tracked in [docs/known-limitations.md](docs/known-limitations.md).

## Current Status

Implemented:

- Milestone 1: App Shell.
- Milestone 2: File Library.
- Milestone 3: Domain Rules.
- Milestone 4: Map Integration.
- Milestone 5: Point CRUD.
- Milestone 6: Collections And Tags.
- Milestone 7: Search.
- Milestone 8: Photos And Files.
- Milestone 9: Polish And Acceptance.
- WPF + BlazorWebView shell.
- Left panel, Leaflet map, right panel preview, bottom status bar.
- Testable shell state in `ChoKuda.Core`.
- File library structure/services for app settings, library settings, point JSON, collection JSON, photos, and files.
- Domain rules for point validation, tag normalization, primary collection behavior, default pin, and attachment file naming.
- Leaflet + Stadia raster map with marker clustering, temporary point selection, and Stadia API key setup.
- Point creation, viewing, editing, deletion, unsaved-change confirmation, and point JSON persistence.
- Collection CRUD, Bootstrap Icons collection styles with the local full `1.13.1` catalog, point collection membership, tag index, and collection/tag filters.
- Search by title, address, description, and tags with left-panel results and map centering.
- Photo/file attachment import through picker or drag-and-drop, classification, preview with basic zoom/pan, system opening, and deletion.
- Local library data lives in one user-selected `ChoKudaLibrary` folder.
- Manual backup is done by closing ChoKuda and copying the whole `ChoKudaLibrary` folder.
- Unit tests for shell state, file library, domain rules, map point projection, point CRUD service, collection service, filtering rules, search service, and attachment services.
- App-level VM tests for search, filters, editors, attachments, collections, and UI operation state.
- Final automatic acceptance checks: build, tests, coverage, and launch smoke.

Not implemented yet:

- Full manual UI acceptance pass on a real Windows desktop session.

## Stack

- .NET SDK 10.0.300
- WPF + BlazorWebView/WebView2
- Blazor + CSS
- Bootstrap Icons `1.13.1` for collection/pin icons, without Bootstrap UI CSS/JS
- `System.Text.Json`
- Leaflet + Stadia raster tiles
- Leaflet.markercluster

## Repository Layout

```text
ChoKuda/
  docs/
    chokuda-planning.md
    MANUAL_TESTING.md
    known-limitations.md
  src/
    ChoKuda.App/
    ChoKuda.Core/
  tests/
    ChoKuda.App.Tests/
    ChoKuda.Core.Tests/
```

## Data And Backup

ChoKuda keeps the valuable user library in one selected folder, usually:

```text
%USERPROFILE%\Documents\ChoKudaLibrary
```

That folder contains:

```text
settings.json
points/
collections/
photos/
files/
```

Application settings such as the selected library path and Stadia API key are stored separately in:

```text
%APPDATA%\ChoKuda\appsettings.json
```

Backup is intentionally manual in MVP: close ChoKuda and copy the whole `ChoKudaLibrary` folder. To restore, replace the folder with the copy or choose the copied folder with `Choose folder`. There is no automatic backup service or migration runner yet.

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
