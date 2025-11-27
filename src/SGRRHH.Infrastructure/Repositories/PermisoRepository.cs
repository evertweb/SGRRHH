using Microsoft.EntityFrameworkCore;
using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;
using SGRRHH.Infrastructure.Data;

namespace SGRRHH.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio de permisos
/// </summary>
public class PermisoRepository : Repository<Permiso>, IPermisoRepository
{
    public PermisoRepository(AppDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtiene todos los permisos con sus relaciones incluidas
    /// </summary>
    public override async Task<IEnumerable<Permiso>> GetAllAsync()
    {
        return await _context.Set<Permiso>()
            .Include(p => p.Empleado)
                .ThenInclude(e => e.Departamento)
            .Include(p => p.Empleado)
                .ThenInclude(e => e.Cargo)
            .Include(p => p.TipoPermiso)
            .Include(p => p.SolicitadoPor)
            .Include(p => p.AprobadoPor)
            .OrderByDescending(p => p.FechaSolicitud)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene un permiso por ID con sus relaciones
    /// </summary>
    public override async Task<Permiso?> GetByIdAsync(int id)
    {
        return await _context.Set<Permiso>()
            .Include(p => p.Empleado)
                .ThenInclude(e => e.Departamento)
            .Include(p => p.Empleado)
                .ThenInclude(e => e.Cargo)
            .Include(p => p.TipoPermiso)
            .Include(p => p.SolicitadoPor)
            .Include(p => p.AprobadoPor)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <summary>
    /// Obtiene todos los permisos pendientes de aprobación
    /// </summary>
    public async Task<IEnumerable<Permiso>> GetPendientesAsync()
    {
        return await _context.Set<Permiso>()
            .Include(p => p.Empleado)
                .ThenInclude(e => e.Departamento)
            .Include(p => p.Empleado)
                .ThenInclude(e => e.Cargo)
            .Include(p => p.TipoPermiso)
            .Include(p => p.SolicitadoPor)
            .Where(p => p.Estado == EstadoPermiso.Pendiente)
            .OrderBy(p => p.FechaSolicitud)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene los permisos de un empleado específico
    /// </summary>
    public async Task<IEnumerable<Permiso>> GetByEmpleadoIdAsync(int empleadoId)
    {
        return await _context.Set<Permiso>()
            .Include(p => p.Empleado)
            .Include(p => p.TipoPermiso)
            .Include(p => p.SolicitadoPor)
            .Include(p => p.AprobadoPor)
            .Where(p => p.EmpleadoId == empleadoId)
            .OrderByDescending(p => p.FechaSolicitud)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene los permisos en un rango de fechas
    /// </summary>
    public async Task<IEnumerable<Permiso>> GetByRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        return await _context.Set<Permiso>()
            .Include(p => p.Empleado)
            .Include(p => p.TipoPermiso)
            .Include(p => p.SolicitadoPor)
            .Include(p => p.AprobadoPor)
            .Where(p => p.FechaInicio <= fechaFin && p.FechaFin >= fechaInicio)
            .OrderBy(p => p.FechaInicio)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene los permisos por estado
    /// </summary>
    public async Task<IEnumerable<Permiso>> GetByEstadoAsync(EstadoPermiso estado)
    {
        return await _context.Set<Permiso>()
            .Include(p => p.Empleado)
            .Include(p => p.TipoPermiso)
            .Include(p => p.SolicitadoPor)
            .Include(p => p.AprobadoPor)
            .Where(p => p.Estado == estado)
            .OrderByDescending(p => p.FechaSolicitud)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene el próximo número de acta disponible
    /// </summary>
    public async Task<string> GetProximoNumeroActaAsync()
    {
        var año = DateTime.Now.Year;
        var ultimoPermiso = await _context.Set<Permiso>()
            .Where(p => p.NumeroActa.StartsWith($"PERM-{año}"))
            .OrderByDescending(p => p.NumeroActa)
            .FirstOrDefaultAsync();

        int siguienteNumero = 1;
        if (ultimoPermiso != null)
        {
            // Extraer el número del formato PERM-YYYY-NNNN
            var partes = ultimoPermiso.NumeroActa.Split('-');
            if (partes.Length == 3 && int.TryParse(partes[2], out int numero))
            {
                siguienteNumero = numero + 1;
            }
        }

        return $"PERM-{año}-{siguienteNumero:D4}";
    }

    /// <summary>
    /// Verifica si existe solapamiento de fechas para un empleado
    /// </summary>
    public async Task<bool> ExisteSolapamientoAsync(int empleadoId, DateTime fechaInicio, DateTime fechaFin, int? excludePermisoId = null)
    {
        var query = _context.Set<Permiso>()
            .Where(p => p.EmpleadoId == empleadoId)
            .Where(p => p.Estado == EstadoPermiso.Pendiente || p.Estado == EstadoPermiso.Aprobado)
            .Where(p => p.FechaInicio <= fechaFin && p.FechaFin >= fechaInicio);

        if (excludePermisoId.HasValue)
        {
            query = query.Where(p => p.Id != excludePermisoId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Obtiene todos los permisos con filtros opcionales
    /// </summary>
    public async Task<IEnumerable<Permiso>> GetAllWithFiltersAsync(
        int? empleadoId = null, 
        EstadoPermiso? estado = null, 
        DateTime? fechaDesde = null, 
        DateTime? fechaHasta = null)
    {
        var query = _context.Set<Permiso>()
            .Include(p => p.Empleado)
                .ThenInclude(e => e.Departamento)
            .Include(p => p.Empleado)
                .ThenInclude(e => e.Cargo)
            .Include(p => p.TipoPermiso)
            .Include(p => p.SolicitadoPor)
            .Include(p => p.AprobadoPor)
            .AsQueryable();

        if (empleadoId.HasValue)
        {
            query = query.Where(p => p.EmpleadoId == empleadoId.Value);
        }

        if (estado.HasValue)
        {
            query = query.Where(p => p.Estado == estado.Value);
        }

        if (fechaDesde.HasValue)
        {
            query = query.Where(p => p.FechaInicio >= fechaDesde.Value);
        }

        if (fechaHasta.HasValue)
        {
            query = query.Where(p => p.FechaFin <= fechaHasta.Value);
        }

        return await query
            .OrderByDescending(p => p.FechaSolicitud)
            .ToListAsync();
    }
}
