# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**SGRRHH** (Sistema de Gestión de Recursos Humanos) is a Blazor WebAssembly application for managing human resources in a small company (~20 employees) in Colombia. It's a complete HR management system with employee records, daily activity tracking, permissions/licenses, vacations, contracts, reports, and document generation.

**Tech Stack:**
- C# .NET 8 (Blazor WebAssembly)
- Vanilla CSS for UI (Windows Classic/Legacy theme)
- Firebase Backend (Auth, Firestore, Storage)
- Clean Architecture

**Deployment:**
- Hosted on Firebase Hosting
- Real-time synchronization via Firestore
- Document storage via Firebase Storage

## Build Commands

### Build & Run
```bash
# Build the solution (from src/ directory)
cd src
dotnet build SGRRHH.sln

# Run the Web application
cd src/SGRRHH.Web
dotnet run

# Run tests
cd src
dotnet test SGRRHH.Tests/SGRRHH.Tests.csproj
```

## Architecture

### Project Structure
```
src/
├── SGRRHH.Core/           # Domain layer (entities, interfaces, business logic)
│   ├── Entities/          # Database entities (Empleado, Permiso, etc.)
│   ├── Enums/            # Enumerations (RolUsuario, EstadoPermiso, etc.)
│   ├── Interfaces/       # Repository & service contracts
│   └── Common/           # ServiceResult pattern
│
├── SGRRHH.Infrastructure/ # Data layer (Firebase Interop, repositories)
│   ├── Repositories/     # Web Repository implementations
│   └── Services/         # Business logic services
│
├── SGRRHH.Web/            # Blazor WebAssembly Frontend
│   ├── Pages/            # Razor pages (ControlDiario, Empleados, etc.)
│   ├── Components/       # Shared UI components and Forms
│   └── wwwroot/          # Static assets and CSS
│
└── SGRRHH.Tests/         # Unit tests (xUnit + Moq)
```

### Key Architectural Patterns

**1. Clean Architecture**
- Core layer has no dependencies on Infrastructure or Web
- Infrastructure depends on Core
- Web depends on both Core and Infrastructure
- Dependency inversion via interfaces in Core

**2. Component Pattern (Blazor)**
- Reusable UI components in `SGRRHH.Web/Components`
- Separation of Form logic from Page layout

**3. Service Result Pattern**
All service methods return `ServiceResult<T>` (defined in `Core/Common/ServiceResult.cs`):
```csharp
// Success
return ServiceResult<Empleado>.Ok(empleado, "Empleado creado");

// Failure
return ServiceResult<Empleado>.Fail("Error al crear empleado");
```

## Database & Backend

**Technology:** Firebase Firestore (NoSQL Cloud Database)

### Key Collections
- **usuarios** - System users (admin, operador, aprobador roles)
- **empleados** - Employee records
- **registros_diarios** - Daily activity records
- **permisos** - Permission requests
- **vacaciones** - Vacation tracking
- **contratos** - Employment contracts

## User Roles & Permissions

Three roles (defined in `Core/Enums/RolUsuario.cs`):

| Role | Permissions |
|------|-------------|
| Administrador | Full access to everything |
| Operador | Create/edit records, request permissions |
| Aprobador | Approve/reject permissions |
