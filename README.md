# PV260 - Software Quality - Team 4 (Popocatepetl)

This repository contains a team project for the Notino C# seminar (Spring 2026) - A console application for managing and comparing reports. Built in C# with a clean terminal interface.

**Members:**
- [Robin Chmelík](https://is.muni.cz/auth/osoba/514315)
- [Matej Kučera](https://is.muni.cz/auth/osoba/514093)
- [Adam Haluška](https://is.muni.cz/auth/osoba/536303)
- [Radoslav Baník](https://is.muni.cz/auth/osoba/536584)
- [Matúš Fedorko](https://is.muni.cz/auth/osoba/matus.fedorko)

**Tutors:**
- [Erika Bača](https://is.muni.cz/auth/osoba/540487)
- [Erik Matuška](https://is.muni.cz/auth/osoba/493344)


---

## What does it do?

- Users log in with their email and choose a role.
- Each role can do different things.
- All reports are saved in a local database.
- The app can compare two reports and show what changed (a "diff").
- Actions are logged so you know who did what and when.

## Roles

| Role | What they can do |
|---|---|
| **Admin** | Download new reports, export diff as PDF. Needs a password. |
| **Power user** | Change UI colors, send reports by email to a list of addresses. |
| **User** | View the diff. Nothing else. |

## Tech stack

| What | Tool |
|---|---|
| Language | C# (.NET 10) |
| Terminal UI | Spectre.Console |
| CLI parsing | System.CommandLine |
| Database | SQLite + Entity Framework Core |
| Email sending | SendGrid |
| PDF export | QuestPDF |
| CSV parsing | CsvHelper |
| Tests | xUnit + FluentAssertions + Moq |

## How to run

```bash
# Clone the repo
git clone <repo-url>
cd Popocatepetl

# Build
dotnet build

# Run
dotnet run --project src/Popocatepetl.CLI

# Run all tests
dotnet test
```

## How to publish (single .exe file)

```bash
dotnet publish src/Popocatepetl.CLI -r win-x64 --self-contained -o ./publish
```

The `.exe` file will be in the `./publish` folder. You can run it on any Windows machine without installing .NET.

## License

This project was created as part of a Software Quality course assignment.