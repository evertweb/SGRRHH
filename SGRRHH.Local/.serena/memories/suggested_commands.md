# Suggested Commands - SGRRHH.Local

## Development
```powershell
# Build the solution
dotnet build

# Run the application (development)
dotnet run --project SGRRHH.Local.Server

# Run with hot reload
dotnet watch run --project SGRRHH.Local.Server

# Access URL: https://localhost:5001
```

## Testing
```powershell
# Run all tests
dotnet test

# Run specific test project
dotnet test SGRRHH.Local.Tests/SGRRHH.Local.Tests.csproj

# Run tests with verbosity
dotnet test -v detailed
```

## Publishing
```powershell
# Publish Release build
dotnet publish SGRRHH.Local.Server -c Release -o publish

# Install locally (requires admin)
.\scripts\Install.ps1
```

## Windows Utilities
- **List directories**: `Get-ChildItem` or `ls`
- **Search files**: `Get-ChildItem -Recurse -Filter *.razor`
- **Find in files**: `Select-String -Path *.cs -Pattern "SearchTerm"`
- **Git**: Standard git commands work in PowerShell
