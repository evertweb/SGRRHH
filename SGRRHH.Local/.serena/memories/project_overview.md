# SGRRHH.Local - Project Overview

## Purpose
Local HR management application (Sistema de Gestión de Recursos Humanos) with SQLite database and .NET 8 backend.

## Tech Stack
- **Backend**: .NET 8, Blazor Server
- **Database**: SQLite
- **UI Framework**: Custom Hospital/ForestechOil design system
- **Architecture**: Clean Architecture (Domain, Infrastructure, Server, Shared layers)

## Project Structure
- **SGRRHH.Local.Domain**: Entities, DTOs, Interfaces, Enums
- **SGRRHH.Local.Infrastructure**: Data access, repositories, services
- **SGRRHH.Local.Server**: Blazor Server app (UI components, pages)
- **SGRRHH.Local.Shared**: Shared DTOs and utilities
- **SGRRHH.Local.Tests**: Unit and integration tests

## Key Features
- Employee management (Empleados)
- Contracts (Contratos)
- Permissions (Permisos)
- Vacations (Vacaciones)
- Daily control (ControlDiario)
- Documents management (Documentos)
- Audit logs (Auditoría)
- Catalogs (Departamentos, Cargos, Proyectos, Actividades, TiposPermiso)
- User management (Usuarios)
- Configuration (Configuración)
- Reports (Reportes)
