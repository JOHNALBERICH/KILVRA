# KILVRA

Tutorial to run the project after cloning.

## Prerequisites
- .NET SDK 9
- SQL Server (LocalDB or full SQL Server)
- `dotnet-ef` tool

## Setup
1. Clone the repository.
2. Open a terminal at the repository root.
3. Ensure the data protection keys folder exists (as configured in `Program.cs`):
   - Create `C:\DataProtectionKeys`
4. Update the connection string in `appsettings.json` (or `appsettings.Development.json`) to point to your SQL Server instance.

## Restore and migrate
```powershell
# restore packages
dotnet restore

# install EF tool (if not installed)
dotnet tool install --global dotnet-ef

# apply migrations
dotnet ef database update
```

## Run the app
```powershell
dotnet run
```

The site should be available at the URL shown in the terminal (typically `https://localhost:xxxx`).
