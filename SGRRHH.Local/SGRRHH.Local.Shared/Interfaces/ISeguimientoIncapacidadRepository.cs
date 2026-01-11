using SGRRHH.Local.Domain.DTOs;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Shared.Interfaces;

/// <summary>
/// Repositorio para seguimiento de incapacidades
/// </summary>
public interface ISeguimientoIncapacidadRepository : IRepository<SeguimientoIncapacidad>
{
    /// <summary>Obtiene el historial de seguimiento de una incapacidad</summary>
    Task<IEnumerable<SeguimientoIncapacidad>> GetByIncapacidadIdAsync(int incapacidadId);
    
    /// <summary>Obtiene el historial como DTOs</summary>
    Task<IEnumerable<SeguimientoIncapacidadDto>> GetHistorialAsync(int incapacidadId);
    
    /// <summary>Registra una acción en el seguimiento</summary>
    Task RegistrarAccionAsync(
        int incapacidadId,
        TipoAccionSeguimientoIncapacidad tipoAccion,
        string descripcion,
        int usuarioId,
        string? datosAdicionales = null);
    
    /// <summary>Obtiene la última acción de un tipo específico</summary>
    Task<SeguimientoIncapacidad?> GetUltimaAccionAsync(int incapacidadId, TipoAccionSeguimientoIncapacidad tipoAccion);
}
