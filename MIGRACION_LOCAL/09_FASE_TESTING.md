# 🧪 FASE 8: Testing y Documentación

## 📋 Contexto

Todas las fases de desarrollo completadas:
- ✅ Estructura base del proyecto
- ✅ Infraestructura SQLite con Dapper
- ✅ Sistema de archivos local
- ✅ Autenticación local con BCrypt
- ✅ UI Premium estilo ForestechOil
- ✅ Todas las páginas migradas
- ✅ Reportes con QuestPDF
- ✅ Backup/Restore, Export, Auditoría

**Objetivo:** Implementar tests, optimización y documentación final.

---

## 🎯 Objetivo de esta Fase

Garantizar calidad con tests de integración, optimizar rendimiento, y documentar para usuarios finales.

---

## 📝 PROMPT PARA CLAUDE

```
Necesito que implementes tests de integración y documentación para SGRRHH.Local.

**PROYECTO:** SGRRHH.Local.Tests/

---

## 1. PROYECTO DE TESTS

### Estructura:

```
SGRRHH.Local.Tests/
├── SGRRHH.Local.Tests.csproj
├── TestDatabaseFixture.cs
├── Repositories/
│   ├── EmpleadoRepositoryTests.cs
│   ├── PermisoRepositoryTests.cs
│   ├── VacacionRepositoryTests.cs
│   └── UsuarioRepositoryTests.cs
├── Services/
│   ├── AuthServiceTests.cs
│   ├── BackupServiceTests.cs
│   └── VacacionServiceTests.cs
└── Integration/
    └── WorkflowTests.cs
```

### SGRRHH.Local.Tests.csproj:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="xunit" Version="2.6.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.4" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    <PackageReference Include="NSubstitute" Version="5.1.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\SGRRHH.Local.Infrastructure\SGRRHH.Local.Infrastructure.csproj" />
  </ItemGroup>

</Project>
```

### TestDatabaseFixture.cs:

```csharp
using Microsoft.Data.Sqlite;
using SGRRHH.Local.Infrastructure.Data;

namespace SGRRHH.Local.Tests;

public class TestDatabaseFixture : IDisposable
{
    public string DatabasePath { get; }
    public DapperContext Context { get; }
    
    public TestDatabaseFixture()
    {
        // Base de datos temporal para tests
        DatabasePath = Path.Combine(Path.GetTempPath(), $"sgrrhh_test_{Guid.NewGuid()}.db");
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "LocalDatabase:Path", DatabasePath }
            })
            .Build();
        
        Context = new DapperContext(configuration, Mock.Of<ILogger<DapperContext>>());
        
        // Crear esquema
        InitializeDatabaseAsync().Wait();
    }
    
    private async Task InitializeDatabaseAsync()
    {
        // Ejecutar scripts de creación de tablas
        using var connection = Context.CreateConnection();
        
        await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS Departamentos (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nombre TEXT NOT NULL,
                Codigo TEXT NOT NULL UNIQUE,
                Activo INTEGER DEFAULT 1
            );
            
            CREATE TABLE IF NOT EXISTS Cargos (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nombre TEXT NOT NULL,
                Descripcion TEXT,
                DepartamentoId INTEGER,
                Activo INTEGER DEFAULT 1,
                FOREIGN KEY (DepartamentoId) REFERENCES Departamentos(Id)
            );
            
            CREATE TABLE IF NOT EXISTS Empleados (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Codigo TEXT NOT NULL UNIQUE,
                Cedula TEXT NOT NULL UNIQUE,
                Nombres TEXT NOT NULL,
                Apellidos TEXT NOT NULL,
                FechaNacimiento TEXT,
                FechaIngreso TEXT NOT NULL,
                FotoPath TEXT,
                CargoId INTEGER,
                DepartamentoId INTEGER,
                Estado INTEGER DEFAULT 0,
                Genero INTEGER,
                EstadoCivil INTEGER,
                Direccion TEXT,
                Telefono TEXT,
                Email TEXT,
                NivelEducacion INTEGER,
                CreadoEn TEXT DEFAULT CURRENT_TIMESTAMP,
                ActualizadoEn TEXT,
                FOREIGN KEY (CargoId) REFERENCES Cargos(Id),
                FOREIGN KEY (DepartamentoId) REFERENCES Departamentos(Id)
            );
            
            CREATE TABLE IF NOT EXISTS Usuarios (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL UNIQUE,
                PasswordHash TEXT NOT NULL,
                EmpleadoId INTEGER UNIQUE,
                Rol INTEGER DEFAULT 3,
                Activo INTEGER DEFAULT 1,
                UltimoAcceso TEXT,
                FOREIGN KEY (EmpleadoId) REFERENCES Empleados(Id)
            );
            
            -- ... más tablas según schema completo
        ");
    }
    
    public void Dispose()
    {
        if (File.Exists(DatabasePath))
            File.Delete(DatabasePath);
    }
}

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<TestDatabaseFixture>
{
}
```

---

### Tests de Repositorios:

**EmpleadoRepositoryTests.cs:**

```csharp
using FluentAssertions;
using SGRRHH.Local.Infrastructure.Repositories;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Tests.Repositories;

[Collection("Database")]
public class EmpleadoRepositoryTests
{
    private readonly TestDatabaseFixture _fixture;
    private readonly EmpleadoRepository _repository;
    
    public EmpleadoRepositoryTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = new EmpleadoRepository(_fixture.Context, Mock.Of<ILogger<EmpleadoRepository>>());
    }
    
    [Fact]
    public async Task CreateAsync_WithValidEmpleado_ReturnsSuccess()
    {
        // Arrange
        var empleado = new Empleado
        {
            Codigo = "EMP-001",
            Cedula = "1234567890",
            Nombres = "Juan Carlos",
            Apellidos = "Pérez García",
            FechaIngreso = DateTime.Today.AddYears(-2),
            Estado = EstadoEmpleado.Activo
        };
        
        // Act
        var result = await _repository.CreateAsync(empleado);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeGreaterThan(0);
        empleado.Id.Should().Be(result.Value);
    }
    
    [Fact]
    public async Task CreateAsync_WithDuplicateCedula_ReturnsFail()
    {
        // Arrange
        var empleado1 = new Empleado { Codigo = "EMP-002", Cedula = "9999999999", Nombres = "Test", Apellidos = "Uno" };
        var empleado2 = new Empleado { Codigo = "EMP-003", Cedula = "9999999999", Nombres = "Test", Apellidos = "Dos" };
        
        await _repository.CreateAsync(empleado1);
        
        // Act
        var result = await _repository.CreateAsync(empleado2);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("cédula");
    }
    
    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsEmpleado()
    {
        // Arrange
        var empleado = new Empleado { Codigo = "EMP-004", Cedula = "1111111111", Nombres = "María", Apellidos = "López" };
        await _repository.CreateAsync(empleado);
        
        // Act
        var result = await _repository.GetByIdAsync(empleado.Id);
        
        // Assert
        result.Should().NotBeNull();
        result!.Nombres.Should().Be("María");
    }
    
    [Fact]
    public async Task SearchAsync_WithCriterio_ReturnsFilteredResults()
    {
        // Arrange
        await _repository.CreateAsync(new Empleado { Codigo = "EMP-005", Cedula = "2222222222", Nombres = "Carlos Alberto", Apellidos = "Mendez" });
        await _repository.CreateAsync(new Empleado { Codigo = "EMP-006", Cedula = "3333333333", Nombres = "Carlos Eduardo", Apellidos = "Torres" });
        
        // Act
        var results = await _repository.SearchAsync("Carlos");
        
        // Assert
        results.Should().HaveCountGreaterOrEqualTo(2);
        results.Should().OnlyContain(e => e.Nombres.Contains("Carlos"));
    }
    
    [Fact]
    public async Task UpdateAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var empleado = new Empleado { Codigo = "EMP-007", Cedula = "4444444444", Nombres = "Original", Apellidos = "Test" };
        await _repository.CreateAsync(empleado);
        
        empleado.Nombres = "Modificado";
        
        // Act
        var result = await _repository.UpdateAsync(empleado);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        
        var updated = await _repository.GetByIdAsync(empleado.Id);
        updated!.Nombres.Should().Be("Modificado");
    }
    
    [Fact]
    public async Task DeleteAsync_WithExistingId_PerformsSoftDelete()
    {
        // Arrange
        var empleado = new Empleado { Codigo = "EMP-008", Cedula = "5555555555", Nombres = "Para", Apellidos = "Eliminar" };
        await _repository.CreateAsync(empleado);
        
        // Act
        var result = await _repository.DeleteAsync(empleado.Id);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        
        var deleted = await _repository.GetByIdAsync(empleado.Id);
        deleted!.Estado.Should().Be(EstadoEmpleado.Inactivo);
    }
}
```

---

**AuthServiceTests.cs:**

```csharp
[Collection("Database")]
public class AuthServiceTests
{
    private readonly LocalAuthService _authService;
    private readonly IUsuarioRepository _usuarioRepository;
    
    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var password = "Admin123!";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        
        // Crear usuario de prueba
        await _usuarioRepository.CreateAsync(new Usuario
        {
            Username = "testadmin",
            PasswordHash = hashedPassword,
            Rol = RolUsuario.Administrador,
            Activo = true
        });
        
        // Act
        var result = await _authService.LoginAsync("testadmin", password);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Rol.Should().Be(RolUsuario.Administrador);
    }
    
    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsFail()
    {
        // Act
        var result = await _authService.LoginAsync("testadmin", "wrongpassword");
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Credenciales");
    }
    
    [Fact]
    public async Task ChangePasswordAsync_WithValidOldPassword_ReturnsSuccess()
    {
        // Arrange
        var oldPassword = "OldPass123!";
        var newPassword = "NewPass456!";
        
        // Act
        var result = await _authService.ChangePasswordAsync(userId, oldPassword, newPassword);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        
        // Verificar que puede loguear con nueva contraseña
        var loginResult = await _authService.LoginAsync("testuser", newPassword);
        loginResult.IsSuccess.Should().BeTrue();
    }
}
```

---

### Tests de Workflows:

**WorkflowTests.cs:**

```csharp
[Collection("Database")]
public class WorkflowTests
{
    [Fact]
    public async Task SolicitudPermisoWorkflow_CompleteFlow_Success()
    {
        // 1. Crear empleado
        var empleado = await CreateTestEmpleado();
        
        // 2. Crear solicitud de permiso
        var permiso = new Permiso
        {
            EmpleadoId = empleado.Id,
            TipoPermisoId = 1,
            FechaSolicitud = DateTime.Today,
            FechaInicio = DateTime.Today.AddDays(1),
            FechaFin = DateTime.Today.AddDays(1),
            HoraInicio = new TimeSpan(8, 0, 0),
            HoraFin = new TimeSpan(12, 0, 0),
            Motivo = "Cita médica",
            Estado = EstadoPermiso.Solicitado
        };
        
        var createResult = await _permisoService.CreateAsync(permiso);
        createResult.IsSuccess.Should().BeTrue();
        
        // 3. Aprobar permiso
        var approveResult = await _permisoService.AprobarAsync(permiso.Id, "aprobador@test.com");
        approveResult.IsSuccess.Should().BeTrue();
        
        // 4. Verificar estado
        var updated = await _permisoRepository.GetByIdAsync(permiso.Id);
        updated!.Estado.Should().Be(EstadoPermiso.Aprobado);
        updated.NumeroActa.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task SolicitudVacacionesWorkflow_ExceedsDays_ReturnsFail()
    {
        // 1. Crear empleado con 15 días disponibles
        var empleado = await CreateTestEmpleado();
        
        // 2. Intentar solicitar 20 días
        var vacacion = new Vacacion
        {
            EmpleadoId = empleado.Id,
            FechaSolicitud = DateTime.Today,
            FechaInicio = DateTime.Today.AddDays(10),
            FechaFin = DateTime.Today.AddDays(30), // 20 días
            Año = DateTime.Today.Year,
            Estado = EstadoVacacion.Solicitada
        };
        
        // Act
        var result = await _vacacionService.CreateAsync(vacacion);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("días disponibles");
    }
}
```

---

## 2. OPTIMIZACIÓN DE RENDIMIENTO

### Índices Adicionales para SQLite:

```sql
-- Índices para búsquedas frecuentes
CREATE INDEX IF NOT EXISTS idx_empleados_nombres ON Empleados(Nombres, Apellidos);
CREATE INDEX IF NOT EXISTS idx_empleados_departamento ON Empleados(DepartamentoId);
CREATE INDEX IF NOT EXISTS idx_empleados_cargo ON Empleados(CargoId);

CREATE INDEX IF NOT EXISTS idx_permisos_empleado_fecha ON Permisos(EmpleadoId, FechaSolicitud);
CREATE INDEX IF NOT EXISTS idx_permisos_estado ON Permisos(Estado);

CREATE INDEX IF NOT EXISTS idx_vacaciones_empleado_año ON Vacaciones(EmpleadoId, Año);

CREATE INDEX IF NOT EXISTS idx_audit_fecha ON AuditLogs(FechaHora);
CREATE INDEX IF NOT EXISTS idx_audit_entidad ON AuditLogs(Entidad, EntidadId);
```

### Paginación Eficiente:

```csharp
public async Task<PagedResult<T>> GetPagedAsync<T>(
    string sql, 
    object? parameters, 
    int page, 
    int pageSize)
{
    var countSql = $"SELECT COUNT(*) FROM ({sql})";
    var pagedSql = $"{sql} LIMIT @PageSize OFFSET @Offset";
    
    using var connection = _context.CreateConnection();
    
    var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);
    
    var items = await connection.QueryAsync<T>(pagedSql, new
    {
        parameters,
        PageSize = pageSize,
        Offset = (page - 1) * pageSize
    });
    
    return new PagedResult<T>
    {
        Items = items.ToList(),
        TotalCount = totalCount,
        Page = page,
        PageSize = pageSize,
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
    };
}
```

---

## 3. DOCUMENTACIÓN USUARIO FINAL

Crear carpeta: **SGRRHH.Local.Server/wwwroot/docs/**

**manual-usuario.md:**

```markdown
# Manual de Usuario - SGRRHH Local

## Índice
1. Inicio de Sesión
2. Navegación con Teclado
3. Gestión de Empleados
4. Permisos
5. Vacaciones
6. Contratos
7. Reportes
8. Configuración
9. Respaldos

---

## 1. Inicio de Sesión

- Ingrese su usuario y contraseña
- Presione Enter o clic en "Ingresar"
- Usuario por defecto: `admin` / `Admin123!`

## 2. Navegación con Teclado

| Tecla | Acción |
|-------|--------|
| F1 | Ayuda |
| F2 | Nuevo registro |
| F3 | Editar seleccionado |
| F4 | Buscar |
| F5 | Actualizar |
| F6 | Guardar |
| F8 | Eliminar |
| F10 | Exportar Excel |
| F12 | Generar PDF |
| Esc | Cancelar/Cerrar |

## 3. Gestión de Empleados

### Crear Empleado
1. Menú lateral → Empleados
2. Presione F2 o botón "Nuevo"
3. Complete los datos obligatorios (*)
4. Cargue foto (opcional)
5. Presione F6 o "Guardar"

### Buscar Empleado
1. Presione F4 o use el campo de búsqueda
2. Escriba nombre, cédula o código
3. Los resultados se filtran automáticamente

## 4. Permisos

### Solicitar Permiso
1. Menú → Permisos → Nuevo
2. Seleccione el empleado
3. Seleccione tipo de permiso
4. Ingrese fechas y motivo
5. Guardar → Estado: "Solicitado"

### Aprobar/Rechazar
1. Abra el permiso pendiente
2. Revise la información
3. Clic en "Aprobar" o "Rechazar"
4. Al aprobar se genera el Acta automáticamente

### Generar Acta PDF
1. Abra un permiso aprobado
2. Presione F12 o "Generar Acta"
3. El PDF se descarga automáticamente

## 5. Vacaciones

### Solicitar Vacaciones
1. Menú → Vacaciones → Nueva Solicitud
2. Seleccione empleado
3. El sistema muestra días disponibles
4. Seleccione rango de fechas
5. Guardar

### Consultar Saldo
- En la lista de empleados se muestra el saldo
- O en el detalle de vacaciones del empleado

## 6. Contratos

### Alertas de Vencimiento
- El dashboard muestra contratos próximos a vencer
- Configuración define días de anticipación (default: 30)

### Renovar Contrato
1. Abra el contrato a vencer
2. Clic en "Renovar"
3. Ajuste nuevo período
4. El anterior queda como "Anterior"

## 7. Reportes

### Tipos Disponibles
- Listado de Empleados
- Permisos por Período
- Vacaciones por Año
- Contratos Vigentes

### Exportar
- **Excel**: F10 o botón "Excel"
- **PDF**: F12 o botón "PDF"

## 8. Configuración

### Datos de la Empresa
- Nombre, NIT, dirección (aparecen en reportes)

### Parámetros del Sistema
- Días de vacaciones anuales
- Días anticipación alertas contratos

## 9. Respaldos

### Backup Manual
1. Configuración → Respaldos
2. Clic "Crear Backup Ahora"
3. Se guarda en carpeta Backups/

### Backup Automático
- Habilitar en Configuración
- Definir hora (ej: 23:00)
- Se ejecuta diariamente

### Restaurar
1. Seleccione backup de la lista
2. Clic "Restaurar"
3. Confirme la acción
4. Reinicie la aplicación
```

---

## ✅ Checklist de Entregables

### Proyecto de Tests:
- [ ] SGRRHH.Local.Tests.csproj
- [ ] TestDatabaseFixture.cs
- [ ] Repositories/EmpleadoRepositoryTests.cs
- [ ] Repositories/PermisoRepositoryTests.cs
- [ ] Repositories/VacacionRepositoryTests.cs
- [ ] Services/AuthServiceTests.cs
- [ ] Services/BackupServiceTests.cs
- [ ] Integration/WorkflowTests.cs

### Optimización:
- [ ] Script de índices adicionales
- [ ] PagedResult<T> genérico
- [ ] Logging de queries lentas

### Documentación:
- [ ] wwwroot/docs/manual-usuario.md
- [ ] README.md del proyecto
- [ ] CHANGELOG.md

### Deployment:
- [ ] Script de instalación para cliente
- [ ] Verificación de .NET 8 Runtime
- [ ] Creación de acceso directo

---

## 🚀 Script de Instalación

**Install.ps1:**

```powershell
# SGRRHH Local - Script de Instalación
$InstallPath = "C:\SGRRHH.Local"
$AppName = "SGRRHH Local"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   Instalador de $AppName" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Verificar .NET 8 Runtime
$dotnetVersion = dotnet --list-runtimes | Select-String "Microsoft.AspNetCore.App 8"
if (-not $dotnetVersion) {
    Write-Host "ERROR: Se requiere .NET 8 Runtime" -ForegroundColor Red
    Write-Host "Descargue de: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
    exit 1
}

# Crear directorio de instalación
Write-Host "Creando directorio de instalación..." -ForegroundColor Yellow
New-Item -ItemType Directory -Path $InstallPath -Force | Out-Null

# Copiar archivos
Write-Host "Copiando archivos..." -ForegroundColor Yellow
Copy-Item -Path ".\publish\*" -Destination $InstallPath -Recurse -Force

# Crear acceso directo en escritorio
$WshShell = New-Object -ComObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("$env:USERPROFILE\Desktop\$AppName.lnk")
$Shortcut.TargetPath = "$InstallPath\SGRRHH.Local.Server.exe"
$Shortcut.WorkingDirectory = $InstallPath
$Shortcut.IconLocation = "$InstallPath\wwwroot\favicon.ico"
$Shortcut.Save()

Write-Host ""
Write-Host "✅ Instalación completada!" -ForegroundColor Green
Write-Host "   Ubicación: $InstallPath" -ForegroundColor White
Write-Host "   Acceso directo creado en el escritorio" -ForegroundColor White
Write-Host ""
Write-Host "Para iniciar, ejecute: $InstallPath\SGRRHH.Local.Server.exe" -ForegroundColor Yellow
```
