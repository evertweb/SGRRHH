using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Firebase.Repositories;

/// <summary>
/// Implementación del repositorio de Empleados para Firestore.
/// Colección: "empleados"
///
/// Campos desnormalizados para evitar múltiples queries:
/// - cargoNombre, departamentoNombre, supervisorNombre
///
/// Incluye cache en memoria para optimizar búsquedas.
/// </summary>
public class EmpleadoFirestoreRepository : FirestoreRepository<Empleado>, IEmpleadoRepository
{
    private const string COLLECTION_NAME = "empleados";
    private const string CODE_PREFIX = "EMP";
    private const string CACHE_KEY_ALL_ACTIVE = "empleados_all_active";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

    private readonly ICacheService? _cache;

    public EmpleadoFirestoreRepository(
        FirebaseInitializer firebase,
        ICacheService? cache = null,
        ILogger<EmpleadoFirestoreRepository>? logger = null)
        : base(firebase, COLLECTION_NAME, logger)
    {
        _cache = cache;
    }
    
    #region Entity <-> Document Mapping
    
    protected override Dictionary<string, object?> EntityToDocument(Empleado entity)
    {
        var doc = base.EntityToDocument(entity);
        
        // Datos básicos
        doc["codigo"] = entity.Codigo;
        doc["cedula"] = entity.Cedula;
        doc["nombres"] = entity.Nombres;
        doc["apellidos"] = entity.Apellidos;
        doc["nombreCompleto"] = entity.NombreCompleto;
        
        // Datos personales
        doc["fechaNacimiento"] = entity.FechaNacimiento.HasValue 
            ? Timestamp.FromDateTime(entity.FechaNacimiento.Value.ToUniversalTime()) 
            : null;
        doc["genero"] = entity.Genero?.ToString();
        doc["estadoCivil"] = entity.EstadoCivil?.ToString();
        doc["direccion"] = entity.Direccion;
        doc["telefono"] = entity.Telefono;
        doc["telefonoEmergencia"] = entity.TelefonoEmergencia;
        doc["contactoEmergencia"] = entity.ContactoEmergencia;
        doc["email"] = entity.Email;
        
        // Foto - URL de Firebase Storage
        doc["fotoUrl"] = entity.FotoPath;
        
        // Datos laborales
        doc["fechaIngreso"] = Timestamp.FromDateTime(entity.FechaIngreso.ToUniversalTime());
        doc["fechaRetiro"] = entity.FechaRetiro.HasValue 
            ? Timestamp.FromDateTime(entity.FechaRetiro.Value.ToUniversalTime()) 
            : null;
        doc["estado"] = entity.Estado.ToString();
        doc["tipoContrato"] = entity.TipoContrato.ToString();
        
        // Referencias con datos desnormalizados
        doc["cargoId"] = entity.CargoId;
        doc["cargoNombre"] = entity.Cargo?.Nombre;
        doc["departamentoId"] = entity.DepartamentoId;
        doc["departamentoNombre"] = entity.Departamento?.Nombre;
        doc["supervisorId"] = entity.SupervisorId;
        doc["supervisorNombre"] = entity.Supervisor?.NombreCompleto;
        
        // Observaciones
        doc["observaciones"] = entity.Observaciones;
        
        // Workflow de aprobación
        doc["creadoPorId"] = entity.CreadoPorId;
        doc["creadoPorNombre"] = entity.CreadoPor?.NombreCompleto;
        doc["fechaSolicitud"] = entity.FechaSolicitud.HasValue 
            ? Timestamp.FromDateTime(entity.FechaSolicitud.Value.ToUniversalTime()) 
            : null;
        doc["aprobadoPorId"] = entity.AprobadoPorId;
        doc["aprobadoPorNombre"] = entity.AprobadoPor?.NombreCompleto;
        doc["fechaAprobacion"] = entity.FechaAprobacion.HasValue 
            ? Timestamp.FromDateTime(entity.FechaAprobacion.Value.ToUniversalTime()) 
            : null;
        doc["motivoRechazo"] = entity.MotivoRechazo;
        
        return doc;
    }
    
    protected override Empleado DocumentToEntity(DocumentSnapshot document)
    {
        var entity = base.DocumentToEntity(document);
        
        // Datos básicos
        if (document.TryGetValue<string>("codigo", out var codigo))
            entity.Codigo = codigo ?? string.Empty;
        if (document.TryGetValue<string>("cedula", out var cedula))
            entity.Cedula = cedula ?? string.Empty;
        if (document.TryGetValue<string>("nombres", out var nombres))
            entity.Nombres = nombres ?? string.Empty;
        if (document.TryGetValue<string>("apellidos", out var apellidos))
            entity.Apellidos = apellidos ?? string.Empty;
        
        // Datos personales
        if (document.TryGetValue<Timestamp?>("fechaNacimiento", out var fechaNac) && fechaNac.HasValue)
            entity.FechaNacimiento = fechaNac.Value.ToDateTime().ToLocalTime();
        
        if (document.TryGetValue<string>("genero", out var generoStr) && !string.IsNullOrEmpty(generoStr))
            if (Enum.TryParse<Genero>(generoStr, out var genero))
                entity.Genero = genero;
        
        if (document.TryGetValue<string>("estadoCivil", out var estadoCivilStr) && !string.IsNullOrEmpty(estadoCivilStr))
            if (Enum.TryParse<EstadoCivil>(estadoCivilStr, out var estadoCivil))
                entity.EstadoCivil = estadoCivil;
        
        if (document.TryGetValue<string>("direccion", out var direccion))
            entity.Direccion = direccion;
        if (document.TryGetValue<string>("telefono", out var telefono))
            entity.Telefono = telefono;
        if (document.TryGetValue<string>("telefonoEmergencia", out var telEmergencia))
            entity.TelefonoEmergencia = telEmergencia;
        if (document.TryGetValue<string>("contactoEmergencia", out var contactoEmerg))
            entity.ContactoEmergencia = contactoEmerg;
        if (document.TryGetValue<string>("email", out var email))
            entity.Email = email;
        
        // Foto
        if (document.TryGetValue<string>("fotoUrl", out var fotoUrl))
            entity.FotoPath = fotoUrl;
        
        // Datos laborales
        if (document.TryGetValue<Timestamp>("fechaIngreso", out var fechaIngreso))
            entity.FechaIngreso = fechaIngreso.ToDateTime().ToLocalTime();
        
        if (document.TryGetValue<Timestamp?>("fechaRetiro", out var fechaRetiro) && fechaRetiro.HasValue)
            entity.FechaRetiro = fechaRetiro.Value.ToDateTime().ToLocalTime();
        
        if (document.TryGetValue<string>("estado", out var estadoStr) && !string.IsNullOrEmpty(estadoStr))
            if (Enum.TryParse<EstadoEmpleado>(estadoStr, out var estado))
                entity.Estado = estado;
        
        if (document.TryGetValue<string>("tipoContrato", out var tipoContratoStr) && !string.IsNullOrEmpty(tipoContratoStr))
            if (Enum.TryParse<TipoContrato>(tipoContratoStr, out var tipoContrato))
                entity.TipoContrato = tipoContrato;
        
        // Referencias - Cargo
        if (document.TryGetValue<int?>("cargoId", out var cargoId) && cargoId.HasValue)
        {
            entity.CargoId = cargoId;
            if (document.TryGetValue<string>("cargoNombre", out var cargoNombre))
            {
                entity.Cargo = new Cargo
                {
                    Id = cargoId.Value,
                    Nombre = cargoNombre ?? string.Empty
                };
            }
        }
        
        // Referencias - Departamento
        if (document.TryGetValue<int?>("departamentoId", out var depId) && depId.HasValue)
        {
            entity.DepartamentoId = depId;
            if (document.TryGetValue<string>("departamentoNombre", out var depNombre))
            {
                entity.Departamento = new Departamento
                {
                    Id = depId.Value,
                    Nombre = depNombre ?? string.Empty
                };
            }
        }
        
        // Referencias - Supervisor
        if (document.TryGetValue<int?>("supervisorId", out var supId) && supId.HasValue)
        {
            entity.SupervisorId = supId;
            if (document.TryGetValue<string>("supervisorNombre", out var supNombre))
            {
                entity.Supervisor = new Empleado
                {
                    Id = supId.Value,
                    Nombres = supNombre ?? string.Empty,
                    Apellidos = string.Empty
                };
            }
        }
        
        // Observaciones
        if (document.TryGetValue<string>("observaciones", out var obs))
            entity.Observaciones = obs;
        
        // Workflow - CreadoPor
        if (document.TryGetValue<int?>("creadoPorId", out var creadoPorId) && creadoPorId.HasValue)
        {
            entity.CreadoPorId = creadoPorId;
            if (document.TryGetValue<string>("creadoPorNombre", out var creadoPorNombre))
            {
                entity.CreadoPor = new Usuario
                {
                    Id = creadoPorId.Value,
                    NombreCompleto = creadoPorNombre ?? string.Empty
                };
            }
        }
        
        if (document.TryGetValue<Timestamp?>("fechaSolicitud", out var fechaSol) && fechaSol.HasValue)
            entity.FechaSolicitud = fechaSol.Value.ToDateTime().ToLocalTime();
        
        // Workflow - AprobadoPor
        if (document.TryGetValue<int?>("aprobadoPorId", out var aprobadoPorId) && aprobadoPorId.HasValue)
        {
            entity.AprobadoPorId = aprobadoPorId;
            if (document.TryGetValue<string>("aprobadoPorNombre", out var aprobadoPorNombre))
            {
                entity.AprobadoPor = new Usuario
                {
                    Id = aprobadoPorId.Value,
                    NombreCompleto = aprobadoPorNombre ?? string.Empty
                };
            }
        }
        
        if (document.TryGetValue<Timestamp?>("fechaAprobacion", out var fechaAprob) && fechaAprob.HasValue)
            entity.FechaAprobacion = fechaAprob.Value.ToDateTime().ToLocalTime();
        
        if (document.TryGetValue<string>("motivoRechazo", out var motivoRechazo))
            entity.MotivoRechazo = motivoRechazo;
        
        return entity;
    }
    
    #endregion
    
    #region IEmpleadoRepository Implementation
    
    /// <summary>
    /// Obtiene un empleado por su código
    /// </summary>
    public async Task<Empleado?> GetByCodigoAsync(string codigo)
    {
        try
        {
            var query = Collection.WhereEqualTo("codigo", codigo).Limit(1);
            var snapshot = await query.GetSnapshotAsync();
            
            var doc = snapshot.Documents.FirstOrDefault();
            return doc == null ? null : DocumentToEntity(doc);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener empleado por código: {Codigo}", codigo);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene un empleado por su cédula
    /// </summary>
    public async Task<Empleado?> GetByCedulaAsync(string cedula)
    {
        try
        {
            var query = Collection.WhereEqualTo("cedula", cedula).Limit(1);
            var snapshot = await query.GetSnapshotAsync();
            
            var doc = snapshot.Documents.FirstOrDefault();
            return doc == null ? null : DocumentToEntity(doc);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener empleado por cédula: {Cedula}", cedula);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene un empleado con todas sus relaciones cargadas
    /// Los datos desnormalizados ya vienen en el documento
    /// </summary>
    public async Task<Empleado?> GetByIdWithRelationsAsync(int id)
    {
        // En Firestore los datos desnormalizados ya están en el documento
        return await GetByIdAsync(id);
    }
    
    /// <summary>
    /// Obtiene todos los empleados con sus relaciones
    /// </summary>
    public async Task<IEnumerable<Empleado>> GetAllWithRelationsAsync()
    {
        try
        {
            var snapshot = await Collection.GetSnapshotAsync();
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderBy(e => e.Apellidos)
                .ThenBy(e => e.Nombres)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener todos los empleados con relaciones");
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene empleados activos con sus relaciones
    /// </summary>
    public async Task<IEnumerable<Empleado>> GetAllActiveWithRelationsAsync()
    {
        try
        {
            var query = Collection
                .WhereEqualTo("activo", true)
                .WhereEqualTo("estado", EstadoEmpleado.Activo.ToString());
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderBy(e => e.Apellidos)
                .ThenBy(e => e.Nombres)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener empleados activos con relaciones");
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene empleados por departamento
    /// </summary>
    public async Task<IEnumerable<Empleado>> GetByDepartamentoAsync(int departamentoId)
    {
        try
        {
            var query = Collection
                .WhereEqualTo("departamentoId", departamentoId)
                .WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderBy(e => e.Apellidos)
                .ThenBy(e => e.Nombres)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener empleados del departamento {DepartamentoId}", departamentoId);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene empleados por cargo
    /// </summary>
    public async Task<IEnumerable<Empleado>> GetByCargoAsync(int cargoId)
    {
        try
        {
            var query = Collection
                .WhereEqualTo("cargoId", cargoId)
                .WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderBy(e => e.Apellidos)
                .ThenBy(e => e.Nombres)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener empleados del cargo {CargoId}", cargoId);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene empleados por estado
    /// </summary>
    public async Task<IEnumerable<Empleado>> GetByEstadoAsync(EstadoEmpleado estado)
    {
        try
        {
            var query = Collection
                .WhereEqualTo("estado", estado.ToString())
                .WhereEqualTo("activo", true);
            var snapshot = await query.GetSnapshotAsync();
            
            return snapshot.Documents
                .Select(DocumentToEntity)
                .OrderBy(e => e.Apellidos)
                .ThenBy(e => e.Nombres)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener empleados con estado {Estado}", estado);
            throw;
        }
    }
    
    /// <summary>
    /// Busca empleados por nombre, apellido o cédula
    /// Nota: Firestore no soporta búsqueda de texto completo, se usa búsqueda por prefijo
    /// </summary>
    public async Task<IEnumerable<Empleado>> SearchAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllActiveAsync();
            
            searchTerm = searchTerm.ToLower().Trim();
            
            // En Firestore la búsqueda de texto completo no es nativa
            // Traemos todos los activos y filtramos en memoria
            var all = await GetAllActiveAsync();
            
            return all.Where(e =>
                e.Nombres.ToLower().Contains(searchTerm) ||
                e.Apellidos.ToLower().Contains(searchTerm) ||
                e.NombreCompleto.ToLower().Contains(searchTerm) ||
                e.Cedula.ToLower().Contains(searchTerm) ||
                e.Codigo.ToLower().Contains(searchTerm)
            ).ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al buscar empleados con término: {SearchTerm}", searchTerm);
            throw;
        }
    }
    
    /// <summary>
    /// Verifica si existe un empleado con el código dado
    /// </summary>
    public async Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null)
    {
        try
        {
            var query = Collection.WhereEqualTo("codigo", codigo);
            var snapshot = await query.GetSnapshotAsync();
            
            if (!excludeId.HasValue)
                return snapshot.Documents.Any();
            
            return snapshot.Documents.Any(doc =>
                doc.TryGetValue<int>("id", out var id) && id != excludeId.Value);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al verificar código existente: {Codigo}", codigo);
            throw;
        }
    }
    
    /// <summary>
    /// Verifica si existe un empleado con la cédula dada
    /// </summary>
    public async Task<bool> ExistsCedulaAsync(string cedula, int? excludeId = null)
    {
        try
        {
            var query = Collection.WhereEqualTo("cedula", cedula);
            var snapshot = await query.GetSnapshotAsync();
            
            if (!excludeId.HasValue)
                return snapshot.Documents.Any();
            
            return snapshot.Documents.Any(doc =>
                doc.TryGetValue<int>("id", out var id) && id != excludeId.Value);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al verificar cédula existente: {Cedula}", cedula);
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene el siguiente código de empleado disponible
    /// </summary>
    public async Task<string> GetNextCodigoAsync()
    {
        return await GetNextCodigoAsync(CODE_PREFIX);
    }
    
    /// <summary>
    /// Cuenta el total de empleados activos
    /// </summary>
    public new async Task<int> CountActiveAsync()
    {
        try
        {
            var query = Collection
                .WhereEqualTo("activo", true)
                .WhereEqualTo("estado", EstadoEmpleado.Activo.ToString());
            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Count;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al contar empleados activos");
            throw;
        }
    }
    
    /// <summary>
    /// Obtiene todos los empleados activos ordenados.
    /// Usa cache en memoria con expiración de 5 minutos para optimizar búsquedas.
    /// </summary>
    public override async Task<IEnumerable<Empleado>> GetAllActiveAsync()
    {
        try
        {
            if (_cache != null)
            {
                return await _cache.GetOrCreateAsync(
                    CACHE_KEY_ALL_ACTIVE,
                    FetchAllActiveFromFirestoreAsync,
                    CacheExpiration);
            }

            return await FetchAllActiveFromFirestoreAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al obtener empleados activos");
            throw;
        }
    }

    private async Task<IEnumerable<Empleado>> FetchAllActiveFromFirestoreAsync()
    {
        var query = Collection.WhereEqualTo("activo", true);
        var snapshot = await query.GetSnapshotAsync();

        return snapshot.Documents
            .Select(DocumentToEntity)
            .OrderBy(e => e.Apellidos)
            .ThenBy(e => e.Nombres)
            .ToList();
    }

    /// <summary>
    /// Invalida el cache de empleados. Llamar después de Add/Update/Delete.
    /// </summary>
    public void InvalidateCache()
    {
        _cache?.InvalidateByPrefix("empleados");
    }
    
    #endregion
    
    #region Helper Methods - Actualizar datos desnormalizados
    
    /// <summary>
    /// Actualiza el nombre del departamento en todos los empleados que lo tienen asignado.
    /// Llamar cuando se cambie el nombre de un departamento.
    /// </summary>
    public async Task ActualizarDepartamentoNombreAsync(int departamentoId, string nuevoNombre)
    {
        try
        {
            var query = Collection.WhereEqualTo("departamentoId", departamentoId);
            var snapshot = await query.GetSnapshotAsync();
            
            var batch = CreateBatch();
            foreach (var doc in snapshot.Documents)
            {
                batch.Update(doc.Reference, new Dictionary<string, object>
                {
                    ["departamentoNombre"] = nuevoNombre,
                    ["fechaModificacion"] = Timestamp.FromDateTime(DateTime.UtcNow)
                });
            }
            
            await batch.CommitAsync();
            _logger?.LogInformation("Actualizado departamentoNombre en {Count} empleados", snapshot.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al actualizar departamentoNombre para {DepartamentoId}", departamentoId);
            throw;
        }
    }
    
    /// <summary>
    /// Actualiza el nombre del cargo en todos los empleados que lo tienen asignado.
    /// Llamar cuando se cambie el nombre de un cargo.
    /// </summary>
    public async Task ActualizarCargoNombreAsync(int cargoId, string nuevoNombre)
    {
        try
        {
            var query = Collection.WhereEqualTo("cargoId", cargoId);
            var snapshot = await query.GetSnapshotAsync();
            
            var batch = CreateBatch();
            foreach (var doc in snapshot.Documents)
            {
                batch.Update(doc.Reference, new Dictionary<string, object>
                {
                    ["cargoNombre"] = nuevoNombre,
                    ["fechaModificacion"] = Timestamp.FromDateTime(DateTime.UtcNow)
                });
            }
            
            await batch.CommitAsync();
            _logger?.LogInformation("Actualizado cargoNombre en {Count} empleados", snapshot.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al actualizar cargoNombre para {CargoId}", cargoId);
            throw;
        }
    }
    
    /// <summary>
    /// Actualiza el nombre del supervisor en todos los empleados que lo tienen asignado.
    /// Llamar cuando se cambie el nombre de un empleado que es supervisor.
    /// </summary>
    public async Task ActualizarSupervisorNombreAsync(int supervisorId, string nuevoNombreCompleto)
    {
        try
        {
            var query = Collection.WhereEqualTo("supervisorId", supervisorId);
            var snapshot = await query.GetSnapshotAsync();
            
            var batch = CreateBatch();
            foreach (var doc in snapshot.Documents)
            {
                batch.Update(doc.Reference, new Dictionary<string, object>
                {
                    ["supervisorNombre"] = nuevoNombreCompleto,
                    ["fechaModificacion"] = Timestamp.FromDateTime(DateTime.UtcNow)
                });
            }
            
            await batch.CommitAsync();
            _logger?.LogInformation("Actualizado supervisorNombre en {Count} empleados", snapshot.Count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al actualizar supervisorNombre para {SupervisorId}", supervisorId);
            throw;
        }
    }
    
    #endregion
}
