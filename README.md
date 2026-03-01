# Hive — Family Calendar

A modern, cross-platform family calendar and task management app built with .NET 10 and Uno Platform. Inspired by Skylight Calendar, Hive helps families stay organized with shared calendars, chore tracking, a star-based reward system, and collaborative lists.

## Features

- **Shared Calendar** — Day, Week, Month, and Schedule views with color-coded events per family member. Supports recurring events (daily, weekly, monthly, yearly) and reminders.
- **Tasks & Chores** — Assign chores and daily routines to kids. Track completion and earn stars. Celebration animations when all tasks are done.
- **Rewards** — Star-based incentive system. Kids redeem earned stars for family rewards like ice cream trips or extra screen time.
- **Lists** — Shared grocery and to-do lists with real-time completion tracking.
- **Profiles** — Individual family member profiles with custom colors and emoji avatars.
- **Calendar Sync** — Import events from Google Calendar, iCloud, Outlook, and ICS feeds.
- **Notifications** — Local reminders for upcoming events and tasks.
- **Dark Mode** — Full dark theme with automatic or manual switching.
- **Cross-Platform** — Runs on Windows, Android, iOS, macOS, and Web (WASM).

## Architecture

```
Hive.sln
├── src/
│   ├── Hive.Core          # Domain models, service interfaces, business logic
│   ├── Hive.Data          # EF Core + SQLite, repository implementations
│   └── Hive               # Uno Platform UI (XAML views, ViewModels, controls)
└── tests/
    └── Hive.Core.Tests    # xUnit tests for recurrence, date logic
```

**Stack:** .NET 10 · Uno Platform 6.5 · CommunityToolkit.Mvvm · EF Core + SQLite · xUnit

**Pattern:** MVVM with service layer. ViewModels use `[ObservableProperty]` and `[RelayCommand]` from CommunityToolkit.Mvvm. Data access goes through service interfaces (`ICalendarService`, `ITaskService`, etc.) backed by EF Core repositories.

**Design System:** "Costa" — Mediterranean-inspired palette with Playfair Display (serif headings) and Source Sans 3 (body text). Colors: Sea blue, Terra red, Olive green, Ochre gold, Purple accent on a warm Stone surface.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Uno Platform templates](https://platform.uno/docs/articles/get-started.html) (`dotnet new install Uno.Templates`)
- For Windows: Visual Studio 2022 with UWP/WinUI workload
- For mobile: Android SDK / Xcode (iOS/macOS)

## Getting Started

```bash
# Clone
git clone https://github.com/mtmattei/Hive.git
cd Hive

# Restore & build
dotnet restore
dotnet build

# Run on Windows
dotnet run --project src/Hive -f net10.0-windows10.0.26100

# Run on WASM (browser)
dotnet run --project src/Hive -f net10.0-browserwasm

# Run tests
dotnet test
```

The app auto-creates a local SQLite database (`hive.db`) and seeds it with a demo family on first launch — 5 profiles, sample events, tasks, rewards, and lists ready to explore.

## Project Phases

| Phase | Status | Description |
|-------|--------|-------------|
| 1 — Scaffold | Done | Project structure, models, services, views, controls, styles |
| 2 — CRUD | Done | Seed data, creation dialogs, full CRUD wiring |
| 3 — Polish | Done | Edit/delete flows, detail views, notifications, calendar sync, dark mode, accessibility |

## License

Private — All rights reserved.
