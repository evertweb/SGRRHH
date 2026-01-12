# SGRRHH.Local - Visión General del Proyecto

## Propósito
Sistema de Gestión de Recursos Humanos Local para empresa forestal colombiana (~20 empleados).

## Stack Tecnológico
- **Framework:** .NET 8.0, Blazor Server
- **Base de datos:** SQLite con Dapper (ORM)
- **Arquitectura:** Clean Architecture (Domain → Infrastructure → Server)
- **UI:** Estilo "hospitalario" con Courier New

## Estructura del Proyecto
```
SGRRHH.Local/
├── SGRRHH.Local.Domain/      # Entidades, Enums, DTOs, Interfaces
├── SGRRHH.Local.Infrastructure/  # Repositorios, Servicios, Data
├── SGRRHH.Local.Server/      # Blazor Server, Components, Pages
├── SGRRHH.Local.Shared/      # Código compartido e interfaces
└── scripts/                  # Migraciones SQL, PowerShell
```

## Sistema de Usuarios Actual (PENDIENTE REDISEÑO)
- **Roles definidos:** Administrador (1), Aprobador (2, deshabilitado), Operador (3, deshabilitado)
- **Estado actual:** TODOS los usuarios funcionan como Administrador
- **Lógica de roles:** Deshabilitada temporalmente

## Estados de Empleado
- PendienteAprobacion = 0
- Activo = 1
- EnVacaciones = 2
- EnLicencia = 3
- Suspendido = 4
- Retirado = 5
- Rechazado = 6

## Entidades con Flujo de Aprobación
- Empleado: CreadoPorId, AprobadoPorId, FechaSolicitud, FechaAprobacion, MotivoRechazo
- Permiso: SolicitadoPorId, AprobadoPorId, FechaAprobacion, MotivoRechazo
- Vacación: SolicitadoPorId, AprobadoPorId, FechaAprobacion, MotivoRechazo
- Nómina: AprobadoPorId, FechaAprobacion
- CompensacionHoras: AprobadoPorId, FechaAprobacion
- RegistroDiario: CreadoPorId

## Comandos de Desarrollo
```powershell
# Build
cd SGRRHH.Local && dotnet build

# Desarrollo con Hot Reload
dotnet watch --project SGRRHH.Local.Server

# Consultar BD
sqlite3 "C:\SGRRHH\Data\sgrrhh.db" ".tables"
```

## Idioma
Todo en español (código, comentarios, UI, documentación).
