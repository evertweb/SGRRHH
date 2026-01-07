# ⚙️ FASE 7: Funcionalidades Avanzadas

## 📋 Contexto

Fases anteriores completadas:
- ✅ Estructura base del proyecto
- ✅ Infraestructura SQLite con Dapper
- ✅ Sistema de archivos local
- ✅ Autenticación local con BCrypt
- ✅ UI Premium estilo ForestechOil
- ✅ Todas las páginas migradas
- ✅ Reportes con QuestPDF

**Objetivo:** Implementar funcionalidades avanzadas: backup/restore, exportación Excel, auditoría, y configuración.

---

## 🎯 Objetivo de esta Fase

Agregar las funcionalidades de respaldo, exportación y configuración para completar la aplicación.

---

## 📝 PROMPT PARA CLAUDE

```
Necesito que implementes las funcionalidades avanzadas para SGRRHH.Local.

**PROYECTO:** SGRRHH.Local.Infrastructure/Services/

---

## FUNCIONALIDADES A IMPLEMENTAR:

### 1. Sistema de Backup/Restore

**Interfaz IBackupService.cs:**

```csharp
public interface IBackupService
{
    // Backup
    Task<Result<string>> CreateBackupAsync(string? description = null);
    Task<Result> CreateScheduledBackupAsync(); // Para timer automático
    
    // Restore
    Task<Result<IEnumerable<BackupInfo>>> GetAvailableBackupsAsync();
    Task<Result> RestoreFromBackupAsync(string backupFileName);
    
    // Mantenimiento
    Task<Result<int>> CleanupOldBackupsAsync(int keepDays = 30);
    Task<Result<long>> GetBackupsFolderSizeAsync();
}

public class BackupInfo
{
    public string FileName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public long SizeBytes { get; set; }
    public string? Description { get; set; }
    public bool IncludesFiles { get; set; }
}
```

**BackupService.cs:**

```csharp
public class BackupService : IBackupService
{
    private readonly DatabasePathResolver _pathResolver;
    private readonly ILogger<BackupService> _logger;
    private readonly string _backupPath;
    private readonly string _databasePath;
    private readonly string _storagePath;
    
    public BackupService(
        DatabasePathResolver pathResolver,
        IConfiguration configuration,
        ILogger<BackupService> logger)
    {
        _pathResolver = pathResolver;
        _logger = logger;
        _backupPath = configuration.GetValue<string>("LocalDatabase:BackupPath") 
            ?? Path.Combine(pathResolver.GetStoragePath(), "Backups");
        _databasePath = pathResolver.GetDatabasePath();
        _storagePath = pathResolver.GetStoragePath();
    }
    
    public async Task<Result<string>> CreateBackupAsync(string? description = null)
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var backupFolder = Path.Combine(_backupPath, timestamp);
            Directory.CreateDirectory(backupFolder);
            
            // 1. Copiar base de datos SQLite
            var dbBackupPath = Path.Combine(backupFolder, "sgrrhh.db");
            File.Copy(_databasePath, dbBackupPath, overwrite: true);
            
            // 2. Comprimir carpeta de fotos
            var fotosPath = Path.Combine(_storagePath, "Fotos");
            if (Directory.Exists(fotosPath))
            {
                var fotosZip = Path.Combine(backupFolder, "Fotos.zip");
                System.IO.Compression.ZipFile.CreateFromDirectory(fotosPath, fotosZip);
            }
            
            // 3. Comprimir documentos
            var docsPath = Path.Combine(_storagePath, "Documentos");
            if (Directory.Exists(docsPath))
            {
                var docsZip = Path.Combine(backupFolder, "Documentos.zip");
                System.IO.Compression.ZipFile.CreateFromDirectory(docsPath, docsZip);
            }
            
            // 4. Guardar metadatos
            var metadata = new
            {
                CreatedAt = DateTime.Now,
                Description = description,
                Version = "1.0.0",
                DatabaseSize = new FileInfo(_databasePath).Length,
                IncludesFiles = true
            };
            
            var metadataPath = Path.Combine(backupFolder, "backup.json");
            await File.WriteAllTextAsync(metadataPath, 
                System.Text.Json.JsonSerializer.Serialize(metadata, new System.Text.Json.JsonSerializerOptions 
                { 
                    WriteIndented = true 
                }));
            
            _logger.LogInformation("Backup creado: {BackupFolder}", backupFolder);
            return Result<string>.Ok(backupFolder, "Backup creado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando backup");
            return Result<string>.Fail($"Error: {ex.Message}");
        }
    }
    
    public async Task<Result> RestoreFromBackupAsync(string backupFolderName)
    {
        try
        {
            var backupFolder = Path.Combine(_backupPath, backupFolderName);
            
            if (!Directory.Exists(backupFolder))
                return Result.Fail("Backup no encontrado");
            
            // 1. Restaurar base de datos
            var dbBackupPath = Path.Combine(backupFolder, "sgrrhh.db");
            if (File.Exists(dbBackupPath))
            {
                // Hacer backup de seguridad antes de restaurar
                var safetyBackup = _databasePath + ".before-restore";
                File.Copy(_databasePath, safetyBackup, overwrite: true);
                
                File.Copy(dbBackupPath, _databasePath, overwrite: true);
            }
            
            // 2. Restaurar fotos
            var fotosZip = Path.Combine(backupFolder, "Fotos.zip");
            if (File.Exists(fotosZip))
            {
                var fotosPath = Path.Combine(_storagePath, "Fotos");
                if (Directory.Exists(fotosPath))
                    Directory.Delete(fotosPath, recursive: true);
                
                System.IO.Compression.ZipFile.ExtractToDirectory(fotosZip, fotosPath);
            }
            
            // 3. Restaurar documentos
            var docsZip = Path.Combine(backupFolder, "Documentos.zip");
            if (File.Exists(docsZip))
            {
                var docsPath = Path.Combine(_storagePath, "Documentos");
                if (Directory.Exists(docsPath))
                    Directory.Delete(docsPath, recursive: true);
                
                System.IO.Compression.ZipFile.ExtractToDirectory(docsZip, docsPath);
            }
            
            _logger.LogInformation("Backup restaurado: {BackupFolder}", backupFolder);
            return Result.Ok("Backup restaurado exitosamente. Reinicie la aplicación.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restaurando backup");
            return Result.Fail($"Error: {ex.Message}");
        }
    }
    
    public async Task<Result<IEnumerable<BackupInfo>>> GetAvailableBackupsAsync()
    {
        try
        {
            if (!Directory.Exists(_backupPath))
                return Result<IEnumerable<BackupInfo>>.Ok(Enumerable.Empty<BackupInfo>());
            
            var backups = new List<BackupInfo>();
            
            foreach (var dir in Directory.GetDirectories(_backupPath).OrderByDescending(d => d))
            {
                var metadataPath = Path.Combine(dir, "backup.json");
                var info = new BackupInfo
                {
                    FileName = Path.GetFileName(dir),
                    CreatedAt = Directory.GetCreationTime(dir)
                };
                
                // Calcular tamaño total
                info.SizeBytes = Directory.GetFiles(dir, "*", SearchOption.AllDirectories)
                    .Sum(f => new FileInfo(f).Length);
                
                // Leer metadatos si existen
                if (File.Exists(metadataPath))
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(metadataPath);
                        var metadata = System.Text.Json.JsonSerializer.Deserialize<dynamic>(json);
                        info.Description = metadata?.Description?.ToString();
                        info.IncludesFiles = metadata?.IncludesFiles ?? false;
                    }
                    catch { }
                }
                
                backups.Add(info);
            }
            
            return Result<IEnumerable<BackupInfo>>.Ok(backups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listando backups");
            return Result<IEnumerable<BackupInfo>>.Fail($"Error: {ex.Message}");
        }
    }
    
    public async Task<Result<int>> CleanupOldBackupsAsync(int keepDays = 30)
    {
        try
        {
            var cutoffDate = DateTime.Now.AddDays(-keepDays);
            var deleted = 0;
            
            foreach (var dir in Directory.GetDirectories(_backupPath))
            {
                var createdAt = Directory.GetCreationTime(dir);
                if (createdAt < cutoffDate)
                {
                    Directory.Delete(dir, recursive: true);
                    deleted++;
                }
            }
            
            _logger.LogInformation("Limpieza de backups: {Deleted} eliminados", deleted);
            return Result<int>.Ok(deleted, $"{deleted} backups antiguos eliminados");
        }
        catch (Exception ex)
        {
            return Result<int>.Fail($"Error: {ex.Message}");
        }
    }
}
```

---

### 2. Exportación a Excel

**Interfaz IExportService.cs:**

```csharp
public interface IExportService
{
    Task<Result<byte[]>> ExportToExcelAsync<T>(IEnumerable<T> data, string sheetName, 
        Dictionary<string, Func<T, object>>? columns = null);
    Task<Result<byte[]>> ExportEmpleadosToExcelAsync(ExportEmpleadosOptions? options = null);
    Task<Result<byte[]>> ExportPermisosToExcelAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<Result<byte[]>> ExportVacacionesToExcelAsync(int año);
}
```

**ExportService.cs (usando ClosedXML):**

```csharp
using ClosedXML.Excel;

public class ExportService : IExportService
{
    public async Task<Result<byte[]>> ExportToExcelAsync<T>(
        IEnumerable<T> data, 
        string sheetName,
        Dictionary<string, Func<T, object>>? columns = null)
    {
        try
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);
            
            var properties = typeof(T).GetProperties();
            var colIndex = 1;
            
            // Headers
            if (columns != null)
            {
                foreach (var col in columns.Keys)
                {
                    worksheet.Cell(1, colIndex).Value = col;
                    worksheet.Cell(1, colIndex).Style.Font.Bold = true;
                    worksheet.Cell(1, colIndex).Style.Fill.BackgroundColor = XLColor.LightGray;
                    colIndex++;
                }
            }
            else
            {
                foreach (var prop in properties)
                {
                    worksheet.Cell(1, colIndex).Value = prop.Name;
                    worksheet.Cell(1, colIndex).Style.Font.Bold = true;
                    colIndex++;
                }
            }
            
            // Data
            var rowIndex = 2;
            foreach (var item in data)
            {
                colIndex = 1;
                
                if (columns != null)
                {
                    foreach (var col in columns.Values)
                    {
                        var value = col(item);
                        worksheet.Cell(rowIndex, colIndex).Value = value?.ToString() ?? "";
                        colIndex++;
                    }
                }
                else
                {
                    foreach (var prop in properties)
                    {
                        var value = prop.GetValue(item);
                        worksheet.Cell(rowIndex, colIndex).Value = value?.ToString() ?? "";
                        colIndex++;
                    }
                }
                
                rowIndex++;
            }
            
            // Autofit columns
            worksheet.Columns().AdjustToContents();
            
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            
            return Result<byte[]>.Ok(stream.ToArray());
        }
        catch (Exception ex)
        {
            return Result<byte[]>.Fail($"Error exportando: {ex.Message}");
        }
    }
    
    public async Task<Result<byte[]>> ExportEmpleadosToExcelAsync(ExportEmpleadosOptions? options = null)
    {
        // Obtener empleados filtrados
        // Definir columnas específicas
        // Llamar a ExportToExcelAsync
        
        var columns = new Dictionary<string, Func<Empleado, object>>
        {
            { "Código", e => e.Codigo },
            { "Cédula", e => e.Cedula },
            { "Nombres", e => e.Nombres },
            { "Apellidos", e => e.Apellidos },
            { "Cargo", e => e.Cargo?.Nombre ?? "" },
            { "Departamento", e => e.Departamento?.Nombre ?? "" },
            { "Fecha Ingreso", e => e.FechaIngreso.ToString("dd/MM/yyyy") },
            { "Estado", e => e.Estado.ToString() },
            { "Teléfono", e => e.Telefono ?? "" },
            { "Email", e => e.Email ?? "" }
        };
        
        // ... implementar
    }
}
```

---

### 3. Servicio de Auditoría

**IAuditService.cs:**

```csharp
public interface IAuditService
{
    Task LogAsync(string accion, string entidad, int? entidadId, string descripcion, object? datos = null);
    Task<IEnumerable<AuditLog>> GetByEntidadAsync(string entidad, int entidadId);
    Task<IEnumerable<AuditLog>> GetRecentAsync(int count = 100);
    Task<IEnumerable<AuditLog>> SearchAsync(AuditSearchOptions options);
}

public class AuditSearchOptions
{
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int? UsuarioId { get; set; }
    public string? Entidad { get; set; }
    public string? Accion { get; set; }
}
```

**AuditService.cs:**

```csharp
public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _repository;
    private readonly IAuthService _authService;
    private readonly ILogger<AuditService> _logger;
    
    public async Task LogAsync(string accion, string entidad, int? entidadId, 
        string descripcion, object? datos = null)
    {
        try
        {
            var log = new AuditLog
            {
                FechaHora = DateTime.Now,
                UsuarioId = _authService.CurrentUser?.Id,
                UsuarioNombre = _authService.CurrentUser?.NombreCompleto ?? "Sistema",
                Accion = accion,
                Entidad = entidad,
                EntidadId = entidadId,
                Descripcion = descripcion,
                DatosAdicionales = datos != null 
                    ? System.Text.Json.JsonSerializer.Serialize(datos) 
                    : null
            };
            
            await _repository.CreateAsync(log);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error registrando auditoría");
        }
    }
    
    // ... otros métodos
}
```

---

### 4. Servicio de Configuración

**IConfigurationService.cs:**

```csharp
public interface IConfiguracionService
{
    Task<string?> GetAsync(string clave);
    Task<T?> GetAsync<T>(string clave);
    Task SetAsync(string clave, string valor);
    Task SetAsync<T>(string clave, T valor);
    Task<Dictionary<string, string>> GetAllAsync();
}
```

**Claves de configuración predefinidas:**

```csharp
public static class ConfigKeys
{
    // Empresa
    public const string EmpresaNombre = "empresa.nombre";
    public const string EmpresaNit = "empresa.nit";
    public const string EmpresaDireccion = "empresa.direccion";
    public const string EmpresaTelefono = "empresa.telefono";
    public const string EmpresaEmail = "empresa.email";
    public const string EmpresaRepresentante = "empresa.representante";
    
    // Sistema
    public const string SistemaVacacionesDiasAnuales = "sistema.vacaciones.dias_anuales"; // Default: 15
    public const string SistemaBackupAutomatico = "sistema.backup.automatico"; // true/false
    public const string SistemaBackupHora = "sistema.backup.hora"; // Default: "23:00"
    public const string SistemaBackupRetenerDias = "sistema.backup.retener_dias"; // Default: 30
    
    // Notificaciones
    public const string NotificarContratosVencer = "notificar.contratos.dias_antes"; // Default: 30
}
```

---

### 5. Timer de Backup Automático

En Program.cs:

```csharp
// Configurar backup automático
builder.Services.AddHostedService<BackupSchedulerService>();

public class BackupSchedulerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BackupSchedulerService> _logger;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            
            using var scope = _scopeFactory.CreateScope();
            var configService = scope.ServiceProvider.GetRequiredService<IConfiguracionService>();
            var backupService = scope.ServiceProvider.GetRequiredService<IBackupService>();
            
            var autoBackup = await configService.GetAsync<bool>(ConfigKeys.SistemaBackupAutomatico);
            if (!autoBackup) continue;
            
            var backupHora = await configService.GetAsync(ConfigKeys.SistemaBackupHora) ?? "23:00";
            var horaActual = DateTime.Now.ToString("HH:mm");
            
            if (horaActual == backupHora)
            {
                _logger.LogInformation("Ejecutando backup automático programado");
                await backupService.CreateScheduledBackupAsync();
                
                // Limpiar backups antiguos
                var retenerDias = await configService.GetAsync<int>(ConfigKeys.SistemaBackupRetenerDias);
                await backupService.CleanupOldBackupsAsync(retenerDias > 0 ? retenerDias : 30);
                
                // Esperar un minuto para no ejecutar dos veces
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
```

---

**IMPORTANTE:**
- Los backups deben incluir BD + archivos (fotos, documentos)
- Excel export debe manejar caracteres especiales correctamente
- Auditoría no debe bloquear operaciones principales (fire-and-forget)
- Configuraciones deben tener valores por defecto
```

---

## ✅ Checklist de Entregables

### Backup/Restore:
- [ ] Shared/Interfaces/IBackupService.cs
- [ ] Infrastructure/Services/BackupService.cs
- [ ] Server/Components/Pages/Configuracion.razor (sección backup)

### Exportación Excel:
- [ ] Shared/Interfaces/IExportService.cs
- [ ] Infrastructure/Services/ExportService.cs
- [ ] Botón "Exportar Excel" en páginas principales

### Auditoría:
- [ ] Shared/Interfaces/IAuditService.cs
- [ ] Infrastructure/Services/AuditService.cs
- [ ] Server/Components/Pages/Auditoria.razor (visor de logs)

### Configuración:
- [ ] Shared/Interfaces/IConfiguracionService.cs
- [ ] Shared/ConfigKeys.cs
- [ ] Infrastructure/Services/ConfiguracionService.cs
- [ ] Server/Components/Pages/Configuracion.razor

### Background Services:
- [ ] Infrastructure/Services/BackupSchedulerService.cs
- [ ] Registro en Program.cs

---

## 🔗 Dependencias NuGet

```xml
<PackageReference Include="ClosedXML" Version="0.102.2" />
<!-- Para compresión ZIP (ya incluido en .NET) -->
```
