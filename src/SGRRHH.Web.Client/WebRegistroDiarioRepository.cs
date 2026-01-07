using SGRRHH.Core.Entities;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Web.Client;

public class WebRegistroDiarioRepository : WebFirestoreRepository<RegistroDiario>, IRegistroDiarioRepository
{
    public WebRegistroDiarioRepository(FirebaseJsInterop firebase) 
        : base(firebase, "registros-diarios")
    {
    }
    
    public async Task<RegistroDiario?> GetByFechaEmpleadoAsync(DateTime fecha, int empleadoId)
    {
        var filters = new List<FirestoreFilter>
        {
            new("empleadoId", "==", empleadoId),
            // Timestamp comparison in Firestore requires careful handling, but equality usually works if stored exactly.
            // However, typically we queried by range or stored YYYY-MM-DD string.
            // Assuming Date is stored as Timestamp, exact match might be hard if time component differs.
            // Let's assume we can filter by the stored property "fecha".
            // Since MapToEntity handles JSON, we rely on how it was saved. 
            // If saved as string or midnight timestamp, this works.
            // For now, let's use the composite query assuming exact match on serialized value. 
            // If this fails, we might need a range query for the day.
             new("fecha", "==", fecha) 
        };

        var results = await _firebase.QueryCollectionCompositeAsync<object>(_collectionName, filters, limit: 1);
        return results.Select(MapToEntity).FirstOrDefault();
    }
    
    public async Task<RegistroDiario?> GetByIdWithDetallesAsync(int id)
    {
        return await GetByIdAsync(id);
    }
    
    public async Task<IEnumerable<RegistroDiario>> GetByEmpleadoRangoFechasAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin)
    {
         var filters = new List<FirestoreFilter>
        {
            new("empleadoId", "==", empleadoId),
            new("fecha", ">=", fechaInicio),
            new("fecha", "<=", fechaFin)
        };

        var results = await _firebase.QueryCollectionCompositeAsync<object>(
            _collectionName, filters, orderByField: "fecha", direction: "desc");
            
        return results.Select(MapToEntity).ToList();
    }
    
    public async Task<IEnumerable<RegistroDiario>> GetByFechaAsync(DateTime fecha)
    {
        // Try strict equality first
        var results = await _firebase.QueryCollectionAsync<object>(_collectionName, "fecha", "==", fecha);
        return results.Select(MapToEntity).ToList();
    }
    
    public async Task<IEnumerable<RegistroDiario>> GetByRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        var filters = new List<FirestoreFilter>
        {
            new("fecha", ">=", fechaInicio),
            new("fecha", "<=", fechaFin)
        };
        
        var results = await _firebase.QueryCollectionCompositeAsync<object>(
            _collectionName, filters, orderByField: "fecha", direction: "desc");

        return results.Select(MapToEntity).ToList();
    }
    
    public async Task<IEnumerable<RegistroDiario>> GetByEmpleadoWithDetallesAsync(int empleadoId, int? cantidad = null)
    {
         var filters = new List<FirestoreFilter>
        {
            new("empleadoId", "==", empleadoId)
        };

        var results = await _firebase.QueryCollectionCompositeAsync<object>(
            _collectionName, filters, orderByField: "fecha", direction: "desc", limit: cantidad ?? 0);
            
        return results.Select(MapToEntity).ToList();
    }
    
    public async Task<IEnumerable<RegistroDiario>> GetByEmpleadoMesActualAsync(int empleadoId)
    {
        var hoy = DateTime.Today;
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
        var finMes = inicioMes.AddMonths(1).AddDays(-1);
        return await GetByEmpleadoRangoFechasAsync(empleadoId, inicioMes, finMes);
    }
    
    public async Task<bool> ExistsByFechaEmpleadoAsync(DateTime fecha, int empleadoId)
    {
        var registro = await GetByFechaEmpleadoAsync(fecha, empleadoId);
        return registro != null;
    }
    
    public async Task<decimal> GetTotalHorasByEmpleadoRangoAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin)
    {
        var registros = await GetByEmpleadoRangoFechasAsync(empleadoId, fechaInicio, fechaFin);
        return registros.Sum(r => r.TotalHoras);
    }
    
    public Task<DetalleActividad> AddDetalleAsync(int registroId, DetalleActividad detalle)
    {
        // TODO: Implement with Firebase subcollection or embedded document
        throw new NotImplementedException("AddDetalleAsync not implemented in Web version");
    }
    
    public Task UpdateDetalleAsync(int registroId, DetalleActividad detalle)
    {
        throw new NotImplementedException("UpdateDetalleAsync not implemented in Web version");
    }
    
    public Task UpdateDetalleAsync(DetalleActividad detalle)
    {
        throw new NotImplementedException("UpdateDetalleAsync not implemented in Web version");
    }
    
    public Task DeleteDetalleAsync(int registroId, int detalleId)
    {
        throw new NotImplementedException("DeleteDetalleAsync not implemented in Web version");
    }
    
    public Task<DetalleActividad?> GetDetalleByIdAsync(int registroId, int detalleId)
    {
        throw new NotImplementedException("GetDetalleByIdAsync not implemented in Web version");
    }
    
    public Task<DetalleActividad?> GetDetalleByIdAsync(int detalleId)
    {
        throw new NotImplementedException("GetDetalleByIdAsync not implemented in Web version");
    }

    public async Task<IEnumerable<DetalleActividad>> GetDetallesByProyectoAsync(int proyectoId)
    {
        // En la versión web, los detalles están embebidos en los registros
        var registros = await GetAllAsync();
        var detalles = new List<DetalleActividad>();

        foreach (var registro in registros.Where(r => r.Activo && r.DetallesActividades != null))
        {
            var detallesProyecto = registro.DetallesActividades!
                .Where(d => d.ProyectoId == proyectoId && d.Activo);

            foreach (var detalle in detallesProyecto)
            {
                detalle.RegistroDiario = registro;
                detalles.Add(detalle);
            }
        }

        return detalles.OrderByDescending(d => d.RegistroDiario?.Fecha).ToList();
    }

    public async Task<IEnumerable<DetalleActividad>> GetDetallesByProyectoRangoFechasAsync(int proyectoId, DateTime fechaInicio, DateTime fechaFin)
    {
        var registros = await GetByRangoFechasAsync(fechaInicio, fechaFin);
        var detalles = new List<DetalleActividad>();

        foreach (var registro in registros.Where(r => r.DetallesActividades != null))
        {
            var detallesProyecto = registro.DetallesActividades!
                .Where(d => d.ProyectoId == proyectoId && d.Activo);

            foreach (var detalle in detallesProyecto)
            {
                detalle.RegistroDiario = registro;
                detalles.Add(detalle);
            }
        }

        return detalles.OrderByDescending(d => d.RegistroDiario?.Fecha).ToList();
    }

    public async Task<decimal> GetTotalHorasByProyectoAsync(int proyectoId)
    {
        var detalles = await GetDetallesByProyectoAsync(proyectoId);
        return detalles.Sum(d => d.Horas);
    }

    public async Task<IEnumerable<ProyectoHorasEmpleado>> GetHorasPorEmpleadoProyectoAsync(int proyectoId)
    {
        var detalles = await GetDetallesByProyectoAsync(proyectoId);

        var resumen = detalles
            .Where(d => d.RegistroDiario != null)
            .GroupBy(d => d.RegistroDiario!.EmpleadoId)
            .Select(g => new ProyectoHorasEmpleado
            {
                EmpleadoId = g.Key,
                EmpleadoNombre = g.First().RegistroDiario?.Empleado?.NombreCompleto ?? $"Empleado {g.Key}",
                TotalHoras = g.Sum(d => d.Horas),
                CantidadActividades = g.Count(),
                UltimaActividad = g.Max(d => d.RegistroDiario?.Fecha)
            })
            .OrderByDescending(x => x.TotalHoras)
            .ToList();

        return resumen;
    }
}
