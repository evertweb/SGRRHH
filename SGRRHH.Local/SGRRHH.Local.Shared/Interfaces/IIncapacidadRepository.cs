using SGRRHH.Local.Domain.DTOs;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Shared.Interfaces;

/// <summary>
/// Repositorio para gestión de incapacidades
/// </summary>
public interface IIncapacidadRepository : IRepository<Incapacidad>
{
    // ===== CONSULTAS BÁSICAS =====
    
    /// <summary>Obtiene una incapacidad por su número único</summary>
    Task<Incapacidad?> GetByNumeroAsync(string numeroIncapacidad);
    
    /// <summary>Obtiene todas las incapacidades de un empleado</summary>
    Task<IEnumerable<Incapacidad>> GetByEmpleadoIdAsync(int empleadoId);
    
    /// <summary>Obtiene incapacidades por estado</summary>
    Task<IEnumerable<Incapacidad>> GetByEstadoAsync(EstadoIncapacidad estado);
    
    /// <summary>Obtiene incapacidades por tipo</summary>
    Task<IEnumerable<Incapacidad>> GetByTipoAsync(TipoIncapacidad tipo);
    
    /// <summary>Obtiene incapacidades en un rango de fechas</summary>
    Task<IEnumerable<Incapacidad>> GetByRangoFechasAsync(DateTime desde, DateTime hasta);
    
    /// <summary>Obtiene incapacidades con filtros combinados</summary>
    Task<IEnumerable<IncapacidadResumen>> GetAllWithFiltersAsync(
        int? empleadoId = null,
        EstadoIncapacidad? estado = null,
        TipoIncapacidad? tipo = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        bool? soloActivas = null,
        bool? soloPendientesTranscripcion = null,
        bool? soloPendientesCobro = null);
    
    // ===== CONSULTAS PARA GESTIÓN =====
    
    /// <summary>Obtiene incapacidades activas</summary>
    Task<IEnumerable<IncapacidadResumen>> GetActivasAsync();
    
    /// <summary>Obtiene incapacidades pendientes de transcripción</summary>
    Task<IEnumerable<IncapacidadResumen>> GetPendientesTranscripcionAsync();
    
    /// <summary>Obtiene incapacidades pendientes de cobro (ya transcritas)</summary>
    Task<IEnumerable<IncapacidadResumen>> GetPendientesCobroAsync();
    
    /// <summary>Obtiene incapacidades próximas a vencer</summary>
    Task<IEnumerable<IncapacidadResumen>> GetProximasVencerAsync(int dias = 3);
    
    // ===== PRÓRROGAS =====
    
    /// <summary>Obtiene las prórrogas de una incapacidad</summary>
    Task<IEnumerable<Incapacidad>> GetProrrogasAsync(int incapacidadOrigenId);
    
    /// <summary>Obtiene el total de días acumulados incluyendo prórrogas</summary>
    Task<int> GetTotalDiasAcumuladosAsync(int incapacidadOrigenId);
    
    // ===== ESTADÍSTICAS =====
    
    /// <summary>Obtiene estadísticas generales de incapacidades</summary>
    Task<EstadisticasIncapacidades> GetEstadisticasAsync();
    
    /// <summary>Obtiene el total pendiente por cobrar</summary>
    Task<decimal> GetTotalPorCobrarAsync();
    
    /// <summary>Obtiene el total de días de incapacidad en un mes</summary>
    Task<int> GetTotalDiasIncapacidadMesAsync(int año, int mes);
    
    // ===== OPERACIONES ESPECIALES =====
    
    /// <summary>Genera el próximo número de incapacidad</summary>
    Task<string> GenerarNumeroIncapacidadAsync();
    
    /// <summary>Crea una incapacidad desde un permiso existente</summary>
    Task<Incapacidad> CrearDesdePermisoAsync(int permisoId, CrearIncapacidadDto dto, int usuarioId);
    
    /// <summary>Crea una prórroga vinculada a una incapacidad anterior</summary>
    Task<Incapacidad> CrearProrrogaAsync(int incapacidadAnteriorId, CrearProrrogaDto dto, int usuarioId);
    
    /// <summary>Registra la transcripción de una incapacidad ante EPS/ARL</summary>
    Task RegistrarTranscripcionAsync(RegistrarTranscripcionDto dto, int usuarioId);
    
    /// <summary>Registra el cobro de una incapacidad</summary>
    Task RegistrarCobroAsync(RegistrarCobroDto dto, int usuarioId);
    
    /// <summary>Finaliza una incapacidad</summary>
    Task FinalizarAsync(int incapacidadId, int usuarioId, string? observaciones = null);
    
    /// <summary>Cancela una incapacidad</summary>
    Task CancelarAsync(int incapacidadId, int usuarioId, string motivo);
    
    // ===== DETALLE =====
    
    /// <summary>Obtiene el detalle completo de una incapacidad</summary>
    Task<IncapacidadDetalleDto?> GetDetalleAsync(int incapacidadId);
    
    // ===== REPORTES =====
    
    /// <summary>Obtiene incapacidades para el reporte de cobro</summary>
    Task<IEnumerable<Incapacidad>> GetParaReporteCobroAsync(int año, int mes);
    
    /// <summary>Genera el DTO del reporte de cobro a EPS</summary>
    Task<ReporteCobroEpsDto> GetReporteCobroAsync(int año, int mes);
}
