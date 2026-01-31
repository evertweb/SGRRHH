---
trigger: model_decision
description: Apply when creating projects, classes, or configuring DI. Enforces .NET 8 + Blazor Server + Dapper stack with Clean Architecture layers.
---

# Arquitectura del Proyecto - SGRRHH

## Stack Tecnológico OBLIGATORIO

| Tecnología | Versión | Uso |
|------------|---------|-----|
| **.NET** | 8.0 | Framework base |
| **Blazor Server** | 8.0 | UI Framework |
| **Dapper** | 2.1+ | ORM (Micro-ORM) |
| **SQLite** | 3.x | Base de datos |
| **C#** | 12.0 | Lenguaje |

> [!CAUTION]
> **PROHIBIDO:**
> - ❌ Entity Framework Core
> - ❌ Blazor WebAssembly (solo Server)
> - ❌ SQL Server, PostgreSQL, MySQL (solo SQLite)
> - ❌ Repository pattern con EF Core

## Clean Architecture - Capas

```
SGRRHH.Local/
├── SGRRHH.Local.Domain/        # Capa de Dominio
├── SGRRHH.Local.Shared/        # Capa Compartida
├── SGRRHH.Local.Infrastructure/ # Capa de Infraestructura
└── SGRRHH.Local.Server/        # Capa de Presentación
```

### 1. Domain (Dominio)

**Ubicación:** `SGRRHH.Local.Domain/`

**Contiene:**
- Entidades de negocio (`Empleado`, `Contrato`, `Vacacion`)
- Enums (`EstadoEmpleado`, `TipoContrato`)
- Interfaces de repositorios (`IEmpleadoRepository`)
- Interfaces de servicios de dominio
- Lógica de negocio pura (sin dependencias externas)

**Dependencias:** NINGUNA (capa más interna)

**Ejemplo:**
```csharp
// SGRRHH.Local.Domain/Entities/Empleado.cs
namespace SGRRHH.Local.Domain.Entities;

public class Empleado
{
    public int Id { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public EstadoEmpleado Estado { get; set; }
}
```

### 2. Shared (Compartida)

**Ubicación:** `SGRRHH.Local.Shared/`

**Contiene:**
- DTOs (Data Transfer Objects)
- Modelos de vista
- Validaciones compartidas
- Constantes

**Dependencias:** Puede referenciar Domain

**Ejemplo:**
```csharp
// SGRRHH.Local.Shared/DTOs/EmpleadoDto.cs
namespace SGRRHH.Local.Shared.DTOs;

public class EmpleadoDto
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}
```

### 3. Infrastructure (Infraestructura)

**Ubicación:** `SGRRHH.Local.Infrastructure/`

**Contiene:**
- Implementación de repositorios con Dapper
- Servicios de infraestructura (Email, SMS, etc.)
- Acceso a datos
- Configuración de conexión SQLite

**Dependencias:** Puede referenciar Domain y Shared

**Ejemplo:**
```csharp
// SGRRHH.Local.Infrastructure/Repositories/EmpleadoRepository.cs
using Dapper;
using Microsoft.Data.Sqlite;

namespace SGRRHH.Local.Infrastructure.Repositories;

public class EmpleadoRepository : IEmpleadoRepository
{
    private readonly string _connectionString;
    
    public EmpleadoRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")!;
    }
    
    private SqliteConnection GetConnection() 
        => new SqliteConnection(_connectionString);
    
    public async Task<IEnumerable<EmpleadoDto>> GetAllAsync()
    {
        using var conn = GetConnection();
        return await conn.QueryAsync<EmpleadoDto>(@"
            SELECT id AS Id, nombres || ' ' || apellidos AS NombreCompleto
            FROM empleados WHERE activo = 1");
    }
}
```

### 4. Server (Presentación)

**Ubicación:** `SGRRHH.Local.Server/`

**Contiene:**
- Componentes Blazor (`.razor`)
- Pages con routing (`@page`)
- Layouts
- CSS y assets estáticos
- `Program.cs` (configuración de servicios)

**Dependencias:** Puede referenciar todas las capas anteriores

**Ejemplo:**
```razor
@* SGRRHH.Local.Server/Components/Pages/Empleados.razor *@
@page "/empleados"
@inject IEmpleadoRepository EmpleadoRepo

<PageTitle>EMPLEADOS</PageTitle>

<div class="page-container">
    <h1 class="page-title">EMPLEADOS</h1>
    @* ... *@
</div>
```

## Inyección de Dependencias

**Ubicación:** `SGRRHH.Local.Server/Program.cs`

Registrar servicios siguiendo este patrón:

```csharp
// Repositorios
builder.Services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();
builder.Services.AddScoped<IContratoRepository, ContratoRepository>();

// Servicios
builder.Services.AddScoped<ILiquidacionService, LiquidacionService>();
builder.Services.AddScoped<IVacacionService, VacacionService>();

// Servicios de infraestructura
builder.Services.AddSingleton<IEmailService, EmailService>();
```

**Scope correcto:**
- `AddScoped` - Para repositorios y servicios de negocio (1 instancia por request)
- `AddSingleton` - Para servicios sin estado (Email, Logger)
- `AddTransient` - Raramente usado

## Anti-Patterns PROHIBIDOS

### ❌ NO crear servicios en la capa UI

```csharp
// ❌ INCORRECTO
public partial class Empleados : ComponentBase
{
    private async Task<List<EmpleadoDto>> GetEmpleadosAsync()
    {
        using var conn = new SqliteConnection(_connectionString);
        return await conn.QueryAsync<EmpleadoDto>("SELECT * FROM empleados");
    }
}
```

```csharp
// ✅ CORRECTO
[Inject] private IEmpleadoRepository EmpleadoRepo { get; set; } = default!;

protected override async Task OnInitializedAsync()
{
    empleados = await EmpleadoRepo.GetAllAsync();
}
```

### ❌ NO usar Entity Framework

Este proyecto usa **Dapper** por diseño. NO agregar EF Core bajo ninguna circunstancia.

### ❌ NO mezclar lógica de negocio en repositorios

```csharp
// ❌ INCORRECTO - Lógica de negocio en repositorio
public async Task<bool> AprobarEmpleadoAsync(int id)
{
    // Validaciones de negocio aquí ❌
    if (empleado.Estado != EstadoEmpleado.PendienteAprobacion)
        throw new Exception("No se puede aprobar");
    
    return await conn.ExecuteAsync("UPDATE...");
}
```

```csharp
// ✅ CORRECTO - Lógica en servicio de dominio
public class EmpleadoService
{
    public async Task<bool> AprobarAsync(int id)
    {
        var empleado = await _repo.GetByIdAsync(id);
        
        // Validaciones de negocio aquí ✅
        if (empleado.Estado != EstadoEmpleado.PendienteAprobacion)
            throw new InvalidOperationException("...");
        
        empleado.Estado = EstadoEmpleado.Activo;
        return await _repo.UpdateAsync(empleado);
    }
}
```

## Flujo de Datos

```
UI (Blazor Component)
    ↓ [Inject]
Servicio de Aplicación (opcional)
    ↓ [Inject]
Repositorio (Dapper + SQLite)
    ↓
Base de Datos
```

## Nomenclatura de Archivos

| Tipo | Formato | Ejemplo |
|------|---------|---------|
| Entidad | `{Nombre}.cs` | `Empleado.cs` |
| Interfaz Repositorio | `I{Entidad}Repository.cs` | `IEmpleadoRepository.cs` |
| Repositorio | `{Entidad}Repository.cs` | `EmpleadoRepository.cs` |
| DTO | `{Entidad}Dto.cs` | `EmpleadoDto.cs` |
| Servicio | `{Nombre}Service.cs` | `LiquidacionService.cs` |
| Página Blazor | `{Nombre}.razor` | `Empleados.razor` |
| Componente UI | `{Nombre}.razor` | `SelectorEstado.razor` |

---

**Última actualización:** 2026-01-28
