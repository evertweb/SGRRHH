---
name: dapper-repository
description: Patrón de repositorio con Dapper para SGRRHH. Usar al crear repositorios, queries SQL, migraciones de base de datos o acceso a datos.
license: MIT
compatibility: opencode
metadata:
  project: SGRRHH
  orm: dapper
  database: sqlite
  architecture: repository-pattern
---
# Repositorios Dapper - SGRRHH

## Ubicación de Archivos

| Tipo | Ruta |
|------|------|
| Repositorios | `SGRRHH.Local.Infrastructure/Repositories/` |
| Servicios | `SGRRHH.Local.Infrastructure/Services/` |
| Interfaces | `SGRRHH.Local.Domain/Interfaces/` |
| DTOs | `SGRRHH.Local.Shared/DTOs/` |
| Migraciones | `SGRRHH.Local/scripts/migration_*.sql` |

## Base de Datos

| Propiedad | Valor |
|-----------|-------|
| Tipo | SQLite |
| Ruta Producción | `C:\SGRRHH\Data\sgrrhh.db` |
| Ruta Desarrollo | Según `appsettings.json` |
| ORM | Dapper (NO Entity Framework) |

## Nomenclatura SQL

> [!IMPORTANT]
> Usar **snake_case** para tablas y columnas en SQL.

| Elemento | Formato | Ejemplo |
|----------|---------|---------|
| Tablas | snake_case | `empleados`, `tipos_permiso` |
| Columnas | snake_case | `fecha_creacion`, `tipo_contrato` |
| Interfaces C# | I{Entidad}Repository | `IEmpleadoRepository` |
| Clases C# | {Entidad}Repository | `EmpleadoRepository` |

## Patrón de Repositorio Base

```csharp
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

public class EntidadRepository : IEntidadRepository
{
    private readonly string _connectionString;
    
    public EntidadRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
    }
    
    private SqliteConnection GetConnection()
        => new SqliteConnection(_connectionString);
    
    public async Task<IEnumerable<EntidadDto>> GetAllAsync()
    {
        using var conn = GetConnection();
        return await conn.QueryAsync<EntidadDto>(@"
            SELECT 
                id AS Id,
                nombre AS Nombre,
                fecha_creacion AS FechaCreacion,
                activo AS Activo
            FROM entidades
            WHERE activo = 1
            ORDER BY nombre");
    }
    
    public async Task<EntidadDto?> GetByIdAsync(int id)
    {
        using var conn = GetConnection();
        return await conn.QueryFirstOrDefaultAsync<EntidadDto>(@"
            SELECT 
                id AS Id,
                nombre AS Nombre,
                fecha_creacion AS FechaCreacion
            FROM entidades
            WHERE id = @Id", new { Id = id });
    }
    
    public async Task<int> CreateAsync(EntidadDto entidad)
    {
        using var conn = GetConnection();
        return await conn.ExecuteScalarAsync<int>(@"
            INSERT INTO entidades (nombre, fecha_creacion, activo)
            VALUES (@Nombre, @FechaCreacion, 1);
            SELECT last_insert_rowid();", entidad);
    }
    
    public async Task<bool> UpdateAsync(EntidadDto entidad)
    {
        using var conn = GetConnection();
        var rows = await conn.ExecuteAsync(@"
            UPDATE entidades
            SET nombre = @Nombre,
                fecha_modificacion = CURRENT_TIMESTAMP
            WHERE id = @Id", entidad);
        return rows > 0;
    }
    
    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = GetConnection();
        var rows = await conn.ExecuteAsync(@"
            UPDATE entidades
            SET activo = 0
            WHERE id = @Id", new { Id = id });
        return rows > 0;
    }
}
```

## Interfaz de Repositorio

```csharp
// SGRRHH.Local.Domain/Interfaces/IEntidadRepository.cs
public interface IEntidadRepository
{
    Task<IEnumerable<EntidadDto>> GetAllAsync();
    Task<EntidadDto?> GetByIdAsync(int id);
    Task<int> CreateAsync(EntidadDto entidad);
    Task<bool> UpdateAsync(EntidadDto entidad);
    Task<bool> DeleteAsync(int id);
}
```

## DTO (Data Transfer Object)

```csharp
// SGRRHH.Local.Shared/DTOs/EntidadDto.cs
public class EntidadDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public bool Activo { get; set; } = true;
}
```

## Registro en Program.cs

```csharp
// Program.cs
builder.Services.AddScoped<IEntidadRepository, EntidadRepository>();
builder.Services.AddScoped<IEntidadService, EntidadService>();
```

## Migraciones SQL

### Formato de Nombre
```
migration_{nombre_descriptivo}_v{N}.sql
```

Ejemplos:
- `migration_empleados_v1.sql`
- `migration_tipos_permiso_v1.sql`
- `migration_dotacion_epp_v1.sql`

### Estructura de Migración
```sql
-- migration_nueva_tabla_v1.sql
-- Descripción: Crear tabla para nueva funcionalidad
-- Fecha: 2026-01-15
-- Autor: [nombre]

-- Crear tabla
CREATE TABLE IF NOT EXISTS nueva_tabla (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    nombre TEXT NOT NULL,
    descripcion TEXT,
    empleado_id INTEGER NOT NULL,
    fecha_creacion TEXT DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion TEXT,
    activo INTEGER DEFAULT 1,
    FOREIGN KEY (empleado_id) REFERENCES empleados(id)
);

-- Índices
CREATE INDEX IF NOT EXISTS idx_nueva_tabla_empleado 
ON nueva_tabla(empleado_id);

-- Datos iniciales (si aplica)
INSERT OR IGNORE INTO nueva_tabla (id, nombre) VALUES 
(1, 'Valor por defecto');
```

## Comandos Útiles SQLite

```powershell
# Ver todas las tablas
sqlite3 "C:\SGRRHH\Data\sgrrhh.db" ".tables"

# Ver esquema de una tabla
sqlite3 "C:\SGRRHH\Data\sgrrhh.db" ".schema empleados"

# Ver primeras 10 filas
sqlite3 "C:\SGRRHH\Data\sgrrhh.db" "SELECT * FROM empleados LIMIT 10;"

# Ejecutar migración
sqlite3 "C:\SGRRHH\Data\sgrrhh.db" < scripts/migration_xxx_v1.sql

# Backup
sqlite3 "C:\SGRRHH\Data\sgrrhh.db" ".backup backup.db"
```

## Mapeo de Tipos SQLite ↔ C#

| SQLite | C# | Notas |
|--------|-----|-------|
| INTEGER | int, long, bool | bool: 0/1 |
| TEXT | string, DateTime | DateTime como ISO 8601 |
| REAL | double, decimal | |
| BLOB | byte[] | Para archivos |

## Transacciones

```csharp
public async Task<bool> TransferenciaAsync(int origenId, int destinoId, decimal monto)
{
    using var conn = GetConnection();
    await conn.OpenAsync();
    using var transaction = await conn.BeginTransactionAsync();
    
    try
    {
        await conn.ExecuteAsync(@"
            UPDATE cuentas SET saldo = saldo - @Monto WHERE id = @Id",
            new { Id = origenId, Monto = monto }, transaction);
        
        await conn.ExecuteAsync(@"
            UPDATE cuentas SET saldo = saldo + @Monto WHERE id = @Id",
            new { Id = destinoId, Monto = monto }, transaction);
        
        await transaction.CommitAsync();
        return true;
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

## Queries con Múltiples Resultados

```csharp
public async Task<(EmpleadoDto Empleado, IEnumerable<ContratoDto> Contratos)> 
    GetEmpleadoConContratosAsync(int id)
{
    using var conn = GetConnection();
    using var multi = await conn.QueryMultipleAsync(@"
        SELECT * FROM empleados WHERE id = @Id;
        SELECT * FROM contratos WHERE empleado_id = @Id ORDER BY fecha_inicio DESC;",
        new { Id = id });
    
    var empleado = await multi.ReadFirstOrDefaultAsync<EmpleadoDto>();
    var contratos = await multi.ReadAsync<ContratoDto>();
    
    return (empleado!, contratos);
}
```

## Tablas Comunes del Proyecto

| Tabla | Descripción |
|-------|-------------|
| `empleados` | Empleados de la empresa |
| `contratos` | Contratos laborales |
| `vacaciones` | Solicitudes de vacaciones |
| `permisos` | Permisos laborales |
| `incapacidades` | Incapacidades médicas |
| `usuarios` | Usuarios del sistema |
| `eps` | EPS registradas |
| `afp` | Fondos de pensión |
| `arl` | ARL registradas |
| `cajas_compensacion` | Cajas de compensación |