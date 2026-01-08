using System.Text.Json;
using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _repository;
    private readonly IAuthService _authService;
    private readonly ILogger<AuditService> _logger;
    
    public AuditService(
        IAuditLogRepository repository,
        IAuthService authService,
        ILogger<AuditService> logger)
    {
        _repository = repository;
        _authService = authService;
        _logger = logger;
    }
    
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
                    ? JsonSerializer.Serialize(datos, new JsonSerializerOptions 
                    { 
                        WriteIndented = false 
                    }) 
                    : null
            };
            
            await _repository.AddAsync(log);
            _logger.LogInformation("Auditoría registrada: {Accion} en {Entidad} #{EntidadId}", 
                accion, entidad, entidadId);
        }
        catch (Exception ex)
        {
            // No lanzar excepción para no bloquear la operación principal
            _logger.LogWarning(ex, "Error registrando auditoría: {Accion} en {Entidad}", 
                accion, entidad);
        }
    }
    
    public async Task<IEnumerable<AuditLog>> GetByEntidadAsync(string entidad, int entidadId)
    {
        try
        {
            var logs = await _repository.GetAllAsync();
            return logs.Where(l => l.Entidad == entidad && l.EntidadId == entidadId)
                .OrderByDescending(l => l.FechaHora);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo logs de auditoría para {Entidad} #{EntidadId}", 
                entidad, entidadId);
            return Enumerable.Empty<AuditLog>();
        }
    }
    
    public async Task<IEnumerable<AuditLog>> GetRecentAsync(int count = 100)
    {
        try
        {
            var logs = await _repository.GetAllAsync();
            return logs.OrderByDescending(l => l.FechaHora)
                .Take(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo logs recientes de auditoría");
            return Enumerable.Empty<AuditLog>();
        }
    }
    
    public async Task<IEnumerable<AuditLog>> SearchAsync(AuditSearchOptions options)
    {
        try
        {
            var logs = await _repository.GetAllAsync();
            var query = logs.AsEnumerable();
            
            // Aplicar filtros
            if (options.FechaInicio.HasValue)
            {
                query = query.Where(l => l.FechaHora >= options.FechaInicio.Value);
            }
            
            if (options.FechaFin.HasValue)
            {
                var fechaFinConHora = options.FechaFin.Value.Date.AddDays(1).AddSeconds(-1);
                query = query.Where(l => l.FechaHora <= fechaFinConHora);
            }
            
            if (options.UsuarioId.HasValue)
            {
                query = query.Where(l => l.UsuarioId == options.UsuarioId.Value);
            }
            
            if (!string.IsNullOrWhiteSpace(options.Entidad))
            {
                query = query.Where(l => l.Entidad.Contains(options.Entidad, 
                    StringComparison.OrdinalIgnoreCase));
            }
            
            if (!string.IsNullOrWhiteSpace(options.Accion))
            {
                query = query.Where(l => l.Accion.Contains(options.Accion, 
                    StringComparison.OrdinalIgnoreCase));
            }
            
            return query.OrderByDescending(l => l.FechaHora);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error buscando logs de auditoría");
            return Enumerable.Empty<AuditLog>();
        }
    }
}
