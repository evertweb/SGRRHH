using SGRRHH.Local.Domain.DTOs;

namespace SGRRHH.Local.Shared.Interfaces;

/// <summary>
/// Servicio para gestión de alertas de permisos
/// </summary>
public interface IAlertaPermisoService
{
    /// <summary>Obtiene todas las alertas activas</summary>
    Task<List<AlertaPermiso>> GetAlertasActivasAsync();
    
    /// <summary>Obtiene alertas para un usuario específico (según su rol)</summary>
    Task<List<AlertaPermiso>> GetAlertasPorUsuarioAsync(int usuarioId);
    
    /// <summary>Obtiene el conteo de alertas por tipo</summary>
    Task<Dictionary<TipoAlertaPermiso, int>> GetConteoAlertasAsync();
    
    /// <summary>Verifica y actualiza permisos vencidos</summary>
    Task ProcesarPermisosVencidosAsync();
    
    /// <summary>Obtiene permisos que vencen en los próximos N días</summary>
    Task<List<AlertaPermiso>> GetAlertasProximasAVencerAsync(int dias = 3);
}
