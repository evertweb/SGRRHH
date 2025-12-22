# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**SGRRHH** (Sistema de Gestión de Recursos Humanos) is a Windows desktop application for managing human resources in a small company (~20 employees) in Colombia. It's a complete HR management system with employee records, daily activity tracking, permissions/licenses, vacations, contracts, reports, and PDF document generation.

**Tech Stack:**
- C# .NET 8
- WPF (Windows Presentation Foundation) for UI
- SQLite database (local, file-based)
- Entity Framework Core for ORM
- Clean Architecture with MVVM pattern

**Deployment:**
- Non-self-contained Windows executable (~12 MB ZIP)
- Requires .NET 8 Runtime installed on user machines
- Auto-updates via GitHub Releases
- Firebase backend (Firestore + Storage + Auth)
- ~20 employees, 3 concurrent users

## Build Commands

### Build & Run
```bash
# Build the solution (from src/ directory)
cd src
dotnet build SGRRHH.sln --configuration Debug
dotnet build SGRRHH.sln --configuration Release

# Run the application
cd src/SGRRHH.WPF
dotnet run

# Run tests
cd src
dotnet test SGRRHH.Tests/SGRRHH.Tests.csproj
```

### Publishing & Updates

The system uses GitHub Releases for automatic updates:

```bash
# Method 1: Automatic via GitHub Actions (recommended)
# 1. Update version in csproj
# 2. Commit and push
# 3. Create and push tag
git tag v1.1.5
git push origin v1.1.5
# GitHub Actions builds and publishes automatically

# Method 2: Manual via VS Code tasks
# Task: "1. Build + Actualizar Local" - Compile and copy to C:\SGRRHH
# Task: "2b. Publicar TODO" - Upload to GitHub + update local
```

**Update Components:**
- `GithubUpdateService.cs` - Checks GitHub API for new releases
- `SGRRHH.Updater/` - External process that applies updates
- `UpdateDialog.xaml` - UI for update notification
- `.github/workflows/release.yml` - Automated build and release

**Note:** ZIP size is ~12 MB (non-self-contained). Requires .NET 8 Runtime.

## Architecture

### Project Structure
```
src/
├── SGRRHH.Core/           # Domain layer (entities, interfaces, business logic)
│   ├── Entities/          # Database entities (Empleado, Permiso, etc.)
│   ├── Enums/            # Enumerations (RolUsuario, EstadoPermiso, etc.)
│   ├── Interfaces/       # Repository & service contracts
│   ├── Models/           # DTOs and view models
│   └── Common/           # ServiceResult pattern
│
├── SGRRHH.Infrastructure/ # Data layer (EF Core, repositories, services)
│   ├── Data/
│   │   ├── AppDbContext.cs          # EF Core context
│   │   └── DatabaseInitializer.cs   # Seed data
│   ├── Repositories/     # Repository implementations
│   └── Services/         # Business logic services
│
├── SGRRHH.WPF/           # Presentation layer (WPF UI)
│   ├── Views/            # XAML views (windows and user controls)
│   ├── ViewModels/       # MVVM ViewModels (CommunityToolkit.Mvvm)
│   ├── Converters/       # XAML value converters
│   └── App.xaml.cs       # DI container configuration
│
└── SGRRHH.Tests/         # Unit tests (xUnit + Moq)
```

### Key Architectural Patterns

**1. Clean Architecture**
- Core layer has no dependencies on Infrastructure or WPF
- Infrastructure depends on Core
- WPF depends on both Core and Infrastructure
- Dependency inversion via interfaces in Core

**2. MVVM Pattern**
- Uses CommunityToolkit.Mvvm for ObservableObject, RelayCommand
- All ViewModels inherit from ObservableObject
- Commands use [RelayCommand] attributes
- Navigation via WeakReferenceMessenger

**3. Service Result Pattern**
All service methods return `ServiceResult<T>` (defined in `Core/Common/ServiceResult.cs`):
```csharp
// Success
return ServiceResult<Empleado>.Ok(empleado, "Empleado creado");

// Failure
return ServiceResult<Empleado>.Fail("Error al crear empleado");
```

**4. Repository Pattern**
Generic base repository (`IRepository<T>`) with specialized repositories for each entity. All repositories include navigation properties via `.Include()` for efficient loading.

**5. Dependency Injection**
All services, repositories, and ViewModels registered in `App.xaml.cs` using Microsoft.Extensions.DependencyInjection. Services are scoped, ViewModels are transient.

## Database

**Technology:** SQLite with Entity Framework Core

**Location:** `data/sgrrhh.db` (created automatically on first run)

**Concurrency:** WAL mode enabled for multi-PC access via network share (configured in `AppSettings.GetConnectionString()`)

### Key Entities
- **Usuario** - System users (admin, operador, aprobador roles)
- **Empleado** - Employee records with personal info, photo, dates
- **Departamento/Cargo** - Departments and job positions
- **RegistroDiario** - Daily activity records per employee
- **Permiso** - Permission/license requests with approval workflow
- **TipoPermiso** - 13 Colombian labor law permission types
- **Vacacion** - Vacation tracking (15 days/year per Colombian law)
- **Contrato** - Employment contracts (Indefinido, Fijo, Aprendizaje, Obra)
- **ConfiguracionSistema** - System configuration key-value pairs
- **AuditLog** - Audit trail for important actions

### Seed Data
Los usuarios del sistema se gestionan directamente en Firebase:
- Los usuarios deben crearse desde Firebase Console o el panel de administración
- Las credenciales por defecto fueron **eliminadas** del código fuente por seguridad
- Los departamentos, cargos y tipos de permiso se gestionan desde la app

**NOTA:** Las credenciales antiguas (admin123, secretaria123, ingeniera123) fueron removidas del código. Si aún existen en Firebase, cambiar las contraseñas.

## Navigation & Messaging

Navigation between views uses `WeakReferenceMessenger` from CommunityToolkit.Mvvm:

```csharp
// Send navigation message
WeakReferenceMessenger.Default.Send(new NavigateToViewMessage("Empleados"));

// Receive in MainViewModel
WeakReferenceMessenger.Default.Register<NavigateToViewMessage>(this, (r, m) => {
    CurrentView = CreateView(m.ViewName);
});
```

`NavigateToViewMessage` is defined in `WPF/Messages/`.

## User Roles & Permissions

Three roles (defined in `Core/Enums/RolUsuario.cs`):

| Role | Spanish | Permissions |
|------|---------|-------------|
| Administrador | Admin | Full access to everything |
| Operador | Secretaria | Create/edit employees, request permissions, daily records |
| Aprobador | Ingeniera | Approve/reject permissions, view-only for most modules |

**Permission Flow Example:**
1. Operador creates permission request → Estado: Pendiente
2. Aprobador reviews in "Bandeja de Aprobación" → Approves/Rejects
3. System generates PDF "Acta de Permiso" on approval

## Document Generation

Uses **QuestPDF** (v2024.3.3) for PDF generation in `Infrastructure/Services/DocumentService.cs`:

**Document Types:**
1. **Acta de Permiso** - Permission approval certificate
2. **Certificado Laboral** - Employment certificate
3. **Constancia de Trabajo** - Work verification letter

**Logo:** Reads from `data/config/logo.png` if exists, or uses placeholder.

**Preview:** WPF views use `WebView2` control for PDF preview before saving/printing.

## Colombian Labor Regulations

The system implements Colombian labor law requirements:

**Vacaciones (Vacations):**
- 15 days per year (cumulative)
- Calculated pro-rata based on hire date
- Service: `VacacionService.cs` (lines ~70-100)

**Permission Types:**
13 types configured in `DatabaseInitializer.cs`:
- Calamidad doméstica, Cita médica, Luto, Lactancia, Paternidad/Maternidad, etc.
- Each has default settings: Remunerado (paid), RequiereSoporte (requires documentation)

## Error Handling & Logging

**Global Exception Handler:**
Set up in `App.xaml.cs` (`SetupGlobalExceptionHandling()`):
- Catches unhandled UI exceptions (DispatcherUnhandledException)
- Catches background thread exceptions (AppDomain.UnhandledException)
- Catches async task exceptions (TaskScheduler.UnobservedTaskException)

**Error Logs:**
Written to `data/logs/error_YYYY-MM-DD.log` with timestamp, exception type, message, and stack trace.

**User-Friendly Messages:**
Specific error types (SQLite, IO, UnauthorizedAccess) have friendly Spanish messages for users.

## Important Code Locations

### Main Entry Point
- `SGRRHH.WPF/App.xaml.cs` - DI configuration, DB initialization, login flow

### Authentication
- `Infrastructure/Services/AuthService.cs` - Login with BCrypt password hashing
- `WPF/Views/LoginWindow.xaml` - Login UI

### Main Navigation
- `WPF/MainWindow.xaml` - Shell window with sidebar navigation
- `WPF/ViewModels/MainViewModel.cs` - Handles view switching, role-based menu

### Dashboard
- `WPF/Views/DashboardView.xaml` - Home screen with statistics and alerts
- `WPF/ViewModels/DashboardViewModel.cs` - Loads real-time data (pending permissions, expiring contracts, employee count)

### Employee Management
- `WPF/Views/EmpleadosListView.xaml` - Employee list with search/filter
- `WPF/Views/EmpleadoFormWindow.xaml` - Create/edit employee
- `WPF/Views/EmpleadoDetailWindow.xaml` - Read-only employee details

### Permission Workflow
- `WPF/Views/PermisosListView.xaml` - Permission requests (Operador view)
- `WPF/Views/BandejaAprobacionView.xaml` - Approval queue (Aprobador view)
- `Infrastructure/Services/PermisoService.cs` - Approval logic

### Backup & Configuration
- `WPF/Views/ConfiguracionView.xaml` - System settings, backup/restore, audit log
- `Infrastructure/Services/BackupService.cs` - SQLite backup API usage

## Known Issues & Warnings

**Build Warnings (2 total):**
- `NU1701` en `CredentialManagement 1.0.2` - Paquete restaurado usando .NET Framework en lugar de net8.0
  - No afecta funcionalidad, es un warning de compatibilidad

**Version:** 1.1.12 (production-ready)

## Testing

**Framework:** xUnit with Moq for mocking

**Run Tests:**
```bash
cd src
dotnet test SGRRHH.Tests/SGRRHH.Tests.csproj --verbosity normal
```

**Note:** Tests are currently minimal. The project was primarily manually tested.

## Network Deployment

**Current Setup:** Firebase-based (cloud)

All PCs connect directly to Firebase (Firestore + Storage). No local network sharing required.

**Configuration:** Each PC has its own `C:\SGRRHH\` folder with:
- `appsettings.json` - Firebase project settings, app version
- `firebase-credentials.json` - Service account credentials

**Auto-Updates:** Handled via GitHub Releases. See `docs/ACTUALIZACIONES.md` for details.

## Data Storage

**Firebase Firestore** - All data stored in cloud:
- Empleados, Permisos, Contratos, etc.
- Real-time synchronization across PCs

**Firebase Storage** - Files:
- Employee photos
- Document attachments

**Local folder (C:\SGRRHH\):**
```
C:\SGRRHH\
├── SGRRHH.exe           # Main executable
├── appsettings.json     # Configuration
├── firebase-credentials.json
├── SGRRHH.Updater.exe   # Update applier
├── updater_log.txt      # Update process log
└── data/
    └── logs/            # Error logs
```

## Development Tips

**When Adding New Entities:**
1. Create entity in `Core/Entities/` inheriting from `EntidadBase`
2. Add DbSet to `AppDbContext.cs`
3. Configure in `OnModelCreating()` if needed
4. Create repository interface in `Core/Interfaces/`
5. Implement repository in `Infrastructure/Repositories/`
6. Create service interface/implementation
7. Register in `App.xaml.cs` DI container

**When Adding New Views:**
1. Create XAML in `WPF/Views/`
2. Create ViewModel in `WPF/ViewModels/` (inherit ObservableObject)
3. Register ViewModel in `App.xaml.cs`
4. Add navigation case in `MainViewModel.CreateView()`
5. Add menu item in `MainWindow.xaml` sidebar

**MVVM Best Practices:**
- Use `[ObservableProperty]` for bindable properties (auto-generates OnPropertyChanged)
- Use `[RelayCommand]` for commands (auto-generates ICommand properties)
- Keep ViewModels testable (inject services via constructor)
- Never reference WPF types in ViewModels (except ICommand, ObservableCollection)

## Documentation Reference

For full details, see these files in `docs/`:
- `00_CONTEXTO_IA.md` - Quick context for AI assistants
- `03_REQUISITOS_DEFINITIVOS.md` - Complete requirements
- `04_ARQUITECTURA_TECNICA.md` - Detailed technical architecture
- `05_ROADMAP.md` - Development phases breakdown
- `06_ESTADO_ACTUAL.md` - Current project status
