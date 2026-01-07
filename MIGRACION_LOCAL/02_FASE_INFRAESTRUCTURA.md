# 🗄️ FASE 1: Capa de Infraestructura SQLite

## 📋 Contexto

Fase anterior completada: Tenemos la estructura base del proyecto SGRRHH.Local con entidades y enums.

**Objetivo:** Implementar la capa de acceso a datos usando Dapper + SQLite.

---

## 🎯 Objetivo de esta Fase

Implementar DapperContext, DatabasePathResolver, script de inicialización de BD, y todos los repositorios.

---

## 📝 PROMPT PARA CLAUDE

```
Ahora necesito que implementes la capa de infraestructura para SGRRHH.Local usando Dapper + SQLite.

**PROYECTO:** SGRRHH.Local.Infrastructure

**REFERENCIA:** Usa el mismo patrón que ForestechOil:
- C:\programajava\forestech-blazor\ForestechOil.Infrastructure\Data\DapperContext.cs
- C:\programajava\forestech-blazor\ForestechOil.Infrastructure\Repositories\ProductRepository.cs

---

## ARCHIVOS A CREAR EN Data/:

### 1. DatabasePathResolver.cs
```csharp
// Resuelve la ruta de la base de datos según configuración
// Debe crear el directorio si no existe
// Métodos:
//   - string GetDatabasePath()
//   - string GetConnectionString()
//   - string GetStoragePath()
//   - void EnsureDirectoriesExist()
```

### 2. DapperContext.cs
```csharp
// Contexto de Dapper para SQLite
// Igual que ForestechOil pero adaptado
// Incluir método RunMigrationsAsync() para crear tablas
```

### 3. DatabaseInitializer.cs
```csharp
// Script SQL completo para crear las 16+ tablas
// Incluir índices para performance
// Incluir datos iniciales (admin user, tipos de permiso, etc.)
```

---

## REPOSITORIOS A CREAR (en Repositories/):

Crear un repositorio por cada entidad principal. Usar el patrón de ForestechOil:

1. **EmpleadoRepository.cs**
   - GetAllAsync()
   - GetByIdAsync(int id)
   - GetByCedulaAsync(string cedula)
   - GetByCodigoAsync(string codigo)
   - GetByDepartamentoAsync(int departamentoId)
   - GetActivosAsync()
   - CreateAsync(Empleado entity)
   - UpdateAsync(Empleado entity)
   - DeleteAsync(int id) - Soft delete
   - SearchAsync(string searchTerm)

2. **UsuarioRepository.cs**
   - GetByUsernameAsync(string username)
   - ValidateCredentialsAsync(string username, string passwordHash)
   - GetByEmpleadoIdAsync(int empleadoId)
   - UpdateLastAccessAsync(int userId)

3. **PermisoRepository.cs**
   - GetByEmpleadoAsync(int empleadoId)
   - GetByEstadoAsync(EstadoPermiso estado)
   - GetPendientesAprobacionAsync()
   - GetByFechasAsync(DateTime inicio, DateTime fin)
   - GenerarNumeroActaAsync() - Formato: PERM-YYYY-NNNN

4. **VacacionRepository.cs**
   - GetByEmpleadoAsync(int empleadoId)
   - GetByPeriodoAsync(int año)
   - GetPendientesAprobacionAsync()
   - CalcularDiasDisponiblesAsync(int empleadoId, int año)

5. **ContratoRepository.cs**
   - GetByEmpleadoAsync(int empleadoId)
   - GetActivoByEmpleadoAsync(int empleadoId)
   - GetVigentesAsync()

6. **DepartamentoRepository.cs** (CRUD básico)
7. **CargoRepository.cs** (CRUD básico + GetByDepartamento)
8. **TipoPermisoRepository.cs** (CRUD básico)
9. **ProyectoRepository.cs** (CRUD básico)
10. **ActividadRepository.cs** (CRUD + GetByProyecto)
11. **ProyectoEmpleadoRepository.cs** (relación M:N)
12. **RegistroDiarioRepository.cs**
    - GetByEmpleadoFechaAsync(int empleadoId, DateTime fecha)
    - GetByFechaAsync(DateTime fecha)
    - GetByRangoFechasAsync(DateTime inicio, DateTime fin)
13. **DetalleActividadRepository.cs**
14. **DocumentoEmpleadoRepository.cs**
    - GetByEmpleadoAsync(int empleadoId)
    - GetByTipoAsync(TipoDocumentoEmpleado tipo)
15. **AuditLogRepository.cs**
    - GetByEntidadAsync(string entidad, int? entidadId)
    - GetByUsuarioAsync(int usuarioId)
    - GetByFechasAsync(DateTime inicio, DateTime fin)
16. **ConfiguracionRepository.cs**
    - GetByClaveAsync(string clave)
    - SetValorAsync(string clave, string valor)

---

## PATRÓN DE REPOSITORIO (ejemplo):

```csharp
public class EmpleadoRepository : IEmpleadoRepository
{
    private readonly DapperContext _context;
    private readonly ILogger<EmpleadoRepository> _logger;

    public EmpleadoRepository(DapperContext context, ILogger<EmpleadoRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Empleado>> GetAllAsync()
    {
        using var connection = _context.GetConnection();
        var sql = @"
            SELECT e.*, d.*, c.*
            FROM empleados e
            LEFT JOIN departamentos d ON e.departamento_id = d.id
            LEFT JOIN cargos c ON e.cargo_id = c.id
            WHERE e.activo = 1
            ORDER BY e.apellidos, e.nombres";
        
        var empleados = await connection.QueryAsync<Empleado, Departamento, Cargo, Empleado>(
            sql,
            (empleado, departamento, cargo) =>
            {
                empleado.Departamento = departamento;
                empleado.Cargo = cargo;
                return empleado;
            },
            splitOn: "id,id");
        
        return empleados;
    }

    public async Task<Result> CreateAsync(Empleado entity)
    {
        using var connection = _context.GetConnection();
        
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.Activo = true;

        var sql = @"
            INSERT INTO empleados (codigo, cedula, nombres, apellidos, ...)
            VALUES (@Codigo, @Cedula, @Nombres, @Apellidos, ...);
            SELECT last_insert_rowid();";

        try
        {
            entity.Id = await connection.ExecuteScalarAsync<int>(sql, entity);
            _logger.LogDebug("Empleado created: {Id}", entity.Id);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating empleado");
            return Result.Fail($"Error: {ex.Message}");
        }
    }
}
```

---

## DATOS INICIALES (en DatabaseInitializer):

```sql
-- Usuario administrador por defecto
INSERT INTO usuarios (username, password_hash, nombre_completo, rol, activo)
VALUES ('admin', '$2a$11$...hashedpassword...', 'Administrador', 1, 1);
-- Password: admin123 (hasheado con BCrypt)

-- Tipos de permiso comunes
INSERT INTO tipos_permiso (nombre, descripcion, color, dias_maximos) VALUES
('Cita Médica', 'Permiso para asistir a cita médica', '#4CAF50', 1),
('Calamidad Doméstica', 'Permiso por calamidad doméstica', '#F44336', 5),
('Licencia de Luto', 'Permiso por fallecimiento de familiar', '#9C27B0', 5),
('Licencia de Maternidad', 'Licencia de maternidad (18 semanas)', '#E91E63', 126),
('Licencia de Paternidad', 'Licencia de paternidad (2 semanas)', '#2196F3', 14),
('Permiso Personal', 'Permiso personal compensable', '#FF9800', 1),
('Diligencia Personal', 'Diligencia personal (banco, notaría)', '#607D8B', 1);

-- Departamentos ejemplo
INSERT INTO departamentos (codigo, nombre, activo) VALUES
('ADM', 'Administración', 1),
('OPE', 'Operaciones', 1),
('RRH', 'Recursos Humanos', 1);
```

---

## INTERFACES A CREAR (en SGRRHH.Local.Shared/Interfaces/):

Crear una interfaz por cada repositorio:
- IEmpleadoRepository.cs
- IUsuarioRepository.cs
- IPermisoRepository.cs
- etc.

Ejemplo:
```csharp
public interface IEmpleadoRepository : IRepository<Empleado>
{
    Task<Empleado?> GetByCedulaAsync(string cedula);
    Task<Empleado?> GetByCodigoAsync(string codigo);
    Task<IEnumerable<Empleado>> GetByDepartamentoAsync(int departamentoId);
    Task<IEnumerable<Empleado>> GetActivosAsync();
    Task<IEnumerable<Empleado>> SearchAsync(string searchTerm);
}
```

---

**IMPORTANTE:**
- Usar Dapper con parámetros nombrados (@Param)
- Implementar soft delete (campo activo = 0)
- Incluir logging con ILogger
- Manejar errores con Result pattern
- Configurar MatchNamesWithUnderscores en Program.cs
- Los repositorios deben ser Scoped en DI
```

---

## ✅ Checklist de Entregables

- [ ] Data/DatabasePathResolver.cs
- [ ] Data/DapperContext.cs
- [ ] Data/DatabaseInitializer.cs (SQL de creación)
- [ ] Interfaces en Shared/ (16 archivos)
- [ ] Repositorios en Infrastructure/ (16 archivos):
  - [ ] EmpleadoRepository.cs
  - [ ] UsuarioRepository.cs
  - [ ] PermisoRepository.cs
  - [ ] VacacionRepository.cs
  - [ ] ContratoRepository.cs
  - [ ] DepartamentoRepository.cs
  - [ ] CargoRepository.cs
  - [ ] TipoPermisoRepository.cs
  - [ ] ProyectoRepository.cs
  - [ ] ActividadRepository.cs
  - [ ] ProyectoEmpleadoRepository.cs
  - [ ] RegistroDiarioRepository.cs
  - [ ] DetalleActividadRepository.cs
  - [ ] DocumentoEmpleadoRepository.cs
  - [ ] AuditLogRepository.cs
  - [ ] ConfiguracionRepository.cs

---

## 📊 Esquema de Tablas Completo

Ver archivo `00_ROADMAP_MIGRACION.md` sección "Esquema de Base de Datos SQLite" para el SQL completo de las 16+ tablas.

---

## 🔗 Dependencias NuGet Requeridas

```xml
<!-- SGRRHH.Local.Infrastructure.csproj -->
<PackageReference Include="Dapper" Version="2.1.35" />
<PackageReference Include="Microsoft.Data.Sqlite" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
```
