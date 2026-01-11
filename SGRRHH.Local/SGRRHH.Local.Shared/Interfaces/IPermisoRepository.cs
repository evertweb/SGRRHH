using SGRRHH.Local.Domain.DTOs;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;

namespace SGRRHH.Local.Shared.Interfaces;

public interface IPermisoRepository : IRepository<Permiso>
{
    Task<IEnumerable<Permiso>> GetPendientesAsync();
    
    Task<IEnumerable<Permiso>> GetByEmpleadoIdAsync(int empleadoId);
    
    Task<IEnumerable<Permiso>> GetByRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin);
    
    Task<IEnumerable<Permiso>> GetByEstadoAsync(EstadoPermiso estado);
    
    Task<string> GetProximoNumeroActaAsync();
    
    Task<bool> ExisteSolapamientoAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin, int? excludePermisoId = null);
    
    Task<IEnumerable<Permiso>> GetAllWithFiltersAsync(int? empleadoId = null, EstadoPermiso? estado = null, DateTime? fechaDesde = null, DateTime? fechaHasta = null);
    
    // ===== NUEVOS MÉTODOS PARA SEGUIMIENTO =====
    
    /// <summary>Obtiene permisos aprobados pendientes de documento</summary>
    Task<IEnumerable<Permiso>> GetPendientesDocumentoAsync();
    
    /// <summary>Obtiene permisos en proceso de compensación</summary>
    Task<IEnumerable<Permiso>> GetEnCompensacionAsync();
    
    /// <summary>Obtiene permisos para descuento en nómina por período</summary>
    Task<IEnumerable<Permiso>> GetParaDescuentoNominaAsync(string periodo);
    
    /// <summary>Obtiene permisos con documento vencido</summary>
    Task<IEnumerable<Permiso>> GetConDocumentoVencidoAsync();
    
    /// <summary>Obtiene permisos con compensación vencida</summary>
    Task<IEnumerable<Permiso>> GetConCompensacionVencidaAsync();
    
    /// <summary>Actualiza la resolución de un permiso</summary>
    Task ActualizarResolucionAsync(int permisoId, TipoResolucionPermiso resolucion);
    
    /// <summary>Registra entrega de documento</summary>
    Task RegistrarEntregaDocumentoAsync(int permisoId, string rutaDocumento, int usuarioId);
    
    /// <summary>Obtiene estadísticas de seguimiento para el dashboard</summary>
    Task<EstadisticasSeguimiento> GetEstadisticasSeguimientoAsync();
    
    /// <summary>Obtiene permisos con información completa para seguimiento</summary>
    Task<IEnumerable<PermisoConSeguimiento>> GetPermisosParaSeguimientoAsync(
        EstadoPermiso? estado = null, 
        TipoResolucionPermiso? tipoResolucion = null,
        bool? soloVencidos = null,
        bool? soloPendientesDocumento = null,
        bool? soloEnCompensacion = null);
    
    /// <summary>Marca un permiso como completado</summary>
    Task MarcarCompletadoAsync(int permisoId);
    
    /// <summary>Actualiza las horas compensadas de un permiso</summary>
    Task ActualizarHorasCompensadasAsync(int permisoId, int horasCompensadas);
}


