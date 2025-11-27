using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de auditoría
/// </summary>
public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;
    
    // Función para obtener el usuario actual (se configura desde la capa de presentación)
    private static Func<Usuario?>? _getCurrentUser;
    
    public static void ConfigureCurrentUserProvider(Func<Usuario?> getCurrentUser)
    {
        _getCurrentUser = getCurrentUser;
    }
    
    public AuditService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }
    
    public async Task RegistrarAsync(string accion, string entidad, int? entidadId, string descripcion, string? datosAdicionales = null)
    {
        // Obtener usuario actual si está disponible
        var usuario = _getCurrentUser?.Invoke();
        
        await RegistrarAsync(
            usuario?.Id ?? 0,
            usuario?.NombreCompleto ?? "Sistema",
            accion,
            entidad,
            entidadId,
            descripcion,
            datosAdicionales);
    }
    
    public async Task RegistrarAsync(int usuarioId, string usuarioNombre, string accion, string entidad, int? entidadId, string descripcion, string? datosAdicionales = null)
    {
        try
        {
            var auditLog = new AuditLog
            {
                FechaHora = DateTime.Now,
                UsuarioId = usuarioId > 0 ? usuarioId : null,
                UsuarioNombre = usuarioNombre,
                Accion = accion,
                Entidad = entidad,
                EntidadId = entidadId,
                Descripcion = descripcion,
                DatosAdicionales = datosAdicionales,
                FechaCreacion = DateTime.Now
            };
            
            await _auditLogRepository.AddAsync(auditLog);
            await _auditLogRepository.SaveChangesAsync();
        }
        catch
        {
            // No lanzar excepciones por errores de auditoría para no interrumpir operaciones
        }
    }
    
    public async Task<List<AuditLog>> ObtenerRegistrosAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null, string? entidad = null, int? usuarioId = null, int maxRegistros = 100)
    {
        return await _auditLogRepository.GetFilteredAsync(fechaDesde, fechaHasta, entidad, usuarioId, maxRegistros);
    }
    
    public async Task<List<AuditLog>> ObtenerRegistrosPorEntidadAsync(string entidad, int entidadId)
    {
        return await _auditLogRepository.GetByEntidadAsync(entidad, entidadId);
    }
    
    public async Task<int> LimpiarRegistrosAntiguosAsync(int diasAntiguedad)
    {
        var fechaLimite = DateTime.Now.AddDays(-diasAntiguedad);
        var registrosEliminados = await _auditLogRepository.DeleteOlderThanAsync(fechaLimite);
        await _auditLogRepository.SaveChangesAsync();
        return registrosEliminados;
    }
}
