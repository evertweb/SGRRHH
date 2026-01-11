using Microsoft.Extensions.Logging;
using SGRRHH.Local.Domain.DTOs;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Services;

/// <summary>
/// Servicio para gestión de alertas de permisos
/// </summary>
public class AlertaPermisoService : IAlertaPermisoService
{
    private readonly IPermisoRepository _permisoRepository;
    private readonly ISeguimientoPermisoRepository _seguimientoRepository;
    private readonly ILogger<AlertaPermisoService> _logger;
    private const int DiasAlertaAnticipada = 3;

    public AlertaPermisoService(
        IPermisoRepository permisoRepository,
        ISeguimientoPermisoRepository seguimientoRepository,
        ILogger<AlertaPermisoService> logger)
    {
        _permisoRepository = permisoRepository;
        _seguimientoRepository = seguimientoRepository;
        _logger = logger;
    }

    public async Task<List<AlertaPermiso>> GetAlertasActivasAsync()
    {
        var alertas = new List<AlertaPermiso>();
        var hoy = DateTime.Now.Date;

        // 1. Documentos vencidos
        var docVencidos = await _permisoRepository.GetConDocumentoVencidoAsync();
        foreach (var p in docVencidos)
        {
            alertas.Add(new AlertaPermiso
            {
                PermisoId = p.Id,
                NumeroActa = p.NumeroActa,
                TipoAlerta = TipoAlertaPermiso.DocumentoVencido,
                Mensaje = $"Documento vencido desde {p.FechaLimiteDocumento:dd/MM/yyyy}",
                FechaAlerta = DateTime.Now,
                FechaLimite = p.FechaLimiteDocumento,
                DiasRestantes = p.FechaLimiteDocumento.HasValue 
                    ? (int)(p.FechaLimiteDocumento.Value.Date - hoy).TotalDays 
                    : null,
                EsUrgente = true
            });
        }

        // 2. Compensaciones vencidas
        var compVencidas = await _permisoRepository.GetConCompensacionVencidaAsync();
        foreach (var p in compVencidas)
        {
            var horasFaltantes = (p.HorasCompensar ?? 0) - p.HorasCompensadas;
            alertas.Add(new AlertaPermiso
            {
                PermisoId = p.Id,
                NumeroActa = p.NumeroActa,
                TipoAlerta = TipoAlertaPermiso.CompensacionVencida,
                Mensaje = $"Compensación vencida. Faltan {horasFaltantes} horas por compensar",
                FechaAlerta = DateTime.Now,
                FechaLimite = p.FechaLimiteCompensacion,
                DiasRestantes = p.FechaLimiteCompensacion.HasValue 
                    ? (int)(p.FechaLimiteCompensacion.Value.Date - hoy).TotalDays 
                    : null,
                EsUrgente = true
            });
        }

        // 3. Documentos por vencer (próximos 3 días)
        var pendientesDoc = await _permisoRepository.GetPendientesDocumentoAsync();
        foreach (var p in pendientesDoc.Where(x => x.FechaLimiteDocumento.HasValue))
        {
            var diasRestantes = (int)(p.FechaLimiteDocumento!.Value.Date - hoy).TotalDays;
            if (diasRestantes >= 0 && diasRestantes <= DiasAlertaAnticipada)
            {
                alertas.Add(new AlertaPermiso
                {
                    PermisoId = p.Id,
                    NumeroActa = p.NumeroActa,
                    TipoAlerta = TipoAlertaPermiso.DocumentoPorVencer,
                    Mensaje = diasRestantes == 0 
                        ? "¡Documento vence HOY!" 
                        : $"Documento vence en {diasRestantes} día(s)",
                    FechaAlerta = DateTime.Now,
                    FechaLimite = p.FechaLimiteDocumento,
                    DiasRestantes = diasRestantes,
                    EsUrgente = diasRestantes <= 1
                });
            }
        }

        // 4. Compensaciones por vencer (próximos 3 días)
        var enCompensacion = await _permisoRepository.GetEnCompensacionAsync();
        foreach (var p in enCompensacion.Where(x => x.FechaLimiteCompensacion.HasValue))
        {
            var diasRestantes = (int)(p.FechaLimiteCompensacion!.Value.Date - hoy).TotalDays;
            if (diasRestantes >= 0 && diasRestantes <= DiasAlertaAnticipada)
            {
                var horasFaltantes = (p.HorasCompensar ?? 0) - p.HorasCompensadas;
                alertas.Add(new AlertaPermiso
                {
                    PermisoId = p.Id,
                    NumeroActa = p.NumeroActa,
                    TipoAlerta = TipoAlertaPermiso.CompensacionPorVencer,
                    Mensaje = diasRestantes == 0 
                        ? $"¡Compensación vence HOY! Faltan {horasFaltantes}h" 
                        : $"Compensación vence en {diasRestantes} día(s). Faltan {horasFaltantes}h",
                    FechaAlerta = DateTime.Now,
                    FechaLimite = p.FechaLimiteCompensacion,
                    DiasRestantes = diasRestantes,
                    EsUrgente = diasRestantes <= 1
                });
            }
        }

        // 5. Permisos pendientes de aprobación por más de 3 días
        var pendientes = await _permisoRepository.GetPendientesAsync();
        foreach (var p in pendientes)
        {
            var diasPendiente = (int)(hoy - p.FechaSolicitud.Date).TotalDays;
            if (diasPendiente >= DiasAlertaAnticipada)
            {
                alertas.Add(new AlertaPermiso
                {
                    PermisoId = p.Id,
                    NumeroActa = p.NumeroActa,
                    TipoAlerta = TipoAlertaPermiso.AprobacionPendiente,
                    Mensaje = $"Permiso pendiente de aprobación hace {diasPendiente} días",
                    FechaAlerta = DateTime.Now,
                    DiasRestantes = diasPendiente,
                    EsUrgente = diasPendiente >= 5
                });
            }
        }

        // Ordenar: urgentes primero, luego por fecha límite
        return alertas
            .OrderByDescending(a => a.EsUrgente)
            .ThenBy(a => a.FechaLimite ?? DateTime.MaxValue)
            .ToList();
    }

    public async Task<List<AlertaPermiso>> GetAlertasPorUsuarioAsync(int usuarioId)
    {
        // Por ahora retorna todas las alertas
        // En el futuro se puede filtrar por permisos del departamento del usuario
        return await GetAlertasActivasAsync();
    }

    public async Task<Dictionary<TipoAlertaPermiso, int>> GetConteoAlertasAsync()
    {
        var alertas = await GetAlertasActivasAsync();
        return alertas
            .GroupBy(a => a.TipoAlerta)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task ProcesarPermisosVencidosAsync()
    {
        _logger.LogInformation("Iniciando procesamiento de permisos vencidos...");
        
        // Procesar documentos vencidos
        var docVencidos = await _permisoRepository.GetConDocumentoVencidoAsync();
        foreach (var permiso in docVencidos)
        {
            // Registrar en seguimiento si no se ha registrado antes
            var seguimientos = await _seguimientoRepository.GetByPermisoIdAsync(permiso.Id);
            var yaRegistrado = seguimientos.Any(s => s.TipoAccion == TipoAccionSeguimiento.DocumentoVencido);
            
            if (!yaRegistrado)
            {
                await _seguimientoRepository.RegistrarAccionAsync(
                    permiso.Id,
                    TipoAccionSeguimiento.DocumentoVencido,
                    $"Documento venció el {permiso.FechaLimiteDocumento:dd/MM/yyyy} sin ser entregado",
                    1, // Usuario sistema
                    null);
                
                _logger.LogWarning("Documento vencido registrado para permiso {NumeroActa}", permiso.NumeroActa);
            }
        }

        // Procesar compensaciones vencidas
        var compVencidas = await _permisoRepository.GetConCompensacionVencidaAsync();
        foreach (var permiso in compVencidas)
        {
            var seguimientos = await _seguimientoRepository.GetByPermisoIdAsync(permiso.Id);
            var yaRegistrado = seguimientos.Any(s => s.TipoAccion == TipoAccionSeguimiento.CompensacionVencida);
            
            if (!yaRegistrado)
            {
                var horasFaltantes = (permiso.HorasCompensar ?? 0) - permiso.HorasCompensadas;
                await _seguimientoRepository.RegistrarAccionAsync(
                    permiso.Id,
                    TipoAccionSeguimiento.CompensacionVencida,
                    $"Compensación venció el {permiso.FechaLimiteCompensacion:dd/MM/yyyy}. Faltan {horasFaltantes} horas",
                    1, // Usuario sistema
                    null);
                
                _logger.LogWarning("Compensación vencida registrada para permiso {NumeroActa}", permiso.NumeroActa);
            }
        }

        _logger.LogInformation("Procesamiento de permisos vencidos completado");
    }

    public async Task<List<AlertaPermiso>> GetAlertasProximasAVencerAsync(int dias = 3)
    {
        var alertas = await GetAlertasActivasAsync();
        return alertas
            .Where(a => a.TipoAlerta == TipoAlertaPermiso.DocumentoPorVencer 
                     || a.TipoAlerta == TipoAlertaPermiso.CompensacionPorVencer)
            .Where(a => a.DiasRestantes.HasValue && a.DiasRestantes.Value <= dias && a.DiasRestantes.Value >= 0)
            .ToList();
    }
}
